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

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public ApplicationSettings()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SettingsHelper.AppSettings[AppSettings.REFRESH_INTERVAL_KEY] = (int.Parse(this.comboBoxInterval.Text) * 1000).ToString();
            SettingsHelper.AppSettings[AppSettings.MAX_OPEN_FILES] = (int.Parse(this.integerTextBoxMaxOpenFiles.Text)).ToString();

            if (this.SaveClick != null)
            {
                this.SaveClick(this, e);
            }
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
            this.comboBoxInterval.Text = (int.Parse(SettingsHelper.AppSettings[AppSettings.REFRESH_INTERVAL_KEY]) / 1000).ToString();
            this.integerTextBoxMaxOpenFiles.Text = (int.Parse(SettingsHelper.AppSettings[AppSettings.MAX_OPEN_FILES])).ToString();
        }
    }
}
