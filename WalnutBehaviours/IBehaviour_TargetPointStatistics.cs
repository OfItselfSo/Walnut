using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

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

namespace WalnutBehaviours
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// An interface to insure that a behaviour stack can read and write a
    /// set of target point statistics
    /// 
    /// </summary>
    public interface IBehaviour_TargetPointStatistics
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the number of non empty target points found since the last Empty
        /// Target Point
        /// 
        /// </summary>
        int NumTargetPointsFound_Valid { get; set; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the number of empty target points found since the last Valid
        /// Target point was found or from start
        /// 
        /// </summary>
        int NumTargetPointsFound_Empty { get; set; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the time at which the last Valid Target Point was found
        /// 
        /// </summary>
        DateTime TimeAtWhichLastValidTargetPointWasFound { get; set; }

        // IBehaviour_TargetPointStatistics sample implementation
        //public int NumTargetPointsFound_Valid
        //{
        //    get
        //    {
        //        lock (lockObject)
        //        {
        //            return numTargetPointsFound_Valid;
        //        }
        //    }
        //    set
        //    {
        //        lock (lockObject)
        //        {
        //            numTargetPointsFound_Valid = value;
        //        }
        //    }
        //}
        //public int NumTargetPointsFound_Empty
        //{
        //    get
        //    {
        //        lock (lockObject)
        //        {
        //            return numTargetPointsFound_Empty;
        //        }
        //    }
        //    set
        //    {
        //        lock (lockObject)
        //        {
        //            numTargetPointsFound_Empty = value;
        //        }
        //    }
        //}

        //public DateTime TimeAtWhichLastValidTargetPointWasFound
        //{
        //    get
        //    {
        //        lock (lockObject)
        //        {
        //            return timeAtWhichLastValidTargetPointWasFound;
        //        }
        //    }
        //    set
        //    {
        //        lock (lockObject)
        //        {
        //            timeAtWhichLastValidTargetPointWasFound = value;
        //        }
        //    }
        //}

    }
}
