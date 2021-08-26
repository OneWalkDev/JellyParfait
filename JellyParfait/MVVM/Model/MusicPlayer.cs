using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyParfait.MVVM.Model
{
    public class MusicPlayer : IDisposable
    {
        public string Path { get; }

        private WaveOutEvent _Device = null;
        private AudioFileReader _Stream = null;

        public PlaybackState State => _Device.PlaybackState;
        public TimeSpan Current => _Stream.CurrentTime;
        public TimeSpan Total => _Stream.TotalTime;

        public event EventHandler<StoppedEventArgs> PlaybackStopped
        {
            add => _Device.PlaybackStopped += value;
            remove => _Device.PlaybackStopped -= value;
        }

        public MusicPlayer(string path)
        {
            Path = path;
            _Stream = new AudioFileReader(path);
            _Stream.Position = 0;
            _Device = new WaveOutEvent();
            _Device.Init(_Stream);
        }

        public void Play()
        {
            _Device.Play();
        }

        public void Pause()
        {
            _Device.Pause();
        }

        public void Stop()
        {
            _Device.Stop();
        }

        public void Dispose()
        {
            if (_Device != null)
            {
                _Device.Stop();
                _Device.Dispose();
                _Device = null;

                _Stream.Dispose();
                _Stream = null;
            }
        }
    }
}
