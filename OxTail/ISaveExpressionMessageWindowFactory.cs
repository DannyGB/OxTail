using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTail
{
    public interface ISaveExpressionMessageWindowFactory
    {
        ISaveExpressionMessage CreateWindow();
    }
}
