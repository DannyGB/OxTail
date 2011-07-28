using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Ninject;
using OxTailHelpers.Data;
using OxTailLogic.Data;
using OxTailLogic;
using OxTail.Controls;

namespace OxTail.Modules
{
    internal class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMostRecentFilesData>().To<MostRecentFilesData>();
            Bind<ILastOpenFilesData>().To<LastOpenFilesData>();
            Bind<IAppSettingsData>().To<AppSettingsData>();
            Bind<IHighlightItemData>().To<HighlightData>();
            Bind<ISavedExpressionsData>().To<SavedExpressionData>();
            Bind<ISettingsHelper>().To<SettingsHelper>().InSingletonScope().WithConstructorArgument("appSettingsData", this.Kernel.Get<IAppSettingsData>());
            Bind<IHighlightsHelper>().To<HighlightsHelper>().InSingletonScope().WithConstructorArgument("highlightData", this.Kernel.Get<IHighlightItemData>());
            Bind<Highlighting>().ToSelf().WithConstructorArgument("highlightData", base.Kernel.Get<ISettingsHelper>());
        }
    }
}
