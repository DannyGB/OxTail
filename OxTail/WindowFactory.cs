using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;
using Ninject;

namespace OxTail
{
    public class WindowFactory : IWindowFactory
    {
        private Ninject.IKernel Kernel { get; set; }

        public IWindow CreateWindow(string window)
        {
            Kernel = new StandardKernel();
            Kernel.Bind<IWindow>().To<About>().Named("About");
            Kernel.Bind<IWindow>().To<Highlight>().Named("Highlight");
            Kernel.Bind<IWindow>().To<ExpressionBuilderDITest>().Named("ExpressionBuilder");
            Kernel.Bind<IWindow>().To<ApplicationSettings>().Named("ApplicationSettings");

            return Kernel.Get<IWindow>(window);
        }
    }
}
