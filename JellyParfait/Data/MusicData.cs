using GalaSoft.MvvmLight.Command;

namespace JellyParfait.Data {

    class MusicData {

        public string Title { get; set; }

        public string Uri { get; set; }


        public RelayCommand ClickCommand {
            get {
                return new RelayCommand(() => {
                    var a = 1;
                });
            }
        }

    }
}
