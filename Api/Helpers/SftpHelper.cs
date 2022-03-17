using System;
using System.Collections.Generic;
using Api.Options;
using Microsoft.AspNetCore.Http;
using Renci.SshNet;

namespace Api.Helpers
{
    public class SftpHelper : IDisposable
    {
        private readonly SftpClient _client;
        private readonly string _path;

        public SftpHelper(LinuxCredentials linuxCredentials, string path)
        {
            _path = path;

            _client = new SftpClient(linuxCredentials.Host,
                linuxCredentials.Port,
                linuxCredentials.Username,
                linuxCredentials.Password);

            _client.Connect();
        }
        
        public string CreateUserFolder(int userId)
        {
            if (!_client.Exists(_path))
            {
                this.RestoreUserFolder();
            }

            string userDirectory = $"{_path}/{userId}-{DateTime.Now:yyyyMMddTHHmmss}";

            _client.CreateDirectory(userDirectory);

            return userDirectory;
        }
        
        public IEnumerable<string> SendFiles(IFormFileCollection files, string currentPath)
        {
            foreach (IFormFile file in files)
            {
                string filePath = currentPath + "/" + file.FileName;
                
                _client.UploadFile(file.OpenReadStream(), filePath, false);
                
                yield return file.FileName;
            }
        }
        
        private void RestoreUserFolder()
        {
            if (!_path.Contains('/'))
            {
                throw new ArgumentException("The path must match the format /dir/dir");
            }
            
            string[] folders = _path.Split('/');

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
}