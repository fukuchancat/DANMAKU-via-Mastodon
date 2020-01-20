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
        public InstanceInputBox()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Default.Instance = TextBox.Text;

            try
            {
                Authorize authorize = new Authorize();
                authorize.CreateApp(Default.Instance, "DANMAKU via Mastodon", Scope.Read).Wait();

                Default.ClientId = authorize.ClientId;
                Default.ClientSecret = authorize.ClientSecret;
                Default.Save();

                Process.Start(authorize.GetAuthorizeUri());
            }
            catch
            {
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
