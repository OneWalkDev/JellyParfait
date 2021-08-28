using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JellyParfait.Windows {
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Equalizer : Window {
        public Equalizer() {
            InitializeComponent();
        }

        private string[] EqualizerKey = new string[9] { "EQ_32", "EQ_64", "EQ_125", "EQ_250", "EQ_500", "EQ_1k", "EQ_2k", "EQ_4k", "EQ_8k" };

        private int[] EqualizerValue = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };


        private void EQ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var slider = (Slider)sender;
            var key = Array.IndexOf(EqualizerKey, slider.Name);
            if(slider.Value != EqualizerValue[key]) {
                EqualizerValue[key] = (int)slider.Value;
                var text = (TextBlock)FindName(EqualizerKey[key] + "_Text");
                text.Text = slider.Value.ToString();
            }
        }
    }
}
