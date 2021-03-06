﻿/*****************************************************************
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
using OxTailHelpers;
using OxTailLogic;
using System.IO;
using System.Media;
using System.Reflection;
using OxTailLogic.Audio;

namespace OxTail.Controls
{
    /// <summary>
    /// Interaction logic for ApplicationSettings.xaml
    /// </summary>
    public partial class ApplicationSettings : UserControl
    {
        private void Bind()
        {
        }

        /// <summary>
        /// Save button clicked
        /// </summary>
        public event RoutedEventHandler SaveClick;

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        public event RoutedEventHandler CancelClick;

        private readonly ISettingsHelper SettingsHelper;

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public ApplicationSettings(ISettingsHelper settingsHelper)
        {
            InitializeComponent();
            this.SettingsHelper = settingsHelper;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

            if (!CheckSoundFileExists())
            {
                return;
            }

            this.SettingsHelper.AppSettings[AppSettings.REFRESH_INTERVAL_KEY] = (int.Parse(this.comboBoxInterval.Text) * 1000).ToString();
            this.SettingsHelper.AppSettings[AppSettings.MAX_OPEN_FILES] = this.sliderMaxOpenFiles.Value.ToString();
            this.SettingsHelper.AppSettings[AppSettings.MAX_MRU_FILES] = this.sliderMaxMruOpenFiles.Value.ToString();
            this.SettingsHelper.AppSettings[AppSettings.REOPEN_FILES] = this.checkBoxReopenFiles.IsChecked.ToString();
            this.SettingsHelper.AppSettings[AppSettings.PLAY_SOUND] = this.checkBoxPlaySound.IsChecked.ToString();
            this.SettingsHelper.AppSettings[AppSettings.PLAY_SOUND_FILE] = this.textBoxSoundFile.Text;
            this.SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY] = this.checkBoxMinimiseToTray.IsChecked.ToString();
            this.SettingsHelper.AppSettings[AppSettings.PAUSE_ON_FOUND] = this.checkBoxPauseOnFound.IsChecked.ToString();

            if (this.SaveClick != null)
            {
                this.SaveClick(this, e);
            }
        }

        private bool CheckSoundFileExists()
        {
            if (!File.Exists(this.textBoxSoundFile.Text) && this.checkBoxPlaySound.IsChecked.Value)
            {
                MessageBox.Show(LanguageHelper.GetLocalisedText(Application.Current as IApplication, Constants.FILE_DOES_NOT_EXIST), Constants.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.CancelClick != null)
            {
                this.CancelClick(sender, e);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.comboBoxInterval.Text = (int.Parse(this.SettingsHelper.AppSettings[AppSettings.REFRESH_INTERVAL_KEY]) / 1000).ToString();
            this.sliderMaxOpenFiles.Value = (double.Parse(this.SettingsHelper.AppSettings[AppSettings.MAX_OPEN_FILES]));
            this.sliderMaxMruOpenFiles.Value = (double.Parse(this.SettingsHelper.AppSettings[AppSettings.MAX_MRU_FILES]));
            this.checkBoxReopenFiles.IsChecked = (bool.Parse(this.SettingsHelper.AppSettings[AppSettings.REOPEN_FILES]));
            this.checkBoxPlaySound.IsChecked = (bool.Parse(this.SettingsHelper.AppSettings[AppSettings.PLAY_SOUND]));
            this.textBoxSoundFile.Text = this.SettingsHelper.AppSettings[AppSettings.PLAY_SOUND_FILE];
            this.checkBoxMinimiseToTray.IsChecked = (bool.Parse(this.SettingsHelper.AppSettings[AppSettings.MINIMISE_TO_TRAY]));
            this.checkBoxPauseOnFound.IsChecked = (this.SettingsHelper.AppSettings[AppSettings.PAUSE_ON_FOUND] == null) ? false : (bool.Parse(this.SettingsHelper.AppSettings[AppSettings.PAUSE_ON_FOUND]));

            this.ToggleTextBox(this.checkBoxPlaySound.IsChecked.Value);
        }

        private void sliderMaxOpenFiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.labelSelectedMaxOpen.Content = string.Format("({0})", e.NewValue);
        }

        private void sliderMaxMruOpenFiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.labelSelectedMru.Content = string.Format("({0})", e.NewValue);
        }

        private void buttonLookup_Click(object sender, RoutedEventArgs e)
        {
            this.textBoxSoundFile.Text = OxTail.Helpers.FileHelper.ShowOpenFileDialog("wav files (*.wav)|*.wav");
        }

        private void checkBoxPlaySound_Checked(object sender, RoutedEventArgs e)
        {
            this.ToggleTextBox(this.checkBoxPlaySound.IsChecked.Value);
        }

        private void checkBoxPlaySound_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ToggleTextBox(this.checkBoxPlaySound.IsChecked.Value);
        }

        private void ToggleTextBox(bool p)
        {
            this.textBoxSoundFile.IsEnabled = p;
        }

        private void buttonTestSound_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSoundFileExists())
            {
                return;
            }

            AudioHelper.Play(this.textBoxSoundFile.Text);
        }

        private void checkBoxPauseOnFound_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void checkBoxPauseOnFound_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
