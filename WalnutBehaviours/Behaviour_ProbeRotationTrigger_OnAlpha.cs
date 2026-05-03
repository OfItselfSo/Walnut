using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BBBCSIO;
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

namespace WalnutBehaviours
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// An Behaviour to trigger the rotation of the probe when it percieves the 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_ProbeRotationControl
    /// NOTE: we expect the global data store to implement IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_ProbeRotationTrigger_OnAlpha : Behaviour_Base
    {
        // this is what we trigger on for left and right rotations
        private byte alphaValueForTrigger_Left = 0;
        private byte alphaValueForTrigger_Right = 0;
        // this determines the number of steps. The probe endstops should trigger and stop the motion at the limits
        private uint ProbeRotationNumSteps = 200;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_ProbeRotationTrigger_OnAlpha(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_ProbeRotationControl) == false) throw new Exception("The behaviour stack does not implement IBehaviour_ProbeRotationControl");
            if ((globalDataIn is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) throw new Exception("The behaviour stack does not implement IBehaviour_IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay");
        }

        public uint ProbeRotationNumSteps1 { get => ProbeRotationNumSteps; set => ProbeRotationNumSteps = value; }
        public byte AlphaValueForTrigger_Right { get => alphaValueForTrigger_Right; set => alphaValueForTrigger_Right = value; }
        public byte AlphaValueForTrigger_Left { get => alphaValueForTrigger_Left; set => alphaValueForTrigger_Left = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) return;
            if ((globalDataStore is IBehaviour_ProbeRotationControl) == false) return;

            // Do we have a trigger?
            byte currentAlpha = (globalDataStore as IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay).SourcePointLowestAlphaValueFoundOnMask_Overlay;
            if(currentAlpha == 254)
            {
             //   int foo = 1;
            }
            if (currentAlpha == AlphaValueForTrigger_Left)
            {
                // we have a trigger to rotate left
                // set it to rotate left, the value for this direction is defined in the global data store
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDir = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDirLeft;
                // set the number of steps. This should be turned off by the endstops
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationSteps = ProbeRotationNumSteps;
                // activate it
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationWanted = true;
                return;
            }
            else if (currentAlpha == AlphaValueForTrigger_Right)
            {
                // we have a trigger to rotate right
                // set it to rotate right, the value for this direction is defined in the global data store
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDir = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDirRight;
                // set the number of steps. This should be turned off by the endstops
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationSteps = ProbeRotationNumSteps;
                // activate it
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationWanted = true;
                return;
            }
        }
    }
}
