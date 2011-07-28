using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTailHelpers;
using OxTail.Controls;

namespace OxTail
{
    public class SaveExpressionMessageWindowFactory : ISaveExpressionMessageWindowFactory
    {
        private readonly Ninject.IKernel Kernel;

        public SaveExpressionMessageWindowFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public OxTailHelpers.ISaveExpressionMessage CreateWindow()
        {
            return Kernel.Get<ISaveExpressionMessage>();
        }
    }
}
