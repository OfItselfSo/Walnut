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


namespace WalnutCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Class to contain information about a line 
    /// 
    /// </summary>

    [SerializableAttribute]
    public class ColoredRotatedLine : ColoredRotatedObject
    {
        private Point centerPoint = new Point();
        private int lineLength = 0;

        // the color of the center pixel. We use BGR rather than RGB because a lot of stuff
        // in EMGUCV prefers that
        private byte[] centerPixelBGRValue = new byte[3];

        public const float HORIZONTAL_LINE_ANGLE = 0;
        public const float VERTICAL_LINE_ANGLE = 180;
        private float angle = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="centerPointIn">the centerPoint</param>
        public ColoredRotatedLine(Point centerPointIn)
        {
            CenterPoint = centerPointIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_LINE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="centerPointIn">the centerPoint</param>
        /// <param name="lineLengthIn">the Line length</param>
        public ColoredRotatedLine(Point centerPointIn, int lineLengthIn)
        {
            CenterPoint = centerPointIn;
            LineLength = lineLengthIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_LINE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets center point of the object, will never get/set null
        /// </summary>
        public override Point CenterPoint
        {
            get
            {
                if (centerPoint == null) centerPoint = new Point();
                return centerPoint;
            }
            set
            {
                centerPoint = value;
                if (centerPoint == null) centerPoint = new Point();
            }
        }

        public int LineLength { get => lineLength; set => lineLength = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The ToString()
        /// </summary>
        public override string ToString()
        {
            return "center=(" + CenterPoint.X.ToString() + "," + CenterPoint.Y.ToString() +"), len=" + LineLength.ToString() + ", center=(" + CenterPoint.X.ToString() + "," + CenterPoint.Y.ToString() + "), " + ObjColor.ToString() + ", BGR=(" + CenterPixelBGRValue[0].ToString() + "," + CenterPixelBGRValue[1].ToString() + "," + CenterPixelBGRValue[2].ToString() + ")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the angle between the horizontal axis and the first side (i.e. width) in degrees
        /// 
        /// Always 0 for circles.
        /// </summary>
        public override float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
            }
        }

    }
}
