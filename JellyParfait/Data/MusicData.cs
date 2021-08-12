using GalaSoft.MvvmLight.Command;
using System.Diagnostics;

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

        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    main.SetQuere(QuereId);
                    Debug.Print(QuereId.ToString());
                    main.PlayMusic(this);
                    main.ChangeTitle(Title);
                });
            }
        }

    }
}
