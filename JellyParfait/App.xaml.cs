using System;
using System.Windows;


namespace JellyParfait {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        private MainWindow mainWindow;

        private static System.Windows.Forms.NotifyIcon notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e) {
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
    }
}
