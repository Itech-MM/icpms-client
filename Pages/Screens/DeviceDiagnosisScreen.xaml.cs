using System.Windows;
using icpms_client.Pages.Screens.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens;

public partial class DeviceDiagnosisScreen
{
    private readonly DeviceDiagnosisViewModel _viewModel;

    public DeviceDiagnosisScreen()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider!.GetRequiredService<DeviceDiagnosisViewModel>();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) => await _viewModel.LoadAsync();

    private void OnUnloaded(object sender, RoutedEventArgs e) => _viewModel.CancelRun();
}