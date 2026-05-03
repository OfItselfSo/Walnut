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
    /// An Behaviour to remove the target pixels around the source point. 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_SourcePoint
    /// NOTE: we expect the global data store to implement IBehaviour_TargetPointColor
    /// 
    /// NOTE: this class expects the mainObject to implement IBehaviour_ColorPixelsByColor
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_RemoveTargetPixelsAtSourcePoint : Behaviour_Base
    {
        // the width+height of the erasure rectangle
        private int erasureRectWidth = 20;
        private int erasureRectHeight = 20;
        private bool wantTransparent = true;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_RemoveTargetPixelsAtSourcePoint(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_SourcePoint) == false) throw new Exception("The behaviour stack does not implement IBehaviour_SourcePoint");
            if ((globalDataIn is IBehaviour_TargetPoint) == false) throw new Exception("The behaviour stack does not implement IBehaviour_TargetPoint");
            if ((globalDataIn is IBehaviour_TargetPointColor) == false) throw new Exception("The behaviour stack does not implement IBehaviour_TargetPointColor");
        }

        public bool WantTransparent { get => wantTransparent; set => wantTransparent = value; }
        public int ErasureRectWidth { get => erasureRectWidth; set => erasureRectWidth = value; }
        public int ErasureRectHeight { get => erasureRectHeight; set => erasureRectHeight = value; }

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
            if ((globalDataStore is IBehaviour_TargetPointColor) == false) return;
            if (globalDataStore.MainObject == null) return;
            if ((globalDataStore.MainObject is IBehaviour_ColorPixelsByColor) == false) return;

            // get the current target point color with full alpha from the global settings
            Color targetPointColor = (globalDataStore as IBehaviour_TargetPointColor).TargetPointColor;
            // get the color we change the target point color to with full alpha from the global settings
            Color changeToPointColor = (globalDataStore as IBehaviour_TargetPointColor).TargetPointColorAlt;

            // convert the target point color
            (globalDataStore.MainObject as IBehaviour_ColorPixelsByColor).ColorPixelsByColor((globalDataStore as IBehaviour_SourcePoint).SourcePoint, ErasureRectWidth, ErasureRectHeight, targetPointColor, changeToPointColor, WantTransparent);
        }

    }
}
