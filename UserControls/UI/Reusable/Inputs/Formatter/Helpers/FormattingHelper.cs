using System.Text.RegularExpressions;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Formatter.Helpers
{
    public static class FormattingHelper
    {
        /// <summary>
        /// Checks if the provided text is allowed based on the formatting type.
        /// </summary>
        /// <param name="formattingType">The formatting type (Text or Number).</param>
        /// <param name="text">The text to validate.</param>
        /// <returns>True if the text is valid for the specified formatting type; otherwise, false.</returns>
        public static bool IsAllowText(FormattingType formattingType, string text)
        {
            switch (formattingType)
            {
                case FormattingType.Text:
                    return true; // Allow any input for text

                case FormattingType.Number:
                    return IsValidNumber(text); // Validate numbers

                default:
                    return true; // Default: Allow input
            }
        }

        private static bool IsValidNumber(string text)
        {
            // Validate numbers with optional decimal points
            return Regex.IsMatch(text, @"^\d*\.?\d*$");
        }

    }
}