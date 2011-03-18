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
    using System.Windows.Shapes;
    using OxTailHelpers;

    /// <summary>
    /// Interaction logic for SaveExpressionMessage.xaml
    /// </summary>
    public partial class SaveExpressionMessage : BaseWindow, ISaveExpressionMessage
    {
        /// <summary>
        /// Initializes the instance
        /// </summary>
        public SaveExpressionMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The label to display on the dialog
        /// </summary>
        public string Label
        {
            get
            {
                if (this.labelMessage.Content == null)
                {
                    return string.Empty;
                }

                return this.labelMessage.Content.ToString();
            }

            set
            {
                this.labelMessage.Content = value;
                this.Title = value;
            }
        }

        /// <summary>
        /// The message to display in the Textbox
        /// </summary>
        public string Message
        {
            get
            {
                return this.textBoxMessage.Text;
            }
            set
            {
                this.textBoxMessage.Text = value;
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxMessage.Focus();
        }
    }
}
