using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTailLogic
{
    public class SettingsHelper
    {
        private static object padlock = new object();
        private static AppSettings appSettings;
        public static AppSettings AppSettings 
        {
            get
            {
                CheckAppSettingsInit();
                return appSettings;
            }

            set
            {
                lock (padlock)
                {
                    CheckAppSettingsInit();
                    appSettings = value;
                }
            }
        }

        private static void CheckAppSettingsInit()
        {
            if (appSettings == null)
            {
                appSettings = new AppSettings();
            }
        }
    }
}
