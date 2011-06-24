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
        public IKernel Kernel;

        public OxTailHelpers.ISaveExpressionMessage CreateWindow()
        {
            Kernel = new StandardKernel();
            Kernel.Bind<ISaveExpressionMessage>().To<SaveExpressionMessage>();

            return Kernel.Get<ISaveExpressionMessage>();
        }
    }
}
