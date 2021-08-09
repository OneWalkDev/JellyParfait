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
        /// スライダーにクリックのときのフラグ
        /// </summary>
        private bool sliderClick;

        /// <summary>
        /// キュー
        /// </summary>
        private List<MusicData> quere = new List <MusicData>();

        private int nowQuere = -1;


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
            prev();
        }

        private void prev() {
            if (nowQuere <= 0) return;
            if (quere.Count == 0) return;
            player.Dispose();
            nowQuere--;
            playMusic(quere[nowQuere].Url);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            next();
        }

        private void next() {
            if (nowQuere == -1) return;
            if (quere.Count <= nowQuere + 1) {
                nowQuere=0;
            } else {
                nowQuere++;
            }
            player.Dispose();
            
            playMusic(quere[nowQuere].Url);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }

        private void MusicTimeSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Debug.Print("MouseUp");
            if (isPlay()) {
                Debug.Print(MusicTimeSlider.Value.ToString());
                Debug.Print(Math.Floor(MusicTimeSlider.Value).ToString());
                player.Stop();
                /* playMusic(quere[nowQuere].Url,timespan); */
                media.Position = 0;
                media.Position = (long)(media.WaveFormat.AverageBytesPerSecond * Math.Floor(MusicTimeSlider.Value));
                media.CurrentTime = media.CurrentTime.Add(TimeSpan.FromSeconds(MusicTimeSlider.Value));
                media.CurrentTime = media.CurrentTime.Subtract(TimeSpan.FromSeconds(MusicTimeSlider.Value));
                //media.CurrentTime = TimeSpan.FromSeconds(Math.Floor(MusicTimeSlider.Value));
                Debug.Print(media.CurrentTime.ToString());
                player.Play();
            }
            sliderClick = false;
        }

        private void MusicTimeSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
           sliderClick = true;
           if (isPlay()) {
                Debug.Print("MouseDown");
                Debug.Print(MusicTimeSlider.Value.ToString());
                //player.Dispose();
            }
        }

        private async void addQuere(string youtubeUrl) {
            if (quere.Exists(x => x.YoutubeUrl == youtubeUrl)) {
                var msgbox = MessageBox.Show(this, "すでにその曲は存在しているようです。追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgbox == MessageBoxResult.No) return;
            }
            MusicData musicData = null;
            await Task.Run(() => musicData = getVideoObject(youtubeUrl).Result);
            if (musicData == null) return;
            if (musicData.Url == string.Empty) return;

            Debug.Print(musicData.Url);

            quere.Add(musicData);

            MusicQuere.ItemsSource = null;
            MusicQuere.ItemsSource = quere;

            if (quere.Count == 1) {
                nowQuere = 0;
                playMusic(musicData.Url);
            }
        }

        public async void playMusic(string googlevideo) {
            await Task.Run(() => {
            var time = new TimeSpan(0, 0, 0);
            if (media != null && isPlay()) {
                player.Stop();
            }
            player = new WaveOutEvent();
            media = new MediaFoundationReader(googlevideo);
            player.Init(media);
            player.Volume = 0.5f;
            Dispatcher.Invoke(() => {
                resetTime();
                setTimeSlider(media.TotalTime);
                changeTitle(quere[nowQuere].Title);
            });

            start();
            while (player.PlaybackState == PlaybackState.Playing) {
                Thread.Sleep(200);
                if (sliderClick) continue;
                if (time != media.CurrentTime) {
                    Dispatcher.Invoke(() => setNowTime(media.CurrentTime));
                    time = media.CurrentTime;
                    Debug.Print(media.CurrentTime.ToString());
                    }
                }
            });
            next();
        }

        private async Task<MusicData> getVideoObject(string youtubeUrl) {
            try {
                var youtubeClient = new YoutubeClient();
                var video = await youtubeClient.Videos.GetAsync(youtubeUrl);
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                var url = streamInfo.Url;
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
            if (player != null)  player.Play();     
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

        private bool isPlay() {
            if (player == null) return false;
            return player.PlaybackState == PlaybackState.Playing;
        }
    }
}
