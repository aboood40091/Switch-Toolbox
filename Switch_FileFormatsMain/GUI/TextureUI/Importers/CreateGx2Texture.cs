﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Switch_Toolbox.Library;
using Syroot.NintenTools.Bfres.GX2;

namespace FirstPlugin
{
    public class GTXSwizzle
    {
        public static GX2.GX2Surface CreateGx2Texture(byte[] imageData, GTXImporterSettings setting)
        {
            return GX2.CreateGx2Texture(imageData,
                setting.TexName,
                setting.tileMode,
                (uint)setting.AAMode,
                setting.TexWidth,
                setting.TexHeight,
                setting.Depth,
                (uint)setting.Format,
                setting.swizzle,
                (uint)setting.SurfaceDim,
                setting.MipCount);
        }
    }
}
