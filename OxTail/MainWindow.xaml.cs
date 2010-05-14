﻿namespace OxTail
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using OxTailLogic.Helpers;
    using System.IO;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("(c) Dan Beavon ;)", "OxTail");
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            string filename = FileHelper.ShowOpenFileDialog();

            if (filename != string.Empty)
            {
                Stream content = FileHelper.OpenFile(filename);
                this.richTextBoxLogDetail.Document = FileHelper.CreateFlowDocument(content);
            }
        }
    }
}