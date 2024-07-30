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

namespace Example
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Register Ventuz.ImageSharp.Native image formats to default configuration

            Ventuz.ImageSharp.Native.Formats.Register();

            // Add logging

            Ventuz.ImageSharp.Native.Logging.LogCallback += (level, str) =>
            {
                System.Diagnostics.Trace.WriteLine($"{level}: {str}");
            };

            // show Open File dialog

            static string MakeFilterExts(IEnumerable<string> exts) => String.Join(';', exts.Select(e => $"*.{e}"));

            var allformats = SixLabors.ImageSharp.Configuration.Default.ImageFormats;
            var extensions = Ventuz.ImageSharp.Native.Formats.SupportedFormats.SelectMany(f => f.FileExtensions);
            var allextensions = allformats.SelectMany(f => f.FileExtensions);

            var filter = "New Image Formats|" + MakeFilterExts(extensions) +
                         String.Concat(allformats.Select(f => $"|{f.Name}|{MakeFilterExts(f.FileExtensions)}")) +
                         "|All Image Formats|" + MakeFilterExts(allextensions) +
                         "|All files (*.*)|*.*";

            using OpenFileDialog ofd = new()
            {
                Title = "Ventuz.ImageSharp.Native example",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = filter,
                SelectReadOnly = true,
            };

            if ( ofd.ShowDialog() == DialogResult.Cancel )
                return;

            // load and show chosen file

            Application.Run(new MainForm(ofd.FileName));
        }
    }
}