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
using System.Windows.Shapes;
using OxTail.Controls;
using OxTailHelpers;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : BaseWindow, IFindWindow
    {
        private readonly IExpressionBuilderWindowFactory WindowFactory;

        public Find(IExpressionBuilderWindowFactory windowFactory)
        {
            this.WindowFactory = windowFactory;

            InitializeComponent();
        }

        private void find_ExpressionBuilderButtonClick(object sender, EventArgs e)
        {
            IExpressionBuilderWindow builder = this.WindowFactory.CreateWindow();
            builder.ShowDialog();

            if (builder.DialogResult.HasValue && builder.DialogResult.Value)
            {
                this.find.SearchTerm = builder.Expression.Text;
            }
        }

        private void find_FindButtonClick(object sender, FindEventArgs e)
        {
            if (FindCriteria != null)
            {
                FindCriteria(this, e);
            }
        }
        
        public event FindText FindCriteria;
    }
}
