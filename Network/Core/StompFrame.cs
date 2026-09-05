using System.Text;

namespace icpms_client.Network.Core;

public class StompFrame
{
    public string Command { get; }
    public Dictionary<string, string> Headers { get; }
    public string Body { get; }

    public StompFrame(string command, Dictionary<string, string> headers, string body)
    {
        Command = command;
        Headers = headers;
        Body = body;
    }

    public static string Build(string command, Dictionary<string, string>? headers = null, string body = "")
    {
        headers ??= new Dictionary<string, string>();
        var sb = new StringBuilder();
        sb.Append(command).Append('\n');
        foreach (var kv in headers)
            sb.Append(kv.Key).Append(':').Append(kv.Value).Append('\n');
        sb.Append('\n').Append(body).Append('\0');
        return sb.ToString();
    }

    public static StompFrame Parse(string raw)
    {
        raw = raw.TrimEnd('\0', '\n');
        int split = raw.IndexOf("\n\n", StringComparison.Ordinal);
        string headerPart = split >= 0 ? raw[..split] : raw;
        string body = split >= 0 ? raw[(split + 2)..] : string.Empty;

        var lines = headerPart.Split('\n');
        var headers = new Dictionary<string, string>();
        for (int i = 1; i < lines.Length; i++)
        {
            int idx = lines[i].IndexOf(':');
            if (idx > 0)
                headers[lines[i][..idx]] = lines[i][(idx + 1)..];
        }
        return new StompFrame(lines[0], headers, body);
    }
}