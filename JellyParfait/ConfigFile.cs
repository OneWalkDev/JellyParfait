using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JellyParfait {
    public class ConfigFile {

        private static readonly string filename = "option.txt";

        public ConfigData config { get; set; }


        public ConfigFile() {
            config = new ConfigData();
            if (!File.Exists(MainWindow.path + filename)) {
                File.Create(MainWindow.path + filename).Close();
                config.Volume = (float)0.5;
                config.EQ_32 = (float)0.0;
                config.EQ_64 = (float)0.0;
                config.EQ_125 = (float)0.0;
                config.EQ_250 = (float)0.0;
                config.EQ_500 = (float)0.0;
                config.EQ_1000 = (float)0.0;
                config.EQ_2000 = (float)0.0;
                config.EQ_4000 = (float)0.0;
                config.EQ_8000 = (float)0.0;
                config.EQ_12000 = (float)0.0;
                config.DiscordActivity = false;
                Write();
            }
            Read();
        }

        public void Write() {
            if (File.Exists(MainWindow.path + filename)) {
                File.Delete(MainWindow.path + filename);
            }
            File.Create(MainWindow.path + filename).Close();
            Encoding enc = Encoding.GetEncoding("utf-8");
            using (StreamWriter writer = new StreamWriter(MainWindow.path + filename, true, enc)) {
                writer.WriteLine(config.Volume);
                writer.WriteLine(config.EQ_32);
                writer.WriteLine(config.EQ_64);
                writer.WriteLine(config.EQ_125);
                writer.WriteLine(config.EQ_250);
                writer.WriteLine(config.EQ_500);
                writer.WriteLine(config.EQ_1000);
                writer.WriteLine(config.EQ_2000);
                writer.WriteLine(config.EQ_4000);
                writer.WriteLine(config.EQ_8000);
                writer.WriteLine(config.EQ_12000);
                writer.WriteLine(config.DiscordActivity);
            }
        }

        public void Read() {
            using (StreamReader file = new StreamReader(MainWindow.path + filename)) {
                string line;
                int count = 0;
                while ((line = file.ReadLine()) != null) {
                    switch (count) {
                        case 0:
                            config.Volume = float.Parse(line);
                            break;
                        case 1:
                            config.EQ_32 = float.Parse(line);
                            break;
                        case 2:
                            config.EQ_64 = float.Parse(line);
                            break;
                        case 3:
                            config.EQ_125 = float.Parse(line);
                            break;
                        case 4:
                            config.EQ_250 = float.Parse(line);
                            break;
                        case 5:
                            config.EQ_500 = float.Parse(line);
                            break;
                        case 6:
                            config.EQ_1000 = float.Parse(line);
                            break;
                        case 7:
                            config.EQ_2000 = float.Parse(line);
                            break;
                        case 8:
                            config.EQ_4000 = float.Parse(line);
                            break;
                        case 9:
                            config.EQ_8000 = float.Parse(line);
                            break;
                        case 10:
                            config.EQ_12000 = float.Parse(line);
                            break;
                        default:
                            config.DiscordActivity = bool.Parse(line);
                            break;
                    }
                    count++;
                }
            }
        }
    }

    public class ConfigData {
        public float Volume { get; set; }

        public float EQ_32 { get; set; }

        public float EQ_64 { get; set; }

        public float EQ_125 { get; set; }

        public float EQ_250 { get; set; }

        public float EQ_500 { get; set; }

        public float EQ_1000 { get; set; }

        public float EQ_2000 { get; set; }

        public float EQ_4000 { get; set; }

        public float EQ_8000 { get; set; }

        public float EQ_12000 { get; set; }

        public bool DiscordActivity { get; set; }
    }
}
