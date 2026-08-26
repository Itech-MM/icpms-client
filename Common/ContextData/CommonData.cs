using System.Collections.ObjectModel;
using icpms_client.Utils.General;
using icpms_client.ViewModels.Wrappers;

namespace icpms_client.Common.ContextData;

public static class CommonData
{
    public static string FtpFolderPath { get; set; } = string.Empty;
    public static bool IsInitialize { get; set; } = false;
    

    public static void InitializeFtp(string host, string userName, string password)
    {
        FtpUtil = new FtpUtil(host, userName, password);
    }

    public static FtpUtil? FtpUtil { get; private set; }

    public static ObservableCollection<LabelValueWrapper> PaymentTypeList = [];

}