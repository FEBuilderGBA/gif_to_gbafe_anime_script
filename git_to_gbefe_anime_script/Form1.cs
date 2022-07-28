using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace git_to_gbefe_anime_script
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(FilenameTextBox.Text))
            {
                return;
            }
            string txtFilename = Convert(FilenameTextBox.Text);
            U.SelectFileByExplorer(txtFilename);
        }

        string Convert(string giffilename)
        {

            string prefixBaseFilename = Path.Combine(Path.GetDirectoryName(giffilename), Path.GetFileNameWithoutExtension(giffilename));

            //https://dobon.net/vb/dotnet/graphics/selectactiveframe.html

            //画像を読み込む
            Image img = Image.FromFile(giffilename);
            //FrameDimensionを取得する
            FrameDimension fd = new FrameDimension(img.FrameDimensionsList[0]);

            Dictionary<string, string> AnimeDic = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();

            uint[] waits = U.GetGifWaits(giffilename);

            //フレーム数を取得する
            int frameCount = img.GetFrameCount(fd);
            for (int i = 0; i < frameCount; i++)
            {
                //フレームを選択する
                img.SelectActiveFrame(fd, i);

                //同一判定にMD5を利用する
                string key = U.md5(img);

                string imgFilename;
                if (!AnimeDic.TryGetValue(key, out imgFilename))
                {
                    //知らないので、ファイルに出力して追加
                    int image_count = AnimeDic.Count;
                    imgFilename = prefixBaseFilename + "_" + image_count.ToString("000") + ".png";
                    img.Save(imgFilename, ImageFormat.Png);
                    AnimeDic[key] = imgFilename;
                }

                uint currentWait = 0;
                if (i < waits.Length)
                {
                    currentWait = waits[i];
                }
                else
                {
                    currentWait = 0;
                }

                long gbaFrame = U.GifFrameToGameFrame(currentWait);
                string currentFilename = Path.GetFileName(imgFilename);
                sb.AppendLine(gbaFrame + " p- " + currentFilename);
            }

            string txtFilename = prefixBaseFilename + ".txt";
            File.WriteAllText(txtFilename, sb.ToString());
            return txtFilename;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            U.AllowDropFilename(this, new string[]{".GIF"}, (string filename) =>
            {
                FilenameTextBox.Text = filename;
                button2.PerformClick();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "gif|*.gif";
            DialogResult dr = open.ShowDialog();
            if (dr != DialogResult.OK)
            {
                return;
            }
            FilenameTextBox.Text = open.FileName;
        }

    }
}
