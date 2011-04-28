using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public class LastOpenFiles
    {
        public int ID { get; set; }
        public string Filename { get; set; }

        public LastOpenFiles()
            : this(string.Empty)
        {
        }

        public LastOpenFiles(string filename)
        {
            this.Filename = filename;
        }
    }
}
