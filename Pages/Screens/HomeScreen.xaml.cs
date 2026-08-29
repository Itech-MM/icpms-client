using System.Windows;
using System.Windows.Controls;
using icpms_client.Pages.Screens.ViewModels;
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
        DataContext = _viewModel;

        Loaded += HomeScreen_Loaded;
    }

    private async void HomeScreen_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.InitializeEntranceAnprCamera(new RtspPlayer(EntranceAnprControl));
        _viewModel.InitializeEntranceCctvCamera(new RtspPlayer(EntranceCctvControl));
        _viewModel.InitializeExitAnprCamera(new RtspPlayer(ExitAnprControl));
        _viewModel.InitializeExitCctvCamera(new RtspPlayer(ExitCctvControl));

        await _viewModel.InitializeAsync();
    }
}