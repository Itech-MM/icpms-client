namespace icpms_client.ViewModels.Wrappers;

public class LabelValueWrapper
{
    public string Label { get; set; } // Display text
    public string Value { get; set; } // Underlying value
    
    public bool IsEnabled { get; set; }
    public bool IsFocus { get; set; }
    public bool IsManual { get; set; }
    
    public object? ReferenceObject { get; set; }

    public LabelValueWrapper() 
    {
        Label = string.Empty;
        Value = string.Empty;
        IsEnabled = true;
    }

    public LabelValueWrapper(string label, string value, bool isEnabled = true, bool isManual = false, bool isFocus = false, object? referenceObject = null)
    {
        Label = label;
        Value = value;
        IsEnabled = isEnabled;
        IsManual = isManual;
        IsFocus = isFocus;
        ReferenceObject = referenceObject;
    }

    public static LabelValueWrapper Empty => new("", "");
    
    public override string ToString()
    {
        return Label;
    }
}