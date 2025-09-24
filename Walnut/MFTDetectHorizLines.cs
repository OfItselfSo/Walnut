using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Transform;
using TantaCommon;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;
using WalnutCommon;
using System.Linq;

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

/// This file implements a Synchronous Media Foundation Transform (MFT)
/// which detects black/grey/dark horizontal and vertical lines as  
/// they pass through the transform.
/// 
/// This transform only supports one media type (ARGB) and the input and
/// output types must both be this.
/// 
/// This class uses the TantaMFTBase_Sync base class and much of the 
/// standard processing is handled there. This base class is an only
/// slightly modified version of the MFTBase class which ships with the 
/// MF.Net samples. The MFTBase class (and hence TantaMFTBase_Sync) is
/// designed to factor out all of the common code required to build an 
/// Synchronous MFT in C# MF.Net. 
/// 
/// In the interests of simplicity, this particular MFT is not designed
/// to be "independent" of the rest of the application. In other words,
/// it will not be placed in an independent assembly (DLL) it will not 
/// be COM visible or registered with MFTRegister so that other applications
/// can use it. This MFT is expected to be instantiated with a standard
/// C# new operator and simply given to the topology as a binary
/// 

namespace Walnut
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// An MFT to detect horizontal lines on the screen. 
    /// 
    /// This MFT can handle 1 media type (ARGB). You will also note that it
    /// hard codes the support for this type
    /// 
    /// </summary>
    public sealed class MFTDetectHorizLines : MFTImageRecognitionBase
    {
        // these are the colors of the lines (if we draw them)
        private Color verticalLineColor = Color.Orange;
        private Color horizLineColor = Color.Blue;

        private const int DEFAULT_MARKER_PEN_WIDTH = 2;
        private Pen verticalLineMarkerPen = new Pen(Color.Orange, DEFAULT_MARKER_PEN_WIDTH);
        private Pen horizLineMarkerPen = new Pen(Color.Yellow, DEFAULT_MARKER_PEN_WIDTH);

        // a length of <=1 means do not draw it, these units are pixels
        private int verticalLineLength = 100;   // will be drawn centered horizontally
        private int horizLineLength = 100;      // will be drawn centered vertically

        // this is the width of the line
        private int verticalLineWidth = 0;    
        private int horizLineWidth = 0;

        // we detect colors with this
      //  private ColorDetector colorDetectorObj = new ColorDetector(ColorDetector.DEFAULT_GRAY_DETECTION_RANGE);

        // these are the ranges for our line detection
        private Color topOfHorizRange = Color.FromArgb(10, 10, 10);
        private Color botOfHorizRange = Color.FromArgb(0, 0, 0);
        private Color topOfVertRange = Color.FromArgb(110, 110, 110);
        private Color botOfVertRange = Color.FromArgb(40, 40, 40);
        // minimum number of active pixels to be considered a line
        private int minPixelsInLineHoriz = 100;
        private int minPixelsInLineVert = 100;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public MFTDetectHorizLines() : base()
        {
            // init this now
            m_FrameCount = 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Destructor
        /// </summary>
        ~MFTDetectHorizLines()
        {
        }

        // ########################################################################
        // ##### TantaMFTBase_Sync Overrides, all child classes must implement these
        // ########################################################################

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This is the routine that performs the transform. Assumes InputSample is set.
        ///
        /// An override of the abstract version in TantaMFTBase_Sync. 
        /// </summary>
        /// <param name="outputSampleDataStruct">The structure to populate with output data.</param>
        /// <returns>S_Ok unless error.</returns>
        protected override HResult OnProcessOutput(ref MFTOutputDataBuffer outputSampleDataStruct)
        {
            HResult hr = HResult.S_OK;
            IMFMediaBuffer outputMediaBuffer = null;

            // we are processing in place, the input sample is the output sample, the media buffer of the
            // input sample is the media buffer of the output sample.

            try
            {
                // Get the data buffer from the input sample. If the sample contains more than one buffer, 
                // this method copies the data from the original buffers into a new buffer, and replaces 
                // the original buffer list with the new buffer. The new buffer is returned in the inputMediaBuffer parameter.
                // If the sample contains a single buffer, this method returns a pointer to the original buffer. 
                // In typical use, most samples do not contain multiple buffers.
                hr = InputSample.ConvertToContiguousBuffer(out outputMediaBuffer);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("OnProcessOutput call to InputSample.ConvertToContiguousBuffer failed. Err=" + hr.ToString());
                }

                // now that we have an output buffer, do the work to find the objects.
                // Writing into outputMediaBuffer will write to the appropriate location in the outputSample
                IdentifiedObjects = DetectObjectsInBuffer(outputMediaBuffer);
                if ((IdentifiedObjects != null) && (WantOriginLowerLeft == true))
                {
                    // we need to convert the origin to a lower left (0,0) system. Essentially this means subracting the y coord from the 
                    // image height
                    foreach (ColoredRotatedLine rectObj in IdentifiedObjects)
                    {
                        rectObj.CenterPoint = new Point(rectObj.CenterPoint.X, (m_imageHeightInPixels - rectObj.CenterPoint.Y));
                    }
                }

                // Set status flags.
                outputSampleDataStruct.dwStatus = MFTOutputDataBufferFlags.None;
                // The output sample is the input sample. We get a new IUnknown for the Input
                // sample since we are going to release it below. The client will release this 
                // new IUnknown
                outputSampleDataStruct.pSample = Marshal.GetIUnknownForObject(InputSample);

            }
            finally
            {
                // clean up
                SafeRelease(outputMediaBuffer);

                // Release the current input sample so we can get another one.
                // the act of setting it to null releases it because the property
                // is coded that way
                InputSample = null;
            }

            return HResult.S_OK;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect Circles in the output buffer
        /// </summary>
        /// <param name="outputMediaBuffer">Output buffer</param>
        private List<ColoredRotatedObject> DetectObjectsInBuffer(IMFMediaBuffer outputMediaBuffer)
        {
            IntPtr destRawDataPtr = IntPtr.Zero;			// Destination buffer.
            int destStride = 0;	                            // Destination stride.
            bool destIs2D = false;
            // the return value
            List<ColoredRotatedObject> circles = null;

            try
            {
                // Lock the output buffer. Use the IMF2DBuffer interface  
                // (if available) as it is faster
                if ((outputMediaBuffer is IMF2DBuffer) == false)
                {
                    // not an IMF2DBuffer - get the raw data from the IMFMediaBuffer 
                    int maxLen = 0;
                    int currentLen = 0;
                    TantaWMFUtils.LockIMFMediaBufferAndGetRawData(outputMediaBuffer, out destRawDataPtr, out maxLen, out currentLen);
                    // the stride is always this. The Lock function does not return it
                    destStride = m_lStrideIfContiguous;
                }
                else
                {
                    // we are an IMF2DBuffer, we get the stride here as well
                    TantaWMFUtils.LockIMF2DBufferAndGetRawData((outputMediaBuffer as IMF2DBuffer), out destRawDataPtr, out destStride);
                    destIs2D = true;
                }

                // count this now. We only use this to write it on the screen
                m_FrameCount++;

                // We could eventually offer the ability to write on other formats depending on the 
                // current media type. We have this hardcoded to ARGB for now
                circles =  DetectLongestLinesInImageOfTypeRGB32(destRawDataPtr,
                                destStride,
                                m_imageWidthInPixels,
                                m_imageHeightInPixels);

                // Set the data size on the output buffer. It probably is already there
                // since the output buffer is the input buffer
                HResult hr = outputMediaBuffer.SetCurrentLength(m_cbImageSize);
                if (hr != HResult.S_OK)
                {
                    circles = null;
                    throw new Exception("DetectObjectsInBuffer call to outputMediaBuffer.SetCurrentLength failed. Err=" + hr.ToString());
                }
            }
            finally
            {
                // we MUST unlock
                if (destIs2D == false) TantaWMFUtils.UnLockIMFMediaBuffer(outputMediaBuffer);
                else TantaWMFUtils.UnLockIMF2DBuffer((outputMediaBuffer as IMF2DBuffer));
            }
            // return what we got
            return circles;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect the largest horizonal and vertical lines on the screen and 
        /// (alternately mark them).
        /// 
        /// Note: this uses TopOfHorizRange, BottomOfHorizRange, TopOfVertRange, BottomOfVertRange)
        ///  colors to make its decisions. This must be set externally
        /// </summary>
        /// <param name="pDest">Pointer to the destination buffer.</param>
        /// <param name="lDestStride">Stride of the destination buffer, in bytes.</param>
        /// <param name="dwWidthInPixels">Frame width in pixels.</param>
        /// <param name="dwHeightInPixels">Frame height, in pixels.</param>
        /// <returns>list with the lines or empty list for fail</returns>
        private unsafe List<ColoredRotatedObject>  DetectLongestLinesInImageOfTypeRGB32(
            IntPtr pDest,
            int lDestStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // the return value
            List<ColoredRotatedObject> linesList = new List<ColoredRotatedObject>();

            int[] pixelCountPerCol = new int[m_imageWidthInPixels];
            int[] pixelCountPerRow = new int[(m_imageHeightInPixels - BottomOfScreenSkipHeight)];

            // Although the actual data is down in unmanaged memory
            // we do not need to use "unsafe" access to get at it. 
            // The new DirectBitmap() call does this for us. 

            // Get a directBitmap wrapper around the video data for fast pixel access
            // remember this is a copy, not the original so writing on it will have no effect
            // we must Dispose() at the end, hence the "using" call

            using (DirectBitmap dBitmap = new DirectBitmap(m_imageWidthInPixels, m_imageHeightInPixels, pDest))
            {
                // run along the x axis
                for (int x = 0; x <= m_imageWidthInPixels - 1; x++)
                {
                    // for every pixel (top to bottom) at that x position
                    for (int y = 0; y <= (m_imageHeightInPixels - BottomOfScreenSkipHeight - 1); y++)
                    {
                        // get the pixel. not very efficient
                        Color pixelColor = dBitmap.GetPixel(x, y);

                        //if ((x == 361) && (y == 253))
                        //{
                        //    int foo = 1;
                        //}
                        // is the pixel in the color range for a Horiz line
                        if (ColorDetector.IsInRange(pixelColor, TopOfHorizRange, BotOfHorizRange) == true)
                        {
                            pixelCountPerRow[y]++;  // yes, count it
                        }
                        // is the pixel in the color range for a Vert line
                        if (ColorDetector.IsInRange(pixelColor, TopOfVertRange, BotOfVertRange) == true)
                        {
                            pixelCountPerCol[x]++;  // yes, count it
                        }
                    }
                }
            }

            // we now have the counts of the number of pixels in range for each row and column
            // now find the point of max counts in each row and col
            int maxXVal = 0;
            int maxYVal = 0;
            int xValLoc = -1;
            int yValLoc = -1;

            for (int x = 0; x < pixelCountPerCol.Length; x++)
            {
                if ((pixelCountPerCol[x] > maxXVal) && (pixelCountPerCol[x] >= MinPixelsInLineVert))
                {
                    maxXVal = pixelCountPerCol[x];
                    xValLoc = x;
                }
            }
            for (int y = 0; y < pixelCountPerRow.Length; y++)
            {
                if ((pixelCountPerRow[y] > maxYVal) && (pixelCountPerRow[y] >= MinPixelsInLineHoriz))
                //if (pixelCountPerRow[y] > maxYVal)
                {
                    maxYVal = pixelCountPerRow[y];
                    yValLoc = y;
                }
            }

            // now create the horizontal line, if we found one
            if (yValLoc >= 0)
            {
                ColoredRotatedLine horizLine = new ColoredRotatedLine(new Point(320, yValLoc));
                horizLine.LineLength = 100;
                horizLine.Angle = ColoredRotatedLine.HORIZONTAL_LINE_ANGLE;  // this makes it horizontal
                linesList.Add(horizLine);
            }

            // now create the vertical line, if we found one
            if (xValLoc >= 0)
            {
                ColoredRotatedLine vertLine = new ColoredRotatedLine(new Point(xValLoc, 240));
                vertLine.LineLength = 100;
                vertLine.Angle = ColoredRotatedLine.VERTICAL_LINE_ANGLE;  // this makes it vertical
                linesList.Add(vertLine);
            }

            //// draw some crosses
            //if ((xValLoc >= 0) || (yValLoc >= 0))
            //{
            //    using (Bitmap bitmapFrame = new Bitmap(m_imageWidthInPixels, m_imageHeightInPixels, m_lStrideIfContiguous, PixelFormat.Format32bppRgb, pDest))
            //    {
            //        // get a graphics object
            //        using (Graphics graphicsObj = Graphics.FromImage(bitmapFrame))
            //        {
            //            // draw the cross
            //            if (xValLoc >= 0) Utils.DrawVerticalLineFromCenterPoint(graphicsObj, new Point(xValLoc, 240), VerticalLineLength, verticalLineMarkerPen);
            //            if (yValLoc >= 0) Utils.DrawHorizLineFromCenterPoint(graphicsObj, new Point(320, yValLoc), HorizLineLength, horizLineMarkerPen);
            //        }
            //    }
            //}

            // return what we got
            return linesList;
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// De-Duplicates found lines in the list. Duplicates happen because of the 
        /// fuzzy edges. 
        /// 
        /// Note: the algorythm here assumes duplicates will be sequential in the list
        /// </summary>
        /// <param name="minSeparationDistance">the minimum separation distance in pixels</param>
        /// <param name="linesList">a list of ColoredRotatedLine objects which may have duplicates</param>
        /// <returns>a list of ColoredRotatedLine objects</returns>
        public List<ColoredRotatedObject> DeDuplicateLines(List<ColoredRotatedObject> lineList, int minSeparationDistance) 
        {
            float lastCenterX = -1;
            float lastCenterY = -1;
            // calc this now for fast comparisions
            int sepDistCircled = minSeparationDistance * minSeparationDistance;

            List<ColoredRotatedObject> outList = new List<ColoredRotatedObject>();
            if (lineList == null) return outList;

            // lets run through the list 
            foreach (ColoredRotatedLine lineObj in lineList)
            {
                // yes, this is the right way around
                float currentCenterX = lineObj.CenterPoint.X;
                float currentCenterY = lineObj.CenterPoint.Y;
                double distance = ( Math.Pow((currentCenterX - lastCenterX), 2) + Math.Pow((currentCenterY - lastCenterY), 2));
                // record for next loop
                lastCenterX = currentCenterX;
                lastCenterY = currentCenterY;

                // now test
                if (distance > sepDistCircled)
                {
                    // we are two distinct lines, so copy it to the outList
                    outList.Add(lineObj);
                }
            }

            return outList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the color we use to draw in the vertical line. 
        /// </summary>
        public Color VerticalLineColor 
        { 
            get => verticalLineColor;
            set
            {
                verticalLineColor = value;
                // regenerate the pen
                verticalLineMarkerPen.Color = verticalLineColor;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the color we use to draw in the horizontal line. 
        /// </summary>
        public Color HorizLineColor
        {
            get => horizLineColor;
            set
            {
                horizLineColor = value;
                // regenerate the pen
                horizLineMarkerPen.Color = horizLineColor;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the length of the vertical line. A value <=1 means do not draw.
        /// Will be drawn centered horizontally
        /// </summary>
        public int VerticalLineLength { get => verticalLineLength; set => verticalLineLength = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the length of the horizontal line. A value <=1 means do not draw
        /// Will be drawn centered vertically
        /// </summary>
        public int HorizLineLength { get => horizLineLength; set => horizLineLength = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the width (thickness) of the vertical line. 
        public int VerticalLineWidth
        {
            get => verticalLineWidth;
            set
            {
                verticalLineWidth = value;
                // regenerate the pen
                verticalLineMarkerPen.Width = verticalLineWidth;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the width (thickness) of the horizontal line. 
        public int HorizLineWidth
        {
            get => horizLineWidth;
            set
            {
                horizLineWidth = value;
                // regenerate the pen
                horizLineMarkerPen.Width = horizLineWidth;
            }
        }

        public Color TopOfHorizRange { get => topOfHorizRange; set => topOfHorizRange = value; }
        public Color BotOfHorizRange { get => botOfHorizRange; set => botOfHorizRange = value; }
        public Color TopOfVertRange { get => topOfVertRange; set => topOfVertRange = value; }
        public Color BotOfVertRange { get => botOfVertRange; set => botOfVertRange = value; }
        public int MinPixelsInLineHoriz { get => minPixelsInLineHoriz; set => minPixelsInLineHoriz = value; }
        public int MinPixelsInLineVert { get => minPixelsInLineVert; set => minPixelsInLineVert = value; }
    }
}
