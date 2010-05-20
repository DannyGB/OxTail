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
    using System.Xml.Serialization;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Interaction logic for Highlighting.xaml
    /// </summary>
    public partial class Highlighting : UserControl
    {        
        public ObservableCollection<Pattern> Patterns { get; set; }

        public Highlighting()
        {
            InitializeComponent();
            
            this.buttonColour.SelectedColour = ((SolidColorBrush)this.textBoxPattern.Foreground).Color;
            this.buttonBackColour.SelectedColour = ((SolidColorBrush)this.textBoxPattern.Background).Color;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Patterns.Add(new Pattern(this.textBoxPattern.Text, this.buttonColour.SelectedColour, this.checkBoxIgnoreCase.IsChecked, this.buttonBackColour.SelectedColour));            
        }

        private void buttonColour_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.buttonColour.ShowColourSelectDialog())
            {
                this.textBoxPattern.Foreground = buttonColour.ColorBrush;                
            }
        }        

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.listViewPatterns.DataContext == null || this.listViewPatterns.DataContext.GetType() != typeof(ObservableCollection<Pattern>))
            {
                return;
            }

            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(Pattern))
            {
                return;
            }

            ((ObservableCollection<Pattern>)this.listViewPatterns.DataContext).Remove((Pattern)this.listViewPatterns.SelectedItem);
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(Pattern))
            {
                return;
            }

            Pattern selectedPattern = (Pattern)this.listViewPatterns.SelectedItem;
            this.textBoxPattern.Text = selectedPattern.StringPattern;
            this.textBoxPattern.Foreground = new SolidColorBrush(selectedPattern.Colour);
            this.textBoxPattern.Background = new SolidColorBrush(selectedPattern.BackColour);
            this.checkBoxIgnoreCase.IsChecked = selectedPattern.IgnoreCase;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(Pattern))
            {
                return;
            }

            ((Pattern)this.listViewPatterns.SelectedItem).StringPattern = this.textBoxPattern.Text;
            ((Pattern)this.listViewPatterns.SelectedItem).Colour =  ((SolidColorBrush)this.textBoxPattern.Foreground).Color;
        }

        private void buttonBackColour_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)this.buttonBackColour.ShowColourSelectDialog())
            {
                this.textBoxPattern.Background = buttonBackColour.ColorBrush;                
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Pattern>));
            using (XmlTextWriter writer = new XmlTextWriter(@"c:\temp\highlights.xml", Encoding.UTF8))
            {
                serializer.Serialize(writer, this.listViewPatterns.DataContext);
            }
        }

        public void Bind()
        {
            this.listViewPatterns.DataContext = Patterns;
        }
    }
}
