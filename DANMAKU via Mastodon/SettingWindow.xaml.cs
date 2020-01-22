using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static DANMAKU_via_Mastodon.Properties.Settings;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        /// <summary>
        /// Constractor
        /// </summary>
        public SettingWindow()
        {
            Application.Current.MainWindow.Close(); // Close main window
            InitializeComponent();
        }

        /// <summary>
        /// Processing when ReAuth button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReAuthButton_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                do
                {
                    new InstanceInputBox().ShowDialog();
                }
                while (string.IsNullOrEmpty(Default.Instance));
                new CodeInputBox().ShowDialog();
            }
            while (string.IsNullOrEmpty(Default.Instance) || string.IsNullOrEmpty(Default.AccessToken) || string.IsNullOrEmpty(Default.ClientId) || string.IsNullOrEmpty(Default.ClientSecret));
        }

        /// <summary>
        /// Processing when OK button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Re-open main window when closed setting window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            Default.Save();
            MainWindow window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Show();
            base.OnClosed(e);
        }
    }

    /// <summary>
    /// Validation check for empty textbox
    /// </summary>
    public class EmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = Convert.ToString(value);

            if (string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, "required");
            }
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// Converter that inverts the bool
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }
    }

    /// <summary>
    /// Converter to convert enum to bool for Binding
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converter that converts String to FontFamily
    /// </summary>
    public class StringToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FontFamily((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
