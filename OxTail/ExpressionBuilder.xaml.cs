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
    /// Interaction logic for ExpressionBuilder.xaml
    /// </summary>
    public partial class ExpressionBuilder : BaseWindow, IExpressionBuilderWindow
    {
        public IExpression Expression { get; set; }

        public readonly IRegularExpressionBuilder regularExpressionBuilder;

        public ExpressionBuilder(IRegularExpressionBuilder builder)
        {
            InitializeComponent();

            regularExpressionBuilder = builder;
            regularExpressionBuilder.OkClick += new RoutedEventHandler(RegularExpressionBuilder_OkClick);
            regularExpressionBuilder.CancelClick +=new RoutedEventHandler(RegularExpressionBuilder_CancelClick);
            this.grid.Children.Add((UIElement)regularExpressionBuilder);
        }

        private void RegularExpressionBuilder_OkClick(object sender, RoutedEventArgs e)
        {
            this.Expression = this.regularExpressionBuilder.Expression;
            this.DialogResult = true;
            this.Close();
        }

        private void RegularExpressionBuilder_CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
