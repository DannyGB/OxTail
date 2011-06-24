using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;
using Ninject;

namespace OxTail
{
    public class ExpressionBuilderWindowFactory : IExpressionBuilderWindowFactory
    {
        private static IKernel Kernel;

        public IExpressionBuilderWindow CreateWindow()
        {
            Kernel = new StandardKernel();
            Kernel.Bind<IExpressionBuilderWindow>().To<ExpressionBuilderDITest>().Named("ExpressionBuilder");
            //Kernel.Bind<IExpressionBuilderWindow>().To<ExpressionBuilder>().Named("ExpressionBuilder");

            return Kernel.Get<IExpressionBuilderWindow>("ExpressionBuilder");
        }
    }
}
