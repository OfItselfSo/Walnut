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
    /// An Behaviour to detect a target point value. 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_TargetPoint
    /// 
    /// NOTE: this class expects the mainObject to implement IBehaviour_DetectPointViaColor
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_TargetPointColorDecider : Behaviour_Base
    {
        private int numberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor = 1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_TargetPointColorDecider(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_TargetPoint) == false) throw new Exception("The behaviour stack does not implement IBehaviour_TargetPoint");
            if ((globalDataIn is IBehaviour_TargetPointColor) == false) throw new Exception("The behaviour stack does not implement IBehaviour_TargetPointColor");
            if ((globalDataIn is IBehaviour_TargetPointStatistics) == false) throw new Exception("The behaviour stack does not implement IBehaviour_TargetPointStatistics");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This is the number of invalid target points we require before we toggle
        /// the TargetPointColor with the TargetPointColorAlt
        /// 
        /// </summary>
        public int NumberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor { get => numberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor; set => numberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_TargetPoint) == false) return;
            if ((globalDataStore is IBehaviour_TargetPointColor) == false) return;
            if ((globalDataStore is IBehaviour_TargetPointStatistics) == false) return;

            // get the target point color sith full alpha from the global settings
            if((globalDataStore as IBehaviour_TargetPoint).TargetPoint.IsEmpty == false)
            {
                // we have a valid target point, count it and reset the empty count
                (globalDataStore as IBehaviour_TargetPointStatistics).NumTargetPointsFound_Valid++;
                (globalDataStore as IBehaviour_TargetPointStatistics).NumTargetPointsFound_Empty=0;
                // remember the last time we found a valid point
                (globalDataStore as IBehaviour_TargetPointStatistics).TimeAtWhichLastValidTargetPointWasFound = DateTime.Now;
            }
            else
            {
                // we have a valid target point, count it and reset the valid count
                (globalDataStore as IBehaviour_TargetPointStatistics).NumTargetPointsFound_Valid=0;
                (globalDataStore as IBehaviour_TargetPointStatistics).NumTargetPointsFound_Empty++;
            }

            // make our decisions
            MakeDecisonsAboutTargetPointColor();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Make decisions based on the TargetPixel Statistics
        /// 
        /// </summary>
        private void MakeDecisonsAboutTargetPointColor()
        {
            // in this implementation we toggle the TargetPointColor with the TargetPointColorAlt if
            // we have not seen a NumTargetPointsFound_Empty >= NumberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor
            if((globalDataStore as IBehaviour_TargetPointStatistics).NumTargetPointsFound_Empty >= NumberOfInvalidTargetPointsWeNeedToSeeBeforeChangingTargetColor)
            {
                // we toggle the target colors
                Color tmpColor = (globalDataStore as IBehaviour_TargetPointColor).TargetPointColor;
                (globalDataStore as IBehaviour_TargetPointColor).TargetPointColor = (globalDataStore as IBehaviour_TargetPointColor).TargetPointColorAlt;
                (globalDataStore as IBehaviour_TargetPointColor).TargetPointColorAlt = tmpColor;
            }
        }
    }
}
