using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers
{
    public interface IExpression
    {
        int ID { get; }
        string Text { get; }
        string Name { get; }
    }
}
