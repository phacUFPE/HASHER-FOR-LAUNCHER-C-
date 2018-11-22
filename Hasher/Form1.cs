using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Hasher
{
    public partial class Form1 : Form
    {

        string[] skipFiles = { "Hasher.exe", "_hlist", "_version", "_links", "README.md" };

        int fileCount;

        public Form1()
        {
            InitializeComponent();
        }

        public static string CalculateMD5(string filename)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("ERRO: {0}", ex.Message));
                return "NONE";
            }
        }

        private void RecursiveHash(string directory, StreamWriter fileToWrite)
        {
            string more = directory.Remove(0, AppDomain.CurrentDomain.BaseDirectory.Length);
            try
            {
                if (Directory.Exists(directory))
                {
                    string[] files = Directory.GetFiles(directory);
                    foreach (string hFile in files)
                    {
                        bool allowHash = true;
                        if (hFile.Replace(directory, "").Replace("\\", "").StartsWith(".")) { MessageBox.Show(hFile); continue; }
                        foreach (string notDownload in skipFiles)
                        {
                            if (hFile.Replace(directory, "").Replace("\\", "").Contains(notDownload))
                            {
                                MessageBox.Show(hFile);
                                allowHash = false;
                                break;
                            }
                        }
                        if (!allowHash) { continue; }
                        string hash = CalculateMD5(hFile);
                        fileCount += 1;
                        fileToWrite.WriteLine(String.Format("file=\"{0}\", hash=\"{1}\";", more + hFile.Replace(directory, ""), hash));
                    }
                    string[] dirs = Directory.GetDirectories(directory);

                    foreach (string d in dirs)
                    {
                        RecursiveHash(d, fileToWrite);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("ERRO: {0}", ex.Message));
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            fileCount = 0;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {                        
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "_hlist"))            
            {
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "_hlist").Close();                
            }
            try
            {
                StreamWriter file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "_hlist", false);
                RecursiveHash(AppDomain.CurrentDomain.BaseDirectory, file);
                file.WriteLine(String.Format("totalfiles={0}", fileCount));
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("ERRO: {0}", ex.Message));
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("HASHER COMPLETO!");
            btnStart.Enabled = true;
        }
    }
}
