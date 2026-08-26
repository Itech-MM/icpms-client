using System.Windows.Controls;

namespace icpms_client.ViewModels.Wrappers
{
    public class TabItemWrapper(string tag, UserControl? content)
    {
        public string Tag { get; set; } = tag;
        public UserControl? Content { get; set; } = content;

        // Constructor with parameters

        // Empty constructor
        public TabItemWrapper() : this(string.Empty, null)
        {
        }
    }
}