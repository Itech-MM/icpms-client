using System.Windows;
using System.Windows.Controls;
using icpms_client.Pages.Screens.ViewModels;
using icpms_client.Services.UIServices;
using icpms_client.Tools.VideoPlayer;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens;

public partial class HomeScreen
{
    private readonly HomeScreenViewModel _viewModel;

    public HomeScreen()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider!.GetRequiredService<HomeScreenViewModel>();
        _viewModel.OnVisitorSaved += OnVisitorSaved;
        DataContext = _viewModel;

        Loaded += HomeScreen_Loaded;
    }

    private void OnVisitorSaved(bool obj)
    {
        ParkingSection.Refresh();
        ShiftSummarySection.Refresh();
    }

    private async void HomeScreen_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.InitializeEntranceAnprCamera(new RtspPlayer(EntranceAnprControl));
        _viewModel.InitializeEntranceCctvCamera(new RtspPlayer(EntranceCctvControl));
        _viewModel.InitializeExitAnprCamera(new RtspPlayer(ExitAnprControl));
        _viewModel.InitializeExitCctvCamera(new RtspPlayer(ExitCctvControl));

        await _viewModel.InitializeAsync();
        
        ToastService.ShowInfo("Home screen loaded.");
    }
}