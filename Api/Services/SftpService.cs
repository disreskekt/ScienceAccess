using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Api.Exceptions;
using Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Api.Services;

public class SftpService : IDisposable
    {
        private readonly SftpClient _client;
        private readonly string _baseFolderPath;

        public SftpService(IOptions<BaseFolder> baseFolderOptions, IOptions<LinuxCredentials> linuxCredentialOptions)
        {
            _baseFolderPath = baseFolderOptions.Value.Path;

            LinuxCredentials linuxCredentials = linuxCredentialOptions.Value;

            _client = new SftpClient(linuxCredentials.Host,
                linuxCredentials.Port,
                linuxCredentials.Username,
                linuxCredentials.Password);

            _client.Connect();
        }

        public string RestoreTaskFolder(string email, string guid)
        {
            string taskDirectory = $"{_baseFolderPath}/{email}/{guid}";
            
            RestoreFolder(taskDirectory);

            return taskDirectory;
        }
        
        public IEnumerable<string> SendFiles(IFormFileCollection files, string currentPath)
        {
            List<string> filenames = new List<string>(files.Count);

            foreach (IFormFile file in files)
            {
                string filePath = currentPath + "/" + file.FileName;

                _client.UploadFile(file.OpenReadStream(), filePath, true);

                filenames.Add(file.FileName);
            }

            return filenames;
        }

        public string[] ListOfFiles(string path)
        {
            if (!_client.Exists(path))
            {
                throw new WrongFilenameException($"{path} doesn't exist");
            }
            
            return _client.ListDirectory(path).Select(file => file.Name).ToArray();
        }
        
        public byte[] GetFile(string path, string filename)
        {
            string fullPath = path + "/" + filename;

            if (!_client.Exists(fullPath))
            {
                throw new WrongFilenameException($"{filename} doesn't exist");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                _client.DownloadFile(fullPath, memoryStream);

                return memoryStream.ToArray();
            }
        }

        public byte[] GetFiles(string path, string[] filenames)
        {
            string tempDateTime = "";
            
            try
            {
                tempDateTime = DateTime.Now.ToString("yyyyMMddTHHmmssfffffff");

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

                return array;
            }
            finally
            {
                if (File.Exists($"temp_{tempDateTime}.zip"))
                {
                    File.Delete($"temp_{tempDateTime}.zip");
                }
            }
        }

        public void DeleteFile(string path, string filename)
        {
            string fullPath = path + "/" + filename;

            if (_client.Exists(fullPath))
            {
                _client.DeleteFile(fullPath);
            }
        }
        
        private void AddFileToZip(ZipArchive zip, string path, string filename, string tempDateTime)
        {
            try
            {
                string fullPath = path + "/" + filename;

                using (FileStream fileStream =
                       new FileStream($"temp_{tempDateTime}_{filename}",
                           FileMode.Create))
                {
                    _client.DownloadFile(fullPath, fileStream);
                }

                zip.CreateEntryFromFile($"temp_{tempDateTime}_{filename}", $"{filename}", CompressionLevel.Optimal);
            }
            finally
            {
                if (File.Exists($"temp_{tempDateTime}_{filename}"))
                {
                    File.Delete($"temp_{tempDateTime}_{filename}");
                }
            }
        }

        public void RestoreFolder(string pathToRestore)
        {
            if (_client.Exists(pathToRestore))
            {
                return;
            }
            
            if (!pathToRestore.StartsWith('/'))
            {
                throw new ArgumentException("The path must match the format /dir/dir");
            }

            string[] folders = pathToRestore.Split('/');

            string growingPath = "/";

            foreach (string folder in folders)
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }
                
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