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
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Collections.ObjectModel;
    using System;

    /// <summary>
    /// Interaction logic for Highlighting.xaml
    /// </summary>
    public partial class Highlighting : UserControl
    {        
        public event RoutedEventHandler OpenExpressionBuilder;
        public ObservableCollection<HighlightItem> Patterns { get; set; }        

        public Highlighting()
        {
            InitializeComponent();
            
            this.buttonColour.SelectedColour = ((SolidColorBrush)this.textBoxPattern.Foreground).Color;
            this.buttonBackColour.SelectedColour = ((SolidColorBrush)this.textBoxPattern.Background).Color;
        }

        public string Pattern 
        {
            set
            {
                this.textBoxPattern.Text = value;
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Patterns.Add(new HighlightItem(this.textBoxPattern.Text, this.buttonColour.SelectedColour, this.checkBoxIgnoreCase.IsChecked, this.buttonBackColour.SelectedColour));            
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
            if (this.listViewPatterns.DataContext == null || this.listViewPatterns.DataContext.GetType() != typeof(ObservableCollection<HighlightItem>))
            {
                return;
            }

            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(HighlightItem))
            {
                return;
            }

            ((ObservableCollection<HighlightItem>)this.listViewPatterns.DataContext).Remove((HighlightItem)this.listViewPatterns.SelectedItem);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(HighlightItem))
            {
                return;
            }

            ((HighlightItem)this.listViewPatterns.SelectedItem).Pattern = this.textBoxPattern.Text;
            ((HighlightItem)this.listViewPatterns.SelectedItem).ForeColour = ((SolidColorBrush)this.textBoxPattern.Foreground).Color;
            ((HighlightItem)this.listViewPatterns.SelectedItem).BackColour = ((SolidColorBrush)this.textBoxPattern.Background).Color;
            ((HighlightItem)this.listViewPatterns.SelectedItem).IgnoreCase = this.checkBoxIgnoreCase.IsChecked;

            this.listViewPatterns.DataContext = null;
            this.listViewPatterns.DataContext = Patterns;
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
        }

        public void Bind()
        {
            this.listViewPatterns.DataContext = Patterns;            
        }

        private void listViewPatterns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(HighlightItem))
            {
                return;
            }

            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                HighlightItem selectedPattern = (HighlightItem)e.AddedItems[0];
                this.textBoxPattern.Text = selectedPattern.Pattern;
                this.textBoxPattern.Foreground = new SolidColorBrush(selectedPattern.ForeColour);
                this.textBoxPattern.Background = new SolidColorBrush(selectedPattern.BackColour);
                this.checkBoxIgnoreCase.IsChecked = selectedPattern.IgnoreCase;
            }
        }

        private void buttonExpressionBuilder_Click(object sender, RoutedEventArgs e)
        {
            if (OpenExpressionBuilder != null)
            {
                this.OpenExpressionBuilder(this, e);
            }
        }
    }
}
