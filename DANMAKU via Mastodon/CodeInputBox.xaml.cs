using System.Windows;
using System.Windows.Controls;
using TootNet;
using static DANMAKU_via_Mastodon.Properties.Settings;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// InputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class CodeInputBox : Window
    {
        /// <summary>
        /// Constractor
        /// </summary>
        public CodeInputBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Processing when OK button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize Authorize instance
            Authorize authorize = new Authorize
            {
                Instance = Default.Instance,
                ClientId = Default.ClientId,
                ClientSecret = Default.ClientSecret
            };

            // Create token with code authentication
            Tokens tokens = authorize.AuthorizeWithCode(TextBox.Text).Result;

            // Save access token
            Default.AccessToken = tokens.AccessToken;

            // Set DialogResult true and close window
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Processing when Cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Set instance null and save
            Default.Instance = null;
            Default.Save();

            // Set DialogResult false and close window
            DialogResult = false;
            Close();
        }
    }
}
