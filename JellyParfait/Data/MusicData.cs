using GalaSoft.MvvmLight.Command;

namespace JellyParfait.Data {

    class MusicData {

        private MainWindow main;

        public MusicData(MainWindow main) {
            this.main = main;
        }

        public string Title { get; set; }

        public string Url { get; set; }

        public string YoutubeUrl { get; set; }

        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    main.playMusic(Url);
                    main.changeTitle(Title);
                });
            }
        }

    }
}
