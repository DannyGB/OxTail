using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers.Data
{
    public interface ILastOpenFilesData : IData
    {
        List<LastOpenFiles> Read();
        List<LastOpenFiles> Write(List<LastOpenFiles> files);
        void Clear();
    }
}
