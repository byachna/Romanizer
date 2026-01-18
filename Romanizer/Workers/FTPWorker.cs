using FluentFTP;
using Microsoft.Extensions.Configuration;
using Romanizer.Models;

namespace Romanizer.Workers
{
    public class FTPWorker
    {
        // FTP worker implementation will go here
        private readonly IConfiguration _configuration;
        private FTPSettings _ftpSettings;
        private AppSettings _appSettings;

        public FTPWorker(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void LoadSettings()
        {
            // Load FTP settings from configuration
            _ftpSettings = _configuration.GetSection("FTPSettings").Get<FTPSettings>()
                ?? new FTPSettings();      
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>()
                ?? new AppSettings();      
        }

        public void ConnectAndDownload()
        {
            Utilities.WriteMessage("Connecting to FTP server...", ConsoleColor.Cyan);
            LoadSettings();

            using (var ftpClient = new FtpClient(_ftpSettings.Host, _ftpSettings.Username, _ftpSettings.Password, port: _ftpSettings.Port))
            {
                ftpClient.AutoConnect();
                Utilities.WriteMessage("Connected to FTP server.", ConsoleColor.Green);
                
                var items = ftpClient.GetListing(_ftpSettings.RemoteRomDirectory);
                Utilities.WriteMessage($"\r\n\tFound {items.Length} file/folder(s) in directory.", ConsoleColor.DarkGray);

                foreach (var item in items)
                {
                    if (ItemIsExcluded(item.FullName))
                    {
                        Utilities.WriteMessage($"\t• Skipping excluded item: {item.Name}", ConsoleColor.Yellow);
                        continue;
                    }
                    
                    if (item.Type == FtpObjectType.Directory || item.Type == FtpObjectType.File)
                    {
                        Utilities.WriteMessage($"\t• Downloading: {item.Name}", ConsoleColor.Cyan);
                        var localPath = Path.Combine(_appSettings.Directories.InputDirectory, item.Name);
                        ftpClient.DownloadDirectory(localPath, item.FullName);
                        Utilities.WriteMessage($"\t• Downloaded to: {localPath}", ConsoleColor.Green);
                    }

                    AddItemToExcludeList(item.FullName);
                }

                ftpClient.Disconnect();
            }
        }

        private bool ItemIsExcluded(string fullName)
        {
            var exclusionFilePath = Path.Combine(_appSettings.Directories.InputDirectory, _ftpSettings.ExclusionFileName);
            if (!File.Exists(exclusionFilePath))
            {
                return false;
            }

            var excludedItems = File.ReadAllLines(exclusionFilePath);
            return excludedItems.Contains(fullName);
        }

        private void AddItemToExcludeList(string fullName)
        {
            Utilities.WriteMessage($"\t• Adding to future exclude list: {fullName}", ConsoleColor.Yellow);
            
            File.AppendAllText(
                Path.Combine(_appSettings.Directories.InputDirectory, _ftpSettings.ExclusionFileName),
                fullName + Environment.NewLine);
        }
    }
}