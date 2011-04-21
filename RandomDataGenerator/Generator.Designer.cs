namespace RandomDataGenerator
{
    partial class Generator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            this.buttonFileOpen = new System.Windows.Forms.Button();
            this.checkBoxEmptyFileOnStart = new System.Windows.Forms.CheckBox();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFileName.Location = new System.Drawing.Point(13, 13);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.Size = new System.Drawing.Size(271, 20);
            this.textBoxFileName.TabIndex = 0;
            this.textBoxFileName.Text = "D:\\My Dropbox\\oxtail\\Test Files\\randomdata_3.txt";
            // 
            // buttonFileOpen
            // 
            this.buttonFileOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFileOpen.Location = new System.Drawing.Point(290, 13);
            this.buttonFileOpen.Name = "buttonFileOpen";
            this.buttonFileOpen.Size = new System.Drawing.Size(27, 23);
            this.buttonFileOpen.TabIndex = 1;
            this.buttonFileOpen.Text = "...";
            this.buttonFileOpen.UseVisualStyleBackColor = true;
            this.buttonFileOpen.Click += new System.EventHandler(this.buttonFileOpen_Click);
            // 
            // checkBoxEmptyFileOnStart
            // 
            this.checkBoxEmptyFileOnStart.AutoSize = true;
            this.checkBoxEmptyFileOnStart.Location = new System.Drawing.Point(13, 39);
            this.checkBoxEmptyFileOnStart.Name = "checkBoxEmptyFileOnStart";
            this.checkBoxEmptyFileOnStart.Size = new System.Drawing.Size(109, 17);
            this.checkBoxEmptyFileOnStart.TabIndex = 2;
            this.checkBoxEmptyFileOnStart.Text = "Empty file on start";
            this.checkBoxEmptyFileOnStart.UseVisualStyleBackColor = true;
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(125, 67);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(75, 23);
            this.buttonPause.TabIndex = 3;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonContinue
            // 
            this.buttonContinue.Location = new System.Drawing.Point(218, 67);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(75, 23);
            this.buttonContinue.TabIndex = 4;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(33, 66);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // Generator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 111);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.checkBoxEmptyFileOnStart);
            this.Controls.Add(this.buttonFileOpen);
            this.Controls.Add(this.textBoxFileName);
            this.Name = "Generator";
            this.Text = "Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.Button buttonFileOpen;
        private System.Windows.Forms.CheckBox checkBoxEmptyFileOnStart;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonStart;
    }
}