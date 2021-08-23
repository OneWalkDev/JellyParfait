using JellyParfait.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

namespace JellyParfait {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {

        /// <summary>
        /// フォルダへのパス
        /// </summary>
        private readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\JellyParfait\";

        private readonly string cachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\JellyParfait\cache\";

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

        private bool download;

        private bool first;

        private MouseButton mouseButton;


        public MainWindow() {
            InitializeComponent();
            if (!Directory.Exists(path)) first = true;
            Directory.CreateDirectory(cachePath);
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
            if (IsPlay()) {
                Stop();
                player.Dispose();
            }
            Application.Current.Shutdown();
        }

        public async void Version_Infomation_Click(object sender, RoutedEventArgs e) {
            await this.ShowMessageAsync("JellyParfait","JellyParfait version 0.9β\n\nCopylight(C)2021 yurisi\nAll rights reserved.\n\n本ソフトウェアはオープンソースソフトウェアです。\nGPL-3.0 Licenseに基づき誰でも複製や改変ができます。\n\nGithub\nhttps://github.com/yurisi0212/JellyParfait"); ;
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) Search();
        }



        private async void SearchTextBox_Loaded(object sender, RoutedEventArgs e) {
            //if (first) {
                var settings = new MetroDialogSettings {
                    DefaultText = "https://www.youtube.com/watch?list=PL1kIh8ZwhZzKMU8MELWCfveQifBZWUIhi",
                };
                var dialog = await this.ShowInputAsync("ようこそJellyParfaitへ！", "youtubeのURLを入力してください！(プレイリストでもOK)\n初回はキャッシュのダウンロードがあるので時間がかかります", settings);
                if (dialog == null) return;
                if (dialog == String.Empty) return;
                searchTextBox.Text = dialog;
                Search();
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Search();
        }

        private void MusicQuere_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            mouseButton = e.ChangedButton;
        }

        private void MusicQuere_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (nowQuere != MusicQuere.SelectedIndex) {
                if (MusicQuere.SelectedIndex != -1) {
                    SetQuere(MusicQuere.SelectedIndex);
                }
            }
            MusicQuere.SelectedIndex = -1;
        }

        private void MusicTimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if (player != null) {
                Pause();
                if (player.PlaybackState == PlaybackState.Paused) {
                    media.Position = (long)(media.WaveFormat.AverageBytesPerSecond * Math.Floor(MusicTimeSlider.Value));
                    Play();
                }
            }
            sliderClick = false;
        }

        private void MusicTimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            sliderClick = true;
            if (player == null) {
                MusicTimeSlider.Value = 0;
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
            try {
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
            } catch (AggregateException) {
                Dispatcher.Invoke(() => MessageBox.Show(this, "Error\n有効な動画ではありませんでした。(ライブ配信は対応していません。)", "JellyParfait - Error", MessageBoxButton.OK, MessageBoxImage.Warning));
            }

        }

        private async Task<MusicData> GetVideoObject(string youtubeUrl) {
            try {
                var youtubeClient = new YoutubeClient();
                var video = await youtubeClient.Videos.GetAsync(youtubeUrl);
                var music = cachePath + video.Id + ".mp3";

                if (!File.Exists(music)) {
                    await youtubeClient.Videos.DownloadAsync(youtubeUrl, music);
                }

                var data = new MusicData(this) {
                    Title = video.Title,
                    Url = music,
                    YoutubeUrl = youtubeUrl,
                    QuereId = quere.Count,
                    Visibility = Visibility.Hidden,
                    Id = video.Id,
                    Color = "white"
                };
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
                player = new WaveOutEvent() { DesiredLatency = 200 };
                media = new MediaFoundationReader(data.Url) {
                    Position = 0
                };
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
                    }
                }
            });

            if (!Clicked) {
                if (player.PlaybackState != PlaybackState.Paused) Next();
            }

            if (nowQuere != data.QuereId) {
                data.Visibility = Visibility.Hidden;
                data.Color = "White";
                ReloadListView();
            }
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

        public void PlayerDispose() {
            player.Dispose();
            ChangeTitle(string.Empty);
            ResetTime();
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
            PlayerDispose();
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

            if (Loop_Button.IsChecked == true || quere.Count == 1) {
                PlayerDispose();
                PlayMusic(quere[nowQuere]);
                await Task.Run(() => {
                    while (!Complete) {
                        Thread.Sleep(100);
                    }
                });
                Clicked = false;
                return;
            }

            if (Shuffle_Button.IsChecked == true) {
                if (quere.Count > 1) {
                    while (true) {
                        var rand = new Random().Next(0, quere.Count);
                        if (rand != nowQuere) {
                            Clicked = false;
                            SetQuere(rand);
                            return;
                        }
                    }
                }
            }

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
            MusicTimeSlider.Minimum = 0;
            MusicTimeSlider.Maximum = 1;
        }

        private void SetSliderTimeLabel(TimeSpan totalTime) {
            var seconds = totalTime.Seconds.ToString();
            if (totalTime.Seconds < 10) seconds = "0" + seconds;
            endLabel.Content = totalTime.Minutes + ":" + seconds;
            var totalSec = totalTime.Minutes * 60 + totalTime.Seconds;
            MusicTimeSlider.Value = 0;
            MusicTimeSlider.Maximum = totalSec;
        }
   
        public async void SetQuere(int num) {
            if (Clicked) return;
            if (IsPlay()) Stop();
            Clicked = true;
            nowQuere = num;
            PlayerDispose();
            PlayMusic(quere[num]);
            await Task.Run(() => {
                while (!Complete) {
                    Thread.Sleep(100);
                }
            });
            Clicked = false;
        }

        //MusicData.cs
        public int getQuereId() {
            return nowQuere;
        }

        public void changeClickedFlag(bool flag) {
            Clicked = flag;
        }

        public bool getClickedFlag() {
            return Clicked;
        }

        public void UpMusic(int quereId) {
            if (quereId == 0) return;
            Clicked = true;
            var source = quere[quereId];
            var destination = quere[quereId - 1];
            if (source.QuereId == nowQuere) {
                nowQuere--;
            } else if (destination.QuereId == nowQuere) {
                nowQuere++;
            }
            source.QuereId = quereId - 1;
            destination.QuereId = quereId;
            quere[quereId - 1] = source;
            quere[quereId] = destination;
            ReloadListView();
            Clicked = false;
        }

        public void DownMusic(int quereId) {
            if (quereId == quere.Count - 1) return;
            Clicked = true;
            var source = quere[quereId];
            var destination = quere[quereId + 1];
            if (source.QuereId == nowQuere) {
                nowQuere++;
            } else if (destination.QuereId == nowQuere) {
                nowQuere--;
            }
            source.QuereId = quereId + 1;
            destination.QuereId = quereId;
            quere[quereId + 1] = source;
            quere[quereId] = destination;
            ReloadListView();
            Clicked = false;
        }

        public void DisposeMusicFromQuere(int quereId) {
            Clicked = true;
            var count = 0;
            quere.RemoveAt(quereId);
            if (nowQuere == quereId) {
                PlayerDispose();
            }
            foreach (MusicData music in quere) {
                music.QuereId = count;
                count++;
            }
            if (quereId <= nowQuere) {
                nowQuere--;
            }
            ReloadListView();
            Clicked = false;
        }
    }
}
