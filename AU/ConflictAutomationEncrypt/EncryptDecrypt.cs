namespace ConflictAutomationEncrypt
{
    public partial class EncryptDecrypt : Form
    {
        public EncryptDecrypt()
        {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            var text = txtDecrypted.Text.Trim();
            txtEncrypted.Text = Cryptography.Encrypt3DES(text);
            txtDecrypted.Text = "";
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            var text = txtEncrypted.Text.Trim();
            txtDecrypted.Text = Cryptography.Decrypt3DES(text);
            txtEncrypted.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtKey.Text = Cryptography.GetEncryptionKey();
        }
    }
}
