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
    /// Class to contain information regarding a rectangle, circle, point and its color. 
    /// 
    /// Since the image recognition code acquires an EmguCV RotatedRect or CircleF structure
    /// for each rectangle or circle, we encapsulate that here and add on a field indicating 
    /// the color.
    /// 
    /// </summary>

    [SerializableAttribute]
    public class ColoredRotatedObject
    {
        // these are structures, a value type and can never be null
        private RotatedRect rotRect = new RotatedRect();
        private CircleF circleObj = new CircleF();
        private Point pointObj = new Point();

        // this is set on the constructor, only one of these object types can be populated
        private ColoredObjectType objectType = ColoredObjectType.COLORED_OBJECT_TYPE_UNKNOWN;
        // point objects are always assumed to have a radius of this
        public const int DEFAULT_POINT_RADIUS = 1;

        // the color of the center pixel. We use BGR rather than RGB because a lot of stuff
        // in EMGUCV prefers that
        private byte[] centerPixelBGRValue = new byte[3];

        // this is a C# enum, we default it to Black
        public static KnownColor DEFAULT_COLOR = KnownColor.Black;
        private KnownColor objColor = DEFAULT_COLOR;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rotRectIn">the rotated rect</param>
        public ColoredRotatedObject(RotatedRect rotRectIn)
        {
            rotRect = rotRectIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_RECT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rotRectIn">the rotated rect</param>
        /// <param name="objColorIn">the color of the rectangle</param>
        public ColoredRotatedObject(RotatedRect rotRectIn, KnownColor objColorIn)
        {
            rotRect = rotRectIn;
            objColor = objColorIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_RECT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circleObjIn">the circle</param>
        public ColoredRotatedObject(CircleF circleObjIn)
        {
            circleObj = circleObjIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circleObjIn">the circle</param>
        /// <param name="objColorIn">the color of the rectangle</param>
        public ColoredRotatedObject(CircleF circleObjIn, KnownColor objColorIn)
        {
            circleObj = circleObjIn;
            objColor = objColorIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pointObjIn">the point</param>
        public ColoredRotatedObject(Point pointObjIn)
        {
            pointObj = pointObjIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_POINT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pointObjIn">the point</param>
        /// <param name="objColorIn">the color of the rectangle</param>
        public ColoredRotatedObject(Point pointObjIn, KnownColor objColorIn)
        {
            pointObj = pointObjIn;
            objColor = objColorIn;
            objectType = ColoredObjectType.COLORED_OBJECT_TYPE_POINT;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the RGB color of the center pixel of the rectangle
        /// </summary>
        public byte[] CenterPixelBGRValue
        {
            get
            { 
                if(centerPixelBGRValue == null) centerPixelBGRValue = new byte[3];
                return centerPixelBGRValue;
            }
            set
            {
                centerPixelBGRValue = value;
                if (centerPixelBGRValue == null) centerPixelBGRValue = new byte[3];
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the color
        /// </summary>
        public KnownColor ObjColor
        {
            get { return objColor; }
            set { objColor = value; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets center of the object
        /// </summary>
        public Point Center 
        { 
            get 
            {
                if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE) return new Point((int)circleObj.Center.X, (int)circleObj.Center.Y);
                else if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_POINT) return new Point(pointObj.X, pointObj.Y);
                else return new Point((int)rotRect.Center.X, (int)rotRect.Center.Y);
            } 
            set 
            {
                if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE) circleObj.Center = value;
                else if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_POINT) pointObj = new Point((int)value.X, (int)value.Y);
                else rotRect.Center = value; 
            } 
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the angle between the horizontal axis and the first side (i.e. width) in degrees
        /// 
        /// Always 0 for circles.
        /// </summary>
        public float Angle 
        {
            get
            {
                if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE) return 0;
                else if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_POINT) return 0;
                else return rotRect.Angle;
            }
            set
            {
                if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE) { }
                else if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_POINT) { }
                else rotRect.Angle = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the radius for the enclosed object. For circles this is the 
        /// radius, for rectangles it is the largest radius of the enclosing circle
        /// 
        /// Always DEFAULT_POINT_RADIUS for points.
        /// </summary>
        public int Radius
        {
            get
            {
                if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_CIRCLE) return (int)circleObj.Radius;
                else if (ObjectType == ColoredObjectType.COLORED_OBJECT_TYPE_POINT) return DEFAULT_POINT_RADIUS;
                else return (int)Math.Sqrt(Math.Pow(rotRect.Size.Height/2,2) + Math.Pow(rotRect.Size.Width / 2, 2));
            }
            // we cannot set this is derived from the object passed in at construction time
            //set {}
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        //
        //  The type of object this is
        //
        public ColoredObjectType ObjectType { get => objectType; set => objectType = value; }

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ///// <summary>
        ///// Gets/Sets size of the box
        ///// </summary>
        //public SizeF Size 
        //{ 
        //    get 
        //    { 
        //        return rotRect.Size; 
        //    } 
        //    set 
        //    { 
        //        rotRect.Size = value; 
        //    } 
        //}

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ////
        //// Summary:
        ////     Get the 4 verticies of this Box.
        ////
        //// Returns:
        ////     The vertices of this RotatedRect
        //public PointF[] GetVertices() { return rotRect.GetVertices(); }

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ////
        //// Summary:
        ////     Get the minimum enclosing rectangle for this Box
        ////
        //// Returns:
        ////     The minimum enclosing rectangle for this Box
        //public Rectangle MinAreaRect() { return rotRect.MinAreaRect(); }

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ////
        //// Summary:
        ////     Shift the box by the specific amount
        ////
        //// Parameters:
        ////   x:
        ////     The x value to be offseted
        ////
        ////   y:
        ////     The y value to be offseted
        //public void Offset(int x, int y) { rotRect.Offset(x, y); }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// ToString() override
        /// </summary>
        public override string ToString()
        {
            return ObjectType.ToString() + ", " + ObjColor.ToString() + ", " + Center.ToString() + " (" + CenterPixelBGRValue[0].ToString() + "," + CenterPixelBGRValue[1].ToString() + "," + CenterPixelBGRValue[2].ToString() + ")";
        }

    }
}
