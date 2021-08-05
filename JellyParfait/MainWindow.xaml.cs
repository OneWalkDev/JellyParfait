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

        /// <summary>
        /// 音楽の情報
        /// </summary>
        private MediaFoundationReader media;

        /// <summary>
        /// プレイヤー
        /// </summary>
        private WaveOutEvent player;

        /// <summary>
        /// 
        /// </summary>
        private bool play;



        public MainWindow() {
            InitializeComponent();
        }

        public void exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            playMusic(searchTextBox.Text);
        }

        private async void playMusic(string youtubeUrl) {
            await Task.Run(() => {
                var uri = getVideoUri(youtubeUrl);

                if (uri == "httpError") {
                    Dispatcher.Invoke(() => MessageBox.Show(this,"Error\nインターネットに接続されているか確認してください", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }
                if (uri == "URLFormatError") {
                    Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nURLの形式が間違っています。", "JellyParfait - Error", MessageBoxButton.OK,MessageBoxImage.Warning));
                    return;
                }
                if (uri == "noYoutubeURLError") {
                    Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nYoutubeのURLかどうかを確認してください", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }
                if (uri == "unknownError" || uri == string.Empty) {
                    Dispatcher.Invoke(() => MessageBox.Show(this, "Error\n不明なエラーが発生しました。\nURLが正しいか確認した後もう一度やり直してください", "JellyParfait", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }

                if(media != null && play) {
                    player.Stop();
                }
                player = new WaveOutEvent();
                media = new MediaFoundationReader(uri);
                player.Init(media);
                player.Volume = 0.5f;
                play = true;
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
