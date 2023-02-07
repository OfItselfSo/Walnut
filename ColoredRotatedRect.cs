using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;

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


namespace Walnut
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Class to contain information regarding a rectangle and its color. 
    /// 
    /// Since the image recognition code acquires an EmguCV RotatedRect structure
    /// for each rectangle, we encapsulate that here and add on a field indicating 
    /// the color.
    /// 
    /// </summary>

    public class ColoredRotatedRect
    {
        // this is a structure, a value type and will never be null
        private RotatedRect rotRect = new RotatedRect();
        // this is a C# enum, we default it to Black
        public static KnownColor DEFAULT_COLOR = KnownColor.Black;
        private KnownColor rectColor = DEFAULT_COLOR;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ColoredRotatedRect()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rotRectIn">the rotated rect</param>
        public ColoredRotatedRect(RotatedRect rotRectIn)
        {
            rotRect = rotRectIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rotRectIn">the rotated rect</param>
        /// <param name="rectColorIn">the color of the rectangle</param>
        public ColoredRotatedRect(RotatedRect rotRectIn, KnownColor rectColorIn)
        {
            rotRect = rotRectIn;
            rectColor = rectColorIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rotRectIn">the rotated rect</param>
        /// <param name="rectColorIn">the color of the rectangle</param>
        public ColoredRotatedRect(PointF center, SizeF size, float angle)
        {
            rotRect = new RotatedRect(center, size, angle);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the color
        /// </summary>
        public KnownColor RectColor
        {
            get { return rectColor; }
            set { rectColor = value; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     The center of the box
        public PointF Center { get { return rotRect.Center; } set { rotRect.Center = value; } }
       
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     The size of the box
        public SizeF Size { get { return rotRect.Size; } set { rotRect.Size = value; } }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     The angle between the horizontal axis and the first side (i.e. width) in degrees
        //
        // Remarks:
        //     Possitive value means counter-clock wise rotation
        public float Angle { get { return rotRect.Angle; } set { rotRect.Angle = value; } }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     Get the 4 verticies of this Box.
        //
        // Returns:
        //     The vertives of this RotatedRect
        public PointF[] GetVertices() { return rotRect.GetVertices(); }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     Get the minimum enclosing rectangle for this Box
        //
        // Returns:
        //     The minimum enclosing rectangle for this Box
        public Rectangle MinAreaRect() { return rotRect.MinAreaRect(); }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        // Summary:
        //     Shift the box by the specific amount
        //
        // Parameters:
        //   x:
        //     The x value to be offseted
        //
        //   y:
        //     The y value to be offseted
        public void Offset(int x, int y) { rotRect.Offset(x, y); }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// ToString() override
        /// </summary>
        public override string ToString()
        {
            return RectColor.ToString() + ", " + Center.ToString();
        }

    }
}
