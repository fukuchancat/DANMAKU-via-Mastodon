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
        public SettingWindow()
        {
            Application.Current.MainWindow.Close();
            InitializeComponent();
        }

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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// restart streaming when closing the window
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
    /// validation check for empty textbox
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
