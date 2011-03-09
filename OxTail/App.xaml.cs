using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using OxTail.Controls;
using System.Collections.ObjectModel;
using OxTailHelpers;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IApplication
    {
        public Collection<ResourceDictionary> LanguageDictionary
        {
            get
            {
                return base.Resources.MergedDictionaries;
            }
        }

        public void ApplyCultureDictionary(Uri cultureDictionaryUri)
        {
            // I believe the Resource dictionary is also used for skins so if/when we go that route then we will need
            // to rethink this code.

            // Load the ResourceDictionary into memory.
            ResourceDictionary cultureDict = Application.LoadComponent(cultureDictionaryUri) as ResourceDictionary;
            Collection<ResourceDictionary> mergedDicts = base.Resources.MergedDictionaries;

            if (mergedDicts.Count > 0)
            {
                mergedDicts.Clear();
            }
          
            mergedDicts.Add(cultureDict);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Uri uri = LanguageHelper.LoadAssemblyAndGetResourceCultureFile();
            if (uri != null)
            {
                this.ApplyCultureDictionary(uri);
            }
        }
    }
}
