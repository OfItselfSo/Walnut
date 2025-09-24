using System;
using System.Collections.Generic;
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

/// NOTE: this class and the entire WalnutCommon project is shared with the client which runs on the Beaglebone Black. If your primary
/// interest is in working out how a Typed object is sent between a Server and Client (and back) to transmit complex data you should
/// have a look at the RemCon demonstrator project at http://www.OfItselfSo.com/RemCon which is devoted to that topic. This class 
/// is directly derived from that project.

namespace WalnutCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// An enum to define the type of user data in the Server Client Data object.
    /// Only valid when ServerClientDataContentEnum == USER_DATA
    /// 
    /// NOTE that we use the [SerializableAttribute] so that it can be 
    /// included as a field in the ServerClientData class. This is
    /// probably not necessary for an enum, but classes in general
    /// should use it or the serverClientData class will not be serializable
    /// </summary>
    [SerializableAttribute]
    [Flags]
    public enum UserDataContentEnum
    {   NO_DATA =0,                // there is no data content
        RECT_DATA = 1,             // rectangle data  is present in the ServerClientData
        STEPPER_CONTROL = 2,       // stepper control data is present in the ServerClientData
        PWMA_DATA = 4,             // pwm A data is present in the ServerClientData
        PWMB_DATA = 8,             // pwm B data is present in the ServerClientData
        FLAG_DATA = 16,            // flags are present in the ServerClientData
        SRCTGT_DATA = 32,          // source and target data is present in the ServerClientData
        PATH_DATA = 64,            // source and target data is present in the ServerClientData and a path should be followed

    }
}
