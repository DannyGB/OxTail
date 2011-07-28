using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using OxTailHelpers;
using OxTailHelpers.Data;
using OxTailLogic.Data;
using OxTail.Controls;
using OxTailLogic;
using OxTailLogic.PatternMatching;
using Ninject;

namespace OxTail.Modules
{
    internal class ApplicationModule : NinjectModule
    {
        private readonly IApplication Application;

        public ApplicationModule(IApplication application)
        {
            this.Application = application;
        }     

        public override void Load()
        {
            // Ninject magic whereby it automagically creates the MainWindow
            // using the constructor it knows the most parameters for and magically passes in
            // the correct RecentFileList, which is created using the constructor
            // requireing an IMostRecentFilesData argument which I tell Ninject
            // is of type MostRecentFilesData below. WOW, that's a lot of
            // mystery meat code, however it does mean that the persistence
            // object for the MostRecentFileList is interchangable with a code
            // change just here

            Bind<IRegularExpressionBuilder>().To<RegularExpressionBuilder>();
            Bind<IAppSettings>().To<AppSettings>();
            Bind<ISystemTray>().To<SystemTray>().WithConstructorArgument("application", Application);            
            Bind<IFileWatcher>().To<RationalFileWatcher>();            
            Bind<IStringPatternMatching>().To<StringPatternMatching>();            
        }       
    }
}
