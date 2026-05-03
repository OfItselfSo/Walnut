using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    /// An Behaviour to encapsulate the motor speed values. 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_MotorSpeeds
    /// 
    /// NOTE: this class has no actor. It's existence in the behaviour stack forces
    ///       the stack to implement IBehaviour_MotorSpeeds
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_MotorSpeeds : Behaviour_Base
    {
        // provide some default speeds, later set saved values via properties
        private uint workingSpeed_X = 15;
        private uint workingSpeed_Y = 15;
        private uint workingMaxSpeed_X = 25;
        private uint workingMaxSpeed_Y = 25;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_MotorSpeeds(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_MotorSpeeds) == false) throw new Exception("The behaviour stack does not implement IBehaviour_MotorSpeeds");
        }

        public uint WorkingSpeed_X { get => workingSpeed_X; set => workingSpeed_X = value; }
        public uint WorkingSpeed_Y { get => workingSpeed_Y; set => workingSpeed_Y = value; }
        public uint WorkingMaxSpeed_X { get => workingMaxSpeed_X; set => workingMaxSpeed_X = value; }
        public uint WorkingMaxSpeed_Y { get => workingMaxSpeed_Y; set => workingMaxSpeed_Y = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public override void Startup()
        {
            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_MotorSpeeds) == false) return;

            // set the default speeds in the global data now
            (globalDataStore as IBehaviour_MotorSpeeds).Speed_X = WorkingSpeed_X;
            (globalDataStore as IBehaviour_MotorSpeeds).Speed_Y = WorkingSpeed_Y;
            (globalDataStore as IBehaviour_MotorSpeeds).MaxSpeed_X = WorkingMaxSpeed_X;
            (globalDataStore as IBehaviour_MotorSpeeds).MaxSpeed_Y = WorkingMaxSpeed_Y;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
        }


    }
}
