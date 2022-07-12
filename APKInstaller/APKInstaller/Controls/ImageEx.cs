using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace APKInstaller.Controls
{
    public class ImageEx : Image
    {
        public new string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BxSource.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "Source",
                typeof(string),
                typeof(ImageEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnSourceChanged),
                    null),
                null);

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageEx)d).UpdateSource();
        }

        private void UpdateSource()
        {
            if (Source == null && !string.IsNullOrEmpty(Source)) { return; }

            Uri url;
            try { url = new Uri(Source, UriKind.RelativeOrAbsolute); } catch { return; }
            if (url == null) { return; }

            try
            {
                if (url.IsAbsoluteUri)
                {
                    if (File.Exists(Source))
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(Source, FileMode.Open)))
                        {
                            FileInfo fi = new FileInfo(Source);
                            byte[] bytes = reader.ReadBytes((int)fi.Length);
                            reader.Close();

                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = new MemoryStream(bytes);
                            bitmapImage.EndInit();
                            base.Source = bitmapImage;
                        }
                    }
                    else
                    {
                        using (Stream stream = Application.GetResourceStream(url).Stream)
                        {
                            if (stream != null || stream.Length >= 0)
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {
                                    byte[] bytes = reader.ReadBytes((int)stream.Length);
                                    reader.Close();

                                    BitmapImage bitmapImage = new BitmapImage();
                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                                    bitmapImage.BeginInit();
                                    bitmapImage.StreamSource = new MemoryStream(bytes);
                                    bitmapImage.EndInit();
                                    base.Source = bitmapImage;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Source);
                    if (File.Exists(path))
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                        {
                            FileInfo fi = new FileInfo(path);
                            byte[] bytes = reader.ReadBytes((int)fi.Length);
                            reader.Close();

                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = new MemoryStream(bytes);
                            bitmapImage.EndInit();
                            base.Source = bitmapImage;
                        }
                    }
                    else
                    {
                        using (Stream stream = Application.GetResourceStream(url).Stream)
                        {
                            if (stream != null || stream.Length >= 0)
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {
                                    byte[] bytes = reader.ReadBytes((int)stream.Length);
                                    reader.Close();

                                    BitmapImage bitmapImage = new BitmapImage();
                                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                                    bitmapImage.BeginInit();
                                    bitmapImage.StreamSource = new MemoryStream(bytes);
                                    bitmapImage.EndInit();
                                    base.Source = bitmapImage;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { return; }
        }
    }
}
