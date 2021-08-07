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

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e) {

        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {

        }

        private async void addQuere(string youtubeUrl) {
            MusicData musicData = null;
            await Task.Run(() => musicData = getVideoObject(youtubeUrl).Result);

            if (musicData == null) return;
            if (musicData.Url == string.Empty) return;
            if (quere.Exists(x=>x.YoutubeUrl==musicData.YoutubeUrl)) {
                var msgbox = MessageBox.Show(this, "すでにその曲は存在しているようです。追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgbox == MessageBoxResult.No) return;
            }

            Debug.Print(musicData.Url);

            quere.Add(musicData);

            MusicQuere.ItemsSource = null;
            MusicQuere.ItemsSource = quere;
           
            playMusic(musicData.Url);
        }

        public async void playMusic(string googlevideo) {
            await Task.Run(() => {
                var time = new TimeSpan(0, 0, 0);
                if (media != null && play) {
                    player.Stop();
                }
                player = new WaveOutEvent();
                media = new MediaFoundationReader(googlevideo);
                player.Init(media);
                player.Volume = 0.5f;
                Dispatcher.Invoke(() => {
                    resetTime();
                    setTimeSlider(media.TotalTime);
                    changeTitle(quere[quere.Count - 1].Title);
                });
                play = true;
                start();
                while (player.PlaybackState == PlaybackState.Playing) {
                    Thread.Sleep(200);
                    if (time != media.CurrentTime) {
                        Dispatcher.Invoke(() => setNowTime(media.CurrentTime));
                        time = media.CurrentTime;
                    }
                }
                play = false;
            });
        }

        public async void playMusic(string googlevideo,TimeSpan timeSpan) {
            await Task.Run(() => {
                var time = new TimeSpan(0, 0, 0);
                if (media != null && play) {
                    player.Stop();
                }
                player = new WaveOutEvent();
                media = new MediaFoundationReader(googlevideo);
                player.Init(media);
                player.Volume = 0.5f;
                media.CurrentTime = timeSpan;
                Dispatcher.Invoke(() => resetTime());
                Dispatcher.Invoke(() => setTimeSlider(media.TotalTime));
                play = true;
                start();
                while (player.PlaybackState == PlaybackState.Playing) {
                    Thread.Sleep(200);
                    if (time != media.CurrentTime) {
                        Dispatcher.Invoke(() => setNowTime(media.CurrentTime));
                        time = media.CurrentTime;
                    }
                }
                play = false;
            });
        }

        private async Task<MusicData> getVideoObject(string youtubeUrl) {
            try {
                var youtubeClient = new YoutubeClient();
                var video = await youtubeClient.Videos.GetAsync(youtubeUrl);
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                var url = streamInfo.Url;
                Debug.Print(url);
                var data = new MusicData(this);
                data.Title = video.Title;
                data.Url = streamInfo.Url;
                data.YoutubeUrl = youtubeUrl;
                return data;
            } catch (System.Net.Http.HttpRequestException) {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nインターネットに接続されているか確認してください", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                return null;
            } catch (ArgumentException) {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nURLの形式が間違っています。", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                return null;
            } catch (AggregateException) {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\nYoutubeのURLかどうかを確認してください", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                return null;
            } catch {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\n不明なエラーが発生しました。\nURLが正しいか確認した後もう一度やり直してください", "JellyParfait", MessageBoxButton.OK, MessageBoxImage.Warning));
                return null;
            }
        }

        private void start() {
            if (player != null) {
                player.Play();
            }
           
        }

        private void stop() {
            if (player != null) player.Stop();
        }

        private void pause() {
            if (player != null) {
                player.Pause();
                play = false;
            }
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
