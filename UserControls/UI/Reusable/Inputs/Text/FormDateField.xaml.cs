using System.Windows;
using icpms_client.UserControls.Customs.Forms.Validations.Form;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text;

public partial class FormDateField
{
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime?), typeof(FormDateField),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public event Action<DateTime?>? OnDateChanged;

    public FormDateField()
    {
        InitializeComponent();
    }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FormDateField)d;
        if (control.Picker.SelectedDate != (DateTime?)e.NewValue)
            control.Picker.SelectedDate = (DateTime?)e.NewValue;
    }

    private void Picker_OnSelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SelectedDate = Picker.SelectedDate;
        Value = Picker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        OnDateChanged?.Invoke(Picker.SelectedDate);
    }

    public override ValidationResult Validate() => ValidationResult.Success!;
}