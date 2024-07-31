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
    /// A class to implement a behavior that attempts to drive one input
    /// as close as possible to the other. The only inputs are the current positions
    /// of each input (as a PointF). It is assumed that both can move from causes
    /// external to the control of this behaviour. The one that is not under the control
    /// of the output of this code is called the staticPoint and the driven one is the 
    /// dynamic point
    /// 
    /// The outputs are an unsigned integer (between min and max) and a direction of 0 or 1. 
    /// The meaning of both are determined by the code that consumes it. For example if driving a stepper
    /// motor the number would be the speed and a direction of 0 could mean clock-wise
    /// 
    /// </summary>
    public class Behaviour_MoveClose
    {
        private const uint DIRECTION_CLOCKWISE = 0;
        private const uint DIRECTION_COUNTER_CLOCKWISE = 1;

        private uint lastReturnValue = 0;
        private uint lastReturnDirection = DIRECTION_CLOCKWISE;

        // the min and max output we can send. This is an arbitrary value and the 
        // output will be clamped within the range min/max. It does not need to 
        // correlate to steps/sec but it can
        private uint outputMax = 0;
        private uint outputMin = 0;

        // this queue is the distance queue, in other words the differences in 
        // distances of the two points. This queue can change size during 
        // processing
        private const int DEFAULT_DISTANCE_QUEUE_SIZE = 11;
        private FixedSizeQueue_Double distanceQueue = null;

        // this queue is the fixed point difference queue, we use it to detect 
        // changes in the location of the (supposedly) fixed point
        private const int DEFAULT_FIXEDPOINT_DIFFERENCE_QUEUE_SIZE = 5;
        private FixedSizeQueue_Double fixedPointDifferenceQueue = null;

        // if this goes true we assume we are starting the calculations from scratch
        // it is initialized to true.
        private bool resetFlag = false;

        private int initStage = 0;
        // the last static point we have, the coords are negative if no point seen
        private PointF lastStaticPoint = new PointF(float.MinValue, float.MinValue);
        // if the static point difference is over this much we assume it has 
        // moved and reset so we can adjust (make our dynamic point chase it)
        private const double DEFAULT_STATIC_POINT_MOVED_THRESHOLD = 1000;
        private double staticPointMovedThreshold = DEFAULT_STATIC_POINT_MOVED_THRESHOLD;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// <param name="outputMaxIn">the maximum output</param>
        /// </summary>
        public Behaviour_MoveClose(uint outputMaxIn)
        {
            // set these values
            OutputMax = outputMaxIn;

            // reset the object
            Reset();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the output speed and direction based on the latest positions of the two points. 
        /// 
        /// Note: This routine does retain the input positions for the purposes 
        /// of calculations based on position history
        /// 
        /// Note: it is assumed the positions never go negative. 
        /// 
        /// </summary>
        /// <param name="staticPoint">the point assumed to be the more static of the two</param>
        /// <param name="dynamicPoint">the point assumed to be the more dynamic of the two</param>
        /// <param name="outSpeed">the output speed</param>
        /// <param name="outDirection">the output direction</param>
        /// <returns>z success, nz fail</returns>
        public int GetOutput(PointF staticPoint, PointF dynamicPoint, out uint outSpeed, out uint outDirection)
        {
            double distSlope = 0;

            // init 
            outSpeed = 0;
            outDirection = 0;

            float staticPointX = staticPoint.X;
            float staticPointY = staticPoint.Y;
            float dynamicPointX = dynamicPoint.X;
            float dynamicPointY = dynamicPoint.Y;

            // find the squared distance between the two centers. This functions as our P term
            double rawSquaredDistance = Math.Round((Math.Pow((staticPointX - dynamicPointX), 2) + Math.Pow((staticPointY - dynamicPointY), 2)), 0);

            // add the current distance terms to the queue
            DistanceQueue.Enqueue(rawSquaredDistance);

            // figure out the distances between the lastStaticPoint and this one.
            // they should not move much. If they do we assume the static point has moved and we 
            // have to reset.
            if(lastStaticPoint.X >=0)
            {
                // find the squared distance between the two centers. 
                double staticDistance = Math.Round((Math.Pow((staticPointX - lastStaticPoint.X), 2) + Math.Pow((staticPointY - lastStaticPoint.Y), 2)), 0);
                // record it
                FixedPointDifferenceQueue.Enqueue(staticDistance);
            }
            // record this always
            lastStaticPoint = staticPoint;

            // has our fixed point moved?
            if(FixedPointDifferenceQueue.Average() >= StaticPointMovedThreshold)
            {
                // we assume so. reset as if we were starting from the beginning
                outSpeed = LastReturnValue;
                outDirection = LastReturnDirection;
                // reset everything
                Reset();
                Console.WriteLine("RESET for FIXED POINT MOVE");

                return 0;
            }

//            Console.WriteLine("rawDist=" + rawSquaredDistance.ToString() + ", green=" + staticPoint.ToString() + ", red=" + dynamicPoint.ToString());

            // is this our first run through
            if (ResetFlag == true)
            {
                // on the first run we have no idea of what direction we should go.
                // we set off in a random direction and watch what happens to the distance

                // start in a clockwise direction at preset speed
                LastReturnValue = Clamp(OutputMax);
                LastReturnDirection = DIRECTION_CLOCKWISE;

                // set this now
                outSpeed = LastReturnValue;
                outDirection = LastReturnDirection;
                // reset this 
                ResetFlag = false;
                return 0;
            }

            // on successive run throughs we have more distance data
            // get the slope of the distance data, record this now even though we do not always use it
            distSlope = DistanceQueue.SlopeOfData();

            // not on the first run through, have we filled the Distance queue?
            if (DistanceQueue.Count < DistanceQueue.QueueSize)
            {
                // keep going in current direction at preset speed
                LastReturnValue = Clamp(OutputMax / 2);

                outSpeed = LastReturnValue;
                outDirection = LastReturnDirection;

               // Console.WriteLine("Skip rawDist=" + rawSquaredDistance.ToString() + ", distSlope=" + Math.Round(distSlope, 2).ToString() + ", " + WalnutCommon.Utils.DirectionAsStr(outDirection));

                return 0;
            }

            // we have filled the distance queue, now we figure out if we are moving
            // in the right direction. If not we clear the queue so as to flush out old
            // points and 
            if(initStage == 0)
            {
                // we are on first run through
                if (distSlope < 0)
                {
                    // slope was negative, probably initial direction was correct
                    // change stage and move on
                    initStage = 2;
                    // drop the queue size down now, The larger queue size helps the code figure out which direction
                    // to start in. Once we are heading in the right direction the larger size makes it harder to settle
                    // on the shortest distance. 
                    distanceQueue.QueueSize = 5;
                    // fall through
                    Console.WriteLine("Move to initStage2, same direction, distSlope=" + Math.Round(distSlope, 2).ToString());
                }
                else if (distSlope == 0)
                {
                    // slope is zero, might be an artifact of the measuring, carry on
                    // for a while more
                    LastReturnValue = Clamp(OutputMax /2);
                    LastReturnDirection = SameDirection(LastReturnDirection);

                    outSpeed = LastReturnValue;
                    outDirection = LastReturnDirection;

                    // always set this just before we return
                    DistanceQueue.ClearOld(0);
                    initStage = 0;
                    Console.WriteLine("Stay on initStage=0, distSlope=" + Math.Round(distSlope, 2).ToString());
                    return 0;
                }
                else
                {
                    // slope is positive, probably the initial direction was wrong
                    LastReturnValue = Clamp(OutputMax / 2);
                    LastReturnDirection = ToggleDirection(LastReturnDirection);

                    outSpeed = LastReturnValue;
                    outDirection = LastReturnDirection;

                    // always set this just before we return
                    DistanceQueue.ClearOld(0);
                    initStage = 2;
                    // drop the queue size down now, The larger queue size helps the code figure out which direction
                    // to start in. Once we are heading in the right direction the larger size makes it harder to settle
                    // on the shortest distance. 
                    distanceQueue.QueueSize = 5;

                    Console.WriteLine("Move to initStage2, reverse direction, distSlope=" + Math.Round(distSlope, 2).ToString());
                    return 0;
                }

            }

            // at this point we have: 
            // 1) rawSquaredDistance - the current distance between the static and dynamic points
            // 2) distSlope - the slope of the best fit line through the points in the distance queue
            // 3) lastReturnDirection - the last direction we are going in, so we can switch it if necessary

            // the speed we adjust is based off the slope. The lower the slope the slower the speed. This helps
            // prevent ringing and overshoot
            double speedAdjustor = CalcSpeedAdjustorFromSlope(distSlope);
            // set the speed now, we clamp it so that it cannot exceed the preset max or minimum
            outSpeed = Clamp(Convert.ToUInt32((double)OutputMax * speedAdjustor));

            // is the slope less than zero?
            if (distSlope <= 0)
            {
                // yes, it is. Assume we are going in the correct direction
                outDirection = SameDirection(LastReturnDirection);
         //       Console.WriteLine("A rawDist=" + rawSquaredDistance.ToString() + ", distSlope=" + Math.Round(distSlope, 2).ToString() + ", speedAdjustor=" + Math.Round(speedAdjustor, 2).ToString() + ", " + WalnutCommon.Utils.DirectionAsStr(outDirection));
            }
            else
            {
                // no it is not, assume we are going in the wrong direction and change it
                outDirection = ToggleDirection(LastReturnDirection);
       //         Console.WriteLine("B rawDist=" + rawSquaredDistance.ToString() + ", distSlope=" + Math.Round(distSlope, 2).ToString() + ", speedAdjustor=" + Math.Round(speedAdjustor, 2).ToString() + ", " + WalnutCommon.Utils.DirectionAsStr(outDirection));
            }

            // record these
            LastReturnValue = outSpeed;
            LastReturnDirection = outDirection;

            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the speed adjustor from a given slope. This algorythim is 
        /// arbitrary and is based on what seems to work.
        /// </summary>
        /// <param name="slopeIn">the slope to test</param>
        /// <returns>Returns a value between 0 and 1 appropriate to the slope</returns>
        private double CalcSpeedAdjustorFromSlope(double slopeIn)
        {
            double slope = Math.Abs(slopeIn);
            if (slope > 80) return 1;
            if (slope > 60) return 0.8;
            if (slope > 40) return 0.4;
            if (slope > 20) return 0.3;
            if (slope > 10) return 0.2;
            if (slope > 5) return 0.1;
            return .05;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests for a distance anomaly 
        /// </summary>
        /// <param name="distIn">the current distance</param>
        /// <param name="lastDistIn">the last distance to compare to</param>
        /// <returns>returns true if there is an anomaly, false if there is not</returns>
        private bool IsDistanceAnAnomaly(double distIn, double lastDistIn)
        {
            // we just set a percentage difference
            if (distIn > (lastDistIn * 1.5)) return true;
            if (distIn < (lastDistIn * 0.5)) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Toggles the direction. 
        /// </summary>
        /// <param name="dirVal">the direction value to test</param>
        /// <returns>Returns 0 if current value is not zero. returns 1 if current value is one</returns>
        private uint ToggleDirection(uint dirVal)
        {
            // are we going in the right direction?
            if (dirVal != 0) return 0;
            else return 1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///Keeps the same direction - but cleans it up if necessary
        /// </summary>
        /// <param name="dirVal">the direction value to test</param>
        /// <returns>Returns 0 if current value is 0. returns 1 if current value anything else</returns>
        private uint SameDirection(uint dirVal)
        {
            // are we going in the right direction?
            if (dirVal != 0) return 1;
            else return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A queue of the last X distances we have calculated. Never gets/sets null
        /// </summary>
        private FixedSizeQueue_Double DistanceQueue
        {
            get 
            { 
                if(distanceQueue== null) distanceQueue = new FixedSizeQueue_Double(DEFAULT_DISTANCE_QUEUE_SIZE);
                return distanceQueue; 
            }
            set 
            { 
                distanceQueue = value;
                if (distanceQueue == null) distanceQueue = new FixedSizeQueue_Double(DEFAULT_DISTANCE_QUEUE_SIZE);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A queue of the differences between the fixed point 
        /// </summary>
        private FixedSizeQueue_Double FixedPointDifferenceQueue
        {
            get
            {
                if (fixedPointDifferenceQueue == null) fixedPointDifferenceQueue = new FixedSizeQueue_Double(DEFAULT_FIXEDPOINT_DIFFERENCE_QUEUE_SIZE);
                return fixedPointDifferenceQueue;
            }
            set
            {
                fixedPointDifferenceQueue = value;
                if (fixedPointDifferenceQueue == null) fixedPointDifferenceQueue = new FixedSizeQueue_Double(DEFAULT_FIXEDPOINT_DIFFERENCE_QUEUE_SIZE);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to reset the object as if it was just created.
        /// 
        /// </summary>
        public void Reset()
        {
            LastReturnDirection = 0;
            LastReturnValue = 0;
            InitStage = 0;
            LastStaticPoint = new PointF(float.MinValue, float.MinValue);
            StaticPointMovedThreshold = DEFAULT_STATIC_POINT_MOVED_THRESHOLD;
            DistanceQueue = new FixedSizeQueue_Double(DEFAULT_DISTANCE_QUEUE_SIZE);
            FixedPointDifferenceQueue = new FixedSizeQueue_Double(DEFAULT_FIXEDPOINT_DIFFERENCE_QUEUE_SIZE);
            ResetFlag = true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Limit a variable to the set OutputMax and OutputMin properties
        /// </summary>
        /// <returns>
        /// A value that is between the OutputMax and OutputMin properties
        /// </returns>
        private uint Clamp(uint variableToClamp)
        {
            if (variableToClamp <= OutputMin) { return OutputMin; }
            if (variableToClamp >= OutputMax) { return OutputMax; }
            return variableToClamp;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Signals that the object has been reset and that no data has been processed
        /// 
        /// </summary>
        public bool ResetFlag { get => resetFlag; set => resetFlag = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The last static point we saw
        /// </summary>
        private PointF LastStaticPoint
        {
            get
            {
                return lastStaticPoint;
            }
            set
            {
                lastStaticPoint = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The level above which we consider the static point to have moved.
        /// </summary>
        public double StaticPointMovedThreshold
        {
            get
            {
                return staticPointMovedThreshold;
            }
            set
            {
                staticPointMovedThreshold = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The initialization stage
        /// </summary>
        private int InitStage
        {
            get
            {
                return initStage;
            }
            set
            {
                initStage = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The max output value the control device can accept.
        /// </summary>
        public uint OutputMax
        {
            get 
            {
                return outputMax;
            }
            private set
            { 
                outputMax = value; 
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The minimum ouput value the control device can accept.
        /// </summary>
        public uint OutputMin 
        {
            get
            {
                return outputMin;
            }
            set
            {
                outputMin = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last return value 
        /// </summary>
        public uint LastReturnValue
        {
            get
            {
                return lastReturnValue;
            }
            private set
            {
                lastReturnValue = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last return sign. NOTE: this is always an integer 0 or 1
        /// never anything else
        /// </summary>
        public uint LastReturnDirection
        {
            get
            {
                if (lastReturnDirection <= 0) lastReturnDirection = 0;
                else lastReturnDirection = 1;
                return lastReturnDirection;
            }
            private set
            {
                lastReturnDirection = value;
                if (lastReturnDirection <= 0) lastReturnDirection = 0;
                else lastReturnDirection = 1;
            }
        }
    }
}
