namespace Romanizer.Models;

public class FTPSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RemoteRomDirectory { get; set; } = string.Empty;
    public string ExclusionFileName { get; set; } = "excluded_files.txt";
}