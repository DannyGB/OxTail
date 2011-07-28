using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using OxTailHelpers;
using OxTail.Controls;

namespace OxTail.Modules
{
    internal class FactoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IExpressionFactory>().To<ExpressionFactory>();
            Bind<IWindowFactory>().To<WindowFactory>().InSingletonScope();
            Bind<IFindWindowFactory>().To<FindWindowFactory>().InSingletonScope();
            Bind<IHighlightWindowFactory>().To<HighlightWindowFactory>().InSingletonScope();
            Bind<IExpressionBuilderWindowFactory>().To<ExpressionBuilderWindowFactory>().InSingletonScope();
            Bind<ISaveExpressionMessageWindowFactory>().To<SaveExpressionMessageWindowFactory>().InSingletonScope();
            Bind<IFileFactory>().To<FileFactory>().InSingletonScope();
            Bind<ITabItemFactory>().To<TabItemFactory>().InSingletonScope();
        }
    }
}
