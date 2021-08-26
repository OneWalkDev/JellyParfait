using GalaSoft.MvvmLight.Command;
using JellyParfait.MVVM.Model;
using JellyParfait.MVVM.ViewModel;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace JellyParfait.MVVM.View
{
    /// <summary>
    /// Player.xaml の相互作用ロジック
    /// </summary>
    public partial class Player : UserControl, IPlayer
    {
        private enum PlayerState
        {
            Playing,
            Paused,
            Stopped,
        }

        #region DependencyProperty
        public static readonly DependencyProperty MusicProperty =
            DependencyProperty.Register("Music", typeof(Music), typeof(Player), new UIPropertyMetadata(null, OnMusicChanged));

        public static readonly DependencyProperty CurrentProperty =
            DependencyProperty.Register("Current", typeof(TimeSpan), typeof(Player), new UIPropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register("Total", typeof(TimeSpan), typeof(Player), new UIPropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty TotalSecProperty =
            DependencyProperty.Register("TotalSec", typeof(int), typeof(Player), new UIPropertyMetadata(0));

        public static readonly DependencyProperty ElapsedProperty =
            DependencyProperty.Register("Elapsed", typeof(int), typeof(Player), new UIPropertyMetadata(0));

        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(Player), new UIPropertyMetadata(false));

        public static readonly DependencyProperty ShuffleProperty =
            DependencyProperty.Register("Shuffle", typeof(bool), typeof(Player), new UIPropertyMetadata(false));

        public static readonly DependencyProperty WithPlaylistProperty =
            DependencyProperty.Register("WithPlaylist", typeof(bool), typeof(Player), new UIPropertyMetadata(false));

        public static readonly DependencyProperty PrevCommandProperty =
            DependencyProperty.Register("PrevCommand", typeof(ICommand), typeof(Player), new UIPropertyMetadata(null));

        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(Player), new UIPropertyMetadata(null));

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(PlayerState), typeof(Player), new UIPropertyMetadata(PlayerState.Stopped, OnStateChanged));

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(Player), new UIPropertyMetadata(false));
        #endregion

        #region Property for DependencyProperty
        public Music Music
        {
            get => (Music)GetValue(MusicProperty);
            set => SetValue(MusicProperty, value);
        }

        public TimeSpan Current
        {
            get => (TimeSpan)GetValue(CurrentProperty);
            private set => SetValue(CurrentProperty, value);
        }

        public TimeSpan Total
        {
            get => (TimeSpan)GetValue(TotalProperty);
            private set => SetValue(TotalProperty, value);
        }

        public int TotalSec
        {
            get => (int)GetValue(TotalSecProperty);
            private set => SetValue(TotalSecProperty, value);
        }

        public int Elapsed
        {
            get => (int)GetValue(ElapsedProperty);
            private set => SetValue(ElapsedProperty, value);
        }

        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }

        public bool Shuffle
        {
            get => (bool)GetValue(ShuffleProperty);
            set => SetValue(ShuffleProperty, value);
        }

        public bool WithPlaylist
        {
            get => (bool)GetValue(WithPlaylistProperty);
            set => SetValue(WithPlaylistProperty, value);
        }

        public ICommand PrevCommand
        {
            get => (ICommand)GetValue(PrevCommandProperty);
            set => SetValue(PrevCommandProperty, value);
        }

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        private PlayerState State
        {
            get => (PlayerState)GetValue(StateProperty);
            set
            {
                IsPlaying = value == PlayerState.Playing;
                SetValue(StateProperty, value);
            }
        }

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            private set => SetValue(IsPlayingProperty, value);
        }
        #endregion

        public event EventHandler<PlayerStoppedEventArgs> PlayerStopped;

        public ICommand PlayCommand { get; }

        private Model.MusicPlayer _MusicPlayer = null;
        private DispatcherTimer _Timer = null;
        private CausedStop _LastCausedStop;
        
        public Player()
        {
            InitializeComponent();

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += (_, _) => UpdateTime();

            PlayCommand = new RelayCommand(() =>
            {
                if (State == PlayerState.Stopped || State == PlayerState.Paused)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            });

            DataContext = this;
        }

        public void Play()
        {
            if (_MusicPlayer == null || _MusicPlayer.State == PlaybackState.Playing)
                return;
            
            _MusicPlayer.Play();
            _Timer.Start();
            State = PlayerState.Playing;
        }

        public void Pause()
        {
            if (_MusicPlayer == null || _MusicPlayer.State != PlaybackState.Playing)
                return;

            _MusicPlayer.Pause();
            _Timer.Stop();
            State = PlayerState.Paused;
        }

        public void Stop()
        {
            if (_MusicPlayer == null || _MusicPlayer.State == PlaybackState.Stopped)
                return;

            _Timer.Stop();
            _LastCausedStop = CausedStop.CallStop;
            _MusicPlayer.Stop();
        }

        private void UpdateTime()
        {
            if(_MusicPlayer == null)
            {
                Current = TimeSpan.Zero;
                Total = TimeSpan.Zero;
                Elapsed = 0;
                TotalSec = 0;
            }
            else
            {
                Current = _MusicPlayer.Current;
                Total = _MusicPlayer.Total;
                Elapsed = (int)Current.TotalSeconds;
                TotalSec = (int)Total.TotalSeconds;
            }
        }

        private static void OnStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var player = sender as Player;
            player.UpdateTime();

            var state = (PlayerState)args.NewValue;
            if(state == PlayerState.Stopped)
            {
                player.PlayerStopped?.Invoke(player, new PlayerStoppedEventArgs { CausedStop = player._LastCausedStop });
            }
        }

        private static void OnMusicChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var player = sender as Player;
            
            if(player.IsPlaying)
            {
                player._MusicPlayer.PlaybackStopped -= player.OnStopped;
                player.Stop();
                player._LastCausedStop = CausedStop.ChangeMusic;
                player.State = PlayerState.Stopped;
                player._MusicPlayer.Dispose();
                player._MusicPlayer = null;
            }

            if(args.NewValue == null)
            {
                player.UpdateTime();
                return;
            }


            var music = args.NewValue as Music;
            player._MusicPlayer = new MusicPlayer(music.AudioPath);
            player._MusicPlayer.PlaybackStopped += player.OnStopped;
            player.UpdateTime();
        }

        private void OnStopped(object sender, StoppedEventArgs args)
        {
            _LastCausedStop = CausedStop.EndMusic;
            State = PlayerState.Stopped;
        }
    }
}
