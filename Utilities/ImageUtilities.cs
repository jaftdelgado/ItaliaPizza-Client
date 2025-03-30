﻿using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Utilities
{
    public class ImageUtilities
    {
        public static void SetImageSource(Image imageControl, byte[] imageBytes, string defaultImagePath)
        {
            BitmapImage imageSource;

            if (imageBytes != null && imageBytes.Length > 0)
                imageSource = ConvertToImageSource(imageBytes);
            else
                imageSource = new BitmapImage(new Uri(defaultImagePath, UriKind.Relative));

            imageControl.Source = imageSource;
        }


        public static BitmapImage ConvertToImageSource(byte[] imageBytes)
        {
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
                return bitmap;
            }
        }

        public static byte[] ImageToByteArray(BitmapImage bitmapImage)
        {
            using (var memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
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
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }
    }
}
