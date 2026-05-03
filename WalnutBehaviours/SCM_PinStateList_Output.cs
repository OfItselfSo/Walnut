using System;
using System.Text;
using System.Collections.Generic;

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

/// NOTE: this class and the entire WalnutCommon project is shared with the client which runs on the Beaglebone Black. If your primary
/// interest is in working out how a Typed object is sent between a Server and Client (and back) to transmit complex data you should
/// have a look at the RemCon demonstrator project at http://www.OfItselfSo.com/RemCon which is devoted to that topic. This class 
/// is directly derived from that project. If your primary interest is in seeing how a C# program running on Windows can control 
/// stepper motors see the Tilo project - it is specifically set up to demo that. http://www.OfItselfSo.com/Tilo

namespace WalnutBehaviours
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// 
    /// This class contains information regarding about the state of output pins
    /// on the client
    /// 
    /// </summary>
    [SerializableAttribute]
    public class SCM_PinStateList_Output : SCM_Base
    {
        // the list of gpio pin data classes
        private List<SCData_PinOutputConfig> pinStateList = new List<SCData_PinOutputConfig>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public SCM_PinStateList_Output()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pinStateList data value. Will never return null 
        /// </summary>
        public List<SCData_PinOutputConfig> PinStateList
        {
            get
            {
                if (pinStateList == null) pinStateList = new List<SCData_PinOutputConfig>();
                return pinStateList;
            }
            set
            {
                pinStateList = value;
                if (pinStateList == null) pinStateList = new List<SCData_PinOutputConfig>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state
        /// </summary>
        public override string GetState()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PinStateList:");
            sb.Append("\r\n");
            foreach (SCData_PinOutputConfig pinObj in PinStateList)
            {
                sb.Append(pinObj.GetState());
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

    }
}
