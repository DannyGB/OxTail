/*****************************************************************
* This file is part of OxTail.
*
* OxTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OxTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

namespace OxTail.Controls
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml.Serialization;
    using OxTail.Helpers;
    using OxTailLogic.PatternMatching;
    using System.Text;
    using System.Text.RegularExpressions;
    using System;
    using System.Reflection;
    using OxTailHelpers;

    /// <summary>
    /// Interaction logic for RegularExpressionBuilder.xaml
    /// </summary>
    public partial class RegularExpressionBuilder : UserControl
    {
        public event RoutedEventHandler OkClick;
        public event RoutedEventHandler CancelClick;
        public const string FILENAME = "SavedExpression.xml";

        public RegularExpressionBuilder()
        {
            InitializeComponent();
        }

        public Expression Expression
        {
            get
            {
                if (this.textBoxExpression.Expression == null)
                {
                    return new Expression(this.textBoxExpression.Text, "UN-NAMED");
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
                this.textBoxExpression.Expression = new Expression();
            }
            else if (((Expression)e.AddedItems[0]).Name != LanguageHelper.GetLocalisedText((Application.Current as IApplication), "chooseItem"))
            {
                this.textBoxExpression.Expression = ((Expression)e.AddedItems[0]);
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
            SaveExpressionMessage msg = new SaveExpressionMessage();
            msg.labelMessage.Content = LanguageHelper.GetLocalisedText((Application.Current as IApplication), "enterExpressionName");
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.ShowDialog();

            if (msg.DialogResult.HasValue && msg.DialogResult.Value)
            {
                ((ObservableCollection<Expression>)this.comboBoxSavedExpressions.DataContext).Add(CreateExpression(this.textBoxExpression.Text, msg.Message));
            }

            this.SaveExpressions();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Expression> expr = new ObservableCollection<Expression>();

            if (!File.Exists(FILENAME))
            {
                expr.Add(this.CreateExpression(string.Empty, LanguageHelper.GetLocalisedText((Application.Current as IApplication), "chooseItem")));
                expr.Add(this.CreateExpression(@"^([a-zA-Z0-9_\-\.]+)@(([a-zA-Z0-9\-]+\.)+)([a-zA-Z]{2,4})$", "Email"));
                expr.Add(this.CreateExpression(@"^([a-zA-Z]{1,2}\w{1,2})+(\d{1}[a-zA-Z]{2})+$", "Postcode"));
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Expression>));
                expr = (ObservableCollection<Expression>)FileHelper.DeserializeFromExecutableDirectory(FILENAME, serializer);
            }

            this.comboBoxSavedExpressions.DataContext = expr;
            this.FillExampleRegExData();
            this.textBoxExpression.Focus();
        }

        private void FillExampleRegExData()
        {
            Stream s = FileHelper.GetResourceStream(Assembly.GetExecutingAssembly(), "OxTail.Controls.ExampleRegexData.txt");

            if (s != null)
            {
                this.textBoxTextInput.Text = FileHelper.GetStringFromStream(s);
            } 
        }

        private void SaveExpressions()
        {
            ObservableCollection<Expression> list = (ObservableCollection<Expression>)this.comboBoxSavedExpressions.DataContext;

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Expression>));
            FileHelper.SerializeToExecutableDirectory(FILENAME, serializer, list);
        }

        private Expression CreateExpression(string text, string content)
        {
            Expression item = new Expression(text, content);           
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
            Expression expr = (Expression)this.comboBoxSavedExpressions.SelectedItem;
            if (expr.Name != "Choose Item:")
            {
                ((ObservableCollection<Expression>)this.comboBoxSavedExpressions.DataContext).Remove(expr);
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

            IStringPatternMatching patternMatching = StringPatternMatching.CreatePatternMatching();
            MatchCollection coll = patternMatching.MatchPattern(this.textBoxTextInput.Text, new StringBuilder(this.textBoxExpression.Text));

            foreach (Match m in coll)
            {
		        this.textBoxFoundMatches.Text += m.Value + Environment.NewLine;

            }
        }
    }
}
