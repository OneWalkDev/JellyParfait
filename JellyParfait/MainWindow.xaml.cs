using NAudio.Wave;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VideoLibrary;

namespace JellyParfait {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        public void exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            playMusic("https://www.youtube.com/watch?v=yCl53-3qIwY");
        }
        private async void playMusic(string youtubeUrl) {
            await Task.Run(() => {
                using (var player = new WaveOutEvent()) {
                    using (var reader = new MediaFoundationReader(getVideoUrl(youtubeUrl))) {
                        player.Init(reader);
                        player.Volume = 0.5f;
                        player.Play();
                    }
                    while (player.PlaybackState == PlaybackState.Playing) {
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        private string getVideoUrl(string youtubeUrl) {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(youtubeUrl);
            Debug.Print(video.Uri);
            return video.Uri;

           
        }
    }
}
