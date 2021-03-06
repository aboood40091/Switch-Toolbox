﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Switch_Toolbox;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.Forms;
using VGAudio.Formats;
using VGAudio.Containers.NintendoWare;
using CSCore;
using CSCore.Codecs;

namespace FirstPlugin
{
    public class MP3 : IEditor<AudioPlayer>, IFileFormat
    {
        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "MPEG-1 Audio Layer-3" };
        public string[] Extension { get; set; } = new string[] { "*.mp3" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                bool IsValidSig = reader.CheckSignature(3, "ID3");
                bool IsValidExt = Utils.HasExtension(FileName, ".mp3");

                if (IsValidExt || IsValidSig)
                    return true;
                else
                    return false;
            }
        }

        public Type[] Types
        {
            get
            {
                List<Type> types = new List<Type>();
                return types.ToArray();
            }
        }

        public AudioPlayer OpenForm()
        {
            AudioPlayer form = new AudioPlayer();
            form.Text = FileName;
            form.Dock = DockStyle.Fill;
            form.LoadFile(waveSource, this, mp3Struct);

            return form;
        }

        IWaveSource waveSource;
        object mp3Struct;

        public void Load(System.IO.Stream stream)
        {
            CanSave = true;
            waveSource = CodecFactory.Instance.GetCodec(stream, ".mp3");

            stream.Position = 0;

            mp3Struct = CSCore.Tags.ID3.ID3v1.FromStream(stream);
        }
        public void Unload()
        {

        }
        public byte[] Save()
        {
            return null;
        }
    }
}
