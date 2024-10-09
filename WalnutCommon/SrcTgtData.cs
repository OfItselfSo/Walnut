using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class SrcTgtData
    {
        private PointF srcPoint = new PointF(float.NaN, float.NaN);
        private PointF tgtPoint = new PointF(float.NaN, float.NaN);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public SrcTgtData()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="srcPointIn">the source point</param>
        /// <param name="tgtPointIn">the target point</param>
        public SrcTgtData(PointF srcPointIn, PointF tgtPointIn)
        {
            SrcPoint = srcPointIn;
            TgtPoint = tgtPointIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="srcRectIn">the source Rect</param>
        /// <param name="tgtRectIn">the target Rect</param>
        public SrcTgtData(ColoredRotatedRect srcRectIn, ColoredRotatedRect tgtRectIn)
        {
            if(srcRectIn!=null) SrcPoint = srcRectIn.Center;
            if(tgtRectIn!=null) TgtPoint = tgtRectIn.Center;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the src data is populated
        /// </summary>
        /// <returns>true - is populated, false is not</returns>
        public bool SrcIsPopulated()
        {
            if (float.IsNaN(SrcPoint.X) == true) return false;
            if (float.IsNaN(SrcPoint.Y) == true) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the tgt data is populated
        /// </summary>
        /// <returns>true - is populated, false is not</returns>
        public bool TgtIsPopulated()
        {
            if (float.IsNaN(TgtPoint.X) == true) return false;
            if (float.IsNaN(TgtPoint.Y) == true) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the class is populated
        /// </summary>
        /// <returns>true - is populated, false is not</returns>
        public bool IsFullyPopulated()
        {
            if (SrcIsPopulated() == false) return false;
            if (TgtIsPopulated() == false) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the class has at least one of the coords populated
        /// </summary>
        /// <returns>true - is populated, false is not</returns>
        public bool IsMinimallyPopulated()
        {
            // we have to have one
            if ((SrcIsPopulated() == false) && (TgtIsPopulated() == false)) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the SrcPoint
        /// </summary>
        public PointF SrcPoint
        {
            get { return srcPoint; }
            set { srcPoint = value; }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the TgtPoint
        /// </summary>
        public PointF TgtPoint
        {
            get { return tgtPoint; }
            set { tgtPoint = value; }
        }

    }
}
