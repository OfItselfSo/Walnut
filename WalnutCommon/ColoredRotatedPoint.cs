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
    /// Class to contain information regarding a point and its color. 
    /// 
    /// </summary>

    [SerializableAttribute]
    public class ColoredRotatedPoint : ColoredRotatedObject
    {
        private Point centerPoint = new Point();
        // the color of the center pixel. We use BGR rather than RGB because a lot of stuff
        // in EMGUCV prefers that
        private byte[] centerPixelBGRValue = new byte[3];

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pointObjIn">the point</param>
        public ColoredRotatedPoint(Point pointObjIn)
        {
            CenterPoint = pointObjIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_POINT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pointObjIn">the point</param>
        /// <param name="objColorIn">the color of the rectangle</param>
        public ColoredRotatedPoint(Point pointObjIn, KnownColor objColorIn)
        {
            CenterPoint = pointObjIn;
            ObjColor = objColorIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_POINT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets point object, will never get/set null
        /// </summary>
        public override Point CenterPoint 
        { 
            get 
            {
                if(centerPoint == null) {centerPoint = new Point(); }   
                return centerPoint;
            } 
            set 
            {
                centerPoint = value;
                if (centerPoint == null) { centerPoint = new Point(); }
            }
        }
        public override string ToString()
        {
            return ObjectType.ToString() + ", " + ", center=(" + CenterPoint.X.ToString() + "," + CenterPoint.Y.ToString() + "), " + ObjColor.ToString() + ", BGR=(" + CenterPixelBGRValue[0].ToString() + "," + CenterPixelBGRValue[1].ToString() + "," + CenterPixelBGRValue[2].ToString() + ")";
        }

    }
}
