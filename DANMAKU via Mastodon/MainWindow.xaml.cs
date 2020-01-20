using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        public int[] Spaces;
        public double LineHeight;
        public IDisposable Disposable;

        public MainWindow()
        {
            InitializeComponent();

            PropertyInfo propertyInfo = typeof(FontStyles).GetProperty(Default.FontStyle, BindingFlags.Static | BindingFlags.Public);
            FontFamily = new FontFamily(Default.FontFamily);
            FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(Default.FontWeight);
            FontSize = Default.FontSize;
            FontStyle = (FontStyle)propertyInfo.GetValue(null, null);

            LineHeight = FontFamily.LineSpacing * FontSize;
            Spaces = new int[(int)(SystemParameters.PrimaryScreenHeight / LineHeight)];
            Disposable = CreateDisposable();
        }

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
        /// connect to twitter streaming
        /// </summary>
        public IDisposable CreateDisposable()
        {
            Tokens tokens = CreateTokens();

            // if the query is empty, connect to timeline
            if ((Default.StreamingType == StreamingType.Hashtag && string.IsNullOrWhiteSpace(Default.Tag)) || (Default.StreamingType == StreamingType.List && string.IsNullOrWhiteSpace(Default.List)))
            {
                Default.StreamingType = 0;
            }

            IObservable<StreamingMessage> observable;
            switch (Default.StreamingType)
            {
                case StreamingType.User:
                    observable = tokens.Streaming.UserAsObservable();
                    break;
                case StreamingType.Hashtag:
                    observable = tokens.Streaming.HashtagAsObservable(tag => Default.Tag);
                    break;
                case StreamingType.Public:
                    observable = tokens.Streaming.PublicAsObservable();
                    break;
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
                .Subscribe(x => Add(x.Status.Content));
        }

        /// <summary>
        /// add received tweet to the screen
        /// </summary>
        /// <param name="str">tweet text</param>
        private void Add(string str)
        {
            int line = str.Count(c => c == '\n') + 1;
            int pos = CalcPos(line);

            FillSpaces(line, pos);
            FireLabel(str, pos);
        }

        /// <summary>
        /// calclate the position to draw a tweet
        /// </summary>
        /// <param name="line">number of lines of tweet</param>
        /// <returns>position to draw tweet</returns>
        private int CalcPos(int line)
        {
            int pos = 0;

            if (Spaces.Length > line)
            {
                int i = 0;
                bool flag = true;

                while (flag)
                {
                    for (int j = 0; j < Spaces.Length - line; j++)
                    {
                        int sum = 0;
                        for (int k = 0; k < line && k < Spaces.Length; k++)
                        {
                            sum += Spaces[j + k];
                        }

                        if (sum <= i)
                        {
                            pos = j;

                            flag = false;
                            break;
                        }
                    }
                    i++;
                }
            }

            return pos;
        }

        /// <summary>
        /// occupy spaces to draw a tweet
        /// </summary>
        /// <param name="line">number of lines of the tweet</param>
        /// <param name="pos">position to draw the tweet</param>
        private void FillSpaces(int line, int pos)
        {
            for (int k = 0; k < line && k < Spaces.Length; k++)
            {
                Spaces[pos + k]++;
            }

            Task.Delay(5000).ContinueWith(t => ClearSpaces(line, pos));
        }

        /// <summary>
        /// clear the spaces
        /// </summary>
        /// <param name="line">number of lines of the tweet</param>
        /// <param name="pos">position to draw the tweet</param>
        private void ClearSpaces(int line, int pos)
        {
            for (int k = 0; k < line && k < Spaces.Length; k++)
            {
                if (Spaces[pos + k] > 0)
                {
                    Spaces[pos + k]--;
                }
            }
        }

        /// <summary>
        /// draw tweet
        /// </summary>
        /// <param name="str">tweet text</param>
        /// <param name="pos">position to draw the tweet</param>
        private void FireLabel(string str, int pos)
        {
            Root.Dispatcher.Invoke(() =>
            {
                // create label
                Label label = new Label
                {
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(Default.Color),
                    Content = str
                };
                Canvas.SetBottom(label, pos * LineHeight);
                Root.Children.Add(label);

                // set animation
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    From = SystemParameters.PrimaryScreenWidth,
                    To = str.Length * -100,
                    Duration = new Duration(TimeSpan.FromSeconds(10))
                };
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
                Storyboard.SetTarget(doubleAnimation, label);
                Storyboard storyboard = new Storyboard
                {
                    FillBehavior = FillBehavior.HoldEnd
                };
                storyboard.Children.Add(doubleAnimation);
                storyboard.Completed += (s, e) => { Root.Children.Remove(label); };
                storyboard.Begin();
            });
        }

        /// <summary>
        /// make the window click-through
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

        protected override void OnClosed(EventArgs e)
        {
            if (Disposable != null)
            {
                Disposable.Dispose();
            }
            base.OnClosed(e);
        }

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