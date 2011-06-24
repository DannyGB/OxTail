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
using OxTailHelpers;
using System.IO;
using System.Windows.Controls;
using System.Reflection;

namespace OxTail.Controls
{
    public class BaseWindow : Window, IWindow
    {
        protected bool OverrideEscapeKeyClose { get; set; }

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public BaseWindow()
        {
        }

        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == System.Windows.Input.Key.Escape && !OverrideEscapeKeyClose)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Show the form modally
        /// </summary>
        /// <returns></returns>
        public new bool? ShowDialog()
        {
            return base.ShowDialog();
        }

        /// <summary>
        /// Save button clicked
        /// </summary>
        public event RoutedEventHandler SaveClick;
        public new event RoutedEventHandler Closed;

        protected void ThrowSaveClick(object sender, RoutedEventArgs e)
        {
            if (this.SaveClick != null)
            {
                this.SaveClick(sender, e);
            }
        }

        protected void ThrowCloseClick(object sender, RoutedEventArgs e)
        {
            if (this.Closed != null)
            {
                this.Closed(sender, e);
            }
        }
    }
}