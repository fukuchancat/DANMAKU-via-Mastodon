using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TootNet;
using static DANMAKU_via_Mastodon.Properties.Settings;
using Application = System.Windows.Application;

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
            InitializeLists();
        }

        /// <summary>
        /// Initialize lists combobox items
        /// </summary>
        private async void InitializeLists()
        {
            if (string.IsNullOrEmpty(Default.Instance) || string.IsNullOrEmpty(Default.AccessToken) || string.IsNullOrEmpty(Default.ClientId) || string.IsNullOrEmpty(Default.ClientSecret))
            {
                return;
            }
            Tokens tokens = new Tokens(Default.Instance, Default.AccessToken, Default.ClientId, Default.ClientSecret);
            ListComboBox.ItemsSource = await tokens.Lists.GetAsync(); // get lists
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
                while (string.IsNullOrEmpty(Default.Instance) || string.IsNullOrEmpty(Default.ClientId) || string.IsNullOrEmpty(Default.ClientSecret));
                new CodeInputBox().ShowDialog();
            }
            while (string.IsNullOrEmpty(Default.Instance) || string.IsNullOrEmpty(Default.AccessToken) || string.IsNullOrEmpty(Default.ClientId) || string.IsNullOrEmpty(Default.ClientSecret));
            InitializeLists();
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
            // save settings
            Default.Save();

            // Re-open main window
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
            return string.IsNullOrWhiteSpace(value.ToString()) ? new ValidationResult(false, "required") : ValidationResult.ValidResult;
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
