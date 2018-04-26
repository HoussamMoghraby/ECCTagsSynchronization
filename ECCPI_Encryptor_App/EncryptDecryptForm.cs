using ECC_DataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ECCPI_Encryptor_App
{
    public partial class EncryptDecryptForm : Form
    {
        public EncryptDecryptForm()
        {
            InitializeComponent();
        }

        private void buttonEncypt_Click(object sender, EventArgs e)
        {
            try
            {
                string textToEncrypt = inputTextBox.Text;
                if (!string.IsNullOrEmpty(textToEncrypt))
                {
                    string textEncrypted = CryptoProvider.Encrypt_Aes_String(textToEncrypt);
                    outputTextBox.Text = textEncrypted;
                }
            }
            catch (Exception)
            {
                var f = new DialogForm("Text is too short for encryption!");
                f.ShowDialog();
            }
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string textToDecrypt = inputTextBox.Text;
                if (!string.IsNullOrEmpty(textToDecrypt))
                {
                    string textDecrypted = CryptoProvider.Decrypt_Aes(textToDecrypt);
                    outputTextBox.Text = textDecrypted;
                }
            }
            catch (Exception)
            {
                var f = new DialogForm("Input text must be and encrypted text in order to decrypt it!");
                f.ShowDialog();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
