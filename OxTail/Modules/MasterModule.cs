using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using OxTailHelpers;
using Ninject;

namespace OxTail.Modules
{
    internal class MasterModule : NinjectModule
    {
        private readonly IApplication Application;

        public MasterModule(IApplication application)
        {
            this.Application = application;
        }

        public override void Load()
        {
            Bind<INinjectModule>().To<ApplicationModule>().Named("ApplicationModule").WithConstructorArgument("application", this.Application);
            Bind<INinjectModule>().To<WindowModule>().Named("WindowModule");
            Bind<INinjectModule>().To<FactoryModule>().Named("FactoryModule");
            Bind<INinjectModule>().To<DataModule>().Named("DataModule");

            this.Kernel.Load(this.Kernel.Get<INinjectModule>("ApplicationModule"), this.Kernel.Get<INinjectModule>("WindowModule"), this.Kernel.Get<INinjectModule>("FactoryModule"), this.Kernel.Get<INinjectModule>("DataModule"));
        }
    }
}
