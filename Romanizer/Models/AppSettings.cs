namespace Romanizer.Models;

public class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Directories Directories { get; set; } = new();
}

public class Directories
{
    public string InputDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
}

public class LoggingSettings
{
    public LogLevel LogLevel { get; set; } = new();
}

public class LogLevel
{
    public string Default { get; set; } = "Information";
}
