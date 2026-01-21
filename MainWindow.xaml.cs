using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;
using System.ComponentModel;


namespace YearProgress
{
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer _minuteTimer = new();
        static readonly string PlacementPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YearProgress", "window.json");

        private bool _isCompactMode = false;
        public bool IsCompactMode => _isCompactMode;

        private const double FullBarWidth = 220;
        private const double CompactBarWidth = FullBarWidth * 0.25; // 55

        public MainWindow()
        {
            InitializeComponent();
            UpdateUI();
            StartMinuteUpdatesAligned();


        }

        sealed class WindowPlacement
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }

        public void SetCompactMode(bool compact)
        {
            _isCompactMode = compact;

            TitleText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            YearProgressBarControl.Width = compact ? CompactBarWidth : FullBarWidth;
            MainBorder.Padding = compact ? new Thickness(2, 1, 6, 1) : new Thickness(6, 1, 6, 1);

            UpdateUI();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var placement = LoadPlacement();
            if (placement != null)
            {
                Left = placement.Left;
                Top = placement.Top;
                Width = placement.Width;
                Height = placement.Height;
                EnsureOnScreen();
            }
        }

        void Window_Closing(object? sender, CancelEventArgs e)
        {
            // If minimized/maximized, RestoreBounds holds the “normal” size/pos.
            var bounds = (WindowState == WindowState.Normal) ? new Rect(Left, Top, Width, Height) : RestoreBounds;

            SavePlacement(new WindowPlacement
            {
                Left = bounds.Left,
                Top = bounds.Top,
                Width = bounds.Width,
                Height = bounds.Height
            });
        }

        static WindowPlacement? LoadPlacement()
        {
            try
            {
                if (!File.Exists(PlacementPath)) return null;
                string json = File.ReadAllText(PlacementPath);
                return JsonSerializer.Deserialize<WindowPlacement>(json);
            }
            catch
            {
                return null;
            }
        }

        static void SavePlacement(WindowPlacement placement)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PlacementPath)!);
                string json = JsonSerializer.Serialize(placement);
                File.WriteAllText(PlacementPath, json);
            }
            catch
            {
                // Ignore save failures (app still closes normally).
            }
        }

        void EnsureOnScreen()
        {
            // Keep it visible somewhere on the virtual desktop area
            double vLeft = SystemParameters.VirtualScreenLeft;
            double vTop = SystemParameters.VirtualScreenTop;
            double vRight = vLeft + SystemParameters.VirtualScreenWidth;
            double vBottom = vTop + SystemParameters.VirtualScreenHeight;

            const double margin = 10;

            if (Left + Width < vLeft + margin) Left = vLeft + margin;
            if (Top + Height < vTop + margin) Top = vTop + margin;
            if (Left > vRight - margin) Left = vRight - Width - margin;
            if (Top > vBottom - margin) Top = vBottom - Height - margin;
        }

        void StartMinuteUpdatesAligned()
        {
            var now = DateTime.Now;
            var nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            var initialDelay = nextMinute - now;

            var oneShot = new DispatcherTimer { Interval = initialDelay };
            oneShot.Tick += (_, _) =>
            {
                oneShot.Stop();

                UpdateUI();

                _minuteTimer.Interval = TimeSpan.FromMinutes(1);
                _minuteTimer.Tick += (_, _) => UpdateUI();
                _minuteTimer.Start();
            };
            oneShot.Start();
        }

        void UpdateUI()
        {
            double percent = YearProgressCalculator.GetProgressPercent(DateTime.Now);
            YearProgressBarControl.Value = percent;
            double shown = Math.Floor(percent * 100) / 100;

            PercentText.Text = _isCompactMode ? $"{shown:0.00}%" : $"{shown:0.000}%";
        }

        void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}

