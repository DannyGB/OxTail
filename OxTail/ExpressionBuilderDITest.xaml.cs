using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxTail.Controls;
using OxTailHelpers;

namespace OxTail
{
    /// <summary>
    /// Interaction logic for ExpressionBuilderDITest.xaml
    /// </summary>
    public partial class ExpressionBuilderDITest : BaseWindow, IExpressionBuilderWindow
    {
        public ExpressionBuilderDITest()
        {
            InitializeComponent();
        }


        public OxTailHelpers.Expression Expression
        {
            get { return new OxTailHelpers.Expression(); }
        }
    }
}
