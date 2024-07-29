using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Example
{
    public partial class Form1 : Form
    {
      
        public Form1()
        {
            InitializeComponent();

            Configuration cfg = Configuration.Default.Clone();

            DecoderOptions opt = new()
            {
                SkipMetadata = false
            };

            var id = SixLabors.ImageSharp.Image.Identify(opt, @"C:\Users\TammoHinrichs\Pictures\avif-sample-images-master\fox.profile1.8bpc.yuv444.odd-width.odd-height.avif");
            using var image = SixLabors.ImageSharp.Image.Load(opt, @"C:\Users\TammoHinrichs\Pictures\avif-sample-images-master\fox.profile1.8bpc.yuv444.odd-width.odd-height.avif");
         
            image.Mutate(i => i.AutoOrient());

            using MemoryStream ms = new();
            using var img2 = image.CloneAs<Rgba32>();
            img2.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            pictureBox1.Image = System.Drawing.Image.FromStream(ms);
        }
    }
}


