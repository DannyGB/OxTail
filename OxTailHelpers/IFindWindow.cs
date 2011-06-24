using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public interface IFindWindow : IWindow
    {
        event FindText FindCriteria;
    }
}
