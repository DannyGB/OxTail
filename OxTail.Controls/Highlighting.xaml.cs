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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Data;
    using System.Xml;
    using System;
    using OxTailHelpers;

    public enum Direction
    {
        Up,
        Down
    }

    /// <summary>
    /// Interaction logic for Highlighting.xaml
    /// </summary>
    public partial class Highlighting : UserControl
    {
        public event RoutedEventHandler OpenExpressionBuilder;
        public HighlightCollection<HighlightItem> Patterns { get; set; }
        private const string SORT_HEADER = "Order";

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
            Patterns.Add(new HighlightItem(this.textBoxPattern.Text, this.buttonColour.SelectedColour, this.buttonBackColour.SelectedColour));
            this.Sort(SORT_HEADER, ListSortDirection.Descending);
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
            if (this.listViewPatterns.DataContext == null || this.listViewPatterns.DataContext.GetType() != typeof(HighlightCollection<HighlightItem>))
            {
                return;
            }

            if (this.listViewPatterns.SelectedItem == null || this.listViewPatterns.SelectedItem.GetType() != typeof(HighlightItem))
            {
                return;
            }

            ((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext).Remove((HighlightItem)this.listViewPatterns.SelectedItem);
            ICollectionView view = CollectionViewSource.GetDefaultView(this.listViewPatterns.DataContext);
            view.Refresh();
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

            ICollectionView view = CollectionViewSource.GetDefaultView(this.listViewPatterns.DataContext);
            if (view != null && !view.CanSort)
            {
                MessageBox.Show(LanguageHelper.GetLocalisedText((Application.Current as IApplication), "noSortingAllowed"));
                return;
            }

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Descending));
            view.Refresh();
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
                this.buttonColour.SelectedColour = selectedPattern.ForeColour;
                this.buttonBackColour.SelectedColour = selectedPattern.BackColour;
            }
        }

        private void buttonExpressionBuilder_Click(object sender, RoutedEventArgs e)
        {
            if (OpenExpressionBuilder != null)
            {
                this.OpenExpressionBuilder(this, e);
            }
        }
      
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(listViewPatterns.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();

        }

        private void buttonOrderDown_Click(object sender, RoutedEventArgs e)
        {
            this.SortItems((HighlightItem)((Button)sender).DataContext, Direction.Down);
        }

        private void buttonOrderUp_Click(object sender, RoutedEventArgs e)
        {
            this.SortItems((HighlightItem)((Button)sender).DataContext, Direction.Up);
        }

        private void SortItems(HighlightItem item, Direction dir)
        {
            int? i = null;
            int? tmp = null;
            switch (dir)
            {
                case Direction.Up:
                    i = ((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext).IndexOf(item);
                    if (i != null && i > 0 && i < ((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext).Count)
                    {
                        tmp = item.Order;
                        item.Order = ((HighlightItem)((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext)[i.Value - 1]).Order;
                        ((HighlightItem)((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext)[i.Value - 1]).Order = tmp.Value;
                    }
                    break;
                case Direction.Down:
                    i = ((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext).IndexOf(item);
                    if (i != null && i >= 0 && i < (((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext).Count - 1))
                    {
                        tmp = item.Order;
                        item.Order = ((HighlightItem)((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext)[i.Value + 1]).Order;
                        ((HighlightItem)((HighlightCollection<HighlightItem>)this.listViewPatterns.DataContext)[i.Value + 1]).Order = tmp.Value;
                    }
                    break;
                default:
                    break;
            }

            this.Sort(SORT_HEADER, ListSortDirection.Descending);
        }
    }
}
