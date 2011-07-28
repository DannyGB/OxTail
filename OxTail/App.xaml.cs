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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using OxTail.Controls;
using System.Collections.ObjectModel;
using OxTailHelpers;
using Ninject;
using OxTailHelpers.Data;
using OxTailLogic.Data;
using OxTailLogic;
using OxTailLogic.PatternMatching;
using Ninject.Modules;
using OxTail.Modules;
using System.Reflection;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IApplication
    {
        private static Ninject.IKernel Kernel { get; set; }

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

        protected override void OnStartup(StartupEventArgs e)
        {
            Kernel = new StandardKernel();

            // Split the kernel loading into seperate modules that contain logically related content
            // There is some overlapping dependencies at the moment that I'd like to refactor.
            // And should you Ninject your Ninject modules?
            Kernel.Load(new INinjectModule[] { new MasterModule(this) });
                        
            MainWindow mainWindow = Kernel.Get<MainWindow>();
            mainWindow.Show();

        }
    }
}
