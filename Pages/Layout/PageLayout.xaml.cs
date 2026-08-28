using System.Windows;
using System.Windows.Controls;

namespace icpms_client.Pages.Layout;

public partial class PageLayout : UserControl
{
    public static readonly DependencyProperty PageContentProperty =
        DependencyProperty.Register(nameof(PageContent), typeof(object), typeof(PageLayout));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public PageLayout()
    {
        InitializeComponent();
    }
}