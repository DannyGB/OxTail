using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTailHelpers;

namespace OxTail
{
    public class HighlightWindowFactory : IHighlightWindowFactory
    {
        private readonly IKernel Kernel;

        public HighlightWindowFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IWindow CreateWindow()
        {
            return Kernel.Get<IHighlightWindow>("Highlight", new Ninject.Parameters.ConstructorArgument("expressionBuilder", Kernel.Get<IExpressionBuilderWindow>()));
        }
    }
}
