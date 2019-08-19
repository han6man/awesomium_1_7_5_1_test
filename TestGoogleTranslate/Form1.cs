using MDH.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGoogleTranslate
{
    public partial class Form1 : Form
    {
        GoogleTranslate googleTranslate;

        public Form1()
        {
            InitializeComponent();
            googleTranslate = new GoogleTranslate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = string.Empty;
            textBox2.Text = googleTranslate.Translate(textBox1.Text, "en", "ru");
            label1.Text = googleTranslate.URL.ToString();
        }
    }
}
