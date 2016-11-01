using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Sabre_WPK_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WPKFile wpk;
        private Microsoft.Win32.OpenFileDialog ofd;
        private Microsoft.Win32.OpenFileDialog ofd2;
        private Microsoft.Win32.OpenFileDialog ofd3;
        public MainWindow()
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + "/" + "WEMFiles" + "/");
            Directory.CreateDirectory(Environment.CurrentDirectory + "/" + "WPKFiles" + "/");
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "WPK files (*.wpk)|*.wpk";
            if(ofd.ShowDialog() == true)
            {
                buttonAutofill.IsEnabled = true;
                buttonExtract.IsEnabled = true;
                buttonExtractAll.IsEnabled = true;
                buttonSave.IsEnabled = true;
                buttonImport.IsEnabled = true;
                listAudioFiles.Items.Clear();
                wpk = new WPKFile(ofd.FileName);
                foreach(var a in wpk.AudioFiles)
                {
                    listAudioFiles.Items.Add(a.Name);
                }
            }
        }

        private void buttonExtractAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(var a in wpk.AudioFiles)
            {
                File.WriteAllBytes(Environment.CurrentDirectory + "/WEMFiles/" + a.Name, a.Data);
            }
        }

        private void buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            string selected = listAudioFiles.Items[listAudioFiles.SelectedIndex].ToString();
            var s = wpk.AudioFiles.Find(x => x.Name == selected);
            File.WriteAllBytes(Environment.CurrentDirectory + "/WEMFiles/" + s.Name, s.Data);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(ofd.FileName));

            bw.Write(wpk.header.Magic.ToCharArray());
            bw.Write(wpk.header.Version);
            bw.Write(wpk.header.AudioCount);
            foreach(var m in wpk.AudioFiles)
            {
                bw.Write(m.mtOff);
            }
            uint LastOffset = wpk.AudioFiles[0].DataOffset;
            foreach(var a in wpk.AudioFiles)
            {
                bw.BaseStream.Seek(a.mtOff, SeekOrigin.Begin);
                a.DataOffset = LastOffset;
                bw.Write(a.DataOffset);
                bw.Write(a.DataSize);
                bw.Write(a.NameLength);
                bw.Write(a.tempName);
                LastOffset += (uint)a.Data.Length;
            }
            foreach(var d in wpk.AudioFiles)
            {
                bw.BaseStream.Seek(d.DataOffset, SeekOrigin.Begin);
                bw.Write(d.Data);
            }
        }

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            BinaryReader br;
            byte[] Data;
            ofd2 = new Microsoft.Win32.OpenFileDialog();
            ofd2.Filter = "WEM files (*.wem)|*.wem";
            if (ofd2.ShowDialog() == true)
            {
                br = new BinaryReader(File.Open(ofd2.FileName, FileMode.Open));
                Data = br.ReadBytes((int)br.BaseStream.Length);
                string selected = listAudioFiles.Items[listAudioFiles.SelectedIndex].ToString();
                var s = wpk.AudioFiles.Find(x => x.Name == selected);
                s.Data = Data;
                s.DataSize = (uint)Data.Length;
            }
        }

        private void buttonAutofill_Click(object sender, RoutedEventArgs e)
        {
            string[] files;
            byte[] Data;    
            int i = 0;
            ofd3 = new Microsoft.Win32.OpenFileDialog();
            BinaryReader br;
            ofd3.Multiselect = true;
            ofd3.Filter = "WEM files (*.wem)|*.wem";
            if (ofd3.ShowDialog() == true)
            {
                files = ofd3.FileNames;
                foreach (var a in wpk.AudioFiles)
                {
                    br = new BinaryReader(File.Open(files[i], FileMode.Open));
                    Data = br.ReadBytes((int)br.BaseStream.Length);
                    a.Data = Data;
                    a.DataSize = (uint)Data.Length;
                    i++;
                }
            }
        }
    }
}
