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

        MediaFoundationReader media;

        WaveOutEvent player;

        public MainWindow() {
            InitializeComponent();
        }

        public void exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            playMusic("https://www.youtube.com/watch");
        }

        private async void playMusic(string youtubeUrl) {
            await Task.Run(() => {
                var uri = getVideoUri(youtubeUrl);

                if (uri == "httpError") {
                    MessageBox.Show("Error\nインターネットに接続されているか確認してください", "JellyParfait - Error", MessageBoxButton.OK);
                    return;
                }
                if (uri == "URLFormatError") {
                    MessageBox.Show("Error\nURLの形式がおかしいです。", "JellyParfait - Error", MessageBoxButton.OK);
                    return;
                }
                if (uri == "noYoutubeError") {
                    MessageBox.Show("Error\nYoutubeのURLかどうかを確認してください", "JellyParfait - Error", MessageBoxButton.OK);
                    return;
                }
                if (uri == "unknownError" || uri == string.Empty) {
                    MessageBox.Show("Error\n不明なエラーが発生しました。\nURLが正しいか確認した後もう一度やり直してください", "JellyParfait", MessageBoxButton.OK);
                    return;
                }

                player = new WaveOutEvent();
                media = new MediaFoundationReader(uri);
                player.Init(media);
                player.Volume = 0.5f;
                start();
                while (player.PlaybackState == PlaybackState.Playing) {
                    Thread.Sleep(1000);
                }
            });
        }

        private string getVideoUri(string youtubeUrl) {
            var youTube = YouTube.Default;
            try {
                var video = youTube.GetVideo(youtubeUrl);
                Debug.Print(video.Uri);
                Debug.Print(video.Info.ToString());
                Dispatcher.Invoke(() => titleLabel.Content = "Now Playing : " + video.Title);
                return video.Uri;
            } catch (System.Net.Http.HttpRequestException){
                return "httpError";
            } catch (VideoLibrary.Exceptions.UnavailableStreamException) {
                return "URLFormatError";
            } catch (System.ArgumentException) {
                return "noYoutubeURLError";
            } catch {
                return "unknownError";
            }
        }

        private void start() {
            if (player != null) player.Play();
        }

        private void stop() {
            if (player != null) player.Stop();
        }

        private void pause() {
            if (player != null) player.Pause();
        }

        private void setTimeLabel() {
            startLabel.Content = "0:00";
            endLabel.Content = "0:00";
        }

        private void moveSlider() {

        }
    }
}
