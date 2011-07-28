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
        private readonly Ninject.IKernel Kernel;

        public FindWindowFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IFindWindow CreateWindow()
        {
            return Kernel.Get<IFindWindow>("Find");
        }
    }
}
