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
        /// Task bar icon
        /// </summary>
        private NotifyIcon notifyIcon;

        /// <summary>
        /// Semaphore to prevent multiple launch
        /// </summary>
        private Semaphore semaphore;

        /// <summary>
        /// Processing at application startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Initialize the context menu of notify icon
            ContextMenuStrip menuStrip = new ContextMenuStrip();

            // Initialize the context menu item
            ToolStripMenuItem settingItem = new ToolStripMenuItem
            {
                Text = "Setting"
            };
            ToolStripMenuItem exitItem = new ToolStripMenuItem
            {
                Text = "Exit"
            };
            menuStrip.Items.Add(settingItem);
            menuStrip.Items.Add(exitItem);

            // Set click event of context menu item
            settingItem.Click += (s, e1) => new SettingWindow().Show();
            exitItem.Click += (s, e1) => Shutdown();

            // Initialize notify icon
            notifyIcon = new NotifyIcon
            {
                Text = "DANMAKU via Mastodon",
                Icon = new Icon("app.ico"),
                Visible = true,
                ContextMenuStrip = menuStrip
            };
            // Initialize semaphore
            semaphore = new Semaphore(1, 1, "DANMAKU via Mastodon", out bool createdNew);

            // If the application is already running, show dialog and exit
            if (!createdNew)
            {
                MessageBox.Show("Application is already running. Please check taskbar.", "DANMAKU via Mastodon");
                Shutdown();
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Processing at application exit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); // Dispose notify icon
            semaphore.Dispose(); // Dispose semaphore just in case
            base.OnExit(e);
        }
    }
}
