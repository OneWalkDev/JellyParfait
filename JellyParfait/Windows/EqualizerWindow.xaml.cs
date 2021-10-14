using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JellyParfait.Windows {
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class EqualizerWindow : Window {
        private MainWindow window;

        private readonly string[] EqualizerKey = new string[10] { "EQ_32", "EQ_64", "EQ_125", "EQ_250", "EQ_500", "EQ_1k", "EQ_2k", "EQ_4k", "EQ_8k", "EQ_16k"};

        private int[] EqualizerValue = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};


        public EqualizerWindow(MainWindow window) {
            InitializeComponent();
            this.window = window;
            var config = window._settings.config;
            EqualizerValue = new int[10] { (int)config.EQ_32, (int)config.EQ_64, (int)config.EQ_125, (int)config.EQ_250, (int)config.EQ_500, (int)config.EQ_1000, (int)config.EQ_2000, (int)config.EQ_4000, (int)config.EQ_8000, (int)config.EQ_12000 };
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e) {
            foreach (var eq in window.GetEqualizer().Select((value, index) => new { value, index })) {
                var slider = (Slider)FindName(EqualizerKey[eq.index]);
                var text = (TextBlock)FindName(EqualizerKey[eq.index] + "_Text");
                slider.Value = eq.value.Gain;
                text.Text = eq.value.Gain.ToString();
            }
        }

        private void EQ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var slider = (Slider)sender;
            var key = Array.IndexOf(EqualizerKey, slider.Name);
            EqualizerValue[key] = (int)slider.Value;
            var text = (TextBlock)FindName(EqualizerKey[key] + "_Text");
            text.Text = slider.Value.ToString();
            window.ChangeEqualizer(key, (int)slider.Value);
        }

    }
}
