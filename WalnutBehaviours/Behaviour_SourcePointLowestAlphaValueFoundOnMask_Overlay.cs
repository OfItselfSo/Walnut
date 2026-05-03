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
    /// An Behaviour to encapsulate a discovered alpha value from the source pixel
    /// on the overlay. This is the value of the pixel on the overlay under the 
    /// source point not the alpha value of whatever marker (red usually) which 
    /// might be on top of it or the color which is on the screen at 
    /// that point
    /// 
    /// This class just forces the global data class to implement source point pixel color overlay variable
    ///    and the poll worker continuously updates it
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay and the 
    ///       main object to implment IBehaviour_SourcePointDetectedLowestAlphaValue_Overlay
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay : Behaviour_Base
    {        
       
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) throw new Exception("The behaviour stack does not implement IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public override void Startup()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) return;

            // ok to update, copy the data across, we just give it Color.IsEmpty
            (globalDataStore as IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay).SourcePointLowestAlphaValueFoundOnMask_Overlay = 255;
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
            if ((globalDataStore is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) return;
            if (globalDataStore.MainObject == null) return;
            if ((globalDataStore.MainObject is IBehaviour_SourcePointDetectedPixelColor_Overlay) == false) return;

            // get the last found alpha from the main object, and set it in the global data store
            (globalDataStore as IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay).SourcePointLowestAlphaValueFoundOnMask_Overlay = (globalDataStore.MainObject as IBehaviour_SourcePointDetectedLowestAlphaValue_Overlay).LastDetectedSourcePointDetectedLowestAlphaValue_Overlay();
        }

    }
}
