using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxTailLogic.PatternMatching;
using OxTail.Properties;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for Highlight.xaml
    /// </summary>
    public partial class Highlight : Window
    {
        public Highlight()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.hightlighting.Patterns = MainWindow.Patterns;
            this.hightlighting.Bind();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Pattern.SaveHighlights(this.hightlighting.Patterns, Settings.Default.HighlightFileLocations);
        }
    }
}
