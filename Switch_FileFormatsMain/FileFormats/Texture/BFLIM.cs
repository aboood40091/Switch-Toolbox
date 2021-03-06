﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.IO;
using Switch_Toolbox.Library.Forms;
using System.ComponentModel;
using Bfres.Structs;

namespace FirstPlugin
{
    public class BFLIM : STGenericTexture, IEditor<ImageEditorForm>, IFileFormat
    {
        public override TEX_FORMAT[] SupportedFormats
        {
            get
            {
                return new TEX_FORMAT[]
                {
                        TEX_FORMAT.BC1_UNORM,
                        TEX_FORMAT.BC1_UNORM_SRGB,
                        TEX_FORMAT.BC2_UNORM,
                        TEX_FORMAT.BC2_UNORM_SRGB,
                        TEX_FORMAT.BC3_UNORM,
                        TEX_FORMAT.BC3_UNORM_SRGB,
                        TEX_FORMAT.BC4_UNORM,
                        TEX_FORMAT.BC4_SNORM,
                        TEX_FORMAT.BC5_UNORM,
                        TEX_FORMAT.BC5_SNORM,
                        TEX_FORMAT.R8G8B8A8_UNORM_SRGB,
                        TEX_FORMAT.R8G8B8A8_UNORM,
                };
            }
        }

        public BFLIMFormat ConvertFormatGenericToBflim(TEX_FORMAT Format)
        {
            switch (Format)
            {
                case TEX_FORMAT.A8_UNORM: return BFLIMFormat.L8_UNORM;
                case TEX_FORMAT.R8G8_UNORM: return BFLIMFormat.LA8;
                case TEX_FORMAT.B5G6R5_UNORM: return BFLIMFormat.RGB565;
                case TEX_FORMAT.R8G8B8A8_UNORM: return BFLIMFormat.RGBA8;
                case TEX_FORMAT.R8G8B8A8_UNORM_SRGB: return BFLIMFormat.RGBA8_SRGB;
                case TEX_FORMAT.R10G10B10A2_UNORM: return BFLIMFormat.RGB10A2_UNORM;
                case TEX_FORMAT.B4G4R4A4_UNORM: return BFLIMFormat.RGBA4;
                case TEX_FORMAT.BC1_UNORM: return BFLIMFormat.BC1_UNORM;
                case TEX_FORMAT.BC1_UNORM_SRGB: return BFLIMFormat.BC1_SRGB;
                case TEX_FORMAT.BC2_UNORM: return BFLIMFormat.BC2_UNORM;
                case TEX_FORMAT.BC2_UNORM_SRGB: return BFLIMFormat.BC2_SRGB;
                case TEX_FORMAT.BC3_UNORM: return BFLIMFormat.BC3_UNORM;
                case TEX_FORMAT.BC3_UNORM_SRGB: return BFLIMFormat.BC3_SRGB;
                case TEX_FORMAT.BC4_UNORM: return BFLIMFormat.BC4A_UNORM;
                case TEX_FORMAT.BC4_SNORM: return BFLIMFormat.BC4L_UNORM;
                case TEX_FORMAT.BC5_UNORM: return BFLIMFormat.BC5_UNORM;
                default:
                    throw new Exception("Unsupported format " + Format);
            }
        }

        public override bool CanEdit { get; set; } = false;

        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "Layout Image" };
        public string[] Extension { get; set; } = new string[] { "*.bflim" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                return reader.CheckSignature(4, "FLIM", reader.BaseStream.Length - 0x28);
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

        ImageEditorForm form;
        public ImageEditorForm OpenForm()
        {
            bool IsDialog = IFileInfo != null && IFileInfo.InArchive;

            Properties prop = new Properties();
            prop.Width = Width;
            prop.Height = Height;
            prop.Depth = Depth;
            prop.MipCount = MipCount;
            prop.ArrayCount = ArrayCount;
            prop.ImageSize = (uint)ImageData.Length;
            prop.Format = Format;

            form = new ImageEditorForm(IsDialog);
            form.editorBase.Text = Text;
            form.editorBase.Dock = DockStyle.Fill;
            form.editorBase.AddFileContextEvent("Save", Save);
            form.editorBase.AddFileContextEvent("Replace", Replace);
            form.editorBase.LoadProperties(prop);
            form.editorBase.LoadImage(this);

            return form;
        }

        private void UpdateForm()
        {
            if (form != null)
            {
                Properties prop = new Properties();
                prop.Width = Width;
                prop.Height = Height;
                prop.Depth = Depth;
                prop.MipCount = MipCount;
                prop.ArrayCount = ArrayCount;
                prop.ImageSize = (uint)ImageData.Length;
                prop.Format = Format;

                form.editorBase.LoadProperties(prop);
                form.editorBase.LoadImage(this);
            }
        }

        private void Replace(object sender, EventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Formats|*.dds; *.png;*.tga;*.jpg;*.tiff|" +
                         "Microsoft DDS |*.dds|" +
                         "Portable Network Graphics |*.png|" +
                         "Joint Photographic Experts Group |*.jpg|" +
                         "Bitmap Image |*.bmp|" +
                         "Tagged Image File Format |*.tiff|" +
                         "All files(*.*)|*.*";

            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FTEX ftex = new FTEX();
                ftex.ReplaceTexture(ofd.FileName, 1, SupportedFormats);
                if (ftex.texture != null)
                {
                    image.Swizzle = (byte)ftex.texture.Swizzle;
                    image.BflimFormat = ConvertFormatGenericToBflim(ftex.Format);
                    image.Height = (ushort)ftex.texture.Height;
                    image.Width = (ushort)ftex.texture.Width;

                    Format = GetFormat(image.BflimFormat);
                    Width = image.Width;
                    Height = image.Height;

                    ImageData = ftex.texture.Data;

                    UpdateForm();
                }
            }
        }

        private void Save(object sender, EventArgs args)
        {
            List<IFileFormat> formats = new List<IFileFormat>();
            formats.Add(this);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = Utils.GetAllFilters(formats);
            sfd.FileName = FileName;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                STFileSaver.SaveFileFormat(this, sfd.FileName);
            }
        }

        public class Properties
        {
            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Height of the image.")]
            [Category("Image Info")]
            public uint Height { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Width of the image.")]
            [Category("Image Info")]
            public uint Width { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Format of the image.")]
            [Category("Image Info")]
            public TEX_FORMAT Format { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Depth of the image (3D type).")]
            [Category("Image Info")]
            public uint Depth { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Mip map count of the image.")]
            [Category("Image Info")]
            public uint MipCount { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("Array count of the image for multiple surfaces.")]
            [Category("Image Info")]
            public uint ArrayCount { get; set; }

            [Browsable(true)]
            [ReadOnly(true)]
            [Description("The image size in bytes.")]
            [Category("Image Info")]
            public uint ImageSize { get; set; }
        }

        Header header;
        Image image;
        byte[] ImageData;

        public void Load(System.IO.Stream stream)
        {
            CanSave = true;

            Text = FileName;

            using (var reader = new FileReader(stream))
            {
                uint FileSize = (uint)reader.BaseStream.Length;
                reader.Seek(FileSize - 0x28, SeekOrigin.Begin);

                header = new Header();
                header.Read(reader);

                reader.Seek(header.HeaderSize + FileSize - 0x28, SeekOrigin.Begin);
                image = new Image();
                image.Read(reader);
                Format = GetFormat(image.BflimFormat);
                Width = image.Width;
                Height = image.Height;

                uint ImageSize = reader.ReadUInt32();
                Console.WriteLine(ImageSize);

                reader.Position = 0;
                ImageData = reader.ReadBytes((int)ImageSize);

                if (!PluginRuntime.bflimTextures.ContainsKey(Text))
                    PluginRuntime.bflimTextures.Add(Text, this);
            }
        }


        private TEX_FORMAT GetFormat(BFLIMFormat format)
        {
            switch (format)
            {
                case BFLIMFormat.L8_UNORM:
                case BFLIMFormat.A8:
                    return TEX_FORMAT.A8_UNORM;
                case BFLIMFormat.LA4:
                    return TEX_FORMAT.B4G4R4A4_UNORM;
                case BFLIMFormat.LA8:
                case BFLIMFormat.HILO8:
                    return TEX_FORMAT.R8G8_UNORM;
                case BFLIMFormat.RGB565:
                    return TEX_FORMAT.B5G6R5_UNORM;
                case BFLIMFormat.RGBX8:
                case BFLIMFormat.RGBA8:
                    return TEX_FORMAT.R8G8B8A8_UNORM;
                case BFLIMFormat.RGBA8_SRGB:
                    return TEX_FORMAT.R8G8B8A8_UNORM_SRGB;
                case BFLIMFormat.RGB10A2_UNORM:
                    return TEX_FORMAT.R10G10B10A2_UNORM;
                case BFLIMFormat.RGB5A1:
                    return TEX_FORMAT.B5G5R5A1_UNORM;
                case BFLIMFormat.RGBA4:
                    return TEX_FORMAT.B4G4R4A4_UNORM;
                case BFLIMFormat.BC1_UNORM:
                    return TEX_FORMAT.BC1_UNORM;
                case BFLIMFormat.BC1_SRGB:
                    return TEX_FORMAT.BC1_UNORM_SRGB;
                case BFLIMFormat.BC2_UNORM:
                    return TEX_FORMAT.BC2_UNORM_SRGB;
                case BFLIMFormat.BC3_UNORM:
                    return TEX_FORMAT.BC3_UNORM;
                case BFLIMFormat.BC3_SRGB:
                    return TEX_FORMAT.BC3_UNORM_SRGB;
                case BFLIMFormat.BC4L_UNORM:
                case BFLIMFormat.BC4A_UNORM:
                    return TEX_FORMAT.BC4_UNORM;
                case BFLIMFormat.BC5_UNORM:
                    return TEX_FORMAT.BC5_UNORM;
                default:
                    throw new Exception("Unsupported format " + format);
            }
        }

        public enum BFLIMFormat : byte
        {
            L8_UNORM,
            A8,
            LA4,
            LA8,
            HILO8,
            RGB565,
            RGBX8,
            RGB5A1,
            RGBA4,
            RGBA8,
            ETC1,
            ETC1A4,
            BC1_UNORM,
            BC2_UNORM,
            BC3_UNORM,
            BC4L_UNORM,
            BC4A_UNORM,
            BC5_UNORM,
            L4_UNORM,
            A4_UNORM,
            RGBA8_SRGB,
            BC1_SRGB,
            BC2_SRGB,
            BC3_SRGB,
            RGB10A2_UNORM,
            RGB565_Indirect_UNORM,
        }

        public override void SetImageData(System.Drawing.Bitmap bitmap, int ArrayLevel)
        {
            throw new NotImplementedException("Cannot set image data! Operation not implemented!");
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0)
        {
            uint bpp = GetBytesPerPixel(Format);

            GX2.GX2Surface surf = new GX2.GX2Surface();
            surf.bpp = bpp;
            surf.height = image.Height;
            surf.width = image.Width;
            surf.aa = (uint)GX2.GX2AAMode.GX2_AA_MODE_1X;
            surf.alignment = image.Alignment;
            surf.depth = 1;
            surf.dim = (uint)GX2.GX2SurfaceDimension.DIM_2D;
            surf.format = (uint)FTEX.ConvertToGx2Format(Format);
            surf.use = (uint)GX2.GX2SurfaceUse.USE_COLOR_BUFFER;
            surf.pitch = 0;
            surf.data = ImageData;
            surf.numMips = 1;
            surf.mipOffset = new uint[0];
            surf.mipData = ImageData;
            surf.tileMode = (uint)GX2.GX2TileMode.MODE_2D_TILED_THIN1;
            surf.swizzle = image.swizzle;
            surf.numArray = 1;

            var surfaces = GX2.Decode(surf);

            return surfaces[ArrayLevel][MipLevel];
        }

        public void Unload()
        {

        }
        public byte[] Save()
        {
            MemoryStream mem = new MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;

                writer.Write(ImageData);

                long headerPos = writer.Position;

                header.Write(writer);
                image.Write(writer);
                writer.Write(ImageData.Length);

                writer.Seek(headerPos + 0x0C, SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
            }
            return mem.ToArray();
        }

        public class Header
        {
            public ushort ByteOrderMark;
            public ushort HeaderSize;
            public uint Version;
            public ushort blockount;
            public ushort padding;

            public void Read(FileReader reader)
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;

                reader.ReadSignature(4, "FLIM");
                ByteOrderMark = reader.ReadUInt16();
                reader.CheckByteOrderMark(ByteOrderMark);
                HeaderSize = reader.ReadUInt16();
                Version = reader.ReadUInt32();
                uint fileSize = reader.ReadUInt32();
                blockount = reader.ReadUInt16();
                padding = reader.ReadUInt16();
            }

            public void Write(FileWriter writer)
            {
                writer.WriteSignature("FLIM");
                writer.Write(ByteOrderMark);
                writer.Write(HeaderSize);
                writer.Write(Version);
                writer.Write(uint.MaxValue);
                writer.Write(blockount);
                writer.Write(padding);
            }
        }
        public class Image
        {
            public uint Size;
            public ushort Width;
            public ushort Height;
            public ushort Alignment;
            public BFLIMFormat BflimFormat;
            public byte Swizzle;

            public uint swizzle
            {
                get
                {
                    return (uint)(((int)((uint)this.Swizzle >> 5) & 7) << 8);
                }
                set
                {
                    this.Swizzle = (byte)((int)this.Swizzle & 31 | (int)(byte)(value >> 8) << 5);
                }
            }

            public void Read(FileReader reader)
            {
                reader.ReadSignature(4, "imag");
                Size = reader.ReadUInt32();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                Alignment = reader.ReadUInt16();
                BflimFormat = reader.ReadEnum<BFLIMFormat>(true);
                Swizzle = reader.ReadByte();
            }

            public void Write(FileWriter writer)
            {
                writer.WriteSignature("imag");
                writer.Write(Size);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Alignment);
                writer.Write(BflimFormat, true);
                writer.Write(Swizzle);
            }
        }
    }
}
