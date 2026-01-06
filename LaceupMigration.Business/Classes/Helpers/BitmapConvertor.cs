using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace LaceupMigration
{
    public class SignatureData
    {
        public byte[] Bytes;
        public int Width;
        public int Height;
    }

    public class BitmapConvertor
    {

        private int mDataWidth;
        private byte[] mRawBitmapData;
        private byte[] mDataArray;
        private byte[] originalData;
        private int mWidth, mHeight;

        public BitmapConvertor()
        {
        }

        public byte[] convertBitmap(SKBitmap data)
        {

            mWidth = data.Width;
            mHeight = data.Height;
            mDataWidth = ((mWidth + 31) / 32) * 4 * 8;
            mDataArray = new byte[(mDataWidth * mHeight)];
            mRawBitmapData = new byte[(mDataWidth * mHeight) / 8];
            originalData = data.Bytes;
            convertArgbToGrayscale(mWidth, mHeight);
            createRawMonochromeData();

            return mRawBitmapData;
        }

        private void convertArgbToGrayscale(int width, int height)
        {
            byte On = 1;
            byte Off = 0;
            int k = 0;
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++, k++)
                {
                    // get one pixel color
                    var pixel = PixelColor(y, x);

                    if (pixel > 0)
                    {
                        mDataArray[k] = On;
                    }
                    else
                    {
                        mDataArray[k] = Off;
                    }
                }
                if (mDataWidth > width)
                {
                    for (int p = width; p < mDataWidth; p++, k++)
                    {
                        mDataArray[k] = 1;
                    }
                }
            }
        }

        int PixelColor(int x, int y)
        {
            int offset = ((mWidth * (mHeight - y - 1)) + x) * 4;
            int r = originalData[offset];
            int g = originalData[offset + 1];
            int b = originalData[offset + 2];
            int a = originalData[offset + 3];
            return r + g + b + a;
        }

        private void createRawMonochromeData()
        {
            int length = 0;
            for (int i = 0; i < mDataArray.Length; i = i + 8)
            {
                byte first = mDataArray[i];
                for (int j = 0; j < 7; j++)
                {
                    byte second = (byte)((first << 1) | mDataArray[i + j]);
                    first = second;
                }
                mRawBitmapData[length] = first;
                length++;
            }
        }

        public static string ConvertSignatureToBitmap(List<SixLabors.ImageSharp.Point> SignaturePoints)
        {
            if (SignaturePoints == null || SignaturePoints.Count == 0)
                return null;

            // Calculate bounding box
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var point in SignaturePoints)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            // Add padding
            float padding = 10;
            minX -= padding;
            minY -= padding;
            maxX += padding;
            maxY += padding;

            // Create bitmap dimensions
            int width = (int)Math.Ceiling(maxX - minX);
            int height = (int)Math.Ceiling(maxY - minY);

            // Create ImageSharp image
            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(ctx => ctx.BackgroundColor(Color.White));

                // Draw signature by setting pixels
                SixLabors.ImageSharp.Point? lastPoint = null;

                foreach (var point in SignaturePoints)
                {
                    if (point == SixLabors.ImageSharp.Point.Empty) // End of line
                    {
                        lastPoint = null;
                    }
                    else
                    {
                        int x = (int)(point.X - minX);
                        int y = (int)(point.Y - minY);

                        if (lastPoint.HasValue)
                        {
                            // Draw line between lastPoint and current point
                            DrawLine(image, lastPoint.Value.X - (int)minX, lastPoint.Value.Y - (int)minY, x, y);
                        }
                        else
                        {
                            // Draw single pixel/dot
                            if (x >= 0 && x < width && y >= 0 && y < height)
                                image[x, y] = Color.Black;
                        }

                        lastPoint = point;
                    }
                }

                // Save the image to a file
                var filePath = Path.GetTempFileName();
                image.SaveAsPng(filePath);

                return filePath;
            }
        }

        // Bresenham's line algorithm to draw line between two points
        private static void DrawLine(Image<Rgba32> image, int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Draw pixel with thickness (2x2 for stroke width of 2)
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int px = x0 + i;
                        int py = y0 + j;
                        if (px >= 0 && px < image.Width && py >= 0 && py < image.Height)
                            image[px, py] = Color.Black;
                    }
                }

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}

