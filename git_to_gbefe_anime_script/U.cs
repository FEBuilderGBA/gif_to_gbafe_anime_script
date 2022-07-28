using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.Reflection;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;

namespace git_to_gbefe_anime_script
{
    public static class U
    {
        public static string md5(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                byte[] bin = ms.ToArray();
                return md5(bin);
            }
        }
        //拡張子を取得 結果は必ず大文字 .PNG みたいに
        public static string GetFilenameExt(string filename)
        {
            try
            {
                return Path.GetExtension(filename).ToUpper();
            }
            catch (ArgumentException)
            {
                return "";
            }
        }
        public static void AllowDropFilename(Form self
            , string[] allowExts
            , Action<string> callback)
        {
            self.AllowDrop = true;
            self.DragEnter += (sender, e) =>
            {
                //ファイルがドラッグされている場合、カーソルを変更する
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                    if (fileName.Length <= 0)
                    {
                        return;
                    }

                    string ext = U.GetFilenameExt(fileName[0]);
                    if (Array.IndexOf(allowExts, ext) < 0)
                    {
                        return;
                    }

                    e.Effect = DragDropEffects.Copy;
                }
            };
            self.DragDrop += (sender, e) =>
            {
                //ドロップされたファイルの一覧を取得
                string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (fileName.Length <= 0)
                {
                    return;
                }
                string ext = U.GetFilenameExt(fileName[0]);
                if (Array.IndexOf(allowExts, ext) < 0)
                {
                    return;
                }
                e.Effect = DragDropEffects.None;
                for (int i = 0; i < fileName.Length; i++)
                {
                    callback(fileName[i]);
                }
            };
        }


        public static string md5(byte[] bin)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = md5.ComputeHash(bin);
            md5.Clear();

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        public static long GifFrameToGameFrame(uint gifframe)
        {
            return (ushort)Math.Round(((double)gifframe) * 60.0D / 100.0D);
        }

        public static string escape_shell_args(string str)
        {
            if (str.Length > 0 && str[str.Length - 1] == '\\')
            {//最後に \ があれば \\ として逃げる. 
                str = str + "\\ ";
            }
            str = str.Replace("\"", "\\\"");
            return '"' + str + '"';
        }
        public static void SelectFileByExplorer(string path)
        {
            if (path == "" || File.Exists(path) == false)
            {
                return;
            }

            try
            {
                string filename = U.escape_shell_args(path);
                Process.Start("EXPLORER.EXE", "/select," + filename);
            }
            catch (Exception ee)
            {
            }
        }

        //C#はクソだからこの手の関数がないので自分で作る。
        //無理ならffmpegを使うしかない
        public static uint[] GetGifWaits(string filename)
        {
            List<uint> list = new List<uint>();
            byte[] bin = File.ReadAllBytes(filename);
            if (bin.Length <= 8)
            {
                return list.ToArray();
            }
            //真面目には作らん。バイナリでフレームデータだけを抜き取ります
            //21
            //f9
            //04
            //?? (00|01|04|08|0xC)
            //time
            //time
            //00
            //00
            int max = bin.Length - 8;
            for (int i = 6 + 7; i < max; i++)
            {
                if (bin[i] != 0x21)
                {
                    continue;
                }
                if (bin[i + 1] != 0xf9)
                {
                    continue;
                }
                if (bin[i + 2] != 0x04)
                {
                    continue;
                }
                if (bin[i + 3] >= 0x10)
                {
                    continue;
                }
                if (bin[i + 6] >= 0x30)
                {
                    continue;
                }
                if (bin[i + 7] != 0x00)
                {
                    continue;
                }
                //とうやら時間が書いてあるフレームデータのようだ
                uint wait = (uint)(((uint)bin[i + 4]) + (((uint)bin[i + 5]) >> 8));
                list.Add(wait);
            }
            return list.ToArray();
        }
    }
}
