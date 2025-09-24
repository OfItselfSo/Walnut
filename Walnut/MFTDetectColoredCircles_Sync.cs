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
/// which detects solid rectangular objects as they pass through the transform.
/// 
/// The detected rectangles are marked on the screen with a black cross.
/// EmguCV is used as the detection mechanism and the tolerances are set
/// quite loosely. It will also pick up circles quite readily.
/// 
/// Circles are very computationally quite intensive to detect alone and so 
/// the decision was made to just use circles. The target data is expected
/// not to have colored circles in it. Effectively we are just identifing
/// the center and size of blobs of color here.
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
    /// An MFT to use EmguCV to detect colored blobs
    ///  in video frames. 
    /// 
    /// This MFT can handle 1 media type (ARGB). You will also note that it
    /// hard codes the support for this type
    /// 
    /// </summary>
    public sealed class MFTDetectColoredCircles_Sync : MFTImageRecognitionBase
    {
        // these are used by EmguCV for the Circle detection and marking
        public const int CENTROID_CROSS_BAR_LEN = 10;
        public static Pen blackPen = new Pen(Color.Black, 1);
  
        // set this up to detect the colors
        private const uint DEFAULT_GRAY_DETECTION_RANGE = 15;
        private ColorDetector colorDetectorObj = new ColorDetector(DEFAULT_GRAY_DETECTION_RANGE);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public MFTDetectColoredCircles_Sync() : base()
        {
            // init this now
            m_FrameCount = 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Destructor
        /// </summary>
        ~MFTDetectColoredCircles_Sync()
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
                // Writing into outputMediaBuffer will write to the approprate location in the outputSample
                IdentifiedObjects = DetectCirclesInBuffer(outputMediaBuffer); // much faster, can go at realtime, more or less any frame rate
                if ((IdentifiedObjects != null) && (WantOriginLowerLeft == true))
                {
                    // we need to convert the origin to a lower left (0,0) system. Essentially this means subracting the y coord from the 
                    // image height
                    foreach (ColoredRotatedObject rectObj in IdentifiedObjects)
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
        private List<ColoredRotatedObject> DetectCirclesInBuffer(IMFMediaBuffer outputMediaBuffer)
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
                circles = DetectCirclesInImageOfTypeRGB32(destRawDataPtr,
                                destStride,
                                m_imageWidthInPixels,
                                m_imageHeightInPixels);

                // Set the data size on the output buffer. It probably is already there
                // since the output buffer is the input buffer
                HResult hr = outputMediaBuffer.SetCurrentLength(m_cbImageSize);
                if (hr != HResult.S_OK)
                {
                    circles = null;
                    throw new Exception("DetectCirclesInBuffer call to outputMediaBuffer.SetCurrentLength failed. Err=" + hr.ToString());
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
        /// Detect circles ARGB formatted image and mark them. We use an EmguCV Mat()
        /// object for this. 
        /// </summary>
        /// <param name="pDest">Pointer to the destination buffer.</param>
        /// <param name="lDestStride">Stride of the destination buffer, in bytes.</param>
        /// <param name="dwWidthInPixels">Frame width in pixels.</param>
        /// <param name="dwHeightInPixels">Frame height, in pixels.</param>
        /// <returns>a list of colored circles or null for fail</returns>
        private unsafe List<ColoredRotatedObject> DetectCirclesInImageOfTypeRGB32(
            IntPtr pDest,
            int lDestStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // the return value
            List<ColoredRotatedObject> circles = null;

            // Although the actual data is down in unmanaged memory
            // we do not need to use "unsafe" access to get at it. 
            // The new BitMap() call does this for us. This is probably
            // only useful in this sort of rare circumstance. Normally
            // you have to copy it about. 

            // Get a bitmap wrapper around the video data.
            using (Bitmap bitmapFrame = new Bitmap(m_imageWidthInPixels, m_imageHeightInPixels, m_lStrideIfContiguous, PixelFormat.Format32bppRgb, pDest))
            {
                // get a graphics object and Mat() object
                using (Graphics graphicsObj = Graphics.FromImage(bitmapFrame))
                using (Mat bitmapAsMat = new Mat(bitmapFrame.Height, bitmapFrame.Width, DepthType.Cv8U, 3))
                {
                    // populate the Mat() object
                    bitmapFrame.ToMat(bitmapAsMat);

                    // now find the circles. 
                    circles = FindCircles(bitmapAsMat);
                    circles = DeDuplicateCircles(circles, 2);

                    // lets draw some crosses on the center of each circle of a specified color
                    foreach (ColoredRotatedObject circleCoord in circles)
                    {
                        // yes, this is the right way around
                        int row = Convert.ToInt32(circleCoord.CenterPoint.Y);
                        int col = Convert.ToInt32(circleCoord.CenterPoint.X);

                        // set the pixel values. Note that GetValues is an extension method on Mat() See MatExtension.cs
                        circleCoord.CenterPixelBGRValue = bitmapAsMat.GetValues(row, col);
                        // is it gray? we ignore these
                        if (colorDetectorObj.IsGray(circleCoord.CenterPixelBGRValue) == true) continue;
                        // not gray, detect the color
                        circleCoord.ObjColor = colorDetectorObj.GetClosestKnownColorBGR(circleCoord.CenterPixelBGRValue);

                        // if we identified a color
                        if (circleCoord.ObjColor != ColoredRotatedObject.DEFAULT_COLOR)
                        //if ((circleCoord.ObjColor == KnownColor.Red) || (circleCoord.ObjColor == KnownColor.Blue) || (circleCoord.ObjColor == KnownColor.Green))
                        {
                            // draw the cross
                            Utils.DrawCrossOnPoint(graphicsObj, new Point(Convert.ToInt32(circleCoord.CenterPoint.X), Convert.ToInt32(circleCoord.CenterPoint.Y)), CENTROID_CROSS_BAR_LEN, blackPen);
                        }

                    } // bottom of foreach
                } // bottom of  using (Mat bitmapAsMat
            } // bottom of using (Graphics graphicsObj

            // return what we got
            return circles;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the primary color of a pixel, KnownColor.Black for error or unknown
        /// </summary>
        /// <param name="pixelValue">3 byte BGR pixel value</param>
        /// <returns>true - in range, false - is not</returns>
        public KnownColor GetBGRPixelPrimaryColor(byte[] pixelValue)
        {
            if (pixelValue == null) return KnownColor.Black;
            if (pixelValue.Length != 3) return KnownColor.Black;

            // trial mechanism - we simply choose the largest of the three BGR values
            // and call it that color. 
            if ((pixelValue[0] > pixelValue[1]) && (pixelValue[0] > pixelValue[2])) return KnownColor.Blue;
            if ((pixelValue[1] > pixelValue[2]) && (pixelValue[1] > pixelValue[0])) return KnownColor.Green;
            if ((pixelValue[2] > pixelValue[0]) && (pixelValue[2] > pixelValue[1])) return KnownColor.Red;
            return KnownColor.Black;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Find the centers of all circles in the input image. Derived from the open
        /// source code at:
        /// https://www.emgu.com/wiki/index.php/Shape_(Triangle,_Rectangle,_Circle,_Line)_Detection_in_CSharp
        /// </summary>
        /// <param name="imageToProcess">the input image</param>
        /// <returns>a list of ColoredRotatedRect objects</returns>
        public List<ColoredRotatedObject> FindCircles(Mat imageToProcess)
        {
            if (imageToProcess == null) throw new Exception("Null image provided");

            // we return this
            List<ColoredRotatedObject> boxList = new List<ColoredRotatedObject>();

            double cannyThreshold = 180; // was 180.0;

            using (Mat grayImage = new Mat())
            using (UMat cannyEdges = new UMat())
            {
                //Convert the image to grayscale
                CvInvoke.CvtColor(imageToProcess, grayImage, ColorConversion.Bgr2Gray);

                //Remove noise
                CvInvoke.GaussianBlur(grayImage, grayImage, new Size(3, 3), 1);

                double circleAccumulatorThreshold = 120;
                CircleF[] circles = CvInvoke.HoughCircles(grayImage, HoughModes.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, 5);
                // add all found circles to the output list we will de-dup later
                for(int i =0; i< circles.Length; i++) 
                {
                    boxList.Add(new ColoredRotatedCircle(circles[i]));
                }

            }
            return boxList;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// De-Duplicates found circles in the list. Duplicates happen because of the 
        /// fuzzy edges. 
        /// 
        /// Note: the algorythm here assumes duplicates will be sequential in the list
        /// </summary>
        /// <param name="minSeparationDistance">the minimum separation distance in pixels</param>
        /// <param name="circles">a list of ColoredRotatedRect objects which may have duplicates</param>
        /// <returns>a list of ColoredRotatedRect objects</returns>
        public List<ColoredRotatedObject> DeDuplicateCircles(List<ColoredRotatedObject> circles, int minSeparationDistance) 
        {
            float lastCenterX = -1;
            float lastCenterY = -1;
            // calc this now for fast comparisions
            int sepDistCircled = minSeparationDistance * minSeparationDistance;

            List<ColoredRotatedObject> outList = new List<ColoredRotatedObject>();
            if (circles == null) return outList;

            // lets run through the list 
            foreach (ColoredRotatedObject circleCoord in circles)
            {
                // yes, this is the right way around
                float currentCenterX = circleCoord.CenterPoint.X;
                float currentCenterY = circleCoord.CenterPoint.Y;
                double distance = ( Math.Pow((currentCenterX - lastCenterX), 2) + Math.Pow((currentCenterY - lastCenterY), 2));
                // record for next loop
                lastCenterX = currentCenterX;
                lastCenterY = currentCenterY;

                // now test
                if (distance > sepDistCircled)
                {
                    // we are two distinct rectangles, so copy it to the outList
                    outList.Add(circleCoord);
                }
            }

            return outList;
        }
    }
}
