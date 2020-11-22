using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TGtime
{
    public partial class Settings : Form
    {
        Form1 main;
        public Settings(Form1 main)
        {
            this.main = main;
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            main.Open();
            textBox1.Text = Properties.Settings.Default.token;
            comboBox1.SelectedIndex = Properties.Settings.Default.timezone;

            checkBox1.Checked = Properties.Proxy.Default.UseProxy;
            textBox2.Text = Properties.Proxy.Default.port.ToString();
            textBox3.Text = Properties.Proxy.Default.ip;
            textBox4.Text = Properties.Settings.Default.group;
            textBox2.Enabled = textBox3.Enabled = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int num;
            if (textBox1.Text.Length < 40 || !textBox1.Text.Contains(':') || !Int32.TryParse(textBox1.Text.Substring(0, textBox1.Text.IndexOf(':')), out num) || textBox1.Text.Contains(' '))
                MessageBox.Show("Ошибка формата API токена");
            else
            {
                Properties.Settings.Default.token = textBox1.Text;
                Properties.Settings.Default.timezone = comboBox1.SelectedIndex;
                Properties.Settings.Default.group = textBox4.Text;
                Properties.Settings.Default.Save();

                if (checkBox1.Checked)
                {
                    string mask = @"^(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})$";
                    var regex = new Regex(mask);
                    if (!regex.IsMatch(textBox3.Text))
                        MessageBox.Show("Ошибка формата IP адреса");
                    else
                    {
                        if (!Int32.TryParse(textBox2.Text, out num))
                            MessageBox.Show("Ошибка формата порта");
                        else
                        {
                            Properties.Proxy.Default.UseProxy = checkBox1.Checked;
                            Properties.Proxy.Default.ip = textBox3.Text;
                            Properties.Proxy.Default.port = Convert.ToInt32(textBox2.Text);
                            Properties.Proxy.Default.Save();
                            MessageBox.Show("Настройки успешно сохранены!");
                        }
                    }

                }
                else
                    Properties.Proxy.Default.UseProxy = checkBox1.Checked; Properties.Proxy.Default.Save(); MessageBox.Show("Настройки успешно сохранены!");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = textBox3.Enabled = checkBox1.Checked;
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            main.Reload();
        }
    }
}
