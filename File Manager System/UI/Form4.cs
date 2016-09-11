using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Manager_System
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        public void Search_in_book(string bok_path)
        {
            My_File Used_File = new My_File(bok_path);
            string[] lines = Used_File.ReadAllLines();
            textBox2.Text = lines.Length.ToString();

            string text = Used_File.ReadAllText();
            string[] words = text.Split(new char[] { ' ', ',', '.', ';', ':', '-', '?', '/' }, StringSplitOptions.RemoveEmptyEntries);
            textBox1.Text = words.Length.ToString();

            string[] top_words = FindTenMostCommon(words);
            foreach (string word in top_words)
            {
                richTextBox1.Text += word;
                richTextBox1.Text += '\n';
            }
        }

        private string[] FindTenMostCommon (string[] words)
        {
            var freq_ord = from word in words
                           where word.Length > 6
                           group word by word into g
                           orderby g.Count() descending
                           select g.Key;

            string[] commonWords = (freq_ord.Take(10).ToArray());
            return commonWords;
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }
    }
}
