using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// タスクバーに表示するアイコン
        /// </summary>
        private NotifyIcon notifyIcon;

        private Semaphore semaphore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            notifyIcon = new NotifyIcon
            {
                Text = "DANMAKU via Mastodon",
                Icon = new Icon("app.ico"),
                Visible = true
            };
            semaphore = new Semaphore(1, 1, "DANMAKU via Mastodon", out bool createdNew);

            if (!createdNew)
            {
                // if the application is already running, show dialog and exit
                MessageBox.Show("Application is already running!", "DANMAKU via Mastodon");
                Shutdown();
            }

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
            semaphore.Dispose();
            base.OnExit(e);
        }
    }
}
