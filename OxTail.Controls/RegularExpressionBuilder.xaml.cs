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
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Text;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Collections;
    using System.IO;

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
                this.textBoxExpression.Text += ((SpecialButton)sender).HeldTextValue;
            }
        }

        private void comboBoxSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.textBoxExpression.Text += ((ComboBoxItem)e.AddedItems[0]).Content;
        }

        private void comboBoxSavedExpressions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.textBoxExpression == null)
            {
                return;
            }

            this.textBoxExpression.Expression = ((Expression)e.AddedItems[0]);
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.SaveExpressions();

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
            msg.labelMessage.Content = "Enter Expression Name:";
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.ShowDialog();

            if (msg.DialogResult.HasValue && msg.DialogResult.Value)
            {                
                ((List<Expression>)this.comboBoxSavedExpressions.DataContext).Add(CreateExpression(this.textBoxExpression.Text, msg.Message));
            }            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Expression> expr = new List<Expression>();

            if (!File.Exists(FILENAME))
            {
                expr.Add(this.CreateExpression(string.Empty, "Choose Item:"));
                expr.Add(this.CreateExpression(@"^([a-zA-Z0-9_\-\.]+)@(([a-zA-Z0-9\-]+\.)+)([a-zA-Z]{2,4})$", "Email"));
                expr.Add(this.CreateExpression(@"^([a-zA-Z]{1,2}\w{1,2})+(\d{1}[a-zA-Z]{2})+$", "Postcode"));
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Expression>));

                FileStream s = new System.IO.FileStream(FILENAME, FileMode.Open);
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    expr = (List<Expression>)serializer.Deserialize(reader);
                }
            }

            this.comboBoxSavedExpressions.DataContext = expr;
        }

        private void SaveExpressions()
        {
            List<Expression> list = (List<Expression>)this.comboBoxSavedExpressions.DataContext;         

            XmlSerializer serializer = new XmlSerializer(typeof(List<Expression>));
            using (XmlTextWriter writer = new XmlTextWriter(FILENAME, Encoding.UTF8))
            {
                serializer.Serialize(writer, list);
            }
        }

        private Expression CreateExpression(string text, string content)
        {
            Expression item = new Expression(text, content);           
            return item;
        }
    }
}
