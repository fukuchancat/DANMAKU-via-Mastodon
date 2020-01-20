using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            notifyIcon = new NotifyIcon
            {
                Text = "DANMAKU via Mastodon",
                Icon = new Icon("app.ico"),
                Visible = true
            };

            // initializing context menu
            ContextMenuStrip menuStrip = new ContextMenuStrip();

            ToolStripMenuItem settingItem = new ToolStripMenuItem();
            ToolStripMenuItem exitItem = new ToolStripMenuItem();

            settingItem.Text = "Setting";
            exitItem.Text = "Exit";

            menuStrip.Items.Add(settingItem);
            menuStrip.Items.Add(exitItem);

            settingItem.Click += (sender, e1) => new SettingWindow().Show();
            exitItem.Click += (sender, e1) => Shutdown();

            notifyIcon.ContextMenuStrip = menuStrip;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
