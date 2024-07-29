

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
            Ventuz.ImageSharp.Native.Formats.Register();

            Ventuz.ImageSharp.Native.Logging.LogCallback += (level, str) =>
            {
                System.Diagnostics.Debug.WriteLine($"{level}: {str}");
            };

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}