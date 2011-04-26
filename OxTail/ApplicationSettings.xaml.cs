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
        /// Save button clicked
        /// </summary>
        public event RoutedEventHandler SaveClick;

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public ApplicationSettings()
        {
            InitializeComponent();
        }

        private void ApplicationSettings_SaveClick(object sender, RoutedEventArgs e)
        {
            if (this.SaveClick != null)
            {
                this.SaveClick(sender, e);
            }            
        }

        private void ApplicationSettings_CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
