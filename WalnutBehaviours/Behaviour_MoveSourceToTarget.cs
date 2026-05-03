using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// An Behaviour to encapsulate a move source to target type action. 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_SourcePoint
    /// NOTE: we expect the global data store to implement IBehaviour_TargetPoint
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_MoveSourceToTarget : Behaviour_Base
    {
        private const int DEFAULT_TARGET_QUEUE_SIZE = 5;
        [NonSerialized]
        private const int workingTargetMovedThreshold = 5;

        //// set up our behaviour helpers
        [NonSerialized]
        BehaviourHelper_MoveLevel behaviourHelperMoveLevelX = null;
        [NonSerialized]
        BehaviourHelper_MoveLevel behaviourHelperMoveLevelY = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_MoveSourceToTarget(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {

        }
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_SourcePoint) == false) return;
            if ((globalDataStore is IBehaviour_TargetPoint) == false) return;
            if ((globalDataStore is IBehaviour_MotorSpeeds) == false) return;
            if ((globalDataStore is IBehaviour_StepperList) == false) return;
  
            // create our behaviour helpers if we need to
            behaviourHelperMoveLevelX = new BehaviourHelper_MoveLevel((globalDataStore as IBehaviour_MotorSpeeds).MaxSpeed_X);
            behaviourHelperMoveLevelY = new BehaviourHelper_MoveLevel((globalDataStore as IBehaviour_MotorSpeeds).MaxSpeed_Y);

            // acquire the Source and Target Points
            Point sourcePoint = (globalDataStore as IBehaviour_SourcePoint).SourcePoint;
            Point targetPoint = (globalDataStore as IBehaviour_TargetPoint).TargetPoint;

            // build a list of what the steppers should do, this can come back with both stepper stopped for fail
            List<SCData_Stepper> outList = MoveSourceToTarget_Stepper(sourcePoint, targetPoint);

            // place the list in the global data
            (globalDataStore as IBehaviour_StepperList).StepperList = outList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An action to move the source onto the target. Stepper Version
        /// 
        /// </summary>
        /// <param name="sourcePoint">the source point</param>
        /// <param name="targetPoint">the target point</param>
        /// <returns>A stepper control list. Will return an all stop list for fail.</returns>
        private List<SCData_Stepper> MoveSourceToTarget_Stepper(Point sourcePoint, Point targetPoint)
        {
            uint outSpeedX = 0;
            uint outDirectionX = 0;
            uint outSpeedY = 0;
            uint outDirectionY = 0;

            // form up new StepperControl objects, and a list to contain them
            SCData_Stepper stepperObjX = new SCData_Stepper(StepperIDEnum.STEPPER_0);
            SCData_Stepper stepperObjY = new SCData_Stepper(StepperIDEnum.STEPPER_1);

            List<SCData_Stepper> stepperList = new List<SCData_Stepper>();
            // add our stepper control modules to the list
            stepperList.Add(stepperObjX);
            stepperList.Add(stepperObjY);

            // turn off the steppers, for now, they can be enabled later
            stepperObjX.Stepper_Enable = 0;
            stepperObjY.Stepper_Enable = 0;

            // we require these helper objects just bail out if we need to
            if (behaviourHelperMoveLevelX == null) return stepperList;
            if (behaviourHelperMoveLevelY == null) return stepperList;

            // check our Source and Target Point Data
            if ((sourcePoint == null) || (sourcePoint.IsEmpty == true))
            {
                // we do not have source data
                Console.WriteLine("No Src Data");
                // turn off the steppers
                stepperObjX.Stepper_Enable = 0;
                stepperObjY.Stepper_Enable = 0;
                // and leave
                return stepperList;
            }
            if ((targetPoint == null) || (targetPoint.IsEmpty == true))
            {
                // we do not have source data
                Console.WriteLine("No Tgt Data");
                // turn off the steppers
                stepperObjX.Stepper_Enable = 0;
                stepperObjY.Stepper_Enable = 0;
                // and leave
                return stepperList;
            }

   //         Console.WriteLine("**(" + sourcePoint.X.ToString() + "," + sourcePoint.Y.ToString() + ")" + " (" + targetPoint.X.ToString() + "," + targetPoint.Y.ToString() + ")");

            // reset the MoveLevel behaviour
            behaviourHelperMoveLevelX.Reset();
            behaviourHelperMoveLevelY.Reset();

            // Process X, have we already reached a point where we can stop?
            if (behaviourHelperMoveLevelX.CanStop() == true)
            {
                // turn off the stepper
                stepperObjX.Stepper_Enable = 0;
            }
            else
            {
                // get the result for X direction
                int retVal = behaviourHelperMoveLevelX.GetOutput((float)targetPoint.X, (float)sourcePoint.X, out outSpeedX, out outDirectionX);
                if (retVal != 0)
                {
                    // turn off the stepper
                    stepperObjX.Stepper_Enable = 0;
                    // and leave
                    return stepperList;
                }
                else
                {
                    // set the stepper speed
                    stepperObjX.Stepper_StepSpeed = outSpeedX;
                    // give it infinite steps, the server will turn it off
                    stepperObjX.NumSteps = SCData_Stepper.INFINITE_STEPS;

                    // set the direction
                    if (outDirectionX != 0) stepperObjX.Stepper_DirState = 1;
                    else stepperObjX.Stepper_DirState = 0;

                    // turn off the stepper
                    stepperObjX.Stepper_Enable = 1;
                }
            }

            // Process Y, have we already reached a point where we can stop?
            if (behaviourHelperMoveLevelY.CanStop() == true)
            {
                // turn off the stepper
                stepperObjY.Stepper_Enable = 0;
            }
            else
            {
                // get the result for Y direction
                int retVal = behaviourHelperMoveLevelY.GetOutput((float)targetPoint.Y, (float)sourcePoint.Y, out outSpeedY, out outDirectionY);
                if (retVal != 0)
                {
                    // turn off the stepper
                    stepperObjY.Stepper_Enable = 0;
                    // and leave
                    return stepperList;
                }
                else
                {
                    // set the stepper speed
                    stepperObjY.Stepper_StepSpeed = outSpeedY;
                    // give it infinite steps, the server will turn it off
                    stepperObjY.NumSteps = SCData_Stepper.INFINITE_STEPS;

                    // set the direction
                    if (outDirectionY != 0) stepperObjY.Stepper_DirState = 0;
                    else stepperObjY.Stepper_DirState = 1;

                    // turn off the stepper
                    stepperObjY.Stepper_Enable = 1;
                }
            }

            // write this out for diagnostics
            //Console.WriteLine("");
            // return the list
            return stepperList;

        }

    }
}
