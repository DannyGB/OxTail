using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Helpers;
using Ninject.Activation;
using Ninject.Parameters;
using OxTailHelpers;

namespace OxTail
{
    public class FileFactory : IFileFactory
    {
        private readonly Ninject.IKernel Kernel;

        public FileFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IFile CreateFile(int id, string filename, string fileType)
        {
            IRequest req = Kernel.CreateRequest(typeof(IFile), null, new IParameter[] {new Parameter("id", id, false), new Parameter("filename", filename, false) }, false, false);

            if (!Kernel.CanResolve(req))
            {
                Kernel.Bind<IFile>().To<LastOpenFiles>().Named("LastOpenedFile").WithConstructorArgument("id", id).WithConstructorArgument("filename", filename);
                Kernel.Bind<IFile>().To<File>().Named("MostRecentFile").WithConstructorArgument("id", id).WithConstructorArgument("filename", filename);
            }

            IFile file = Kernel.Get<IFile>(fileType);
            Kernel.Unbind<IFile>();

            return file;
        }
    }
}
