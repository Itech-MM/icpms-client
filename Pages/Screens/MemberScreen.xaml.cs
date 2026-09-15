using System.Windows;
using icpms_client.Pages.Screens.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens;

public partial class MemberScreen
{
    private readonly MemberScreenViewModel _viewModel;

    public MemberScreen()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider!.GetRequiredService<MemberScreenViewModel>();
        DataContext = _viewModel;

        Loaded += MemberScreen_Loaded;
    }

    private async void MemberScreen_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}