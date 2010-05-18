/*****************************************************************
* This file is part of OxTail.
*
* OxTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OxTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail.Controls
{
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;
using OxTailLogic.PatternMatching;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interaction logic for Highlighting.xaml
    /// </summary>
    public partial class Highlighting : UserControl
    {
        public ObservableCollection<Pattern> patterns { get; set; }
        public Color color;

        public Highlighting()
        {
            InitializeComponent();

            patterns = new ObservableCollection<Pattern>();            
            this.listBoxPatterns.DataContext = patterns;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            patterns.Add(new Pattern(this.textBoxPattern.Text, color, this.checkBoxIgnoreCase.IsChecked));
        }

        private void buttonColour_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if ((bool)colorDialog.ShowDialog())
            {
                color = colorDialog.SelectedColor;
                this.textBoxPattern.Foreground = new SolidColorBrush(color);                
            }
        }
    }
}
