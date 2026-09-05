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
        _viewModel.OnExitVisitorSaved += OnExitVisitorSaved;
        DataContext = _viewModel;

        Loaded += HomeScreen_Loaded;
    }

    private void OnExitVisitorSaved(bool obj)
    {
        ParkingSection.Refresh();
        ShiftSummarySection.Refresh();
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

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        _viewModel.IsShowPayments = false;
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        
        if(_viewModel.ExitIsMember) return;
        
        _viewModel.IsShowPayments = true;
    }
    
    private async void AlertsScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        const double threshold = 40;
        bool nearBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - threshold;

        if (nearBottom && scrollViewer.ScrollableHeight > 0 && DataContext is HomeScreenViewModel vm)
        {
            await vm.LoadMoreSecurityAlertsAsync();
        }
    }
}