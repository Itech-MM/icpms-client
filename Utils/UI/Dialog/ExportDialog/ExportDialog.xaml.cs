using System.Windows;
using icpms_client.Utils.UI.Dialog.ExportDialog;

namespace icpms_client.Utils.UI.Dialog.ExportDialog;

public partial class ExportDialog : Window
{
    public event Action<ExportType>? OnExport; 
    public ExportDialog()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        OnExport?.Invoke(ExportType.None);
    }

    private void ExportExcel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        OnExport?.Invoke(ExportType.Excel);
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        Close();
        OnExport?.Invoke(ExportType.Pdf);
    }
}