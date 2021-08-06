using GalaSoft.MvvmLight.Command;

namespace JellyParfait.Data {

    class MusicData {

        private MainWindow main;

        public MusicData(MainWindow main) {
            this.main = main;
        }

        public string Title { get; set; }

        public string Uri { get; set; }



        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    main.playMusic(Uri);
                    main.changeTitle(Title);
                });
            }
        }

    }
}
