
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace imageresizing
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 5)
                {
                    Console.WriteLine("usage: folder size1 size2 size3 size4");
                    Console.WriteLine(@"eg: c:\document\images\ 1000 500 250 125");
                }
                
                string[] extensions = { "jpg", "jpeg" };
                
                foreach (var extension in extensions)
                {
                    var fileEntries = Directory.GetFiles(args[0], "*." + extension);
                    foreach (var filePath in fileEntries)
                    {
                        for (var i = 1; i < args.Length; i++)
                        {
                            var newWidth = Convert.ToInt32(args[i]);
                            var newHeight = Convert.ToInt32(args[i]);
                            EncodeAndSave(newWidth, newHeight, filePath, Path.GetFileNameWithoutExtension(filePath), extension);
                        }
                    }
                }

                foreach (var extension in extensions)
                {
                    var fileEntries = Directory.GetFiles(args[0], "*." + extension);
                    foreach (var filePath in fileEntries)
                    {
                        var destFilename = @Path.GetDirectoryName(filePath) + @"\archive\" + Path.GetFileName(filePath);
                        if (!File.Exists(destFilename))
                        {
                            using (var fs = File.Create(destFilename)) { fs.Close(); }
                        }

                        var file = new FileInfo(filePath);

                        if (file.Exists)
                        {
                            if  (!File.Exists(destFilename))
                            {
                                File.Move(filePath, destFilename);
                            }

                            File.Delete(filePath);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void EncodeAndSave(int newWidth, int newHeight, string filePath, string filename, string extension)
        {
            try
            {
                var rtb = new RenderTargetBitmap(newWidth, newHeight, 96.0, 96.0, PixelFormats.Pbgra32);

                var group = new DrawingGroup();
                RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(@filePath, UriKind.Relative);
                image.EndInit();

                var newDrawing = new ImageDrawing
                {
                    Rect = new Rect(new Point(), new Size(newWidth, newHeight)),
                    ImageSource = image
                };

                group.Children.Add(newDrawing);

                var dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                    dc.DrawDrawing(group);

                rtb.Render(dv);
                var bmf = BitmapFrame.Create(rtb);
                bmf.Freeze();

                var newpath = @Path.GetDirectoryName(filePath) + @"\archive";

                if (!Directory.Exists(newpath))
                {
                    Directory.CreateDirectory(newpath);
                }

                var fileOut = newpath + @"\" + filename + "-" + newWidth + "." + extension;
                using (var stream = new FileStream(fileOut, FileMode.Create))
                {
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(bmf);
                    encoder.Save(stream);
                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
