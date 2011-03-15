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

namespace OxTail.Controls
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : UserControl
    {
        public delegate void FindText(object sender, FindEventArgs e);
        public event EventHandler ExpressionBuilderButtonClick;
        public event FindText FindButtonClick;

        public Find()
        {
            InitializeComponent();
        }

        public string SearchTerm
        {
            get
            {
                return this.textBoxSearchCriteria.Text;
            }

            set
            {
                this.textBoxSearchCriteria.Text = value;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void buttonExpressionBuilder_Click(object sender, RoutedEventArgs e)
        {
            if (ExpressionBuilderButtonClick != null)
            {
                ExpressionBuilderButtonClick(this, new EventArgs());
            }
        }

        private void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            CallFindButtonClickEvent();
        }

        private void CallFindButtonClickEvent()
        {
            if (FindButtonClick != null && !string.IsNullOrEmpty(this.textBoxSearchCriteria.Text))
            {
                //TODO: Options not yet implemented
                FindButtonClick(this, new FindEventArgs(this.textBoxSearchCriteria.Text, OxTailLogic.PatternMatching.FindOptions.CurrentDocument));
            }
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                CallFindButtonClickEvent();
            }
        }
    }
}
