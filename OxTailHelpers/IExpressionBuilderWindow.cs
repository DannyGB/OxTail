﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OxTailHelpers
{
    public interface IExpressionBuilderWindow : IWindow
    {
        bool? DialogResult { get; }
        IExpression Expression { get; }
    }
}
