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

namespace OxTail
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
    using System.Windows.Shapes;
    using OxTailLogic.PatternMatching;
    using OxTail.Properties;
    using OxTail.Controls;

    /// <summary>
    /// Interaction logic for Highlight.xaml
    /// </summary>
    public partial class Highlight : BaseWindow
    {
        public Highlight()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.hightlighting.Patterns = MainWindow.HighlightItems;
            this.hightlighting.Bind();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HighlightItem.SaveHighlights(this.hightlighting.Patterns, Settings.Default.HighlightFileLocations);
        }

        private void hightlighting_OpenExpressionBuilder(object sender, RoutedEventArgs e)
        {
            ExpressionBuilder builder = new ExpressionBuilder();
            builder.ShowDialog();

            if (builder.DialogResult.HasValue && builder.DialogResult.Value)
            {
                this.hightlighting.Pattern = builder.Expression.Text;
            }
        }
    }
}
