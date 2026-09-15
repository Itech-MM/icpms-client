using System.Windows.Controls;

namespace icpms_client.Pages.Layout;

public partial class PageLayout
{
    public PageLayout()
    {
        InitializeComponent();
    }

    public UserControl? CurrentScreen { get; private set; }

    public UserControl? PushScreen(UserControl screen)
    {
        var previous = CurrentScreen;

        ScreenHost.Children.Add(screen);
        Panel.SetZIndex(screen, ScreenHost.Children.Count);

        CurrentScreen = screen;

        return previous;
    }

    public void RemoveScreen(UserControl screen)
    {
        if (ScreenHost.Children.Contains(screen))
            ScreenHost.Children.Remove(screen);
    }
}