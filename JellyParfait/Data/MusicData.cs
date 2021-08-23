using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;

namespace JellyParfait.Data {

    public class MusicData {

        private MainWindow main;

        public MusicData(MainWindow main) {
            this.main = main;
        }

        public int QuereId { get; set; }

        public string Title { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public string YoutubeUrl { get; set; }

        public Visibility Visibility { get; set; }

        public string Color { get; set; }

        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    if (!main.getClickedFlag()) {
                        if (main.getQuereId() != QuereId) {
                            main.SetQuere(QuereId);
                        } else {
                            main.changeClickedFlag(true);
                            (main.IsPlay() ? (Action)main.Pause : main.Play)();
                            main.changeClickedFlag(false);
                        }
                    }
                });
            }
        }

        public RelayCommand UpCommand {
            get {
                return new RelayCommand(() => {
                    if (!main.getClickedFlag()) {
                        main.UpMusic(QuereId);
                    }
                });
            }
        }

        public RelayCommand DownCommand {
            get {
                return new RelayCommand(() => {
                    if (!main.getClickedFlag()) {
                        main.DownMusic(QuereId);
                    }
                });
            }
        }
        public RelayCommand DisposeCommand {
            get {
                return new RelayCommand(() => {
                    if (!main.getClickedFlag()) {
                        main.DisposeMusicFromQuere(QuereId);
                    }
                });
            }
        }
    }
}
