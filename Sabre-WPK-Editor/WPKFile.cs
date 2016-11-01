using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sabre_WPK_Editor
{
    class WPKFile
    {
        public BinaryReader br;
        public Header header;
        public List<AudioFile> AudioFiles = new List<AudioFile>();
        public uint LastOffset = 0;
        public WPKFile(string fileLocation)
        {
            br = new BinaryReader(File.Open(fileLocation, FileMode.Open));
            header = new Header(br);
            for(int i = 0; i < header.AudioCount; i++)
            {
                AudioFiles.Add(new AudioFile(br.ReadUInt32()));
            }
            LastOffset = AudioFiles[0].DataOffset;
            foreach(var a in AudioFiles)
            {
                br.BaseStream.Seek(a.mtOff, SeekOrigin.Begin);
                a.DataOffset = br.ReadUInt32();
                a.DataSize = br.ReadUInt32();
                a.NameLength = br.ReadUInt32();
                a.tempName = br.ReadChars((int)a.NameLength * 2);
                a.Name = GetWPKName(a.tempName);
            }
            foreach(var a in AudioFiles)
            {
                br.BaseStream.Seek(a.DataOffset, SeekOrigin.Begin);
                a.Data = br.ReadBytes((int)a.DataSize);
            }
            br.Dispose();
            br.Close();
        }
        public class Header
        {
            public string Magic;
            public UInt32 Version;
            public UInt32 AudioCount;
            public Header(BinaryReader br)
            {
                Magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                Version = br.ReadUInt32();
                AudioCount = br.ReadUInt32();
            }
        }
        public class AudioFile
        {
            public UInt32 mtOff;
            public UInt32 DataOffset;
            public UInt32 DataSize;
            public UInt32 NameLength;
            public char[] tempName;
            public string Name;
            public byte[] Data;
            public AudioFile(UInt32 metaOffset)
            {
                mtOff = metaOffset;
            }
            public static void ExtractFile(string fileLocation, string extractPath, UInt32 DataOffset, UInt32 DataSize)
            {
                using (BinaryReader br = new BinaryReader(new FileStream(fileLocation, FileMode.Open)))
                { 
                    br.BaseStream.Seek(DataOffset, SeekOrigin.Begin);
                    File.WriteAllBytes(extractPath, br.ReadBytes((int)DataSize));
                    br.Dispose();
                    br.Close();
                }
            }
        }
        public static string GetWPKName(char[] tempName)
        {
            string name = "";
            foreach(char c in tempName)
            {
                if(c != '\0')
                {
                    name += c;
                }
            }
            return name;
        }
    }
}
