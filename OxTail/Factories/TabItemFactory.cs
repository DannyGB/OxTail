using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Controls;
using OxTailHelpers;
using Ninject.Activation;
using Ninject.Parameters;
using OxTailLogic;

namespace OxTail
{
    public class TabItemFactory : ITabItemFactory
    {
        private Ninject.IKernel Kernel;

        public TabItemFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public Controls.ITabItem CreateTabItem(string filename, IHighlightsHelper hightlightsHelper)
        {
            IRequest req = Kernel.CreateRequest(typeof(ITabItem), null, new IParameter[] { new Parameter("filename", filename, false), new Parameter("hightlightsHelper", hightlightsHelper, false) }, false, false);

            if (!Kernel.CanResolve(req))
            {
                Kernel.Bind<ITabItem>()
                    .To<FileWatcherTabItem>()
                    .WithConstructorArgument("filename", filename)
                    .WithConstructorArgument("hightlightsHelper", hightlightsHelper);
            }

            ITabItem tab = Kernel.Get<ITabItem>();
            Kernel.Unbind<ITabItem>();

            return tab;
        }
    }
}
