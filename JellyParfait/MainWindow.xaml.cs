using JellyParfait.Data;
using MahApps.Metro.Controls;
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
    public partial class MainWindow : MetroWindow {

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
        private List<MusicData> quere = new List<MusicData>();

        /// <summary>
        /// 現在再生されているキュー
        /// </summary>
        private int nowQuere = -1;

        /// <summary>
        /// 連打してはいけないボタンのフラグ
        /// </summary>
        private bool Clicked;

        /// <summary>
        /// 正常に再生できているか確認するフラグ
        /// </summary>
        private bool Complete;

        /// <summary>
        /// 検索中か確認するフラグ
        /// </summary>
        private string Searched = String.Empty;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_Closed(object sender, EventArgs e) {
            if (IsPlay()) {
                Stop();
                player.Dispose();
            }
        }

        public void Exit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        public void Version_Infomation_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("JellyParfait version 0.9β\n\nCopylight(C)2021 yurisi\nAll rights reserved.\n\n本ソフトウェアはオープンソースソフトウェアです。\nGPL-3.0 Licenseに基づき誰でも複製や改変ができます。\n\nGithub\nhttps://github.com/yurisi0212/JellyParfait", "JellyParfait", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Enter) Search();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Search();
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

        private void PrevButton_Click(object sender, RoutedEventArgs e) {
            Prev();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (Clicked) return;
            if (player == null) return;
            (IsPlay() ? (Action)Pause : Play)();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            Next();
        }

        private void Search() {
            if (Searched == searchTextBox.Text) {
                var msgbox = MessageBox.Show(this, "現在検索しているようです。もう一度追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgbox == MessageBoxResult.No) return;
            }
            CheckURL(searchTextBox.Text);
        }
        private async void CheckURL(string youtubeUrl) {
            Searched = youtubeUrl;
            try {
                var youtube = new YoutubeClient();
                var playlist = await youtube.Playlists.GetAsync(youtubeUrl);
                var videos = youtube.Playlists.GetVideosAsync(playlist.Id);
                await foreach (var video in videos) {
                    if (quere.Exists(x => x.YoutubeUrl == video.Url)) {
                        var msgbox = MessageBox.Show(this, video.Title + "\n既に存在しているようです。追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgbox == MessageBoxResult.No) continue;
                    }
                    Debug.Print(video.Url);
                    await Task.Run(() => AddQuere(video.Url));
                }

            } catch (ArgumentException) {
                if (quere.Exists(x => x.YoutubeUrl == youtubeUrl)) {
                    var msgbox = MessageBox.Show(this ,"既に存在しているようです。追加しますか？", "JellyParfait", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msgbox == MessageBoxResult.No) return;
                }
                await Task.Run(() => AddQuere(youtubeUrl));
            } finally {
                Searched = String.Empty;
            }
        }

        private void AddQuere(string youtubeUrl) {
            MusicData musicData = null;
            musicData = GetVideoObject(youtubeUrl).Result;
            if (musicData == null) return;
            if (musicData.Url == string.Empty) return;

            quere.Add(musicData);
            Dispatcher.Invoke(() => ReloadListView());

            if (quere.Count == 1) {
                nowQuere = 0;
                Dispatcher.Invoke(() => PlayMusic(musicData));
            }

        }

        private async Task<MusicData> GetVideoObject(string youtubeUrl) {
            try {
                var youtubeClient = new YoutubeClient();
                var video = await youtubeClient.Videos.GetAsync(youtubeUrl);
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var url = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;

                var data = new MusicData(this);
                data.Title = video.Title;
                data.Url = url;
                data.YoutubeUrl = youtubeUrl;
                data.QuereId = quere.Count;
                data.Visibility = Visibility.Hidden;
                data.Color = "white";

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
                //} catch {
                //    Dispatcher.Invoke(() => MessageBox.Show(this, "Error\n不明なエラーが発生しました。\nURLが正しいか確認した後もう一度やり直してください", "JellyParfait", MessageBoxButton.OK, MessageBoxImage.Warning));
                //    return null;
            }
        }

        public async void PlayMusic(MusicData data) {
            Complete = false;
            await Task.Run(() => {
                if (player != null) {
                    player.Dispose();
                    media.Dispose();
                }
            });

            PlayButton.Content = Resources["Pause"];
            data.Visibility = Visibility.Visible;
            data.Color = "NavajoWhite";
            ReloadListView();

            await Task.Run(() => {
                player = new WaveOutEvent();
                media = new MediaFoundationReader(data.Url);
                media.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                player.Init(media);
                player.Volume = 0.5f
                ;
                Dispatcher.Invoke(() => {
                    ResetTime();
                    SetSliderTimeLabel(media.TotalTime);
                    ChangeTitle(quere[nowQuere].Title);
                });

                var time = new TimeSpan(0, 0, 0);

                player.Play();

                Complete = true;

                while (true) {
                    Thread.Sleep(200);
                    if (player == null) break;
                    if (player.PlaybackState == PlaybackState.Paused) continue;
                    if (player.PlaybackState == PlaybackState.Stopped) break;
                    if (sliderClick) continue;
                    if (time != media.CurrentTime) {
                        Dispatcher.Invoke(() => SetTime(media.CurrentTime));
                        time = media.CurrentTime;
                        Debug.Print(media.CurrentTime.ToString());
                    }
                }
            });

            data.Visibility = Visibility.Hidden;
            data.Color = "White";
            ReloadListView();
            if (player.PlaybackState != PlaybackState.Paused) Next();
        }

        public bool IsPlay() {
            if (player == null) return false;
            return player.PlaybackState == PlaybackState.Playing;
        }

        public void Play() {
            if (player != null) {
                player.Play();
                PlayButton.Content = Resources["Pause"];
            }
        }

        public void Stop() {
            if (player != null) {
                player.Stop();
                PlayButton.Content = Resources["Play"];
            }
        }

        public void Pause() {
            if (player != null) {
                player.Pause();
                PlayButton.Content = Resources["Play"];
            }
        }

        private async void Prev() {
            if (Clicked) return;
            if (quere.Count == 0) return;
            Clicked = true;
            player.Dispose();
            if (nowQuere == 0) {
                nowQuere = quere.Count - 1;
            } else {
                nowQuere--;
            }
            PlayMusic(quere[nowQuere]);
            await Task.Run(() => {
                while (!Complete) {
                    Thread.Sleep(100);
                }
            });
            Clicked = false;
        }

        private async void Next() {
            if (Clicked) return;
            if (nowQuere == -1) return;
            if (quere.Count == 0) return;
            Clicked = true;
            if (quere.Count <= nowQuere + 1) {
                nowQuere = 0;
            } else {
                nowQuere++;
            }
            PlayMusic(quere[nowQuere]);
            await Task.Run(() => {
                while (!Complete) {
                    Thread.Sleep(100);
                }
            });
            Clicked = false;
        }

        private void ChangeTitle(string musicTitle) {
            titleLabel.Content = "Now Playing : " + musicTitle;
        }

        private void ReloadListView() {
            MusicQuere.ItemsSource = null;
            MusicQuere.ItemsSource = quere;
        }

        private void SetTime(TimeSpan time) {
            var seconds = time.Seconds.ToString();
            if (time.Seconds < 10) seconds = "0" + seconds;
            var totalSec = time.Minutes * 60 + time.Seconds;
            startLabel.Content = time.Minutes.ToString() + ":" + seconds;
            MusicTimeSlider.Value = totalSec;
        }

        private void ResetTime() {
            startLabel.Content = "0:00";
            endLabel.Content = "0:00";
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Minimum = MusicTimeSlider.Maximum = 0;
        }

        private void SetSliderTimeLabel(TimeSpan totalTime) {
            var seconds = totalTime.Seconds.ToString();
            if (totalTime.Seconds < 10) seconds = "0" + seconds;
            endLabel.Content = totalTime.Minutes + ":" + seconds;
            var totalSec = totalTime.Minutes * 60 + totalTime.Seconds;
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Maximum = totalSec;
        }

        //MusicData.cs
        public async void SetQuere(int num) {
            if (Clicked) return;
            if (IsPlay()) Stop();
            Clicked = true;
            player.Dispose();
            nowQuere = num;
            PlayMusic(quere[num]);
            await Task.Run(() => {
                while (!Complete) {
                    Thread.Sleep(100);
                }
            });
            Clicked = false;
        }

        public int getQuereId() {
            return nowQuere;
        }

        public void changeClickedFlag(bool flag) {
            Clicked = flag;
        }

        public bool getClickedFlag() {
            return Clicked;
        }
    }
}
