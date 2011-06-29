using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public interface IExpressionFactory
    {
        IExpression CreateFile(int id, string text, string name);
    }
}
