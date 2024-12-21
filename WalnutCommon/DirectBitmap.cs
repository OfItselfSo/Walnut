using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Reg;
using System.Drawing.Drawing2D;

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
