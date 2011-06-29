using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Controls;
using OxTailHelpers;

namespace OxTail
{
    public class TabItemFactory : ITabItemFactory
    {
        private Ninject.IKernel Kernel;

        public TabItemFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public Controls.ITabItem CreateTabItem(string filename, HighlightCollection<HighlightItem> hightlightCollection)
        {
            Kernel.Bind<ITabItem>()
                .To<FileWatcherTabItem>()
                .WithConstructorArgument("filename", filename)
                .WithConstructorArgument("patterns", hightlightCollection);            

            return Kernel.Get<ITabItem>();
        }
    }
}
