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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace RandomDataGenerator
{
    public partial class Generator : Form
    {
        private bool FirstCall { get; set; }
        private System.Timers.Timer Timer { get; set; }

        public Generator()
        {
            InitializeComponent();

            FirstCall = true;
            
            Timer = new System.Timers.Timer(10000);
            Timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                CreateRandomText createRandomText = new CreateRandomText();

                // If we're to clean the file we only want to do it on the first timer tick
                if (FirstCall)
                {
                    createRandomText.Create(this.textBoxFileName.Text, this.checkBoxEmptyFileOnStart.Checked);
                    FirstCall = false;
                }
                else
                {
                    createRandomText.Create(this.textBoxFileName.Text, false);
                }
            }
        }

        private void buttonFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            if (DialogResult.OK == diag.ShowDialog())
            {
                if (File.Exists(diag.FileName))
                {
                    this.textBoxFileName.Text = diag.FileName;
                }
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            this.Timer.Stop();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Timer.Start();
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            this.Timer.Start();
        }
    }
}
