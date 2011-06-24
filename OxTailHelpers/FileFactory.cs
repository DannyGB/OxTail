using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Helpers;

namespace OxTailHelpers
{
    public class FileFactory : IFileFactory
    {
        public IKernel Kernel;

        public IFile CreateFile(int id, string filename, string fileType)
        {
            Kernel = new StandardKernel();
            Kernel.Bind<IFile>().To<LastOpenFiles>().Named("LastOpenedFile").WithConstructorArgument("id", id).WithConstructorArgument("filename", filename);
            Kernel.Bind<IFile>().To<File>().Named("MostRecentFile").WithConstructorArgument("id", id).WithConstructorArgument("filename", filename);

            return Kernel.Get<IFile>(fileType);
        }
    }
}
