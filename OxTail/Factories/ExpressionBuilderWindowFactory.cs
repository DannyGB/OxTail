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
        private readonly Ninject.IKernel Kernel;

        public ExpressionBuilderWindowFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IExpressionBuilderWindow CreateWindow()
        {
            return Kernel.Get<IExpressionBuilderWindow>("ExpressionBuilder");
        }
    }
}
