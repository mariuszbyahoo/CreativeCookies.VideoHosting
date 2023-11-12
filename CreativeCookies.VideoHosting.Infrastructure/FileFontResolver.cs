using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure
{
    internal class FileFontResolver : IFontResolver 
    {
        public string DefaultFontName => "Default";

        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (isBold)
            {
                return new FontResolverInfo("Fonts/GlacialIndifference-Bold.ttf");
            }
            else
            {
                return new FontResolverInfo("Fonts/GlacialIndifference-Regular.ttf");
            }
        }
    }
}
