using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;
using Ninject;
using OxTailLogic;
using OxTailHelpers.Data;
using OxTailLogic.Data;

namespace OxTail
{
    public class WindowFactory : IWindowFactory
    {
        private readonly Ninject.IKernel Kernel;

        public WindowFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IWindow CreateWindow(string window)
        {
            return Kernel.Get<IWindow>(window);
        }
    }
}
