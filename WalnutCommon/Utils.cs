using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public const double SECONDS_PER_CYCLE = 660e-9;  // yep, 660 x 10^-9

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Cycles to Hz
        /// </summary>
        /// <param name="cycles">the number of cycles</param>
        public static uint ConvertCyclesToHz(uint cycles)
        {
            if (cycles == 0) return ConvertCyclesToHz(ServerClientData.DEFAULT_SPEED);
            return (uint)(1 / (SECONDS_PER_CYCLE * ((double)cycles)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Hz to Cycles
        /// </summary>
        /// <param name="hz">the number of Hz</param>
        public static uint ConvertHzToCycles(uint hz)
        {
            if (hz == 0) return ServerClientData.DEFAULT_SPEED;
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
    }
}
