using JellyParfait.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JellyParfait.MVVM.ViewModel
{
    public class MusicWrap : DependencyObject
    {
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(MusicWrap), new UIPropertyMetadata(-1));

        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public Music Music { get; set; }
    }
}
