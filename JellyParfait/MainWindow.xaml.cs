using JellyParfait.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

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
        /// 音楽のプレイ状況
        /// </summary>
        private bool play;

        private List<MusicData> quere = new List <MusicData>();


        public MainWindow() {
            InitializeComponent();
        }

        public void exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            addQuere(searchTextBox.Text);
        }

        private async void addQuere(string youtubeUrl) {
            string uri = "";
            await Task.Run(() => {
                uri = getVideoUri(youtubeUrl);
            });

            if (uri == "httpError") {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nインターネットに接続されているか確認してください", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                return;
            }
            if (uri == "URLFormatError") {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nURLの形式が間違っています。", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
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

            playMusic(uri);
        }

        public async void playMusic(string googlevideo) {
            await Task.Run(() => {
                if (media != null && play) {
                    player.Stop();
                }
                player = new WaveOutEvent();
                media = new MediaFoundationReader(googlevideo);
                player.Init(media);
                player.Volume = 0.5f;
                Dispatcher.Invoke(() => resetTime());
                Dispatcher.Invoke(() => setTimeSlider(media.TotalTime));
                play = true;
                start();
                while (player.PlaybackState == PlaybackState.Playing) {
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => setNowTime(media.CurrentTime));
                }
                play = false;
            });
        }

        private string getVideoUri(string youtubeUrl) {

            try {
                quere.Add(getYoutubeData(youtubeUrl).Result);
                Dispatcher.Invoke(() => {
                    changeTitle(quere[quere.Count-1].Title);
                    MusicQuere.ItemsSource = null;
                    MusicQuere.ItemsSource = quere;
                });
                return quere[quere.Count - 1].Uri;
            } catch (System.Net.Http.HttpRequestException) {
                return "httpError";
            } catch (ArgumentException) {
                return "noYoutubeURLError";
            } catch (AggregateException) {
                return "URLFormatError";
            } catch {
                return "unknownError";
            }

        }

        private async Task<MusicData> getYoutubeData(string youtubeUrl) {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(youtubeUrl);
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var url = streamInfo.Url;
            Debug.Print(url);
            var data = new MusicData(this);
            data.Title = video.Title;
            data.Uri = streamInfo.Url;
            return data;
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

        private void resetTime() {
            startLabel.Content = "0:00";
            endLabel.Content = "0:00";
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Minimum = MusicTimeSlider.Maximum = 0;

        }

        private void setNowTime(TimeSpan time) {
            var seconds = time.Seconds.ToString();
            if (time.Seconds < 10) seconds = "0" + seconds;
            var totalSec = time.Minutes * 60 + time.Seconds;
            startLabel.Content = time.Minutes.ToString() + ":" + seconds;
            MusicTimeSlider.Value = totalSec;
        }

        public void changeTitle(string musicTitle) {
            titleLabel.Content = "Now Playing : " + musicTitle;
        }

        private void setTimeSlider(TimeSpan totalTime) {
            endLabel.Content = totalTime.Minutes + ":" + totalTime.Seconds;
            var totalSec = totalTime.Minutes * 60 + totalTime.Seconds;
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Maximum = totalSec;
        }

    }
}
