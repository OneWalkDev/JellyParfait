using JellyParfait.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;


namespace JellyParfait {
    class FavoriteTextFile {
        public List<string> GetURLs(string path) {
            
            string line;

            List<string> Urls = new List<string>();

            using(StreamReader file = new StreamReader(path)){
                while ((line = file.ReadLine()) != null) {
                    Urls.Add(line);
                }

                file.Close();
            }

            return Urls;
        }

        public void Save(List<MusicData> list) {

            //SaveFileDialogを生成する
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "ファイルを保存する";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\JellyParfait\favorite";
            dialog.FileName = @"お気に入り.favo";
            dialog.Filter = "お気に入りファイル(*.favo;*.favorite)|*.favo;*.favorite|テキストファイル(*.txt;*.text)|*.txt;*.text";
            dialog.FilterIndex = 1;

            //オープンファイルダイアログを表示する
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK) {
                if (File.Exists(dialog.FileName)) {
                    File.Delete(dialog.FileName);
                }
                File.Create(dialog.FileName).Close();
                Encoding enc = Encoding.GetEncoding("utf-8");
                using (StreamWriter writer = new StreamWriter(dialog.FileName, true, enc)) {
                    foreach (var obj in list) {
                        writer.WriteLine(obj.YoutubeUrl);
                    }
                    writer.Close();
                }

            }
            
        }
    }
}
