using System.Windows;
using icpms_client.Pages.Screens.Sections.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens.Sections;

public partial class HomeParkingAreaSummarySection
{
    private readonly HomeParkingAreaSummaryViewModel _viewModel;

    
    
    public HomeParkingAreaSummarySection()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider!.GetRequiredService<HomeParkingAreaSummaryViewModel>();
        DataContext = _viewModel;

        Loaded += Section_Loaded;
    }

    private void Section_Loaded(object sender, RoutedEventArgs e)
    {
        _ = _viewModel.LoadParkingAreaSummaryAsync();
    }
    
    public void Refresh()
    {
        _ = _viewModel.Refresh();
    }
}