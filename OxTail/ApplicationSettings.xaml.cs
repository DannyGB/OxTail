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
using OxTailLogic;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for ApplicationSettings.xaml
    /// </summary>
    public partial class ApplicationSettings : BaseWindow
    {
        /// <summary>
        /// Initialize this instance
        /// </summary>
        public ApplicationSettings(ISettingsHelper settingsHelper)
        {
            InitializeComponent();

            OxTail.Controls.ApplicationSettings ApplicationSettings = new OxTail.Controls.ApplicationSettings(settingsHelper);
            ApplicationSettings.SaveClick += new RoutedEventHandler(ApplicationSettings_SaveClick);
            ApplicationSettings.CancelClick +=new RoutedEventHandler(ApplicationSettings_CancelClick);
            this.Grid.Children.Add(ApplicationSettings);
        }


        private void ApplicationSettings_SaveClick(object sender, RoutedEventArgs e)
        {
            base.ThrowSaveClick(sender, e);        
        }

        private void ApplicationSettings_CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
