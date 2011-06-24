using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public interface IFileFactory
    {
        IFile CreateFile(int id, string filename, string FileType);
    }
}
