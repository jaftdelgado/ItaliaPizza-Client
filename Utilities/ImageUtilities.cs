﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Utilities
{
    public static class ImageUtilities
    {
        public static void SetImageSource(Image imageControl, byte[] imageBytes, string defaultImagePath)
        {
            if (imageBytes != null)
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    imageControl.Source = image;
                }
            }
            else
            {
                imageControl.Source = LoadBitmapFromPackUri(defaultImagePath);
            }
        }

        public static BitmapImage LoadBitmapFromPackUri(string packUri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(packUri, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public static BitmapImage ConvertToImageSource(byte[] imageBytes)
        {
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                return LoadBitmapFromStream(memoryStream);
            }
        }

        public static byte[] ImageToByteArray(BitmapSource bitmapSource)
        {
            if (bitmapSource == null) return null;

            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static bool AreImageEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i]) return false;

            return true;
        }


        public static bool IsImageSizeValid(string path, long maxSizeInBytes)
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length <= maxSizeInBytes;
        }

        public static BitmapImage LoadAndResizeImage(string path, int targetWidth, int targetHeight)
        {
            var originalBitmap = LoadBitmapFromFile(path);
            var cropped = CropImageToCenter(originalBitmap);
            var resized = ResizeImage(cropped, targetWidth, targetHeight);
            return resized;
        }

        public static BitmapImage LoadBitmapFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadBitmapFromStream(stream);
            }
        }

        private static BitmapImage LoadBitmapFromStream(Stream stream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static CroppedBitmap CropImageToCenter(BitmapImage original)
        {
            double cropSize = Math.Min(original.PixelWidth, original.PixelHeight);
            int x = (original.PixelWidth - (int)cropSize) / 2;
            int y = (original.PixelHeight - (int)cropSize) / 2;

            return new CroppedBitmap(original, new Int32Rect(x, y, (int)cropSize, (int)cropSize));
        }

        private static BitmapImage ResizeImage(BitmapSource source, int width, int height)
        {
            var scaleTransform = new ScaleTransform(
                (double)width / source.PixelWidth,
                (double)height / source.PixelHeight);

            var transformedBitmap = new TransformedBitmap(source, scaleTransform);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return LoadBitmapFromStream(ms);
            }
        }
    }
}