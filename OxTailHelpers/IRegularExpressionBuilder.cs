using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OxTailHelpers
{
    public interface IRegularExpressionBuilder
    {
        event RoutedEventHandler OkClick;
        event RoutedEventHandler CancelClick;

        IExpression Expression { get; }
    }
}
