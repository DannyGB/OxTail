using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using OxTailHelpers;
using OxTail.Controls;
using OxTailLogic;
using Ninject;

namespace OxTail.Modules
{
    internal class WindowModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IWindow>().To<About>().Named("About");
            Bind<IWindow>().To<ApplicationSettings>().Named("ApplicationSettings");
            Bind<IFindWindow>().To<Find>().Named("Find");
            Bind<IExpressionBuilderWindow>().To<ExpressionBuilder>().Named("ExpressionBuilder");
            Bind<ISaveExpressionMessage>().To<SaveExpressionMessage>();            
        }        
    }
}
