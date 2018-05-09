using System;
using System.Windows.Forms;

namespace ECCPI_Encryptor_App
{
    public partial class DialogForm : Form
    {
        public DialogForm()
        {
            InitializeComponent();
        }

        public DialogForm(string textToDisplay) : this()
        {
            this.labelText.Text = textToDisplay;
        }

        private void labelText_Click(object sender, EventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
