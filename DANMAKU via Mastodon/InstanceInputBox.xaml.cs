using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TootNet;
using static DANMAKU_via_Mastodon.Properties.Settings;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// InstanceInputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class InstanceInputBox : Window
    {
        /// <summary>
        /// Constractor
        /// </summary>
        public InstanceInputBox()
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
            // Set instance to textbox input
            Default.Instance = TextBox.Text;

            try
            {
                // Issue ClientID and ClientSecrert of the instance
                Authorize authorize = new Authorize();
                authorize.CreateApp(Default.Instance, "DANMAKU via Mastodon", Scope.Read).Wait();

                // Save ClientID and ClientSecrert
                Default.ClientId = authorize.ClientId;
                Default.ClientSecret = authorize.ClientSecret;
                Default.Save();

                // Open authentication url
                Process.Start(authorize.GetAuthorizeUri());
            }
            catch
            {
            }

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
            // Set DialogResult false and close window
            DialogResult = false;
            Close();
        }
    }
}
