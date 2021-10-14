using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;


namespace JellyParfait {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        private MainWindow mainWindow;

        private static System.Windows.Forms.NotifyIcon notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e) {
            
            Mutex mutex_ = new Mutex(false, "JellyParfait");
            if (!mutex_.WaitOne(0, false)) {
                MessageBox.Show("JellyParfaitはすでに起動しています。\n見当たらない場合はタスクトレイに格納されています。", "JellyParfait", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Shutdown();
                mutex_.Dispose();
                DeleteNotifyIcon();
                return;
            }
            mainWindow = new MainWindow();
            mainWindow.Show();
        }

        /// <summary>
        /// System.Windows.Application.Startup イベント を発生させます。
        /// </summary>
        /// <param name="e">イベントデータ を格納している StartupEventArgs</param>
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var icon = GetResourceStream(new Uri("./Resources/JellyParfait_alpha 64x64.ico", UriKind.Relative)).Stream;
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("ウインドウを表示", null, Show_Click);
            menu.Items.Add("終了", null, Exit_Click);
            notifyIcon = new System.Windows.Forms.NotifyIcon {
                Visible = true,
                Icon = new System.Drawing.Icon(icon),
                Text = "JellyParfait",
                ContextMenuStrip = menu
            };
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_Click);
        }

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && !mainWindow.IsVisible) {
                mainWindow.Show();
            }
        }

        private void Show_Click(object sender, EventArgs e) {
            if (!mainWindow.IsVisible) mainWindow.Show();          
        }


        private void Exit_Click(object sender, EventArgs e) {
            if (!mainWindow.IsVisible) {
                mainWindow.Show();
            }
            DeleteNotifyIcon();
            mainWindow.ClickExitButtonFromApp();
        }

        public static void DeleteNotifyIcon() {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            Mutex mutex_;
            if (Mutex.TryOpenExisting("JellyParfait", out mutex_)) {
                mutex_.ReleaseMutex();
                mutex_.Dispose();
            }
        }
    }
}
