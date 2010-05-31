using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace OxTail.Controls
{
    public class ExpressionTextBox : TextBox
    {
        private Expression _expression;

        public Expression Expression 
        {
            get
            {
                return this._expression;
            }
            set
            {
                this._expression = value;
                this.Text = value.Text;
            }
        }
    }
}
