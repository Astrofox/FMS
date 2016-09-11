using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Manager_System
{
    public partial class Form5 : Form
    {
        public bool Encrypt;
        private string Key, Vector;
        public List<My_Entry> Objects;

        public Form5()
        {
            InitializeComponent();
            textBox1.Text = "95 180 217 104 49 201 75 240 43 34 121 62 170 74 227 224 35 165 142 215 138 141 194 203";
            textBox2.Text = "39 106 95 166 97 208 46 207";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Key = textBox1.Text;
            Vector = textBox2.Text;
            My_Container Crypt_Container = new My_Container(Objects);
            if (Encrypt)
            {
                Crypt_Visitor Crypter = new Crypt_Visitor(Key, Vector);
                Crypt_Container.Accept(Crypter);
            }
            else
            {
                Decrypt_Visitor Decrypter = new Decrypt_Visitor(Key, Vector);
                Crypt_Container.Accept(Decrypter);
            }
            this.Close();
        }
    }
}
