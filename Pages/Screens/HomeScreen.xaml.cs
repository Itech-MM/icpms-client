using System.Windows;
using System.Windows.Controls;
using icpms_client.Network.DTO.Vehicle;
using icpms_client.Pages.Screens.ViewModels;
using icpms_client.Services.UIServices;
using icpms_client.Tools.VideoPlayer;
using icpms_client.ViewModels.Wrappers;
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

        PlateSearchField.OnSearch += OnPlateSearch;
        PlateSearchField.OnValueChanged += OnPlateSearchValueChanged;
        
        Loaded += HomeScreen_Loaded;
        Unloaded += HomeScreen_Unloaded;
    }

    private void OnExitVisitorSaved(bool obj)
    {
        ParkingSection.Refresh();
        ShiftSummarySection.Refresh();
        RecentVisitorsSection.Refresh();
    }

    private void OnVisitorSaved(bool obj)
    {
        ParkingSection.Refresh();
        ShiftSummarySection.Refresh();
        RecentVisitorsSection.Refresh();
    }

    private async void HomeScreen_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.InitializeEntranceAnprCamera(EntranceAnprControl);
        _viewModel.InitializeEntranceCctvCamera(EntranceCctvControl);
        _viewModel.InitializeExitAnprCamera(ExitAnprControl);
        _viewModel.InitializeExitCctvCamera(ExitCctvControl);
        
        await _viewModel.InitializeAsync();
        
    }
    
    private void HomeScreen_Unloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnVisitorSaved -= OnVisitorSaved;
        _viewModel.OnExitVisitorSaved -= OnExitVisitorSaved;
        _viewModel.Dispose();
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
    
    private async void OnPlateSearch(string keyword)
    {
        if (DataContext is not HomeScreenViewModel vm) return;

        PlateSearchField.SearchResults.Clear();
        // This now works because of the parameterless constructor
        PlateSearchField.SearchResults.Add(new LabelValueWrapper { Label = "Searching..." });
        PlateSearchField.IsPopupOpen = true;

        var results = await vm.SearchVehiclesAsync(keyword);

        PlateSearchField.SearchResults.Clear();
        if (results.Count == 0)
        {
            PlateSearchField.SearchResults.Add(new LabelValueWrapper { Label = "No matches" });
            return;
        }

        foreach (var vehicle in results)
        {
            PlateSearchField.SearchResults.Add(new LabelValueWrapper
            {
                Label = string.IsNullOrWhiteSpace(vehicle.MemberName)
                    ? vehicle.PlateNumber
                    : $"{vehicle.PlateNumber} — {vehicle.MemberName}",
                Value = vehicle.PlateNumber,
                ReferenceObject = vehicle
            });
        }
    }

    private void OnPlateSearchValueChanged(LabelValueWrapper wrapper)
    {
        if (wrapper.ReferenceObject is VehicleDto vehicle && DataContext is HomeScreenViewModel vm)
        {
            vm.SelectPlateSearchResultCommand.Execute(vehicle);
        }
    }
}