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
        public CodeInputBox()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Authorize authorize = new Authorize()
            {
                Instance = Default.Instance,
                ClientId = Default.ClientId,
                ClientSecret = Default.ClientSecret
            };
            Tokens tokens = authorize.AuthorizeWithCode(TextBox.Text).Result;

            Default.AccessToken = tokens.AccessToken;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Default.Instance = null;
            Default.Save();

            DialogResult = false;
            Close();
        }
    }
}
