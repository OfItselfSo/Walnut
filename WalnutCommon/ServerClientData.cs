using System;
using System.Text;
using System.Collections.Generic;

namespace WalnutCommon
{
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

    /// NOTE: this class and the entire Waldo project is shared with the client which runs on the Beaglebone Black. If your primary
    /// interest is in working out how a Typed object is sent between a Server and Client (and back) to transmit complex data you should
    /// have a look at the RemCon demonstrator project at http://www.OfItselfSo.com/RemCon which is devoted to that topic. This class 
    /// is directly derived from that project.

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class ServerClientData
    {

        // NOTE: this enum also uses the [SerializableAttribute]
        private ServerClientDataContentEnum dataContent = ServerClientDataContentEnum.NO_DATA;
        private string dataStr = "";

        private List<ColoredRotatedRect> rectList = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ServerClientData()
        { }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor. Assumes ServerClientDataContentEnum.USER_DATA
        /// </summary>
        /// <param name="dataStrIn">the string data value</param>
        public ServerClientData(string dataStrIn)
        {
            DataContent = ServerClientDataContentEnum.USER_DATA;
            DataStr = dataStrIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataContentIn">the data content type</param>
        public ServerClientData(ServerClientDataContentEnum dataContentIn)
        {
            DataContent = dataContentIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the data content flag
        /// </summary>
        public ServerClientDataContentEnum DataContent
        {
            get
            {
                return dataContent;
            }
            set
            {
                dataContent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the DataStr data value. Will never return null will return empty
        /// </summary>
        public string DataStr
        {
            get
            {
                if (dataStr == null) dataStr = "";
                return dataStr;
            }
            set
            {
                dataStr = value;
                if (dataStr == null) dataStr = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the rectList data value. Will never return null 
        /// </summary>
        public List<ColoredRotatedRect> RectList
        {
            get
            {
                if (rectList == null) rectList = new List<ColoredRotatedRect>();
                return rectList;
            }
            set
            {
                rectList = value;
                if (rectList == null) rectList = new List<ColoredRotatedRect>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state
        /// </summary>
        public string GetState()
        {
            return ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state
        /// </summary>
        public override string ToString()
        {
            return "RectListCount=" + RectList.Count.ToString();
        }
    }
}
