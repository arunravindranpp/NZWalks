namespace ConflictAutomationEncrypt
{
    partial class EncryptDecrypt
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtKey = new TextBox();
            txtDecrypted = new RichTextBox();
            btnEncrypt = new Button();
            btnDecrypt = new Button();
            txtEncrypted = new RichTextBox();
            SuspendLayout();
            // 
            // txtKey
            // 
            txtKey.Location = new Point(336, 50);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(359, 31);
            txtKey.TabIndex = 0;
            // 
            // txtDecrypted
            // 
            txtDecrypted.Location = new Point(77, 171);
            txtDecrypted.Margin = new Padding(4, 5, 4, 5);
            txtDecrypted.Name = "txtDecrypted";
            txtDecrypted.Size = new Size(270, 418);
            txtDecrypted.TabIndex = 2;
            txtDecrypted.Text = "";
            // 
            // btnEncrypt
            // 
            btnEncrypt.Location = new Point(448, 225);
            btnEncrypt.Margin = new Padding(5, 6, 5, 6);
            btnEncrypt.Name = "btnEncrypt";
            btnEncrypt.Size = new Size(149, 44);
            btnEncrypt.TabIndex = 3;
            btnEncrypt.Text = "Encrypt>";
            btnEncrypt.UseVisualStyleBackColor = true;
            btnEncrypt.Click += btnEncrypt_Click;
            // 
            // btnDecrypt
            // 
            btnDecrypt.Location = new Point(448, 402);
            btnDecrypt.Margin = new Padding(4, 5, 4, 5);
            btnDecrypt.Name = "btnDecrypt";
            btnDecrypt.Size = new Size(149, 42);
            btnDecrypt.TabIndex = 4;
            btnDecrypt.Text = "<Decrypt";
            btnDecrypt.UseVisualStyleBackColor = true;
            btnDecrypt.Click += btnDecrypt_Click;
            // 
            // txtEncrypted
            // 
            txtEncrypted.Location = new Point(732, 171);
            txtEncrypted.Margin = new Padding(4, 5, 4, 5);
            txtEncrypted.Name = "txtEncrypted";
            txtEncrypted.Size = new Size(280, 418);
            txtEncrypted.TabIndex = 5;
            txtEncrypted.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1150, 648);
            Controls.Add(txtEncrypted);
            Controls.Add(btnDecrypt);
            Controls.Add(btnEncrypt);
            Controls.Add(txtDecrypted);
            Controls.Add(txtKey);
            Name = "Form1";
            Text = "ConflictAUEncryptDecrypt";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtKey;
        private RichTextBox txtDecrypted;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private RichTextBox txtEncrypted;
    }
}
