using System.Windows;
using System.Windows.Controls;
using icpms_client.Pages.Screens.Sections.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens.Sections;

public partial class RecentVisitorsSection
{
    public RecentVisitorsSectionViewModel ViewModel { get; }

    public RecentVisitorsSection()
    {
        InitializeComponent();

        ViewModel = App.ServiceProvider!.GetRequiredService<RecentVisitorsSectionViewModel>();
        DataContext = ViewModel;

        Loaded += RecentVisitorsSection_Loaded;
    }

    private bool _hasLoadedOnce;

    private async void RecentVisitorsSection_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoadedOnce) return;
        _hasLoadedOnce = true;

        await ViewModel.InitializeAsync();
    }

    private async void VisitorsScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        const double threshold = 40;
        var nearBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - threshold;

        if (nearBottom && scrollViewer.ScrollableHeight > 0)
        {
            await ViewModel.LoadMoreAsync();
        }
    }
    
    public async void Refresh()
    {
        await ViewModel.RefreshPreservingPositionAsync();
    }
}