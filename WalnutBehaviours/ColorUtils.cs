using Emgu.CV;
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
    /// this class for behaviour utility routines related to color
    /// 
    /// </summary>
    [SerializableAttribute]
    public static class ColorUtils
    {
        // some workingColors, note .net Color.Green is #ff000800" not #ff00ff00" like you would expect. See:
        // https://stackoverflow.com/questions/4342300/why-is-system-drawing-workingColor-green-0-128-0
        // we use these workingColors as a definitive workingColor statement 
        private const string HTML_WHITE = "#ffffffff";
        private const string HTML_GREEN = "#ff00ff00";
        private const string HTML_RED = "#ffff0000";
        private const string HTML_BLUE = "#ff0000ff";
        public static Color TRUE_WHITE = ColorTranslator.FromHtml(HTML_WHITE);
        public static Color TRUE_RED = ColorTranslator.FromHtml(HTML_RED);
        public static Color TRUE_GREEN = ColorTranslator.FromHtml(HTML_GREEN);
        public static Color TRUE_BLUE = ColorTranslator.FromHtml(HTML_BLUE);

        private static Color DEFAULT_COLOR = TRUE_WHITE;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the workingColor with a full alpha channel
        /// 
        /// </summary>
        /// <returns>the working color with a full alpha channel</returns>
        static public Color ColorWithFullAlphaChannel(Color workingColorIn)
        {
            if(workingColorIn == null) return DEFAULT_COLOR;
            return Color.FromArgb(0xFF, workingColorIn);  
        }
    }
}
