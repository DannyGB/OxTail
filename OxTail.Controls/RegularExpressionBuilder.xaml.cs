/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using OxTail.Helpers;
    using OxTailHelpers;
    using OxTailHelpers.Data;
    using OxTailLogic.PatternMatching;

    /// <summary>
    /// Interaction logic for RegularExpressionBuilder.xaml
    /// </summary>
    public partial class RegularExpressionBuilder : UserControl, IRegularExpressionBuilder
    {
        /// <summary>
        /// Ok button clicked
        /// </summary>
        public event RoutedEventHandler OkClick;

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        public event RoutedEventHandler CancelClick;

        private readonly ISavedExpressionsData Data;
        private readonly IExpressionFactory ExpressionFactory;
        private readonly ISaveExpressionMessageWindowFactory SaveExpressionMessageWindowFactory;
        private readonly IStringPatternMatching StringPatternMatching;

        /// <summary>
        /// Initialises instance
        /// </summary>
        public RegularExpressionBuilder(ISavedExpressionsData data, IExpressionFactory expressionFactory, 
            ISaveExpressionMessageWindowFactory saveExpressionMessageWindowFactory, IStringPatternMatching stringPatternMatching)
        {            
            InitializeComponent();
            this.Data = data;
            ExpressionFactory = expressionFactory;
            this.SaveExpressionMessageWindowFactory = saveExpressionMessageWindowFactory;
            this.StringPatternMatching = stringPatternMatching;
        }

        /// <summary>
        /// The entered regular expression
        /// </summary>
        public IExpression Expression
        {
            get
            {
                if (this.textBoxExpression.Expression == null)
                {
                    return ExpressionFactory.CreateFile(0, this.textBoxExpression.Text, Constants.UNAMED);
                }
                else
                {
                    return this.textBoxExpression.Expression;
                }
            }
        }

        private void SpecialButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender.GetType() == typeof(SpecialButton))
            {
                this.InsertIntoExpression(((SpecialButton)sender).HeldTextValue);
            }
        }

        private void comboBoxSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.InsertIntoExpression(((ComboBoxItem)e.AddedItems[0]).Content.ToString());
        }

        private void comboBoxSavedExpressions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.textBoxExpression == null)
            {
                return;
            }

            if (e.AddedItems.Count <= 0)
            {
                this.textBoxExpression.Expression = ExpressionFactory.CreateFile(0, string.Empty, string.Empty);
            }
            else if (((IExpression)e.AddedItems[0]).Name != LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.CHOOSE_ITEM))
            {
                this.textBoxExpression.Expression = ((IExpression)e.AddedItems[0]);
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {            
            if (this.OkClick != null)
            {                
                this.OkClick(this, e);
            }
        }      

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.CancelClick != null)
            {
                this.CancelClick(this, e);
            }
        }

        private void buttonSaveExpression_Click(object sender, RoutedEventArgs e)
        {
            ISaveExpressionMessage msg = this.SaveExpressionMessageWindowFactory.CreateWindow();
            msg.Label = LanguageHelper.GetLocalisedText((Application.Current as IApplication), Constants.ENTER_EXPRESSION_NAME);
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.ShowDialog();

            if (msg.DialogResult.HasValue && msg.DialogResult.Value)
            {
                ((ObservableCollection<IExpression>)this.comboBoxSavedExpressions.DataContext).Add(CreateExpression(this.textBoxExpression.Text, msg.Message));
            }

            this.SaveExpressions();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<IExpression> exprs = Data.Read(new ObservableCollection<IExpression>());

            if (exprs == null || exprs.Count <= 0)
            {
                exprs.Add(this.ExpressionFactory.CreateFile(0, "", "Choose Item"));
                exprs.Add(this.ExpressionFactory.CreateFile(0, @"^([a-zA-Z0-9_\-\.]+)@(([a-zA-Z0-9\-]+\.)+)([a-zA-Z]{2,4})$", "Email"));
                exprs.Add(this.ExpressionFactory.CreateFile(0, @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", "IP Addresses"));
                exprs.Add(this.ExpressionFactory.CreateFile(0, @"([a-zA-Z]{1,2}\w{1,2})+(\d{1}[a-zA-Z]{2})+", "Postcodes"));
                exprs.Add(this.ExpressionFactory.CreateFile(0, @"([a-zA-Z0-9_\-\.]+)@(([a-zA-Z0-9\-]+\.)+)([a-zA-Z]{2,4})", "Email_Anywhere_On_Line"));
            }

            this.comboBoxSavedExpressions.DataContext = exprs;
            this.FillExampleRegExData();
            this.textBoxExpression.Focus();
        }

        private void FillExampleRegExData()
        {
            Stream s = FileHelper.GetResourceStream(Assembly.GetExecutingAssembly(), Constants.EXAMPLE_REGEX_DATA_FILENAME);

            if (s != null)
            {
                this.textBoxTextInput.Text = FileHelper.GetStringFromStream(s);
            } 
        }

        private void SaveExpressions()
        {
            ObservableCollection<IExpression> list = (ObservableCollection<IExpression>)this.comboBoxSavedExpressions.DataContext;
            this.comboBoxSavedExpressions.DataContext = Data.Write(list);
        }

        private IExpression CreateExpression(string text, string content)
        {
            IExpression item = this.ExpressionFactory.CreateFile(0, text, content);           
            return item;
        }

        private void buttonExactNumberOfMatches_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(SpecialButton))
            {
                this.InsertIntoExpression(((SpecialButton)sender).HeldTextValue.Replace("n", textBoxN.Text));
            }
        }

        private void buttonAtLeastNumberOfMatches_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(SpecialButton))
            {
                this.InsertIntoExpression(((SpecialButton)sender).HeldTextValue.Replace("n", textBoxN.Text));
            }
        }

        private void buttonBetweenNandMMatches_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(SpecialButton))
            {
                this.InsertIntoExpression(((SpecialButton)sender).HeldTextValue.Replace("n", textBoxN.Text).Replace("m", textBoxM.Text));
            }
        }

        private void buttonDeleteExpression_Click(object sender, RoutedEventArgs e)
        {
            IExpression expr = (OxTailHelpers.Expression)this.comboBoxSavedExpressions.SelectedItem;
            if (expr.Name != "Choose Item:")
            {
                ((ObservableCollection<IExpression>)this.comboBoxSavedExpressions.DataContext).Remove(expr);
            }
            
            this.SaveExpressions();
        }

        private void InsertIntoExpression(string text)
        {
            int caretIndexHolder = this.textBoxExpression.CaretIndex;
            this.textBoxExpression.Text = this.textBoxExpression.Text.Insert(caretIndexHolder, text);
            this.textBoxExpression.CaretIndex = caretIndexHolder;
            this.textBoxExpression.Focus();
        }

        private void buttonFindMatches_Click(object sender, RoutedEventArgs e)
        {
            this.textBoxFoundMatches.Text = string.Empty;
            MatchCollection coll = this.StringPatternMatching.MatchPattern(this.textBoxTextInput.Text, new StringBuilder(this.textBoxExpression.Text));

            foreach (Match m in coll)
            {
		        this.textBoxFoundMatches.Text += m.Value + Environment.NewLine;

            }
        }
    }
}
