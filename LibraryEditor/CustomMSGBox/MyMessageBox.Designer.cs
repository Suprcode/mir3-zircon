namespace ZirconMessageBox
{
    partial class ZirconMessageBox
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
            btn1 = new System.Windows.Forms.Button();
            lblTimer = new System.Windows.Forms.Label();
            btn2 = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            lblMessage = new System.Windows.Forms.Label();
            lblTitle = new System.Windows.Forms.Label();
            btn3 = new System.Windows.Forms.Button();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // btn1
            // 
            btn1.Location = new System.Drawing.Point(412, 121);
            btn1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn1.Name = "btn1";
            btn1.Size = new System.Drawing.Size(158, 29);
            btn1.TabIndex = 1;
            btn1.Text = "1";
            btn1.UseVisualStyleBackColor = true;
            btn1.Click += btnCancel_Click;
            // 
            // lblTimer
            // 
            lblTimer.AutoSize = true;
            lblTimer.BackColor = System.Drawing.Color.Transparent;
            lblTimer.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTimer.Location = new System.Drawing.Point(10, 138);
            lblTimer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new System.Drawing.Size(0, 16);
            lblTimer.TabIndex = 4;
            // 
            // btn2
            // 
            btn2.Location = new System.Drawing.Point(247, 121);
            btn2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn2.Name = "btn2";
            btn2.Size = new System.Drawing.Size(158, 29);
            btn2.TabIndex = 5;
            btn2.Text = "2";
            btn2.UseVisualStyleBackColor = true;
            btn2.Click += btnOK_Click;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.BackColor = System.Drawing.Color.Transparent;
            panel1.Controls.Add(lblMessage);
            panel1.Location = new System.Drawing.Point(14, 46);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(555, 67);
            panel1.TabIndex = 6;
            // 
            // lblMessage
            // 
            lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            lblMessage.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMessage.Location = new System.Drawing.Point(0, 0);
            lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new System.Drawing.Size(555, 67);
            lblMessage.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Location = new System.Drawing.Point(14, 6);
            lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(0, 13);
            lblTitle.TabIndex = 8;
            // 
            // btn3
            // 
            btn3.Location = new System.Drawing.Point(83, 121);
            btn3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn3.Name = "btn3";
            btn3.Size = new System.Drawing.Size(158, 29);
            btn3.TabIndex = 9;
            btn3.Text = "3";
            btn3.UseVisualStyleBackColor = true;
            btn3.Click += button1_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = System.Drawing.Color.Transparent;
            pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            pictureBox2.Image = Library_Editor.Properties.Resources.close_btn;
            pictureBox2.Location = new System.Drawing.Point(559, 3);
            pictureBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(22, 21);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 10;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Library_Editor.Properties.Resources.title_bar;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(586, 29);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // ZirconMessageBox
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            ClientSize = new System.Drawing.Size(583, 164);
            ControlBox = false;
            Controls.Add(pictureBox2);
            Controls.Add(btn3);
            Controls.Add(lblTitle);
            Controls.Add(pictureBox1);
            Controls.Add(panel1);
            Controls.Add(btn2);
            Controls.Add(lblTimer);
            Controls.Add(btn1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ZirconMessageBox";
            Opacity = 0.8D;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "MyMessageBox";
            Load += MyMessageBox_Load;
            Paint += MyMessageBox_Paint;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblTitle;
        internal System.Windows.Forms.Button btn1;
        internal System.Windows.Forms.Button btn2;
        internal System.Windows.Forms.Button btn3;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}