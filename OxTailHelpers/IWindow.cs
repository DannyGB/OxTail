using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OxTailHelpers
{
    public interface IWindow
    {
        event RoutedEventHandler SaveClick;
        event RoutedEventHandler Closed;

        System.Windows.WindowStartupLocation WindowStartupLocation { get; set; }
        bool? DialogResult { get; set; }

        void Show();
        bool? ShowDialog();
        void Close();
        bool Activate();
        
    }
}
