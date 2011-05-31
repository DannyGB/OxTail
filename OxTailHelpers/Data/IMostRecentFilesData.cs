using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers.Data
{
    public interface IMostRecentFilesData : IData
    {
        List<OxTail.Helpers.File> Read();
        List<OxTail.Helpers.File> Write(List<OxTail.Helpers.File> files);
        void Clear();
    }
}
