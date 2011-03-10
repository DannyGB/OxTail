using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace OxTail.Controls
{
    public interface IColourfulItem
    {
        Color BackColour
        {
            get;
            set;
        }

        Color ForeColour
        {
            get;
            set;
        }

        Color BorderColour
        {
            get;
            set;
        }
    }
}
