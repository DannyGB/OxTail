using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTail.Controls
{
    public class BaseMainWindow : BaseWindow
    {
        public BaseMainWindow()
        {
            this.OverrideEscapeKeyClose = true;
        }
    }
}
