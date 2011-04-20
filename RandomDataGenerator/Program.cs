using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Timers;

namespace RandomDataGenerator
{
    class Program
    {
        static string Filename { get; set; }
        static bool EmptyFileAtStart { get; set; }
        static bool FirstCall { get; set; }

        static void Main(string[] args)
        {
            FirstCall = true;
            Filename = @"D:\My Dropbox\oxtail\Test Files\randomdata_3.txt";
            string tempFilename = GetConsoleInput(string.Format(@"Enter filename to write to (default {0}):", Filename));
            Filename = (tempFilename == string.Empty) ? Filename : tempFilename;
            
            string temp = string.Empty;
            while(temp != "n" && temp != "y")
            {
                temp = GetConsoleInput("Empty file before we start enter y/n?");
                EmptyFileAtStart = (temp == "y") ? true : false;
            }

            Timer timer = new Timer(10000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            Console.WriteLine("Press P to pause");
            Console.WriteLine("Press C to continue");
            Console.WriteLine("Press Q quit");
            string read = Console.ReadLine().ToUpper();

            while (read != "Q")
            {
                switch (read)
                {
                    case "P":
                        timer.Stop();
                        break;
                    case "C":
                        timer.Start();
                        break;
                    default:
                        break;
                }

                read = Console.ReadLine().ToUpper();
            }
        }

        private static string GetConsoleInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CreateRandomText createRandomText = new CreateRandomText();

            // If we're to clean the file we only want to do it on the first timer tick
            if (FirstCall)
            {
                createRandomText.Create(Filename, EmptyFileAtStart);
                FirstCall = false;
            }
            else
            {
                createRandomText.Create(Filename, false);
            }
        }
    }
}
