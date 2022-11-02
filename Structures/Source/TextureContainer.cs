using Arx_Model_Exporter.Helpers;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class TextureContainer
    {
        public string name { get; private set; }
        public MemoryStream Img { get; private set; }

        public TextureContainer(MemoryWrapper ftlFile)
        {
            byte[] bytes = ftlFile.ReadBytes(-1, 256);
            name = Encoding.Default.GetString(bytes).Replace("\0", "");
            string FileName = Path.GetFileName(name);
            Img = new MemoryStream();

            using (Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(ftlFile.folderPath + FileName))
            {
                // Save as PNG
                image.SaveAsPng(Img, new PngEncoder
                {
                    ColorType = PngColorType.RgbWithAlpha,
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    TransparentColorMode = PngTransparentColorMode.Preserve,
                    BitDepth = PngBitDepth.Bit8
                });
            }
        }

    }
}