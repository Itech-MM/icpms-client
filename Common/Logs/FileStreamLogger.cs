using System.IO;
using FlexiStream.Player.Core;

namespace icpms_client.Common.Logs;

public class FileStreamLogger : IStreamLogger
{
    private readonly string _path;
    private readonly object _lock = new();

    public FileStreamLogger(string path = @"D:\FlexiTech\Company Data\Projects\ICPMS\client\logs\player.log")
    {
        _path = path;
    }

    public void Info(string streamName, string message) => Write("INFO", streamName, message);
    public void Warn(string streamName, string message) => Write("WARN", streamName, message);
    public void Error(string streamName, string message, Exception? ex = null) =>
        Write("ERROR", streamName, ex is null ? message : $"{message} {ex}");

    private void Write(string level, string streamName, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}][{streamName}] {message}";
        lock (_lock)
        {
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}