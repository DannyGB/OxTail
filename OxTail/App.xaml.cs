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
            // Ninject magic whereby it automagically creates the MainWindow
            // using the constructor it knows the most parameters for and magically passes in
            // the correct RecentFileList, which is created using the constructor
            // requireing an IMostRecentFilesData argument which I tell Ninject
            // is of type MostRecentFilesData below. WOW, that's a lot of
            // mystery meat code, however it does mean that the persistence
            // object for the MostRecentFileList is interchangable with a code
            // change just here

            // I believe a lot of this work can be done using Kernel.Load but at the moment I want "some" control over whats going on
            // so I can see it in action
            Kernel = new StandardKernel();
            Kernel.Bind<IWindow>().To<About>().Named("About");            
            Kernel.Bind<IWindow>().To<ApplicationSettings>().Named("ApplicationSettings");
            Kernel.Bind<IFindWindow>().To<Find>().Named("Find");
            Kernel.Bind<IExpressionBuilderWindow>().To<ExpressionBuilder>().Named("ExpressionBuilder");

            Kernel.Bind<IMostRecentFilesData>().To<MostRecentFilesData>();
            Kernel.Bind<ILastOpenFilesData>().To<LastOpenFilesData>();
            Kernel.Bind<IAppSettingsData>().To<AppSettingsData>();
            Kernel.Bind<IHighlightItemData>().To<HighlightData>();
            Kernel.Bind<ISavedExpressionsData>().To<SavedExpressionData>();
            
            Kernel.Bind<IRegularExpressionBuilder>().To<RegularExpressionBuilder>();
            Kernel.Bind<IExpressionFactory>().To<ExpressionFactory>();
            Kernel.Bind<IWindowFactory>().To<WindowFactory>().InSingletonScope();
            Kernel.Bind<IFindWindowFactory>().To<FindWindowFactory>().InSingletonScope();
            Kernel.Bind<IHighlightWindowFactory>().To<HighlightWindowFactory>().InSingletonScope();
            Kernel.Bind<IExpressionBuilderWindowFactory>().To<ExpressionBuilderWindowFactory>().InSingletonScope();
            Kernel.Bind<ISaveExpressionMessageWindowFactory>().To<SaveExpressionMessageWindowFactory>().InSingletonScope();
            Kernel.Bind<IFileFactory>().To<FileFactory>().InSingletonScope();
            Kernel.Bind<ITabItemFactory>().To<TabItemFactory>().InSingletonScope();

            Kernel.Bind<IAppSettings>().To<AppSettings>();
            Kernel.Bind<ISystemTray>().To<SystemTray>().WithConstructorArgument("application", this);
            Kernel.Bind<ISettingsHelper>().To<SettingsHelper>().InSingletonScope().WithConstructorArgument("appSettingsData", Kernel.Get<IAppSettingsData>());
            Kernel.Bind<IHighlightsHelper>().To<HighlightsHelper>().InSingletonScope().WithConstructorArgument("highlightData", Kernel.Get<IHighlightItemData>());
            Kernel.Bind<IFileWatcher>().To<RationalFileWatcher>();
            Kernel.Bind<ISaveExpressionMessage>().To<SaveExpressionMessage>();
            Kernel.Bind<IStringPatternMatching>().To<StringPatternMatching>();

            Kernel.Bind<Highlighting>().ToSelf().WithConstructorArgument("highlightData", Kernel.Get<ISettingsHelper>());
            
            
            MainWindow mainWindow = Kernel.Get<MainWindow>();
            mainWindow.Show();

        }
    }
}
