using GalaSoft.MvvmLight.Command;
using JellyParfait.MVVM.Model;
using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace JellyParfait.MVVM.ViewModel
{
    public class MainWindowViewModel : DependencyObject
    {
        private readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\JellyParfait\";
        private readonly string cachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\JellyParfait\cache\";

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(MainWindowViewModel), new UIPropertyMetadata(-1, OnSelectedIndexChanged));

        public static readonly DependencyProperty IsMusicFlyoutOpenProperty =
            DependencyProperty.Register("IsMusicFlyoutOpen", typeof(bool), typeof(MainWindowViewModel), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsPlaylistFlyoutOpenProperty =
            DependencyProperty.Register("IsPlaylistFlyoutOpen", typeof(bool), typeof(MainWindowViewModel), new UIPropertyMetadata(false));

        public IDialogCoordinator Dialog { get; }
        public IPlayer Player { get; }
        
        public ObservableCollection<MusicWrap> Playlist { get; }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public bool IsMusicFlyoutOpen
        {
            get => (bool)GetValue(IsMusicFlyoutOpenProperty);
            set => SetValue(IsMusicFlyoutOpenProperty, value);
        }

        public bool IsPlaylistFlyoutOpen
        {
            get => (bool)GetValue(IsPlaylistFlyoutOpenProperty);
            set => SetValue(IsPlaylistFlyoutOpenProperty, value);
        }

        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }

        public ICommand ForwardCommand { get; }
        public ICommand BackwardCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand OpenMusicFlyoutCommand { get; }
        public ICommand OpenPlaylistFlyoutCommand { get; }
        public ICommand AddMusicCommand { get; }
        public ICommand AddPlaylistCommand { get; }

        private YoutubeClient _Client;
        private bool _PlayNext = false;

        public MainWindowViewModel(IDialogCoordinator dialog, IPlayer player)
        {
            Dialog = dialog;
            Player = player;
            Player.PlayerStopped += OnPlayerStopped;
            Playlist = new ObservableCollection<MusicWrap>();
            
            _Client = new YoutubeClient();

            PrevCommand = new RelayCommand(() =>
            {
                if(SelectedIndex > 0)
                {
                    SelectedIndex -= 1;
                }
            });
            NextCommand = new RelayCommand(() => 
            {
                if (Player.Loop || Playlist.Count == 1)
                {
                    Player.Music = null;
                    Player.Music = Playlist[SelectedIndex].Music;
                    Player.Play();
                    return;
                }
                if (Player.Shuffle)
                {
                    while (true)
                    {
                        var rand = new Random().Next(0, Playlist.Count);
                        if(rand != SelectedIndex)
                        {
                            SelectedIndex = rand;
                            return;
                        }
                    }
                }
                if (SelectedIndex + 1 < Playlist.Count)
                {
                    {
                        SelectedIndex += 1;
                    }
                }
            });

            ForwardCommand = new RelayCommand<int>(index =>
            {
                if(index > 0)
                {
                    Playlist[index].Index -= 1;
                    Playlist[index - 1].Index += 1;
                    Playlist.Move(index, index - 1);
                }
            });
            BackwardCommand = new RelayCommand<int>(index =>
            {
                if(index + 1 < Playlist.Count)
                {
                    Playlist[index].Index += 1;
                    Playlist[index + 1].Index -= 1;
                    Playlist.Move(index, index + 1);
                }
            });
            DeleteCommand = new RelayCommand<int>(index =>
            {
                foreach(var musicWrap in Playlist.Skip(index + 1))
                {
                    musicWrap.Index -= 1;
                }
                if(SelectedIndex == index)
                {
                    NextCommand.Execute(null);
                }
                Playlist.RemoveAt(index);
            });

            OpenMusicFlyoutCommand = new RelayCommand(() =>
            {
                IsMusicFlyoutOpen = true;
            });
            OpenPlaylistFlyoutCommand = new RelayCommand(() =>
            {
                IsPlaylistFlyoutOpen = true;
            });
            AddMusicCommand = new RelayCommand<string>(url =>
            {
                IsMusicFlyoutOpen = false;
                Download(() => FetchMusic(url));
            });
            AddPlaylistCommand = new RelayCommand<string>(url =>
            {
                IsPlaylistFlyoutOpen = false;
                Download(() => FetchPlaylist(url));
            });
        }

        private static void OnSelectedIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var vm = (MainWindowViewModel)sender;
            var index = (int)args.NewValue;
            if (index <= -1)
            {
                vm.Player.Music = null;
            }
            else if(vm.Player.Music != vm.Playlist[index].Music)
            {
                var playing = vm.Player.IsPlaying;
                vm.Player.Music = vm.Playlist[index].Music;
                if (playing || vm._PlayNext)
                {
                    vm.Player.Play();
                }
            }
        }

        private void OnPlayerStopped(object sender, PlayerStoppedEventArgs args)
        {
            if(args.CausedStop == CausedStop.EndMusic)
            {
                _PlayNext = true;
                NextCommand.Execute(null);
                _PlayNext = false;
            }
        }

        private async Task<IEnumerable<string>> FetchMusic(string url)
        {
            var video = await _Client.Videos.GetAsync(url);
            return new string[] { video.Url };
        }

        private async Task<IEnumerable<string>> FetchPlaylist(string url)
        {
            var download = new List<string>();
            await foreach (var video in _Client.Playlists.GetVideosAsync(url))
            {
                //TODO: ShowYesNo
                download.Add(video.Url);
            }
            return download;
        }

        private async void Download(Func<Task<IEnumerable<string>>> fetchFunc)
        {
            var progress = await Dialog.ShowProgressAsync(this, "JellyParfait", "ダウンロード中...");
            var playlist = (await fetchFunc()).ToList();
            try
            {
                foreach (var (url, i) in playlist.Select((url, i) => (url, i)))
                {
                    var video = await _Client.Videos.GetAsync(url);
                    var path = $"{cachePath}{video.Id}.m4a";

                    if (!File.Exists(path))
                    {
                        var manifest = await _Client.Videos.Streams.GetManifestAsync(video.Id);
                        var info = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                        await _Client.Videos.Streams.DownloadAsync(info, path);
                    }

                    Playlist.Add(new MusicWrap
                    {
                        Index = Playlist.Count,
                        Music = new Music
                        {
                            Id = video.Id,
                            Url = url,
                            Title = video.Title,
                            AudioPath = path,
                        }
                    });

                    progress.SetProgress((i + 1.0) / playlist.Count);

                    if (Playlist.Count == 1)
                    {
                        SelectedIndex = 0;
                    }
                }

                await Task.Delay(500);
            }
            catch (HttpRequestException)
            {
                await Dialog.ShowMessageAsync(this, "JellyParfait", "Error\nインターネットに接続されているか確認してください");
            }
            catch (ArgumentException)
            {
                await Dialog.ShowMessageAsync(this, "JellyParfait", "Error\nURLの形式が間違っています。");
            }
            catch (AggregateException)
            {
                await Dialog.ShowMessageAsync(this, "JellyParfait", "Error\nYoutubeのURLかどうかを確認してください");
            }
            catch
            {
                await Dialog.ShowMessageAsync(this, "JellyParfait", "Error\n不明なエラーが発生しました。\nURLが正しいか確認した後もう一度やり直してください");
            }

            await progress.CloseAsync();
        }
    }
}
