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
    /// the same level as the other. The only inputs are the current positions
    /// of each input (as a PointF). It is assumed that both can move from causes
    /// external to the control of this behaviour. The one that is not under the control
    /// of the output of this code is called the staticCoord and the driven one is the 
    /// dynamic point
    /// 
    /// The outputs are an unsigned integer (between min and max) and a direction of 0 or 1. 
    /// The meaning of both are determined by the code that consumes it. For example if driving a stepper
    /// motor the number would be the speed and a direction of 0 could mean clock-wise
    /// 
    /// </summary>
    public class Behaviour_MoveLevel
    {
        private const uint DIRECTION_Positive = 1;
        private const uint DIRECTION_Negative = 0;

        private uint lastReturnValue = 0;
        private uint lastReturnDirection = DIRECTION_Positive;

        private AxisEnum operatingAxis = AxisEnum.AXIS_UNKNOWN;

        // the min and max output we can send. This is an arbitrary value and the 
        // output will be clamped within the range min/max. It does not need to 
        // correlate to steps/sec but it can
        private uint outputMax = 0;
        private uint outputMin = 0;

        // this queue is the distance queue, in other words the differences in 
        // distances of the two points. This queue can change size during 
        // processing
        private const int DEFAULT_DISTANCE_QUEUE_SIZE = 5;
        private FixedSizeQueue_Double distanceQueue = null;

        // this queue is the fixed point difference queue, we use it to detect 
        // changes in the location of the (supposedly) fixed point
        private const int DEFAULT_FIXEDPOINT_DIFFERENCE_QUEUE_SIZE = 5;
        private FixedSizeQueue_Double fixedPointDifferenceQueue = null;

        // if the distance is less than or equal to this the caller can stop all processin
        private const double CANSTOP_DISTANCE = 1;

        // normally the behaviour stops if it does not have a dynamic point but if this is nz
        // we can just proceed with the last known point up to this count.
        private const uint MAX_DYNAMICPOINT_SKIP_COUNT = 5;
        private uint dynamicPointSkipCount = 0;

        // if this goes true we assume we are starting the calculations from scratch
        // it is initialized to true.
        private bool resetFlag = false;

        private int initStage = 0;
        // the last static coord we have
        private float lastStaticCoord = float.NaN;
        // the last dynamic coord we have
        private float lastDynamicCoord = float.NaN;
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="outputMaxIn">the maximum output</param>
        /// <param name="operatingAxisIn">the axis we are using</param>
        public Behaviour_MoveLevel(AxisEnum operatingAxisIn, uint outputMaxIn)
        {
            // set these values
            OutputMax = outputMaxIn;
            OperatingAxis = operatingAxisIn;

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

            // init 
            outSpeed = 0;
            outDirection = 0;

            float staticCoord = float.NaN;
            float dynamicCoord = float.NaN;

            // set up our coords now
            if (OperatingAxis == AxisEnum.AXIS_X)
            {
                staticCoord = staticPoint.X;
                dynamicCoord = dynamicPoint.X;
            }
            else if (OperatingAxis == AxisEnum.AXIS_Y)
            {
                staticCoord = staticPoint.Y;
                dynamicCoord = dynamicPoint.Y;
            }
            else
            {
                // we do not have an axis
                return -1;
            }

            // remember these
            LastStaticCoord = staticCoord;
            LastDynamicCoord = dynamicCoord;

            // find the distance between the two centers. This functions as our P term
            double rawDistance = Math.Round((staticCoord - dynamicCoord), 0) ;

            // set the direction while we still have the sign
            if (rawDistance < 0) outDirection = 0;
            else outDirection = 1;

            // add the current absolute distance terms to the queue
            DistanceQueue.Enqueue(Math.Abs(rawDistance));

            outSpeed = Clamp((uint)(CalcSpeedAdjustorFromDistance(rawDistance)));
            Console.WriteLine("Axis="+OperatingAxis.ToString() + "(" + staticPoint.X.ToString() + "," + staticPoint.Y.ToString() + ")" + " (" + dynamicPoint.X.ToString() + "," + dynamicPoint.Y.ToString() + ")" + " rawDistance=" + rawDistance.ToString() + ", outSpeed="+ outSpeed.ToString() + ", outDirection=" + outDirection.ToString());

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
        private double CalcSpeedAdjustorFromDistance(double distanceIn)
        {
            double distance = Math.Abs(distanceIn);

            // the current motors are so geared down and slow 
            // we can just hit full speed nearly to the end
            if (distance > 10) return 100;
            if (distance > 8) return 100;
            if (distance > 6) return 100;
            if (distance > 4) return 100;
            if (distance > 2) return 80;
            if (distance > 1) return 50;
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we can stop or not
        /// </summary>
        /// <returns>returns true if we can stop, false if we cannot</returns>
        public bool CanStop()
        {
            // we have to have a full queue
            if (DistanceQueue.IsFull() == false) return false;
            // are we below the can stop distance
            if (DistanceQueue.Average() <= CANSTOP_DISTANCE) return true;
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
            LastStaticCoord = float.NaN;
            LastDynamicCoord = float.NaN;
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
        public float LastStaticCoord
        {
            get
            {
                return lastStaticCoord;
            }
            set
            {
                lastStaticCoord = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The last dynamic point we saw
        /// </summary>
        public float LastDynamicCoord
        {
            get
            {
                return lastDynamicCoord;
            }
            set
            {
                lastDynamicCoord = value;
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

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the operating axis
        /// </summary>
        public AxisEnum OperatingAxis { get => operatingAxis; set => operatingAxis = value; }
    }
}
