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
        private IKernel Kernel;

        public Controls.ITabItem CreateTabItem(string filename, HighlightCollection<HighlightItem> hightlightCollection)
        {
            Kernel = new StandardKernel();
            Kernel.Bind<ITabItem>()
                .To<FileWatcherTabItem>()
                .WithConstructorArgument("filename", filename)
                .WithConstructorArgument("patterns", hightlightCollection);

            Kernel.Bind<IFileWatcher>().To<RationalFileWatcher>();

            return Kernel.Get<ITabItem>();
        }
    }
}
