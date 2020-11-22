using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using Telegram.Bot;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace TGtime
{
    public partial class Form1 : Form
    {
        public static bool isSettings = false;
        public static BackgroundWorker bw;
        public static TelegramBotClient Bot;
        Settings settings;
        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        List<Attachment> Attachments = new List<Attachment>();

        public void Open()
        {
            isSettings = true;
        }

        public void Reload()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.token) && !string.IsNullOrEmpty(Properties.Settings.Default.group))
            {
                if (Properties.Proxy.Default.UseProxy)
                {
                    var httpProxy = new WebProxy($"{Properties.Proxy.Default.ip}:{Properties.Proxy.Default.port}");
                    Bot = new TelegramBotClient(Properties.Settings.Default.token, httpProxy);
                }
                Bot = new TelegramBotClient(Properties.Settings.Default.token);
                bw = new BackgroundWorker();
                bw.DoWork += bw_DoWork;
                bw.RunWorkerAsync(Properties.Settings.Default.token);
            }
            isSettings = false;
        }

        public Form1()
        {
            settings = new Settings(this);
            Reload();
            InitializeComponent();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
           settings.ShowDialog();
        }

        private void proxyСерверToolStripMenuItem_Click(object sender, EventArgs e)
        { 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(appdata + @"\TG Time"))
                Directory.CreateDirectory(appdata + @"\TG Time");

            if (!File.Exists(appdata + @"\TG Time\data.json"))
            {
                File.Create(appdata + @"\TG Time\data.json").Dispose();
                File.WriteAllText(appdata + @"\TG Time\data.json", "{\"posts\":[]}");
            }
        }

        void RefreshDataGridView()
        {
            dataGridView2.Rows.Clear();
            Posts posts = JsonConvert.DeserializeObject<Posts>(File.ReadAllText(appdata + @"\TG Time\data.json"));
            foreach (Post post in posts.posts)
            {
                dataGridView2.Rows.Add(post.Id, post.Message, string.Join(", ", post.Attachments.Select(p => p.Path)), post.Date);
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
                RefreshDataGridView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Multiselect = true;
                openDialog.Filter = "Изображения|*.jpg; *.jpeg; *.png" + "|Все файлы|*.*";
                if (openDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (openDialog.FileName == String.Empty)
                        return;
                    label4.Visible = true;
                    label4.Text = "";
                    foreach (string file in openDialog.FileNames)
                    {
                        Attachment attachment = new Attachment();
                        attachment.Path = file;
                        Attachments.Add(attachment);
                        label4.Text += file.Substring(file.LastIndexOf("\\") + 1) + ", ";
                    }
                    label4.Text = label4.Text.Substring(0, label4.Text.Length - 2);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dateTimePicker1.Value == DateTime.Now || DateTime.Now.Ticks > dateTimePicker1.Value.Ticks)
                MessageBox.Show("Время публикации не может быть раньше текущего времени!", "Ошибка");
            else
            {
                Posts posts = JsonConvert.DeserializeObject<Posts>(File.ReadAllText(appdata + @"\TG Time\data.json"));
                Post post = new Post();
                post.Id = posts.posts.Length + 1;
                post.Message = richTextBox1.Text;
                post.Date = dateTimePicker1.Value.ToString();
                post.Attachments = Attachments.ToArray();
                var list = new List<Post>(posts.posts);
                list.Add(post);
                posts.posts = list.ToArray();
                File.WriteAllText(appdata + @"\TG Time\data.json", JsonConvert.SerializeObject(posts));
                Attachments.Clear();
                MessageBox.Show("Успешно");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.SelectedRows.Count; i++)
            {
                dataGridView2.Rows.Remove(dataGridView2.SelectedRows[i]);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var list = new List<Post>();
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                Post post = new Post();
                post.Id = Convert.ToInt32(dataGridView2.Rows[i].Cells[0].Value);
                post.Message = Convert.ToString(dataGridView2.Rows[i].Cells[1].Value);
                post.Date = Convert.ToString(dataGridView2.Rows[i].Cells[3].Value);
                var attachments = Convert.ToString(dataGridView2.Rows[i].Cells[2].Value).Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var _list = new List<Attachment>();
                foreach (string attachment in attachments)
                {
                    Attachment attach = new Attachment();
                    attach.Path = attachment;
                    _list.Add(attach);
                }
                post.Attachments = _list.ToArray();
                list.Add(post);
            }
            Posts posts = new Posts();
            posts.posts = list.ToArray();
            File.WriteAllText(appdata + @"\TG Time\data.json", JsonConvert.SerializeObject(posts));
            RefreshDataGridView();
        }

        static List<int> sent = new List<int>();

        async public static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {
                await Bot.SetWebhookAsync("");
                while (true)
                {
                    if (isSettings)
                        continue;
                    try
                    {
                        Posts posts = JsonConvert.DeserializeObject<Posts>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TG Time\data.json"));
                        foreach (Post post in posts.posts)
                        {
                            var datetime = DateTime.Parse(post.Date);
                            DateTime now = new DateTime();
                            if (Properties.Settings.Default.timezone == 0)
                                now = DateTime.Now;
                            else
                                now = DateTime.UtcNow.AddHours(Properties.Settings.Default.timezone - 12);

                            if (Math.Abs((datetime - now).TotalSeconds) <= 1 && !sent.Contains(post.Id))
                            {
                                if (post.Attachments.Length != 0)
                                {
                                    switch (post.Attachments[0].Path.Substring(post.Attachments[0].Path.LastIndexOf(".") + 1).ToLower())
                                    {
                                        case "bmp":
                                        case "jpg":
                                        case "png":
                                        case "jpeg":
                                            await Bot.SendPhotoAsync(chatId: Properties.Settings.Default.group,
                                                    caption: post.Message,
                                                    photo: File.Open(post.Attachments[0].Path, FileMode.Open));
                                            break;
                                        case "gif":
                                            await Bot.SendAnimationAsync(chatId: Properties.Settings.Default.group,
                                                    caption: post.Message,
                                                    animation: File.Open(post.Attachments[0].Path, FileMode.Open));
                                            break;
                                        case "avi":
                                        case "wmv":
                                        case "mov":
                                        case "mkv":
                                        case "3gp":
                                            await Bot.SendVideoAsync(chatId: Properties.Settings.Default.group,
                                                    caption: post.Message,
                                                    video: File.Open(post.Attachments[0].Path, FileMode.Open));
                                            break;
                                        case "wav":
                                        case "aiff":
                                        case "mp3":
                                        case "aac":
                                        case "ogg":
                                        case "wma":
                                        case "flac":
                                            await Bot.SendAudioAsync(chatId: Properties.Settings.Default.group,
                                                    caption: post.Message,
                                                    audio: File.Open(post.Attachments[0].Path, FileMode.Open));
                                            break;
                                        default:
                                            await Bot.SendDocumentAsync(chatId: Properties.Settings.Default.group,
                                                    caption: post.Message,
                                                    document: File.Open(post.Attachments[0].Path, FileMode.Open));
                                            break;
                                    }
                                    
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(chatId: Properties.Settings.Default.group,
                                                    text: post.Message);
                                }
                                sent.Add(post.Id);
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(10);
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void авторToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Irval1337/TGTime");
        }
    }
}
