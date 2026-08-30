using System.Windows;
using icpms_client.Pages.Screens.Sections.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens.Sections;

public partial class HomeShiftSummarySection
{
    private readonly HomeShiftSummaryViewModel _viewModel;

    public HomeShiftSummarySection()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider!.GetRequiredService<HomeShiftSummaryViewModel>();
        DataContext = _viewModel;

        Loaded += Section_Loaded;
    }

    private void Section_Loaded(object sender, RoutedEventArgs e)
    {
        _ = _viewModel.LoadShiftSummaryAsync();
    }

    public void Refresh()
    {
        _ = _viewModel.Refresh();
    }
}