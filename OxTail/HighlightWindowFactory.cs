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
            Kernel.Bind<IHighlightWindow>().To<Highlight>().Named("Highlight");
            IHighlightWindow window = Kernel.Get<IHighlightWindow>("Highlight");
            Kernel.Unbind<IHighlightWindow>();

            return window;
        }
    }
}
