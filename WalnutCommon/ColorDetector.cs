using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

namespace WalnutCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Class to contain code to accept a colored pixel and determine its color
    /// 
    /// </summary>
    public class ColorDetector
    {
        // if all color values are within this range of each other we assume white gray or black
        private const uint DEFAULT_GRAY_DETECTION_RANGE = 10;
        private uint grayDetectionRange = DEFAULT_GRAY_DETECTION_RANGE;

        // this dictionary correlates our selection of known colors to their hues
        private Dictionary<float, KnownColor> colorHueDict = new Dictionary<float, KnownColor>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ColorDetector()
        {
            SetupColorHueDict();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grayDetectionRangeIn">the grey detection range</param>
        public ColorDetector(uint grayDetectionRangeIn)
        {
            GrayDetectionRange = grayDetectionRangeIn;
            SetupColorHueDict();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the color hue dictionary to some known values
        /// </summary>
        private void SetupColorHueDict()
        { 
            colorHueDict.Add(Color.FromArgb(255, 255, 0, 0).GetHue(), KnownColor.Red);
            colorHueDict.Add(Color.FromArgb(255, 0, 255, 0).GetHue(), KnownColor.Green);
            colorHueDict.Add(Color.FromArgb(255, 0, 0, 255).GetHue(), KnownColor.Blue);
            // the GetHue() always returns an angle value between 0 and 359.9999. This messes
            // up some comparisons close to 0. ie 352 is really RED because it is close 
            // to zero in angle rather than blue which is 270, if we add a second RED at 360
            // the math works out better when iterating the dict
            colorHueDict.Add(360, KnownColor.Red);

            // later make this a settable option. Also need Orange and others. For now three colors
            // makes things more accurate
            //colorHueDict.Add(Color.FromArgb(255, 255, 0, 255).GetHue(), KnownColor.Magenta);
            //colorHueDict.Add(Color.FromArgb(255, 255, 255, 0).GetHue(), KnownColor.Yellow);
            //colorHueDict.Add(Color.FromArgb(255, 0, 255, 255).GetHue(), KnownColor.Cyan);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a BGR byte array to a known color. Uses the limited selection in
        /// the colorHueDict for its choices
        /// 
        /// Note: will check for greys according to the set detection range
        /// 
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <returns>the closest known color, or black for fail</returns>
        public KnownColor GetClosestKnownColorBGR(byte[] pixelValue)
        {
            // check for gray, hues don't work well on blacks, grays and whites
            if (IsGray(pixelValue) == true) return KnownColor.Gray;

            KeyValuePair<float, KnownColor> closestMatch = new KeyValuePair<float, KnownColor>(float.MaxValue, KnownColor.Black);
            float lowestDiff = float.MaxValue;

            // convert to a Color
            Color testColor = BGRPixelToColor(pixelValue);
            // get the Hue of the test color
            float testHue = testColor.GetHue();

            // find the closest hue to the input
            foreach (KeyValuePair<float, KnownColor> pairObj in colorHueDict)
            {
                // get the difference
                float workingHueDiff = Math.Abs(testHue - pairObj.Key);

                // have we have found the closest match so far 
                if (workingHueDiff < lowestDiff)
                {
                    // yes, record it
                    closestMatch = pairObj;
                    lowestDiff = workingHueDiff;
                }
            }
            return closestMatch.Value;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a RGB byte array to a known color. Uses the limited selection in
        /// the colorHueDict for its choices
        /// 
        /// Note: will check for greys according to the set detection range
        /// 
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <returns>the closest known color, or black for fail</returns>
        public KnownColor GetClosestKnownColorRGB(byte[] pixelValue)
        {
            // check for gray, hues don't work well on blacks, grays and whites
            if (IsGray(pixelValue) == true) return KnownColor.Gray;

            KeyValuePair<float, KnownColor> closestMatch = new KeyValuePair<float, KnownColor>(float.MaxValue, KnownColor.Black);
            float lowestDiff = float.MaxValue;

            // convert to a Color
            Color testColor = RGBPixelToColor(pixelValue);
            // get the Hue of the test color
            float testHue = testColor.GetHue();

            // find the closest hue to the input
            foreach (KeyValuePair<float, KnownColor> pairObj in colorHueDict)
            {
                // get the difference
                float workingHueDiff = Math.Abs(testHue - pairObj.Key);

                // have we have found the closest match so far 
                if (workingHueDiff < lowestDiff)
                {
                    // yes, record it
                    closestMatch = pairObj;
                    lowestDiff = workingHueDiff;
                }
            }
            return closestMatch.Value;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a RGB byte array to a known color. Uses the limited selection in
        /// the colorHueDict for its choices
        /// 
        /// Note: will check for greys according to the set detection range
        /// 
        /// </summary>
        /// <param name="colorIn">the Color value</param>
        /// <returns>the closest known color, or black for fail</returns>
        public KnownColor GetClosestKnownColorRGB(Color colorIn)
        {
            // check for gray, hues don't work well on blacks, grays and whites
            if (IsGray(colorIn) == true) return KnownColor.Gray;

            KeyValuePair<float, KnownColor> closestMatch = new KeyValuePair<float, KnownColor>(float.MaxValue, KnownColor.Black);
            float lowestDiff = float.MaxValue;

            // get the Hue of the test color
            float testHue = colorIn.GetHue();

            // find the closest hue to the input
            foreach (KeyValuePair<float, KnownColor> pairObj in colorHueDict)
            {
                // get the difference
                float workingHueDiff = Math.Abs(testHue - pairObj.Key);

                // have we have found the closest match so far 
                if (workingHueDiff < lowestDiff)
                {
                    // yes, record it
                    closestMatch = pairObj;
                    lowestDiff = workingHueDiff;
                }
            }
            return closestMatch.Value;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a BGR byte array to a color value 
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <returns>true - in range, false - is not</returns>
        public static Color BGRPixelToColor(byte[] pixelValue)
        {
            if (pixelValue == null) return Color.Black;
            if (pixelValue.Length != 3) return Color.Black;
            return Color.FromArgb(pixelValue[2], pixelValue[1], pixelValue[0]);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a RGB byte array to a color value 
        /// </summary>
        /// <param name="pixelValue">3 byte RGB pixel value</param>
        /// <returns>true - in range, false - is not</returns>
        public static Color RGBPixelToColor(byte[] pixelValue)
        {
            if (pixelValue == null) return Color.Black;
            if (pixelValue.Length != 3) return Color.Black;
            return Color.FromArgb(pixelValue[0], pixelValue[1], pixelValue[2]);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect if a color is considered grey
        /// </summary>
        /// <param name="testColor">the color to test</param>
        /// <returns>true - is grey, false - is not</returns>
        public bool IsGray(Color testColor)
        {
            // just call this
            return IsGray(new byte[] { testColor.R, testColor.G, testColor.B});
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect if a pixel is grey does not matter RGB or BGR
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <returns>true - is grey, false - is not</returns>
        public bool IsGray(byte[] pixelValue)
        {
            // just call this
            return IsGray(pixelValue, GrayDetectionRange);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect if a pixel is grey does not matter RGB or BGR
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <param name="detectionRange">the range the BGR values can vary from one another </param>
        /// <returns>true - is grey, false - is not</returns>
        public bool IsGray(byte[] pixelValue, uint detectionRange)
        {
            if (pixelValue == null) return false;
            if (pixelValue.Length != 3) return false;
            // all of the pixele values must be in range of one another
            if (Math.Abs(pixelValue[0] - pixelValue[1]) > detectionRange) return false;
            if (Math.Abs(pixelValue[1] - pixelValue[2]) > detectionRange) return false;
            if (Math.Abs(pixelValue[2] - pixelValue[0]) > detectionRange) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the grey value detection range
        /// </summary>
        /// <returns>true - is grey, false - is not</returns>
        public uint GrayDetectionRange { get => grayDetectionRange; set => grayDetectionRange = value; }


    }
}
