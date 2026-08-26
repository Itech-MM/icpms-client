using System.Windows.Markup;

namespace icpms_client.UserControls.UI.Reusable.Utils;

public class LighterBrushConverterExtension: MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new LighterBrushConverter();
    }
}