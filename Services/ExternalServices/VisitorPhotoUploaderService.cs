using icpms_client.Common.ContextData;
using log4net;

namespace icpms_client.Services.ExternalServices;

public static class VisitorPhotoUploaderService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(VisitorPhotoUploaderService));

    public sealed record UploadedPhotos(string? PlatePhotoName, string? VehiclePhotoName);

    public static async Task<UploadedPhotos> UploadAsync(string direction, byte[]? plateBytes, byte[]? vehicleBytes)
    {
        var batchId = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid().ToString("N")[..12]}";

        var plateName = await UploadOneAsync(plateBytes, $"visitors/{direction}_{batchId}_plate.jpg");
        var vehicleName = await UploadOneAsync(vehicleBytes, $"visitors/{direction}_{batchId}_vehicle.jpg");

        return new UploadedPhotos(plateName, vehicleName);
    }

    private static Task<string?> UploadOneAsync(byte[]? bytes, string fileName)
    {
        var ftp = CommonData.FtpUtil;
        if (ftp == null || bytes == null || bytes.Length == 0) return Task.FromResult<string?>(null);

        var folder = CommonData.FtpFolderPath.Trim();
        var remotePath = string.IsNullOrEmpty(folder) ? fileName : $"{folder.TrimEnd('/')}/{fileName}";

        Log.Debug($"full path:: {remotePath}");
        
        return Task.Run<string?>(() =>
        {
            if (ftp.UploadFile(bytes, remotePath)) return fileName;

            Log.Error($"Photo upload failed for {remotePath}");
            return null;
        });
    }
}