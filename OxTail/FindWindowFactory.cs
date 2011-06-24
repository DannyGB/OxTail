using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTailHelpers;

namespace OxTail
{
    public class FindWindowFactory : IFindWindowFactory
    {
        private Ninject.IKernel Kernel { get; set; }

        public IFindWindow CreateWindow()
        {
            Kernel = new StandardKernel();
            Kernel.Bind<IFindWindow>().To<Find>().Named("Find");
            Kernel.Bind<IExpressionBuilderWindowFactory>().To<ExpressionBuilderWindowFactory>();

            return Kernel.Get<IFindWindow>("Find");
        }
    }
}
