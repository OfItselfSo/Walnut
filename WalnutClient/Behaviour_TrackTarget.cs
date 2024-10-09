using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using WalnutCommon;

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

namespace WalnutClient
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to implement a behavior that tracks an input and can answer
    /// various questions about it
    /// 
    /// 
    /// </summary>
    public class Behaviour_TrackTarget
    {

        // this queue is the fixed point difference queue, we use it to detect 
        // changes in the location of the (supposedly) fixed point
        private const int DEFAULT_TARGET_QUEUE_SIZE = 5;
        private uint targetQueueSize = DEFAULT_TARGET_QUEUE_SIZE;
        private FixedSizeQueue_PointF targetQueue = null;

        // if the target point difference is over this much we can assume it has moved
        private const float DEFAULT_TARGET_MOVED_THRESHOLD = 10;
        private float targetMovedThreshold = DEFAULT_TARGET_MOVED_THRESHOLD;

        // the last coord we processed
        private PointF lastTargetCoord = new PointF(float.NaN,float.NaN);


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="targetMovedThreshold">the distance at which we can assume the target has moved</param>
        /// <param name="targetQueueSizeIn">the target queue size</param>
        public Behaviour_TrackTarget(float targetMovedThresholdIn, uint targetQueueSizeIn)
        {
            // set these values
            TargetMovedThreshold = targetMovedThresholdIn;
            TargetQueueSize = targetQueueSizeIn;

            // reset the object
            Reset();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the latest target point. Only keeps the last targetQueueSize number of 
        /// values
        /// </summary>
        /// <param name="targetCoord">the target coord to set</param>
        public void SetTargetPoint(PointF targetCoord)
        {
            // enqueue this 
            TargetQueue.Enqueue(targetCoord);
            // set this as well
            lastTargetCoord = targetCoord;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if our target point has moved
        /// </summary>
        /// <returns>returns true if it has moved, false if it has not</returns>
        public bool HasMoved()
        {
            // sanity checks
            if (float.IsNaN(LastTargetCoord.X) == true) return false;
            if (float.IsNaN(LastTargetCoord.Y) == true) return false;

            // if we are not full we assume we have not moved
            if (TargetQueue.IsFull() == false) return false;

            // see if any of the coords have moved beyond the threshold
            if (Math.Abs((TargetQueue.AverageX() - LastTargetCoord.X)) > TargetMovedThreshold) return true;
            if (Math.Abs((TargetQueue.AverageY() - LastTargetCoord.Y)) > TargetMovedThreshold) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A queue of the input target locations
        /// </summary>
        private FixedSizeQueue_PointF TargetQueue
        {
            get
            {
                if (targetQueue == null) targetQueue = new FixedSizeQueue_PointF(DEFAULT_TARGET_QUEUE_SIZE);
                return targetQueue;
            }
            set
            {
                targetQueue = value;
                if (targetQueue == null) targetQueue = new FixedSizeQueue_PointF(DEFAULT_TARGET_QUEUE_SIZE);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to reset the object as if it was just created.
        /// 
        /// </summary>
        public void Reset()
        {
            TargetQueue = new FixedSizeQueue_PointF((int)TargetQueueSize);
            lastTargetCoord = new PointF(float.NaN, float.NaN);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The last static point we saw
        /// </summary>
        public PointF LastTargetCoord
        {
            get
            {
                return lastTargetCoord;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The level above which we consider the target point to have moved.
        /// </summary>
        public float TargetMovedThreshold
        {
            get
            { 
                if (targetMovedThreshold < 0) targetMovedThreshold = Math.Abs(targetMovedThreshold);
                return targetMovedThreshold;
            }
            set
            {
                targetMovedThreshold = value;
                if (targetMovedThreshold < 0) targetMovedThreshold = Math.Abs(targetMovedThreshold);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The target queue size
        /// </summary>
        public uint TargetQueueSize
        {
            get
            {
                if (targetQueueSize == 0) targetQueueSize = DEFAULT_TARGET_QUEUE_SIZE;
                return targetQueueSize;
            }
            set
            {
                targetQueueSize = value;
                if (targetQueueSize <= 0) targetQueueSize = DEFAULT_TARGET_QUEUE_SIZE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The target queue count
        /// </summary>
        public int TargetQueueCount
        {
            get
            {
                return TargetQueue.Count;
            }
        }
    }
}
