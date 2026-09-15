using System.Windows;
using icpms_client.Pages.Screens.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens;

public partial class ParkingSessionSearchScreen
{
    private readonly ParkingSessionSearchViewModel _viewModel;

    public ParkingSessionSearchScreen()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider!.GetRequiredService<ParkingSessionSearchViewModel>();
        DataContext = _viewModel;

        Loaded += ParkingSessionSearchScreen_Loaded;
    }

    private async void ParkingSessionSearchScreen_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}