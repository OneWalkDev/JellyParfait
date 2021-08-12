using JellyParfait.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        private bool Clicked;

        public MainWindow() {
            InitializeComponent(); 
        }


        public void Exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            AddQuere(searchTextBox.Text);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (Clicked) return;
            if (player == null) return;
            if (IsPlay()) {
                Pause();
            } else {
                Play();
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e) {
            Prev();
        }

        private async void Prev() {
            if (Clicked) return;
            if (nowQuere <= 0) return;
            if (quere.Count == 0) return;
            Clicked = true;
            player.Dispose();
            nowQuere--;
            PlayMusic(quere[nowQuere]);
            await Task.Run(() => Thread.Sleep(1500));
            Clicked = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            Next(); 
        }

        private async void Next() {
            if (Clicked) return;
            if (nowQuere == -1) return;
            Clicked = true;
            if (quere.Count <= nowQuere + 1) {
                nowQuere=0;
            } else {
                nowQuere++;
            }
            Debug.Print(nowQuere.ToString());
            PlayMusic(quere[nowQuere]);
            await Task.Run(() => Thread.Sleep(1500));
            Clicked = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }

        private void MusicTimeSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Debug.Print("MouseUp");
            if (IsPlay()) {
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
           if (IsPlay()) {
                Debug.Print("MouseDown");
                Debug.Print(MusicTimeSlider.Value.ToString());
                //player.Dispose();
            }
        }

        private async void AddQuere(string youtubeUrl) {
            if (quere.Exists(x => x.YoutubeUrl == youtubeUrl)) {
                var msgbox = MessageBox.Show(this, "すでにその曲は存在しているようです。追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgbox == MessageBoxResult.No) return;
            }
            MusicData musicData = null;
            await Task.Run(() => musicData = GetVideoObject(youtubeUrl).Result);
            if (musicData == null) return;
            if (musicData.Url == string.Empty) return;

            Debug.Print(musicData.Url);

            quere.Add(musicData);

            MusicQuere.ItemsSource = null;
            MusicQuere.ItemsSource = quere;

            if (quere.Count == 1) {
                nowQuere = 0;
                PlayMusic(musicData);
            }
        }

        public async void PlayMusic(MusicData data) {
            await Task.Run(() => {
                if (player != null) {
                    player.Dispose();
                    media.Dispose();
                }
            });
            PlayButton.Content = Resources["Pause"];
            await Task.Run(() => {
                player = new WaveOutEvent();
                media = new MediaFoundationReader(data.Url);
                media.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                player.Init(media);
                player.Volume = 0.5f;
                Dispatcher.Invoke(() => {
                    ResetTime();
                    SetTimeSlider(media.TotalTime);
                    ChangeTitle(quere[nowQuere].Title);
                });
                var time = new TimeSpan(0, 0, 0);
                AsyncPlay();
                while (true) {

                    Thread.Sleep(200);
                    
                    if (player == null) break;
                    if (player.PlaybackState != PlaybackState.Playing) break;
                    if (sliderClick) continue;
                    if (time != media.CurrentTime) {
                        Dispatcher.Invoke(() => SetNowTime(media.CurrentTime));
                        time = media.CurrentTime;
                        Debug.Print(media.CurrentTime.ToString());
                    }
                }
            });
            
            if(player.PlaybackState != PlaybackState.Paused) Next();
        }

        private async Task<MusicData> GetVideoObject(string youtubeUrl) {
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
                data.QuereId = quere.Count;
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

        private void AsyncPlay() {
            if (player != null) {
                player.Play();
            }
        }

        private void Play() {
            if (player != null) {
                player.Play();
                PlayButton.Content = Resources["Pause"];
            }
        }

        private void Stop() {
            if (player != null) player.Stop();
        }

        private void Pause() {
            if (player != null) {
                player.Pause();
                PlayButton.Content = Resources["Play"];
            }
        }

        private void ResetTime() {
            startLabel.Content = "0:00";
            endLabel.Content = "0:00";
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Minimum = MusicTimeSlider.Maximum = 0;

        }

        private void SetNowTime(TimeSpan time) {
            var seconds = time.Seconds.ToString();
            if (time.Seconds < 10) seconds = "0" + seconds;
            var totalSec = time.Minutes * 60 + time.Seconds;
            startLabel.Content = time.Minutes.ToString() + ":" + seconds;
            MusicTimeSlider.Value = totalSec;
        }

        public void ChangeTitle(string musicTitle) {
            titleLabel.Content = "Now Playing : " + musicTitle;
        }
         
        private void SetTimeSlider(TimeSpan totalTime) {
            endLabel.Content = totalTime.Minutes + ":" + totalTime.Seconds;
            var totalSec = totalTime.Minutes * 60 + totalTime.Seconds;
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Maximum = totalSec;
        }

        private bool IsPlay() {
            if (player == null) return false;
            return player.PlaybackState == PlaybackState.Playing;
        }

        public void SetQuere(int num) {
            nowQuere = num;
        }

    }
}
