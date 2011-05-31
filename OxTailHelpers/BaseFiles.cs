using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public class BaseFiles
    {
        public int ID { get; set; }
        public string Filename { get; set; }

        public BaseFiles()
            : this(string.Empty)
        {
        }

        public BaseFiles(string filename)
        {
            this.Filename = filename;
        }
    }
}
