using Emgu.CV.Reg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace WalnutCommon
{

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Class to wrap a bitmap and make it faster to do pixel by pixel access.
    /// In other words GetPixel and SetPixel are much faster. (They are improbably
    /// slow on the standard Bitmap). 
    /// 
    /// This class provides faster access at the cost of having the bitmap data up in 
    /// managed memory. (one int per pixel). Other than that, it functions the same and 
    /// you can get access to the wrapped bitmap itself by calling the BitMap property
    /// 
    /// You should Dispose() when done or you will have a memory leak until the GC gets to it
    /// 
    /// Credit: Largely based on the code from 
    ///     //https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
    /// 
    /// </summary>
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        protected GCHandle BitsHandle { get; private set; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, creates a new DirectBitmap
        /// </summary>
        /// <param name="height">the height</param>
        /// <param name="width">the width</param>
        public DirectBitmap(int width, int height)
        {
            // build the new bitmap with memory access
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, creates a new DirectBitmap
        /// </summary>
        /// <param name="height">the height</param>
        /// <param name="width">the width</param>
        public DirectBitmap(int width, int height, IntPtr pDest)
        {
            // build the new bitmap with memory access
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            Marshal.Copy(pDest, Bits, 0, width * height);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, creates a new DirectBitmap from a file on a disk
        /// </summary>
        /// <param name="filenameAndPath">the filename and path</param>
        public DirectBitmap(string filenameAndPath)
        {
            // create a temporary bitmap using the filename and path
            Bitmap tmpBitmap = new Bitmap(filenameAndPath);

            // build the new bitmap with memory access
            Width = tmpBitmap.Width;
            Height = tmpBitmap.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

            // copy the temporary bitmap over onto the new bitmap
            using (Graphics graphics = Graphics.FromImage(Bitmap))
            {
                Rectangle imageRectangle = new Rectangle(0, 0, tmpBitmap.Width, tmpBitmap.Height);
                graphics.DrawImage(tmpBitmap, imageRectangle, imageRectangle, GraphicsUnit.Pixel);
            }
            // clear this 
            tmpBitmap.Dispose();
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Load the overlay from disk in a proprietary binary format. There are terrible
        /// troubles reading .pngs etc into a bitmap. The process simply refuses to properly deal 
        /// with an alpha channel. Doesn't matter what you try you cannot convince a 
        /// Bitmap to properly do it
        /// 
        /// This call just reads a simple binary format and stuffs the data in the Bits array
        /// directly
        /// 
        /// NOTE: the filename should have been checked for validity long before this call
        ///       we do not do it here.
        ///       
        /// Credit: https://www.reddit.com/r/csharp/comments/1bi5am7/how_to_save_an_integer_array_as_a_file_with/
        /// </summary>
        /// <param name="filenameAndPath">the filename and path</param>
        public void LoadAsBinary(string overlayImagePathAndFilename)
        {
            // Read array from a binary file
            using (var fs = new FileStream(overlayImagePathAndFilename, FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                // Read the length of the array
                int length = reader.ReadInt32();
                if (length != Bits.Length) return;

                // Read Each of your integers 
                for (int i = 0; i < length; i++)
                {
                    Bits[i] = reader.ReadInt32();
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Save the overlay to disk in a proprietary binary format. There are terrible
        /// troubles saving .pngs etc from a bitmap. The process simply refuses to properly deal 
        /// with an alpha channel. Doesn't matter what you try you cannot convince a 
        /// Bitmap to properly do it
        /// 
        /// This call just writes a simple binary format from the Bits array 
        /// directly
        /// 
        /// NOTE: the filename should have been checked for validity long before this call
        ///       we do not do it here.
        /// 
        /// Credit: https://www.reddit.com/r/csharp/comments/1bi5am7/how_to_save_an_integer_array_as_a_file_with/
        /// </summary>
        /// <param name="filenameAndPath">the filename and path</param>
        public void SaveAsBinary(string overlayImagePathAndFilename)
        {
            // Write array to a binary file
            using (var fs = new FileStream(overlayImagePathAndFilename, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                // Write the length of the array first
                writer.Write(Bits.Length);
                foreach (var number in Bits)
                {
                    writer.Write(number);
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///    Saves the overlay bitmap to disk as a PNG. We do not check if already 
        ///    exists. These sort of checks should be already done
        ///  
        /// </summary>
        /// <param name="overlayImagePathAndFilename">the path and filename</param>
        public void SaveAsPNG(string overlayImagePathAndFilename)
        {
            if ((overlayImagePathAndFilename == null) || (overlayImagePathAndFilename.Length < 10)) throw new Exception("Invalid overlay path and filename");

            // we do it this way because it is quick and mostly avoids locking issues
            DirectBitmap bmp = new DirectBitmap(Width, Height);
            bmp.CopyFrom(this);
            // save it
            bmp.Bitmap.Save(overlayImagePathAndFilename, ImageFormat.Png);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Copy the contents of a DirectBitmap onto this one.
        /// 
        /// NOTE this is not a very fast copy, should only be done as a one off. The 
        /// height and width must be the same
        /// </summary>
        /// <param name="dBitmap">the DirectBitmap to copy</param>
        public void CopyFrom(DirectBitmap dBitmap)
        {
            if (dBitmap == null) throw new ArgumentNullException();
            if (dBitmap.Width != Width || dBitmap.Height != Height) throw new ArgumentException("Width and Height are not equal");

            // copy the incoming bitmap over onto the new bitmap
            using (Graphics graphics = Graphics.FromImage(Bitmap))
            {
                // it appears the default composting mode is SourceOver (ie alpha blend)
                graphics.CompositingMode = CompositingMode.SourceCopy;
                Rectangle imageRectangle = new Rectangle(0, 0, dBitmap.Width, dBitmap.Height);
                graphics.DrawImage(dBitmap.Bitmap, imageRectangle, imageRectangle, GraphicsUnit.Pixel);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finalizer. we must dispose
        /// </summary>
        ~DirectBitmap()
        {
            Dispose();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// SetPixel  - sets a color on a pixel
        /// </summary>
        /// <param name="colour">the color to set</param>
        /// <param name="x">the x coord</param>
        /// <param name="y">the y coord</param>
        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// SetPixelByIndex  - sets a color on a pixel
        /// </summary>
        /// <param name="colour">the color to set</param>
        /// <param name="index">the index into the Bits array</param>
        public void SetPixelByIndex(int index, Color colour)
        {
            int col = colour.ToArgb();
            Bits[index] = col;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// SetPixel  - sets a color on a pixel
        /// </summary>
        /// <param name="colour">the color to set</param>
        /// <param name="x">the x coord</param>
        /// <param name="y">the y coord</param>
        public void SetPixelInvertedY(int x, int y, Color colour)
        {
            int index = x + ((Height - y) * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// GetPixel  - gets a color of a pixel
        /// </summary>
        /// <param name="x">the x coord</param>
        /// <param name="y">the y coord</param>
        /// <returns>the pixel color</returns>
        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// GetPixelByIndex  - gets a color of a pixel 
        /// </summary>
        /// <param name="index">the index into the Bits array</param>
        /// <returns>the pixel color</returns>
        public Color GetPixelByIndex(int index)
        {
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// GetPixel  - gets a color of a pixel
        /// </summary>
        /// <param name="x">the x coord</param>
        /// <param name="y">the y coord</param>
        /// <returns>the pixel color</returns>
        public Color GetPixelInvertedY(int x, int y)
        {
            int index = x + ((Height-y) * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a color on the bitmap to another color
        /// </summary>
        /// <param name="color">the color to find and convert</param>
        /// <param name="toColor">the color to convert to</param>
        /// <returns>the pixel color</returns>
        public void ConvertColorToColor(Color color, Color toColor)
        {
            int colorARGB = color.ToArgb();
            int colorToARGB = toColor.ToArgb();
            // simple loop does it
            for(int i=0; i< Bits.Length; i++)
            {
                if (Bits[i] == colorARGB) Bits[i] = (colorToARGB);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a color on the bitmap to another color within a rectangle. We 
        /// require the rectangle to be within the bitmap. We do not permit negative
        /// height or widths or a height and width that will take us beyond the boundary
        /// of the bitmap
        /// 
        /// NOTE The rect here needs non inverted coordinates
        /// 
        /// NOTE we return the lowest alpha value we found of all the pixels masked. 
        /// 
        /// </summary>
        /// <param name="color">the color to find and convert</param>
        /// <param name="toColor">the color to convert to</param>
        /// <param name="rect">the rect on the bitmap in which we operate (non inverted coords)</param>
        /// <param name="preserveAlpha">if true we ignore alpha when comparing and preserve it over the change</param>
        /// <returns>the lowest alpha value found while masking</returns>
        public byte ConvertColorToColorInRect(Color color, Color toColor, Rectangle rect, bool preserveAlpha)
        {
            int colorARGB;
            int colorToARGB;
            const byte DEFAULT_ALPHA = 255;
            byte lowestAlpha = DEFAULT_ALPHA;

            // sanity checks
            if (rect == null) return lowestAlpha;
            if (rect.X < 0) return lowestAlpha;
            if (rect.Y < 0) return lowestAlpha;
            if (rect.Width <= 0) return lowestAlpha;
            if (rect.Height <= 0) return lowestAlpha;
            if ((rect.X + rect.Width) >= this.Width) return lowestAlpha;
            if ((rect.Y + rect.Height) >= this.Height) return lowestAlpha;

            // do we want to preserve the alpha channel?
            if (preserveAlpha == true)
            {
                // set up
                colorARGB = color.ToArgb() & 0x00FFFFFF;  // no alpha channel
                colorToARGB = toColor.ToArgb() & 0x00FFFFFF;  // no alpha channel

                // nested loops do it, our rectangle explicitly maps to a collection of pixels on the bitmap
                // we just have to calculate the offsets in the array
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        // calc the pixel location in the array
                        int pixelLocation = y * this.Width + x;
                        // get the current alpha from this position even if we do not need to change the color
                        byte currentAlpha = Utils.GetAlphaChannelFromARGB((uint)(Bits[pixelLocation]));
                        // remember it if we need to do so, we do not update with an alpha of 0
                        if ((currentAlpha!=0) && (currentAlpha < lowestAlpha)) lowestAlpha = currentAlpha;
                        // make the modification
                        if ((Bits[pixelLocation] & 0x00FFFFFF) == colorARGB)
                        {
                            // mask the value
                            Bits[pixelLocation] = (int)((uint)colorToARGB | (uint)(Bits[pixelLocation] & 0xFF000000));
                        }
                    }
                }
            }
            else
            {
                // set up
                colorARGB = color.ToArgb();
                colorToARGB = toColor.ToArgb();

                // nested loops do it, our rectangle explicitly maps to a collection of pixels on the bitmap
                // we just have to calculate the offsets in the array
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        // calc the pixel location in the array
                        int pixelLocation = y * this.Width + x;
                        // get the current alpha from this position even if we do not need to change the color
                        byte currentAlpha = Utils.GetAlphaChannelFromARGB((uint)(Bits[pixelLocation]));
                        // remember it if we need to do so, we do not update with an alpha of 0
                        if ((currentAlpha != 0) && (currentAlpha < lowestAlpha)) lowestAlpha = currentAlpha;
                        // make the modification
                        if (Bits[pixelLocation] == colorARGB)
                        {
                            // mask the value
                            Bits[pixelLocation] = (colorToARGB);
                        }
                    }
                }
            }
            // return what we found
            return lowestAlpha;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the alpha value of a color within a rectangle. We 
        /// require the rectangle to be within the bitmap. We do not permit negative
        /// height or widths or a height and width that will take us beyond the boundary
        /// of the bitmap
        /// 
        /// NOTE The rect here needs non inverted coordinates
        /// 
        /// Note we ignore the existing alpha value when comparing colors
        /// </summary>
        /// <param name="color">the color to find and adjust</param>
        /// <param name="newAlphaValue">the new alpha value</param>
        /// <param name="rect">the rect on the bitmap in which we operate (non inverted coords)</param>
        /// <returns>the pixel color</returns>
        public void SetAlphaValueForColorInRect(Color color, int newAlphaValue, Rectangle rect)
        {
            // sanity checks
            if (rect == null) return;
            if (rect.X < 0) return;
            if (rect.Y < 0) return;
            if (rect.Width <= 0) return;
            if (rect.Height <= 0) return;
            if ((rect.X + rect.Width) >= this.Width) return;
            if ((rect.Y + rect.Height) >= this.Height) return;

            // clear out the alpha channel, for comparisons
            int comparisonColorARGB = (color.ToArgb()) & 0x00FFFFFF;
            // set up the replacement color
            int replacementColorARGB = comparisonColorARGB | (newAlphaValue << 24);

            // nested loops do it, our rectangle explicitly maps to a collection of pixels on the bitmap
            // we just have to calculate the offsets in the array
            for (int x = rect.X; x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    // calc the pixel location in the array
                    int pixelLocation = y * this.Width + x;
                    // make the modification
                    if ((Bits[pixelLocation] & 0x00FFFFFF) == comparisonColorARGB) Bits[pixelLocation] = replacementColorARGB;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Disposes of memory. We NEED to do this as we have allocated and locked
        /// memory in the constructor
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
