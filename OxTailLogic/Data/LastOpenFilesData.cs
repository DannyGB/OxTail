using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers.Data;
using OxTail.Data.SQLite;

namespace OxTailLogic.Data
{
    public class LastOpenFilesData : ILastOpenFilesData
    {
        private ILastOpenFilesData LastOpenFiles { get; set; }

        public LastOpenFilesData()
        {
            this.LastOpenFiles = new LastOpenFilesDataHelper();
        }

        public List<OxTailHelpers.LastOpenFiles> Read()
        {
            return this.LastOpenFiles.Read();
        }

        public List<OxTailHelpers.LastOpenFiles> Write(List<OxTailHelpers.LastOpenFiles> files)
        {
            return this.LastOpenFiles.Write(files);
        }

        public void Clear()
        {
            this.LastOpenFiles.Clear();
        }
    }
}
