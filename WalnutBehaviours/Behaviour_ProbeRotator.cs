using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BBBCSIO;

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
    /// An Behaviour to rotate the probe. In this implementation it means setting
    /// off a stepper command of a certain number of steps. We will pay attention
    /// to the ProbeRotationInhibit flag. Typically ProbeRotationInhibit stops
    /// the movement. We do not expect the completed number of steps to do this
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_ProbeRotationControl
    /// NOTE: we expect the global data store to implement IBehaviour_StepperList
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_ProbeRotator : Behaviour_Base
    {
        // these are define the probe rotation stepper, some are set at startup and
        // some are updated continously
        private uint stepper_Enable = 0;
        private uint stepper_DirState = 0;
        private uint numSteps = 0;
        private bool inhibitRotation_Left = false;
        private bool inhibitRotation_Right = false;

        // these are effectively constants and should only be set at instantiation
        private uint probeRotationDirLeft = 0;
        private uint probeRotationDirRight = 1;

        private uint stepper_StepSpeed = 0;
        private StepperIDEnum stepper_ID = StepperIDEnum.STEPPER_None;

        // indicates if the values have changed since we last looked
        private bool isDirty = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_ProbeRotator(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_ProbeRotationControl) == false) throw new Exception("The behaviour stack does not implement IBehaviour_ProbeRotationControl");
            if ((globalDataIn is IBehaviour_StepperList) == false) throw new Exception("The behaviour stack does not implement IBehaviour_StepperList");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public override void Startup()
        {
            // set these effective constants now so the global data knows about them
            (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDirLeft = ProbeRotationDirLeft;
            (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDirRight = ProbeRotationDirRight;
        }

        public bool InhibitRotation_Left
        {
            get => inhibitRotation_Left;
            set
            {
                if (inhibitRotation_Left != value)
                {
                    IsDirty = true;
                    inhibitRotation_Left = value;
                }
            }
        }
        public bool InhibitRotation_Right
        {
            get => inhibitRotation_Right;
            set
            {
                if (inhibitRotation_Right != value)
                {
                    IsDirty = true;
                    inhibitRotation_Right = value;
                }
            }
        }
        public uint Stepper_Enable
        {
            get => stepper_Enable;
            set
            {
                if (stepper_Enable != value)
                {
                    IsDirty = true;
                    stepper_Enable = value;
                }
            }
        }

        public uint Stepper_DirState { get => stepper_DirState;
            set
            {
                if (stepper_DirState != value)
                {
                    IsDirty = true;
                    stepper_DirState = value;
                }
            }
        }
        public uint Stepper_StepSpeed { get => stepper_StepSpeed;
            set
            {
                if (stepper_StepSpeed != value)
                {
                    IsDirty = true;
                    stepper_StepSpeed = value;
                }
            }
        }
        public StepperIDEnum Stepper_ID { get => stepper_ID;
            set
            {
                if (stepper_ID != value)
                {
                    IsDirty = true;
                    stepper_ID = value;
                }
            }
        }
        public uint NumSteps { get => numSteps;
            set
            {
                if (numSteps != value)
                {
                    IsDirty = true;
                    numSteps = value;
                }
            }
        }
        public bool IsDirty { get => isDirty; set => isDirty = value; }
        public uint ProbeRotationDirLeft { get => probeRotationDirLeft; set => probeRotationDirLeft = value; }
        public uint ProbeRotationDirRight { get => probeRotationDirRight; set => probeRotationDirRight = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_StepperList) == false) return;
            if ((globalDataStore is IBehaviour_ProbeRotationControl) == false) return;

            // get our data down here from the globalDataStore
            InhibitRotation_Left = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Left;
            InhibitRotation_Right = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Right;

            Stepper_DirState = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationDir;
            NumSteps = (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationSteps;

            // do we actually want to rotate the probe
            if ((globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationWanted == true) Stepper_Enable = 1;
            else Stepper_Enable = 0;

            // have any changes been made?
            if (isDirty == false)
            {
                // no change, we do not need to keep sending
                return;
            }

            // we have changes, create our stepper data item
            SCData_Stepper stepperData = new SCData_Stepper(Stepper_ID, Stepper_Enable, Stepper_DirState, Stepper_StepSpeed, NumSteps);
            // do we need to override and force it off? 
            if (((globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Left == true) && (Stepper_DirState == ProbeRotationDirLeft))
            {
                // yes, we do, we are inhibit left and we are rotating left
                stepperData.Stepper_Enable = 0;
                Console.WriteLine("Inhibit left and rotating left now disabling probe rotation");
            }
            if (((globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Right == true) && (Stepper_DirState == ProbeRotationDirRight))
            {
                // yes, we do, we are inhibit right and we are rotating right
                stepperData.Stepper_Enable = 0;
                Console.WriteLine("Inhibit right and rotating right now disabling probe rotation");
            }
            // we put an item on the stepper List
            (globalDataStore as IBehaviour_StepperList).StepperList.Add(stepperData);
            // flag that we sent this
            IsDirty = false;
            Console.WriteLine("probe enable state changed, is now " + stepperData.Stepper_Enable.ToString());
        }
    }
}
