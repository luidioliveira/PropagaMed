using PdfSharpCore.Fonts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PropagaMed.Utils
{
    public class PropagaMedFontResolver : IFontResolver
    {
        public static readonly PropagaMedFontResolver Instance = new PropagaMedFontResolver();

        private static readonly Assembly _assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .First(a => a.GetManifestResourceNames()
                         .Any(r => r.EndsWith("OpenSans-Regular.ttf")));

        private static readonly string _assemblyName = _assembly.GetName().Name switch
        {
            "PropagaMed.Android" => "PropagaMed.Droid",
            "PropagaMed.iOS" => "PropagaMed.iOS",
            var name => name
        };

        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            string resourceName = $"{_assemblyName}.Assets.Fonts.{faceName}";

            using var stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException(
                    $"Fonte '{resourceName}' não encontrada como EmbeddedResource.");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.ToLowerInvariant().Contains("opensans") ||
                familyName.ToLowerInvariant().Contains("open sans"))
            {
                string fileName = isBold
                    ? "OpenSans-Bold.ttf"
                    : "OpenSans-Regular.ttf";

                return new FontResolverInfo(fileName);
            }

            return new FontResolverInfo(familyName);
        }
    }
}