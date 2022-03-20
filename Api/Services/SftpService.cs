using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Api.Exceptions;
using Api.Options;
using Microsoft.AspNetCore.Http;
using Renci.SshNet;

namespace Api.Services;

public class SftpService : IDisposable
    {
        private readonly SftpClient _client;
        private readonly string _userFolderPath;

        public SftpService(LinuxCredentials linuxCredentials, string userFolderPath)
        {
            _userFolderPath = userFolderPath;

            _client = new SftpClient(linuxCredentials.Host,
                linuxCredentials.Port,
                linuxCredentials.Username,
                linuxCredentials.Password);

            _client.Connect();
        }

        public string CreateUserFolder(int userId)
        {
            if (!_client.Exists(_userFolderPath))
            {
                this.RestoreFolder(_userFolderPath);
            }

            string userDirectory = $"{_userFolderPath}/{userId}-{DateTime.Now:yyyyMMddTHHmmss}";

            _client.CreateDirectory(userDirectory);

            return userDirectory;
        }

        public void CheckUserFolder(string path)
        {
            if (!_client.Exists(path))
            {
                this.RestoreFolder(path);
            }
        }
        
        public IEnumerable<string> SendFiles(IFormFileCollection files, string currentPath)
        {
            List<string> filenames = new List<string>(files.Count);

            foreach (IFormFile file in files)
            {
                string filePath = currentPath + "/" + file.FileName;

                _client.UploadFile(file.OpenReadStream(), filePath, false);

                filenames.Add(file.FileName);
            }

            return filenames;
        }

        public byte[] GetFile(string path, string filename)
        {
            string fullPath = path + "/" + filename;

            if (!_client.Exists(fullPath))
            {
                throw new WrongFilenameException($"{fullPath} doesn't exist");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                _client.DownloadFile(fullPath, memoryStream);

                return memoryStream.ToArray();
            }
        }

        public byte[] GetFiles(string path, string[] filenames)
        {
            string tempDateTime = DateTime.Now.ToString("yyyyMMddTHHmmssfffffff");

            using (ZipArchive zip = ZipFile.Open($"temp_{tempDateTime}.zip", ZipArchiveMode.Create))
            {
                foreach (string filename in filenames)
                {
                    if (!_client.Exists((path + "/" + filename)))
                    {
                        throw new WrongFilenameException($"{path + "/" + filename} doesn't exist");
                    }
                    
                    AddFileToZip(zip, path, filename, tempDateTime);
                }
            }

            byte[] array;

            using (FileStream fileStream = new FileStream($"temp_{tempDateTime}.zip", FileMode.Open))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);

                    array = memoryStream.ToArray();
                }
            }

            File.Delete($"temp_{tempDateTime}.zip");

            return array;
        }

        private void AddFileToZip(ZipArchive zip, string path, string filename, string tempDateTime)
        {
            string fullPath = path + "/" + filename;

            if (!_client.Exists(fullPath))
            {
                throw new WrongFilenameException($"{fullPath} doesn't exist");
            }

            using (FileStream fileStream =
                   new FileStream($"temp_{tempDateTime}_{filename}",
                       FileMode.Create))
            {
                _client.DownloadFile(fullPath, fileStream);
            }

            zip.CreateEntryFromFile($"temp_{tempDateTime}_{filename}", $"{filename}", CompressionLevel.Optimal);

            File.Delete($"temp_{tempDateTime}_{filename}");
        }

        private void RestoreFolder(string pathToRestore)
        {
            if (!pathToRestore.Contains('/'))
            {
                throw new ArgumentException("The path must match the format /dir/dir");
            }

            string[] folders = pathToRestore.Split('/');

            string growingPath = "/";

            foreach (string folder in folders)
            {
                growingPath += folder;

                if (!_client.Exists(growingPath))
                {
                    _client.CreateDirectory(growingPath);
                }

                growingPath += "/";
            }
        }

        public void Dispose()
        {
            _client.Disconnect();

            _client.Dispose();
        }
    }