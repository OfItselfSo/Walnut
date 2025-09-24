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
    /// Class to contain information regarding a circleand its color. 
    /// 
    /// </summary>

    [SerializableAttribute]
    public class ColoredRotatedCircle : ColoredRotatedObject
    {
        // these are structures, a value type and can never be null
        private CircleF circleObj = new CircleF();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circleObjIn">the circle</param>
        public ColoredRotatedCircle(CircleF circleObjIn)
        {
            circleObj = circleObjIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circleObjIn">the circle</param>
        /// <param name="objColorIn">the color of the rectangle</param>
        public ColoredRotatedCircle(CircleF circleObjIn, KnownColor objColorIn)
        {
            circleObj = circleObjIn;
            ObjColor = objColorIn;
            ObjectType = ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets center of the object
        /// </summary>
        public override Point CenterPoint
        { 
            get 
            {
                return new Point((int)circleObj.Center.X, (int)circleObj.Center.Y);
            } 
            set 
            {
                circleObj.Center = value;
            } 
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the radius for thecircle. 
        /// </summary>
        public int Radius
        {
            get
            {
                return (int)circleObj.Radius;
            }
            // we cannot set this is derived from the object passed in at construction time
            //set {}
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The ToString
        /// </summary>
        public override string ToString()
        {
            return ObjectType.ToString() + ", " + ", center=(" + CenterPoint.X.ToString() + "," + CenterPoint.Y.ToString()+"), " + ObjColor.ToString() + ", BGR=(" + CenterPixelBGRValue[0].ToString() + "," + CenterPixelBGRValue[1].ToString() + "," + CenterPixelBGRValue[2].ToString() + ")";
        }

    }
}
