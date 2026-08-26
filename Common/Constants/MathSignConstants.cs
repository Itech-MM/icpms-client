using System.Collections.ObjectModel;
using icpms_client.ViewModels.Wrappers;

namespace icpms_client.Common.Constants;

public static class MathSignConstants
{
    public static ObservableCollection<LabelValueWrapper> MathSigns =
    [
        /*new MathSign(1, "="),
        new MathSign(2, ">"),
        new MathSign(3, ">="),
        new MathSign(4, "<"),
        new MathSign(5, "<=")*/
        new LabelValueWrapper("=", "1"),
        new LabelValueWrapper(">", "2"),
        new LabelValueWrapper(">=", "3"),
        new LabelValueWrapper("<", "4"),
        new LabelValueWrapper("<=", "5"),
    ];
}
