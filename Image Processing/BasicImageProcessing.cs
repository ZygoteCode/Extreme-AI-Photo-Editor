using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class BasicImageProcessing
{
    public static Bitmap MakeNegative(Bitmap image)
    {
        int w = image.Width;
        int h = image.Height;
        BitmapData srcData = image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int bytes = srcData.Stride * srcData.Height;
        byte[] buffer = new byte[bytes];
        byte[] result = new byte[bytes];
        Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
        image.UnlockBits(srcData);
        int current = 0;
        int cChannels = 3;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                current = y * srcData.Stride + x * 4;

                for (int c = 0; c < cChannels; c++)
                {
                    result[current + c] = (byte)(255 - buffer[current + c]);
                }

                result[current + 3] = 255;
            }
        }

        Bitmap resImg = new Bitmap(w, h);
        BitmapData resData = resImg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(result, 0, resData.Scan0, bytes);
        resImg.UnlockBits(resData);
        return resImg;
    }

    public static Bitmap MakeBlackAndWhite(Bitmap image)
    {
        Bitmap temp = (Bitmap)image;
        Bitmap bmap = (Bitmap)temp.Clone();
        Color c;

        for (int y = 0; y < bmap.Height; y++)
        {
            for (int x = 0; x < bmap.Width; x++)
            {
                c = bmap.GetPixel(x, y);
                byte gray = (byte)(.21 * c.R + .71 * c.G + .071 * c.B);
                bmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
            }
        }

        return (Bitmap)bmap.Clone();
    }

    public static Bitmap AdjustContrast(Bitmap bmp, int threshold)
    {
        double contrast = Math.Pow((100.0 + threshold) / 100.0, 2);
        Bitmap temp = (Bitmap)bmp;
        Bitmap bmap = (Bitmap)temp.Clone();

        for (int y = 0; y < bmap.Height; y++)
        {
            for (int x = 0; x < bmap.Width; x++)
            {
                Color oldColor = bmap.GetPixel(x, y);
                double red = ((((oldColor.R / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                double green = ((((oldColor.G / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                double blue = ((((oldColor.B / 255.0) - 0.5) * contrast) + 0.5) * 255.0;

                if (red > 255)
                {
                    red = 255;
                }

                if (red < 0)
                {
                    red = 0;
                }

                if (green > 255)
                {
                    green = 255;
                }

                if (green < 0)
                {
                    green = 0;
                }

                if (blue > 255)
                {
                    blue = 255;
                }

                if (blue < 0)
                {
                    blue = 0;
                }

                Color newColor = Color.FromArgb(oldColor.A, (int)red, (int)green, (int)blue);
                bmap.SetPixel(x, y, newColor);
            }
        }

        return (Bitmap)bmap.Clone();
    }

    public static Bitmap AdjustGamma(Bitmap img, double gamma, double c = 1d)
    {
        int width = img.Width;
        int height = img.Height;
        BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int bytes = srcData.Stride * srcData.Height;
        byte[] buffer = new byte[bytes];
        byte[] result = new byte[bytes];
        Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
        img.UnlockBits(srcData);
        int current = 0;
        int cChannels = 3;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                current = y * srcData.Stride + x * 4;

                for (int i = 0; i < cChannels; i++)
                {
                    double range = (double)buffer[current + i] / 255;
                    double correction = c * Math.Pow(range, gamma);
                    result[current + i] = (byte)(correction * 255);
                }
                result[current + 3] = 255;
            }
        }

        Bitmap resImg = new Bitmap(width, height);
        BitmapData resData = resImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(result, 0, resData.Scan0, bytes);
        resImg.UnlockBits(resData);
        return resImg;
    }

    public static Bitmap AdjustBrightnessMethod2(Bitmap btmap, int brightness)
    {
        Bitmap temp = (Bitmap)btmap;
        Bitmap bmap = (Bitmap)temp.Clone();

        if (brightness < -255)
        {
            brightness = -255;
        }

        if (brightness > 255)
        {
            brightness = 255;
        }

        Color c;

        for (int i = 0; i < bmap.Width; i++)
        {
            for (int j = 0; j < bmap.Height; j++)
            {
                c = bmap.GetPixel(i, j);

                int cR = c.R + brightness;
                int cG = c.G + brightness;
                int cB = c.B + brightness;

                if (cR < 0)
                {
                    cR = 1;
                }

                if (cR > 255)
                {
                    cR = 255;
                }

                if (cG < 0)
                {
                    cG = 1;
                }

                if (cG > 255)
                {
                    cG = 255;
                }

                if (cB < 0)
                {
                    cB = 1;
                }

                if (cB > 255)
                {
                    cB = 255;
                }

                bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
            }
        }

        return (Bitmap)bmap.Clone();
    }

    public static Bitmap AdjustBrightnessMethod1(Image image, float brightness)
    {
        float b = brightness;

        ColorMatrix cm = new ColorMatrix(new float[][]
            {
                    new float[] {b, 0, 0, 0, 0},
                    new float[] {0, b, 0, 0, 0},
                    new float[] {0, 0, b, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1},
            });
        ImageAttributes attributes = new ImageAttributes();
        attributes.SetColorMatrix(cm);

        Point[] points =
        {
                new Point(0, 0),
                new Point(image.Width, 0),
                new Point(0, image.Height),
            };
        Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

        Bitmap bm = new Bitmap(image.Width, image.Height);

        using (Graphics gr = Graphics.FromImage(bm))
        {
            gr.DrawImage(image, points, rect, GraphicsUnit.Pixel, attributes);
        }

        return bm;
    }
}