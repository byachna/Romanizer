using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Romanizer.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

public class RomanizerApp
{
    private readonly IConfiguration _configuration;

    public RomanizerApp(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Run()
    {

        GetDirectoryList();
    }

    public static void WriteMessage(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    private void GetDirectoryList()
    {
        var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? new AppSettings();

        var directories = Directory.GetDirectories(appSettings.Directories.InputDirectory);
        foreach (var dir in directories)
        {
            ProcessDirectory(dir);
        }
    }

    private void ProcessDirectory(string dir)
    {
        // Get files in directory
        Console.WriteLine($"Processing directory: {dir}...");
        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            WriteMessage("\tNo files found in directory.", ConsoleColor.DarkGray);
            return;
        }

        Console.WriteLine($"\tFound {files.Length} files in directory.");

        // Check for archive files
        if (ArchiveExists(files, out string? archivePath))
        {
            var archiveName = Path.GetFileName(archivePath!);
            WriteMessage($"\tArchive found: {archiveName}", ConsoleColor.Green);
            ProcessArchive(archivePath!);

        }
        else
        {
            WriteMessage("\tNo archive found.", ConsoleColor.Yellow);
        }

        // Check for .nsp or .xci files and move them to the output folder
        if (RomFilesExist(files, out List<string> romFiles))
        {
            foreach (var romFile in romFiles)
            {
                var romFileName = Path.GetFileName(romFile);
                WriteMessage($"\tROM file found: {romFileName}", ConsoleColor.Green);
                ProcessRomFile(romFile);
            }
        }
        else
        {
            WriteMessage("\tNo ROM files found.", ConsoleColor.Yellow);
        }

    }

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
        WriteMessage($"\tROM file copied to: {destinationPath}", ConsoleColor.Green);
    }

    private static bool RomFilesExist(string[] dirFiles, out List<string> romFiles)
    {
        var validExtensions = new List<string> { ".nsp", ".xci", ".nsz" };
        romFiles = [.. dirFiles.Where(file => validExtensions.Contains(Path.GetExtension(file).ToLower()))];
        return romFiles.Count > 0;
    }

    private void ProcessArchive(string archivePath)
    {
        if (!File.Exists(archivePath))
        {
            WriteMessage($"\tArchive file not found: {archivePath}", ConsoleColor.Red);
            return;
        }

        WriteMessage($"\tProcessing archive: {archivePath}", ConsoleColor.Cyan);
        ExtractArchive(archivePath);
    }

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

                WriteMessage($"\tArchive extracted successfully to: {outputPath}", ConsoleColor.Green);
            }
        }
        catch (Exception ex)
        {
            WriteMessage($"\tError extracting archive: {ex.Message}", ConsoleColor.Red);
        }
    }

    private static bool ArchiveExists(string[] dirFiles, out string? archivePath)
    {
        var validExtensions = new List<string> { ".zip", ".rar", ".7z" };
        var archiveFilePath = dirFiles.FirstOrDefault(file => validExtensions.Contains(Path.GetExtension(file).ToLower()));
        archivePath = null;

        if (archiveFilePath != null)
        {
            archivePath = archiveFilePath;
            return true;
        }
        return false;
    }

}