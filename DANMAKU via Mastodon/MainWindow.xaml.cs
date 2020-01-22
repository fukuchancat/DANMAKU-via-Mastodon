using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TootNet;
using TootNet.Streaming;
using static DANMAKU_via_Mastodon.Properties.Settings;

namespace DANMAKU_via_Mastodon
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Array for calculating Label display position
        /// </summary>
        public int[] Spaces;

        /// <summary>
        /// Line height
        /// </summary>
        public double LineHeight;

        /// <summary>
        /// Disposable for Streaming reception
        /// </summary>
        public IDisposable Disposable;

        /// <summary>
        /// Constractor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Set font
            PropertyInfo propertyInfo = typeof(FontStyles).GetProperty(Default.FontStyle, BindingFlags.Static | BindingFlags.Public);
            FontFamily = new FontFamily(Default.FontFamily);
            FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(Default.FontWeight);
            FontSize = Default.FontSize;
            FontStyle = (FontStyle)propertyInfo.GetValue(null, null);

            // Initialize variables
            LineHeight = FontFamily.LineSpacing * FontSize;
            Spaces = new int[(int)(SystemParameters.PrimaryScreenHeight / LineHeight)];
            Disposable = CreateDisposable();
        }

        /// <summary>
        /// Open input windows if necessary
        /// </summary>
        /// <returns>Token</returns>
        private Tokens CreateTokens()
        {
            while (string.IsNullOrEmpty(Default.Instance) || string.IsNullOrEmpty(Default.AccessToken) || string.IsNullOrEmpty(Default.ClientId) || string.IsNullOrEmpty(Default.ClientSecret))
            {
                while (string.IsNullOrEmpty(Default.Instance))
                {
                    new InstanceInputBox().ShowDialog();
                }
                new CodeInputBox().ShowDialog();
            }
            return new Tokens(Default.Instance, Default.AccessToken, Default.ClientId, Default.ClientSecret);
        }

        /// <summary>
        /// Create disposable for Streaming reception
        /// </summary>
        /// <returns>Disposable</returns>
        public IDisposable CreateDisposable()
        {
            // Issue token
            Tokens tokens = CreateTokens();

            // if the query is empty, connect to user timeline
            if ((Default.StreamingType == StreamingType.Hashtag && string.IsNullOrWhiteSpace(Default.Tag)) || (Default.StreamingType == StreamingType.List && string.IsNullOrWhiteSpace(Default.List)))
            {
                Default.StreamingType = StreamingType.User;
            }

            // Create observable according to the settings
            IObservable<StreamingMessage> observable;
            switch (Default.StreamingType)
            {
                // user timeline
                case StreamingType.User:
                    observable = tokens.Streaming.UserAsObservable();
                    break;
                // hashtag
                case StreamingType.Hashtag:
                    observable = tokens.Streaming.HashtagAsObservable(tag => Default.Tag);
                    break;
                // all public toots
                case StreamingType.Public:
                    observable = tokens.Streaming.PublicAsObservable(local => Default.Local);
                    break;
                // list
                case StreamingType.List:
                    observable = tokens.Streaming.ListAsObservable(list => Default.List);
                    break;
                default:
                    observable = tokens.Streaming.UserAsObservable();
                    break;
            }

            // start streaming
            return observable
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Where(x => x.Type == StreamingMessage.MessageType.Status)
                .Select(x => ToPlainText(x.Status.Content))
                .Subscribe(t => FireLabel(t));
        }

        /// <summary>
        /// Convert html string to plain text
        /// </summary>
        /// <param name="html">html string</param>
        /// <returns>Plain text</returns>
        private string ToPlainText(string html)
        {
            html = html.Replace("<br />", "\n");
            html = html.Replace("</p><p>", "\n\n");
            html = Regex.Replace(html, "<.+?>", "");
            html = HttpUtility.HtmlDecode(html);
            return html.TrimEnd();
        }

        /// <summary>
        /// Secure label display position
        /// </summary>
        /// <param name="str">text</param>
        /// <returns>secured position index</returns>
        private int ReservePosition(string str)
        {
            // Number of lines of text
            int span = str.Count(c => c == '\n') + 1;

            // Label display position
            int pos = span < Spaces.Length ? Enumerable.Range(0, Spaces.Length - span)
                    .OrderBy(i => Enumerable.Range(i, span).Select(j => Spaces[j]).Sum())
                    .First() : 0;

            // Secure position
            for (int i = pos; i < pos + span && i < Spaces.Length; i++)
            {
                Spaces[i]++;
            }

            // Release position after 5 seconds
            Task.Delay(5000).ContinueWith(t =>
            {
                for (int i = pos; i < pos + span && i < Spaces.Length; i++)
                {
                    Spaces[i]--;
                }
            });

            return pos;
        }

        /// <summary>
        /// Fire label to screen
        /// </summary>
        /// <param name="str">text</param>
        private void FireLabel(string str)
        {
            // label position index
            int pos = ReservePosition(str);

            // Processed in dispatcher for UI control
            Root.Dispatcher.Invoke(() =>
            {
                // create label
                Label label = new Label
                {
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(Default.Color),
                    Content = str
                };

                // set label positon and add to canvas
                Canvas.SetBottom(label, pos * LineHeight);
                Root.Children.Add(label);

                // initialize animation
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    From = ActualWidth,
                    Duration = new Duration(TimeSpan.FromSeconds(10))
                };

                // initialize storyboard
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
                Storyboard.SetTarget(doubleAnimation, label);
                Storyboard storyboard = new Storyboard
                {
                    FillBehavior = FillBehavior.HoldEnd
                };
                storyboard.Children.Add(doubleAnimation);

                // Remove label after animation ends
                storyboard.Completed += (s, e) => Root.Children.Remove(label);

                // Processed after loded to calculate label width
                label.Loaded += (s, e) =>
                {
                    doubleAnimation.To = -label.ActualWidth;
                    storyboard.Begin(); // start animation
                };
            });
        }

        /// <summary>
        /// Make the window click-through
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            // get this window's handle
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // change the extended window style to include WS_EX_TRANSPARENT
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_TRANSPARENT);

            base.OnSourceInitialized(e);
        }

        /// <summary>
        /// End stream on window closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            if (Disposable != null)
            {
                Disposable.Dispose();
            }
            base.OnClosed(e);
        }

        /// <summary>
        /// Native Methods required to make windows click-through
        /// </summary>
        private static class NativeMethods
        {
            public const int WS_EX_TRANSPARENT = 0x00000020;
            public const int GWL_EXSTYLE = (-20);

            [DllImport("user32.dll")]
            public static extern int GetWindowLong(IntPtr hwnd, int index);
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        }
    }
}