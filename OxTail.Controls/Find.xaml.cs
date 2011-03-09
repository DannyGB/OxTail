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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OxTail.Controls
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : UserControl
    {
        public delegate void FindText(object sender, FindEventArgs e);
        public event EventHandler ExpressionBuilderButtonClick;
        public event FindText FindButtonClick;

        public Find()
        {
            InitializeComponent();
        }

        public string SearchTerm
        {
            get
            {
                return this.textBoxSearchCriteria.Text;
            }

            set
            {
                this.textBoxSearchCriteria.Text = value;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void buttonExpressionBuilder_Click(object sender, RoutedEventArgs e)
        {
            if (ExpressionBuilderButtonClick != null)
            {
                ExpressionBuilderButtonClick(this, new EventArgs());
            }
        }

        private void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            CallFindButtonClickEvent();
        }

        private void CallFindButtonClickEvent()
        {
            if (FindButtonClick != null && !string.IsNullOrEmpty(this.textBoxSearchCriteria.Text))
            {
                //TODO: Options not yet implemented
                FindButtonClick(this, new FindEventArgs(this.textBoxSearchCriteria.Text, OxTailLogic.PatternMatching.FindOptions.CurrentDocument));
            }
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                CallFindButtonClickEvent();
            }
        }
    }
}
