using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Timers;
using System.Drawing;
using System.Windows.Forms;

namespace RandomDataGenerator
{
    class Program
    {
        static string Filename { get; set; }
        static bool EmptyFileAtStart { get; set; }
        static bool FirstCall { get; set; }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new Generator());
        }            
    }
}
