using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace YearProgress
{
    public partial class App : System.Windows.Application
    {
        NotifyIcon? _tray;
        MainWindow? _window;
        bool _exitRequested;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _window = new MainWindow();
            MainWindow = _window;

            _window.Closing += (_, args) =>
            {
                if (_exitRequested) return;
                args.Cancel = true;
                _window.Hide();
            };

            _window.Show();

            _tray = new NotifyIcon
            {
                Text = "Year Progress",
                Icon = SystemIcons.Application,
                Visible = true
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Show / Hide", null, (_, _) => ToggleWindow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, _) => ExitFromTray());
            _tray.ContextMenuStrip = menu;

            _tray.DoubleClick += (_, _) => ToggleWindow();
        }

        void ToggleWindow()
        {
            if (_window == null) return;

            if (_window.IsVisible)
            {
                _window.Hide();
            }
            else
            {
                _window.Show();
                _window.Activate();
            }
        }

        void ExitFromTray()
        {
            _exitRequested = true;

            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }

            _window?.Close();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }

            base.OnExit(e);
        }
    }
}
