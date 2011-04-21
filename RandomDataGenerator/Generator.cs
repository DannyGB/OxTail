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
