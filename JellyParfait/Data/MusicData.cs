using GalaSoft.MvvmLight.Command;
using System;

namespace JellyParfait.Data {

    public class MusicData {

        private MainWindow main;

        public MusicData(MainWindow main) {
            this.main = main;
        }

        public int QuereId { get; set; }

        public string PlayButton{ get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string YoutubeUrl { get; set; }

        public Uri PlayButton_QuereUri { get; set; }

        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    if(main.getQuereId() != QuereId) {
                        main.SetQuere(QuereId);
                    } else {
                        (main.IsPlay() ? (Action)main.Pause : main.Play)();
                    }
                });
            }
        }

    }
}
