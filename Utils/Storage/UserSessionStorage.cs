using System.IO;
using icpms_client.Network.Session;
using Newtonsoft.Json;

namespace icpms_client.Utils.Storage;

public static class UserSessionStorage
{
    private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "usersession.json");

    public static async Task SaveSessionAsync(UserDataObject session)
    {
        var json = JsonConvert.SerializeObject(session);
        await File.WriteAllTextAsync(FilePath, json);
    }

    public static async Task<UserDataObject?> LoadSessionAsync()
    {
        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);
        return JsonConvert.DeserializeObject<UserDataObject>(json);
    }

    public static void ClearSession()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}