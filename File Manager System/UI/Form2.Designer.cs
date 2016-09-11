namespace File_Manager_System
{
    partial class Form2
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
            this.Path1 = new System.Windows.Forms.TextBox();
            this.Path2 = new System.Windows.Forms.TextBox();
            this.Change1 = new System.Windows.Forms.Button();
            this.Change2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.Log_box = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // Path1
            // 
            this.Path1.Location = new System.Drawing.Point(63, 23);
            this.Path1.Name = "Path1";
            this.Path1.ReadOnly = true;
            this.Path1.Size = new System.Drawing.Size(546, 20);
            this.Path1.TabIndex = 0;
            // 
            // Path2
            // 
            this.Path2.Location = new System.Drawing.Point(63, 67);
            this.Path2.Name = "Path2";
            this.Path2.ReadOnly = true;
            this.Path2.Size = new System.Drawing.Size(546, 20);
            this.Path2.TabIndex = 1;
            // 
            // Change1
            // 
            this.Change1.Location = new System.Drawing.Point(615, 23);
            this.Change1.Name = "Change1";
            this.Change1.Size = new System.Drawing.Size(75, 20);
            this.Change1.TabIndex = 2;
            this.Change1.Text = "Change";
            this.Change1.UseVisualStyleBackColor = true;
            this.Change1.Click += new System.EventHandler(this.Change1_Click);
            // 
            // Change2
            // 
            this.Change2.Location = new System.Drawing.Point(615, 67);
            this.Change2.Name = "Change2";
            this.Change2.Size = new System.Drawing.Size(75, 20);
            this.Change2.TabIndex = 3;
            this.Change2.Text = "Change";
            this.Change2.UseVisualStyleBackColor = true;
            this.Change2.Click += new System.EventHandler(this.Change2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(63, 104);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 50);
            this.button1.TabIndex = 4;
            this.button1.Text = "Syncronize";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Log_box
            // 
            this.Log_box.Location = new System.Drawing.Point(198, 104);
            this.Log_box.Name = "Log_box";
            this.Log_box.ReadOnly = true;
            this.Log_box.Size = new System.Drawing.Size(492, 96);
            this.Log_box.TabIndex = 5;
            this.Log_box.Text = "";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(747, 214);
            this.Controls.Add(this.Log_box);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Change2);
            this.Controls.Add(this.Change1);
            this.Controls.Add(this.Path2);
            this.Controls.Add(this.Path1);
            this.Name = "Form2";
            this.Text = "Syncronization";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Path1;
        private System.Windows.Forms.TextBox Path2;
        private System.Windows.Forms.Button Change1;
        private System.Windows.Forms.Button Change2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox Log_box;
    }
}