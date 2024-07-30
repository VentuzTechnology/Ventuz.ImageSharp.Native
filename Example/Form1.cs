/*
 * Ventuz.ImageSharp.Native
 * Copyright (c) 2024 Ventuz Technology <https://ventuz.com>
 *
 * This file is part of Ventuz.ImageSharp.Native
 *
 * libheif is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * libheif is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Ventuz.ImageSharp.Native.  If not, see <http://www.gnu.org/licenses/>.
 */

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
        }

        public Form1(string path) : this()
        {
            DecoderOptions opt = new()
            {
                SkipMetadata = false
            };

            using var image = SixLabors.ImageSharp.Image.Load(opt, path);
         
            image.Mutate(i => i.AutoOrient());

            using MemoryStream ms = new();
            using var img2 = image.CloneAs<Rgba32>();
            img2.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            pictureBox1.Image = System.Drawing.Image.FromStream(ms);
        }
    }
}


