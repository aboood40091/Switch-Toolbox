﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.IO;
using Syroot.NintenTools.Bfres.GX2;
using Bfres.Structs;

namespace FirstPlugin
{
    public class GTXImporterSettings
    {
        public string TexName;
        public uint TexWidth;
        public uint TexHeight;
        public uint MipCount;
        public uint Depth = 1;
        public uint arrayLength = 1;
        public List<byte[]> DataBlockOutput = new List<byte[]>();
        public List<byte[]> DecompressedData = new List<byte[]>();
        public GX2.GX2SurfaceFormat Format;
        public bool GenerateMipmaps;
        public bool IsSRGB;
        public uint tileMode = 4;
        public uint swizzle = 4;
        public GX2CompSel[] compSel = new GX2CompSel[4];
        public GX2SurfaceDim SurfaceDim = GX2SurfaceDim.Dim2D;
        public GX2AAMode AAMode = GX2AAMode.Mode1X;
        public float alphaRef = 0.5f;

        public void LoadDDS(string FileName, byte[] FileData = null)
        {
            TexName = Path.GetFileNameWithoutExtension(FileName);

            DDS dds = new DDS();

            if (FileData != null)
                dds.Load(new FileReader(new MemoryStream(FileData)));
            else
                dds.Load(new FileReader(FileName));
            MipCount = dds.header.mipmapCount;
            TexWidth = dds.header.width;
            TexHeight = dds.header.height;
            arrayLength = 1;
            if (dds.header.caps2 == (uint)DDS.DDSCAPS2.CUBEMAP_ALLFACES)
            {
                arrayLength = 6;
            }
            DataBlockOutput.Add(dds.bdata);

            Format = (GX2.GX2SurfaceFormat)FTEX.ConvertToGx2Format(dds.Format);;
        }

        public void LoadBitMap(Image Image, string FileName)
        {
            DecompressedData.Clear();

            TexName = Path.GetFileNameWithoutExtension(FileName);
            Format = (GX2.GX2SurfaceFormat)FTEX.ConvertToGx2Format(Runtime.PreferredTexFormat);

            GenerateMipmaps = true;
            LoadImage(new Bitmap(Image));
        }

        public void LoadBitMap(string FileName)
        {
            DecompressedData.Clear();

            TexName = Path.GetFileNameWithoutExtension(FileName);

            Format = (GX2.GX2SurfaceFormat)FTEX.ConvertToGx2Format(Runtime.PreferredTexFormat);
            GenerateMipmaps = true;

            //If a texture is .tga, we need to convert it
            Bitmap Image = null;
            if (Utils.GetExtension(FileName) == ".tga")
            {
                Image = Paloma.TargaImage.LoadTargaImage(FileName);
            }
            else
            {
                Image = new Bitmap(FileName);
            }

            LoadImage(Image);
        }

        private void LoadImage(Bitmap Image)
        {
            Image = BitmapExtension.SwapBlueRedChannels(Image);

            TexWidth = (uint)Image.Width;
            TexHeight = (uint)Image.Height;
            MipCount = (uint)GetTotalMipCount();

            DecompressedData.Add(BitmapExtension.ImageToByte(Image));

            Image.Dispose();
            if (DecompressedData.Count == 0)
            {
                throw new Exception("Failed to load " + Format);
            }
        }

        public int GetTotalMipCount()
        {
            int MipmapNum = 0;
            uint num = Math.Max(TexHeight, TexWidth);

            int width = (int)TexWidth;
            int height = (int)TexHeight;

            while (true)
            {
                num >>= 1;

                width = width / 2;
                height = height / 2;
                if (width <= 0 || height <= 0)
                    break;

                if (num > 0)
                    ++MipmapNum;
                else
                    break;
            }

            return MipmapNum;
        }
        public byte[] GenerateMips(int SurfaceLevel = 0)
        {
            Bitmap Image = BitmapExtension.GetBitmap(DecompressedData[SurfaceLevel], (int)TexWidth, (int)TexHeight);

            List<byte[]> mipmaps = new List<byte[]>();
            mipmaps.Add(STGenericTexture.CompressBlock(DecompressedData[SurfaceLevel],
                (int)TexWidth, (int)TexHeight, FTEX.ConvertFromGx2Format((GX2SurfaceFormat)Format), alphaRef));

            //while (Image.Width / 2 > 0 && Image.Height / 2 > 0)
            //      for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
            for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
            {
                Image = BitmapExtension.Resize(Image, Image.Width / 2, Image.Height / 2);
                mipmaps.Add(STGenericTexture.CompressBlock(BitmapExtension.ImageToByte(Image),
                    Image.Width, Image.Height, FTEX.ConvertFromGx2Format((GX2SurfaceFormat)Format), alphaRef));
            }
            Image.Dispose();

            return Utils.CombineByteArray(mipmaps.ToArray());
        }
        public void Compress()
        {
            DataBlockOutput.Clear();
            foreach (var surface in DecompressedData)
            {
                DataBlockOutput.Add(FTEX.CompressBlock(surface, (int)TexWidth, (int)TexHeight,
                    FTEX.ConvertFromGx2Format((GX2SurfaceFormat)Format), alphaRef));
            }
        }
    }
}
