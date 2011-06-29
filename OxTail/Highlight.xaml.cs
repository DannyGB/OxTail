/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail
{
    using System.Windows;
    using OxTail.Controls;
    using OxTailHelpers;

    /// <summary>
    /// Interaction logic for Highlight.xaml
    /// </summary>
    public partial class Highlight : BaseWindow, IHighlightWindow
    {
        private readonly IExpressionBuilderWindow Builder;
        private readonly Highlighting HighLighting;

        public Highlight(IExpressionBuilderWindow builder, Highlighting highLighting)
        {
            this.Builder = builder;
            this.HighLighting = highLighting;

            InitializeComponent();

            this.grid.Children.Add((UIElement)highLighting);
            this.HighLighting.Bind();
        }

        private void hightlighting_OpenExpressionBuilder(object sender, RoutedEventArgs e)
        {
            Builder.ShowDialog();

            if (Builder.DialogResult.HasValue && Builder.DialogResult.Value)
            {
                this.HighLighting.Pattern = Builder.Expression.Text;
            }
        }
    }
}
