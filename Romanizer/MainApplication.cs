using Microsoft.Extensions.Configuration;
using Romanizer.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Romanizer
{
    public class MainApplication
    {
        private readonly IConfiguration _configuration;

        public MainApplication(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            GetDirectoryList();
        }

        /// <summary>
        /// Retrieves the list of directories to process from appsettings.json
        /// </summary>
        private void GetDirectoryList()
        {
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>()
                ?? new AppSettings();

            var parentInputDirectory = appSettings.Directories.InputDirectory;

            //Process root directory for any files directly inside it
            ProcessDirectory(parentInputDirectory);
        }

        /// <summary>
        /// Processes the given directory for archive and ROM files.
        /// </summary>
        /// <param name="dir">The directory to process.</param>
        private void ProcessDirectory(string dir)
        {            
            // Get files in directory
            Utilities.WriteMessage($"\r\nProcessing directory: {dir}...", ConsoleColor.Magenta);
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Utilities.WriteMessage("No files found in directory.", ConsoleColor.Yellow);
                return;
            }

            Utilities.WriteMessage($"\r\n\tFound {files.Length} files in directory.", ConsoleColor.DarkGray);

            // Check for archive files
            if (ArchivesExist(files, out string[]? archivePaths))
            {
                Utilities.WriteMessage($"\tFound {archivePaths!.Length} archive(s) in directory.", ConsoleColor.DarkGray);
                foreach (var archivePath in archivePaths!)
                {                    
                    ProcessArchive(archivePath);
                }
            }
            else
            {
                Utilities.WriteMessage("\tNo archives found.", ConsoleColor.Yellow);
            }

            // Check for .nsp or .xci files and move them to the output folder
            if (RomFilesExist(files, out List<string> romFiles))
            {
                Utilities.WriteMessage($"\tFound {romFiles.Count} ROM file(s) in directory.", ConsoleColor.DarkGray);
                foreach (var romFile in romFiles)
                {
                    var romFileName = Path.GetFileName(romFile);
                    Utilities.WriteMessage($"\r\n\tMoving ROM file: {romFileName}", ConsoleColor.Green);
                    ProcessRomFile(romFile);
                }
            }
            else
            {
                Utilities.WriteMessage("\r\n\tNo ROM files found.", ConsoleColor.Yellow);
            }

            Utilities.WriteMessage($"\r\nDirectory processing complete!", ConsoleColor.Magenta);
        }

        /// <summary>
        /// Processes the given ROM file by copying it to the output directory.
        /// </summary>
        /// <param name="romFile">The path to the ROM file.</param>
        private void ProcessRomFile(string romFile)
        {
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>()
                   ?? new AppSettings();

            var outputPath = appSettings.Directories.OutputDirectory;

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var destinationPath = Path.Combine(outputPath, Path.GetFileName(romFile));
            File.Move(romFile, destinationPath, overwrite: true);
            Utilities.WriteMessage($"\t• ROM file copied to: {destinationPath}", ConsoleColor.Green);
        }

        /// <summary>
        /// Checks if there are any ROM files (.nsp, .xci, .nsz) in the given list of files.
        /// </summary>
        /// <param name="dirFiles">The list of files to check.</param>
        /// <param name="romFiles">The output list of found ROM files.</param>
        private static bool RomFilesExist(string[] dirFiles, out List<string> romFiles)
        {
            var validExtensions = new List<string> { ".nsp", ".xci", ".nsz" };
            romFiles = [.. dirFiles.Where(file => validExtensions.Contains(Path.GetExtension(file).ToLower()))];
            return romFiles.Count > 0;
        }

        /// <summary>
        /// Processes the given archive file.
        /// </summary>
        /// <param name="archivePath">The path to the archive file.</param>
        private void ProcessArchive(string archivePath)
        {
            if (!File.Exists(archivePath))
            {
                Utilities.WriteMessage($"\tArchive file not found: {archivePath}", ConsoleColor.Red);
                return;
            }

            Utilities.WriteMessage($"\r\n\tProcessing archive: {archivePath}", ConsoleColor.Cyan);
            ExtractArchive(archivePath);
        }

        /// <summary>
        /// Extracts the given archive to the output directory specified in appsettings.json
        /// </summary>
        /// <param name="archivePath">The path to the archive file.</param>
        private void ExtractArchive(string archivePath)
        {
            try
            {
                var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>()
                    ?? new AppSettings();

                var outputPath = appSettings.Directories.OutputDirectory;

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                using (var archive = ArchiveFactory.Open(archivePath))
                {
                    archive.WriteToDirectory(outputPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });

                    Utilities.WriteMessage($"\t• Archive extracted successfully to: {outputPath}", ConsoleColor.Green);
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteMessage($"\t• Error extracting archive: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Checks if there are any archive files (.zip, .rar, .7z) in the given list of files.
        /// </summary>
        /// <param name="dirFiles">The list of files to check.</param>
        /// <param name="archivePaths">The output list of found archive files.</param>
        private static bool ArchivesExist(string[] dirFiles, out string[]? archivePaths)
        {
            var validExtensions = new List<string> { ".zip", ".rar", ".7z" };
            archivePaths = [.. dirFiles.Where(file => validExtensions.Contains(Path.GetExtension(file).ToLower()))];
            archivePaths ??= [];

            return archivePaths.Length > 0;
        }

    }
}