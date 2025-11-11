using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WalnutCommon
{
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

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain utility functions for the Walnut project
    /// </summary>
    public class Utils
    {
        public const double SECONDS_PER_CYCLE = 700e-9;  // yep, 700 x 10^-9

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Cycles to Hz
        /// </summary>
        /// <param name="cycles">the number of cycles</param>
        public static uint ConvertCyclesToHz(uint cycles)
        {
            if (cycles == 0) return 0;
            return (uint)(1 / (SECONDS_PER_CYCLE * ((double)cycles)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Hz to Cycles
        /// </summary>
        /// <param name="hz">the number of Hz</param>
        public static uint ConvertHzToCycles(uint hz)
        {
            if (hz == 0) return 0;
            return (uint)(1 / (SECONDS_PER_CYCLE * ((double)hz)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts an integer direction to a string 
        /// </summary>
        /// <param name="direction">the direction</param>
        public static string DirectionAsStr(uint direction)
        {
            if (direction <= 0) return ("CW");
            else return "CCW";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Toggles the sign of a number while keeping the value
        /// 
        /// Credit: 
        ///   https://stackoverflow.com/questions/1348080/convert-a-positive-number-to-negative-in-c-sharp
        /// 
        /// </summary>
        /// <param name="num1">the number to toggle</param>
        /// <returns>the number with a different sign</returns>
        public static double ToggleSign(double num1)
        {
            return num1 * -1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if two values are the same sign
        /// 
        /// Credit: 
        ///   https://codereview.stackexchange.com/questions/107635/checking-if-two-numbers-have-the-same-sign
        /// 
        /// </summary>
        /// <param name="num1">the first number to test</param>
        /// <param name=" num2">the second number to test</param>
        /// <returns>true if they are the same sign, false if not</returns>
        public static bool AreSameSign(double num1, double num2)
        {
            return ((num1 < 0) == (num2 < 0));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the octant of the dynamic point relative to the static point. The
        /// octants are numbered sequentially clockwise starting with the vertical 
        /// positive Y and positive x
        /// 
        /// Credit: Just used the naive implementation from
        ///   https)//codereview.stackexchange.com/questions/95550/determining-which-octant-has-a-specific-point
        /// 
        /// </summary>
        /// <param name="staticPoint">the point assumed to be the more static of the two</param>
        /// <param name="dynamicPoint">the point assumed to be the more dynamic of the two</param>
        /// <returns>a enum indicating the octant of the dynamic point relative to the static point</returns>
        public static OctantEnum GetRelativeOctant(PointF staticPoint, PointF dynamicPoint)
        {
            double spX = staticPoint.X;
            double spY = staticPoint.Y;
            double dyX = dynamicPoint.X;
            double dyY = dynamicPoint.Y;

            if (dyX > spX)
            {
                if (dyY > spY)
                {
                    if ((dyY - spY) < (dyX - spX)) return OctantEnum.OCT_1;
                    else return OctantEnum.OCT_0;
                }
                else
                {
                    if ((spY - dyY) < (dyX - spX)) return OctantEnum.OCT_2;
                    else return OctantEnum.OCT_3;
                }
            }
            else
            {
                if (dyY > spY)
                {
                    if ((dyY - spY) < (spX - dyX)) return OctantEnum.OCT_6;
                    else return OctantEnum.OCT_7;
                }
                else
                {
                    if ((spY - dyY) < (spX - dyX)) return OctantEnum.OCT_5;
                    else return OctantEnum.OCT_4;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the octant of the dynamic point relative to the static point. The
        /// octants are numbered sequentially clockwise starting with the vertical 
        /// positive Y and positive x
        /// 
        /// </summary>
        /// <param name="staticPoint">the point assumed to be the more static of the two</param>
        /// <param name="dynamicPoint">the point assumed to be the more dynamic of the two</param>
        /// <returns>a enum indicating the quadran of the dynamic point relative to the static point</returns>
        public static QuadrantEnum GetRelativeQuadrant(PointF staticPoint, PointF dynamicPoint)
        {
            double spX = staticPoint.X;
            double spY = staticPoint.Y;
            double dyX = dynamicPoint.X;
            double dyY = dynamicPoint.Y;

            if (dyX > spX)
            {
                if (dyY > spY)
                {
                    return QuadrantEnum.QUAD_0;
                }
                else
                {
                    return QuadrantEnum.QUAD_1;
                }
            }
            else
            {
                if (dyY > spY)
                {
                    return QuadrantEnum.QUAD_3;
                }
                else
                {
                    return QuadrantEnum.QUAD_2;

                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets an interator which returns a clockwise spiral sequence of points about given centerpoint on 
        /// a specific size of grid.
        /// 
        /// NOTE: this returns an iterator and uses the "yield return" pattern. 
        /// 
        /// The specified centerpoint must be on the grid
        /// 
        /// Credit: Adapted from the "mike" response at
        /// https://stackoverflow.com/questions/398299/looping-in-a-spiral
        /// 
        /// The returned iterator will generate a sequence of points. There will be 
        /// no duplicates and no values returned that are off the grid. The order of the coordinates
        /// returned will be a clockwise spiral starting from the specified centerpoint and the 
        /// first coord after that is centerPoint.X+1, centerPoint.Y+0
        /// 
        /// </summary>
        /// <param name="centerPoint">the centerpoint around which the spiral happens</param>
        /// <param name="gridSize">the size of the grid</param>
        /// <returns>a "yield return" iterator which will reliably produce a sequence of points spiralling clockwise around the  
        /// centerpoint but which will always be on the grid</returns>
        public static IEnumerable<Point> GetSpiralGrid(Point centerPoint, Size gridSize)
        {
            int x = 0;
            int y = 0;
            int d = 1;
            int m = 1;
            int count = 0;

            // some sanity checks
            if((centerPoint.X<0) || (centerPoint.Y<0) || (centerPoint.X>=gridSize.Width) || (centerPoint.Y >= gridSize.Height))
            {
                // cannot be having this
                throw new Exception("The centerpoint (" + centerPoint.X.ToString() + "," + centerPoint.Y.ToString() + ") is not on the grid of size (" + gridSize.Width.ToString() + ", " + gridSize.Height.ToString() + ")");
            }

            // set up our boundaries, this is what enables us to not do a 
            // yield return on points which are off the callers grid
            int minX = 0 - centerPoint.X;
            int minY = 0 - centerPoint.Y;
            int maxX = gridSize.Width - centerPoint.X;
            int maxY = gridSize.Height - centerPoint.Y;

            // because we can have a center point anywhere on the grid and the grid can be of any size 
            // we have to potentially loop over a very large area. The points off the grid are filtered out
            // but we still have to cap the maximum number - otherwise we have an endless loop.
            //
            // the algorythm below covers all possible spiral squares for a given gridsize for any centerpoint
            int maximumIterations = (int)Math.Pow((int)Math.Max(gridSize.Width, gridSize.Height), 2) * 4;

            while (count < maximumIterations)
            {
                while (2 * x * d < m)
                {
                    if ((x >= minX) && (x < maxX) && (y >= minY) && (y < maxY)) yield return new Point(x + centerPoint.X, y + centerPoint.Y);
                    count++;
                    x = x + d;
                }
                while (2 * y * d < m)
                {
                    if ((x >= minX) && (x < maxX) && (y >= minY) && (y < maxY)) yield return new Point(x + centerPoint.X, y + centerPoint.Y);
                    //Console.WriteLine($"({x}, {y}) -- {count.ToString()}");
                    y = y + d;
                    count++;
                }
                d = -1 * d;
                m = m + 1;
            }
            if ((x >= minX) && (x < maxX) && (y >= minY) && (y < maxY)) yield return new Point(x + centerPoint.X, y + centerPoint.Y);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to test the GetSpiralGrid yield iterator. Also shows how to use 
        /// it properly.
        /// 
        /// </summary>
        /// <param name="centerX">x center must be on the grid</param>
        /// <param name="centerY">y center must be on the grid</param>
        /// <param name="gridX">max X coord of the grid</param>
        /// <param name="gridY">max Y coord of the grid</param>
        public static void TestSpiralGrid(int gridX, int gridY, int centerX, int centerY)
        {
            // we fill in this for every point we return and count them at the end
            int[,] testGrid = new int[gridX, gridY];

            // the centerpoint
            Point centerPoint = new Point(centerX, centerY);

            // this does the work
            var pixels = Utils.GetSpiralGrid(new Point(centerX, centerY), new Size(gridX, gridY));

            int count = 0;
            int added = 0;
            int skipped = 0;
            foreach (Point i in pixels)
            {
                count++;
                int realX = i.X;
                int realY = i.Y;

                // note GetSpiralGrid sets up boundaries. We should never see any skips on the 
                // yield return
                if (realX < 0 || realY < 0)
                {
                    skipped++;
                    Console.WriteLine($"Skipped: ({i.X}, {i.Y}), ({realX}, {realY}) -- {count.ToString()}");
                    continue;
                }
                if (realX >= gridX || realY >= gridY)
                {
                    skipped++;
                    Console.WriteLine($"Skipped: ({i.X}, {i.Y}), ({realX}, {realY}) -- {count.ToString()}");
                    continue;
                }

                // add it now. We should fill in our testGrid by the time the loop ends. there should be 
                // no duplicates and no values returned that are off the grid. The order of the coordinates
                // returned will be a clockwise spiral starting from the specified centerpoint and the 
                // first coord after that is X+1, Y+0
                testGrid[realX, realY] = 1;
                // Console.WriteLine($"Added: ({realX}, {realY}) -- {count.ToString()}");
            }
            for (int i = 0; i < gridX; i++)
            {
                for (int j = 0; j < gridY; j++)
                {
                    if (testGrid[i, j] != 0) added++;
                }

            }
            Console.WriteLine($"Caller: count={count}, skipped={skipped}, added={added}, total={gridX * gridY}");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Checks if two points are adjacent. The function determines whether they 
        ///  are neighbors by looking at the distance between the positions along 
        ///  both axes, also callled the Manhattan distance. The sum of the two 
        ///  distances must be exactly 1 if the positions are adjacent.
        ///  
        ///  NOTE: diagonals are not considered adjacent
        ///  
        /// Credit: Taken from this code
        ///   https://snipplr.com/view/64354/check-if-coordinates-are-adjacent
        ///  
        /// </summary>
        /// <param name="point1">first point</param>
        /// <param name="point2">second point</param>
        public static bool PointsAreAdjacent(Point point1, Point point2)
        {
            int dx = Math.Abs(point1.X - point2.X);
            int dy = Math.Abs(point1.Y - point2.Y);
            return (dx + dy == 1);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a color to a BGR byte[4]
        /// 
        /// </summary>
        /// <param name="pixel">the pixel color</param>
        public static byte[] RBGPixelToBGR(Color pixel)
        {
            return new byte[4] {pixel.A, pixel.B, pixel.G, pixel.R};            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a cross at a point in a specific color and length
        /// </summary>
        /// <param name="graphicsObj">the grapics object to draw on</param>
        /// <param name="armLength">the lenght of the arm in the cross</param>
        /// <param name="centerPoint">the centerpoint of the cross</param>
        /// <param name="colorOfCross">the color of the cross</param>
        public static void DrawCrossOnPoint(Graphics graphicsObj, Point centerPoint, int armLength, Pen colorOfCross)
        {
            if (graphicsObj == null) return;
            if (centerPoint == null) return;
            if (armLength <= 0) return;

            // apparently we do not need bounds checking here. The call to DrawLine does it
            Point horizStartPoint = new Point(centerPoint.X - armLength, centerPoint.Y);
            Point horizEndPoint = new Point(centerPoint.X + armLength, centerPoint.Y);
            Point vertStartPoint = new Point(centerPoint.X, centerPoint.Y - armLength);
            Point vertEndPoint = new Point(centerPoint.X, centerPoint.Y + armLength);

            // draw the horizontal line
            graphicsObj.DrawLine(colorOfCross, horizStartPoint, horizEndPoint);
            // draw the vertical line
            graphicsObj.DrawLine(colorOfCross, vertStartPoint, vertEndPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a horizontal line at a center point in a specific color and length
        /// </summary>
        /// <param name="graphicsObj">the grapics object to draw on</param>
        /// <param name="lineLength">the length of the line</param>
        /// <param name="centerPoint">the centerpoint of the cross</param>
        /// <param name="colorOfCross">the color of the cross</param>
        public static void DrawHorizLineFromCenterPoint(Graphics graphicsObj, Point centerPoint, int lineLength, Pen colorOfCross)
        {
            if (graphicsObj == null) return;
            if (centerPoint == null) return;
            if (lineLength <= 0) return;

            // apparently we do not need bounds checking here. The call to DrawLine does it
            Point horizStartPoint = new Point(centerPoint.X - lineLength/2, centerPoint.Y);
            Point horizEndPoint = new Point(centerPoint.X + lineLength/2, centerPoint.Y);

            // draw the horizontal line
            graphicsObj.DrawLine(colorOfCross, horizStartPoint, horizEndPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a vertical line at a center point in a specific color and length
        /// </summary>
        /// <param name="graphicsObj">the grapics object to draw on</param>
        /// <param name="lineLength">the length of the line</param>
        /// <param name="centerPoint">the centerpoint of the cross</param>
        /// <param name="colorOfCross">the color of the cross</param>
        public static void DrawVerticalLineFromCenterPoint(Graphics graphicsObj, Point centerPoint, int lineLength, Pen colorOfCross)
        {
            if (graphicsObj == null) return;
            if (centerPoint == null) return;
            if (lineLength <= 0) return;

            // apparently we do not need bounds checking here. The call to DrawLine does it
            Point vertStartPoint = new Point(centerPoint.X, centerPoint.Y - lineLength / 2);
            Point vertEndPoint = new Point(centerPoint.X, centerPoint.Y + lineLength / 2);

            // draw the vertical line
            graphicsObj.DrawLine(colorOfCross, vertStartPoint, vertEndPoint);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        static extern int GetPixel(IntPtr hDC, int x, int y);
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the color of a pixel from a controls handle. Fairly resource
        /// intensive. If you need to get a lot of pixels do it a different way
        /// 
        /// Credit: Derived from https://www.csharp411.com/c-getpixel-and-setpixel/
        /// 
        /// </summary>
        /// <param name="handle">the controls handle</param>
        /// <param name="x">the controls x coord</param>
        /// <param name="y">the controls y coord</param>
        /// <returns>The RGBA color of the pixel or Color.Empty for fail</returns>
        static public Color GetPixelFromHandle(IntPtr handle, int x, int y)
        {
            Color color = Color.Empty;
            if (handle != null)
            {
                IntPtr hDC = GetDC(handle);
                int colorRef = GetPixel(hDC, x, y);
                color = Color.FromArgb(
                    (int)(colorRef & 0x000000FF),
                    (int)(colorRef & 0x0000FF00) >> 8,
                    (int)(colorRef & 0x00FF0000) >> 16);
                ReleaseDC(handle, hDC);
            }
            return color;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a point to (x,y) text. A slightly more pretty ToString. Will
        /// test for null input and return "";
        /// </summary>
        /// <param name="pointIn">the point to convert</param>
        static public string ConvertPointToBracketText(Point pointIn)
        {
            if (pointIn == null) return "";
            return "("+pointIn.X.ToString()+","+pointIn.Y.ToString()+")";
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a (x,y) text to a Point. Any problems and we return point()
        /// </summary>
        /// <param name="bracketText">the text to convert</param>
        /// <returns>the converted point or Point() for fail</returns>
        static public Point ConvertBracketTextToPoint(string bracketText)
        {
            if(bracketText==null) return new Point();
            if(bracketText=="") return new Point();

            try
            {
                // get the draw point out of the brackets
                string[] xAndYCoords = bracketText.Replace("(", "").Replace(")", "").Split(',');
                if (xAndYCoords.Length != 2) return new Point();
                Point workingPoint = new Point(Convert.ToInt32(xAndYCoords[0].Replace(" ", "")), Convert.ToInt32(xAndYCoords[1].Replace(" ", "")));
                return workingPoint;
            }
            catch
            {
                return new Point();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a (R,G,B) text to a color. Any problems and we return null
        /// </summary>
        /// <param name="bracketText">the text to convert</param>
        /// <returns>the converted color or null for fail</returns>
        static public Color? ConvertBracketTextToColor(string bracketText)
        {
            if (bracketText == null) return null;
            if (bracketText == "") return null;

            try
            {
                // get the draw point out of the brackets
                string[] rgbColors = bracketText.Replace("(", "").Replace(")", "").Split(',');
                if (rgbColors.Length != 3) return null;
                Color workingColor = Color.FromArgb(255, Convert.ToInt32(rgbColors[0].Replace(" ", "")), Convert.ToInt32(rgbColors[1].Replace(" ", "")), Convert.ToInt32(rgbColors[2].Replace(" ", "")));
                return workingColor;
            }
            catch
            {
                return null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a color to (R,G,B) text Any problems and we return ""
        /// </summary>
        /// <param name="color">the color to convert</param>
        /// <returns>the color as text or "" for fail</returns>
        public static string ConvertColorToRGBBracketText(Color color)
        {
            if (color == null) return "";
            return "(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + ")";
        }
    }
}
