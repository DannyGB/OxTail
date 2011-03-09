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

namespace OxTail
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : BaseWindow
    {
        public event OxTail.Controls.Find.FindText FindCriteria;

        public Find()
        {
            InitializeComponent();
        }

        private void find_ExpressionBuilderButtonClick(object sender, EventArgs e)
        {
            ExpressionBuilder builder = new ExpressionBuilder();
            builder.ShowDialog();

            if (builder.DialogResult.HasValue && builder.DialogResult.Value)
            {
                this.find.SearchTerm = builder.Expression.Text;
            }
        }

        private void find_FindButtonClick(object sender, FindEventArgs e)
        {
            if (FindCriteria != null)
            {
                FindCriteria(this, e);
            }
        }
    }
}
