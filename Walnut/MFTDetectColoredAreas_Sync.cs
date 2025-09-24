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
    public sealed class MFTDetectColoredAreas_Sync : MFTImageRecognitionBase
    {
        // these are used by EmguCV for the Circle detection and marking
        public const int CENTROID_CROSS_BAR_LEN = 10;
        public static Pen blackPen = new Pen(Color.Black, 1);
  
        // set this up to detect the colors
        private const uint DEFAULT_GRAY_DETECTION_RANGE = 50;
        private ColorDetector colorDetectorObj = new ColorDetector(DEFAULT_GRAY_DETECTION_RANGE);

        // used to optimize the circle finding algorythm
       // Point centerLastCircleFound = new Point(0, 0);
        Point centerLastCircleFound = new Point(320, 240);


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public MFTDetectColoredAreas_Sync() : base()
        {
            // init this now
            m_FrameCount = 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Destructor
        /// </summary>
        ~MFTDetectColoredAreas_Sync()
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
                IdentifiedObjects = DetectAreasInBuffer(outputMediaBuffer); // much faster, can go at realtime, more or less any frame rate
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
        private List<ColoredRotatedObject> DetectAreasInBuffer(IMFMediaBuffer outputMediaBuffer)
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
                circles = DetectAreasInImageOfTypeRGB32(destRawDataPtr,
                                destStride,
                                m_imageWidthInPixels,
                                m_imageHeightInPixels);

                // Set the data size on the output buffer. It probably is already there
                // since the output buffer is the input buffer
                HResult hr = outputMediaBuffer.SetCurrentLength(m_cbImageSize);
                if (hr != HResult.S_OK)
                {
                    circles = null;
                    throw new Exception("DetectAreasInBuffer call to outputMediaBuffer.SetCurrentLength failed. Err=" + hr.ToString());
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
        private unsafe List<ColoredRotatedObject> DetectAreasInImageOfTypeRGB32(
            IntPtr pDest,
            int lDestStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // the return value
            List<ColoredRotatedObject> circles = new List<ColoredRotatedObject>();

            // should be already set
            if (colorDetectorObj==null) colorDetectorObj = new ColorDetector(DEFAULT_GRAY_DETECTION_RANGE);

            // Although the actual data is down in unmanaged memory
            // we do not need to use "unsafe" access to get at it. 
            // The new DirectBitmap() call does this for us. 

            // some hardcoded constants. They really should be set on the object but no need for now
            // note that the code will still find circles of larger diameter than this (but not smaller)
            // but the centroid point will be off center
            const int ASSUMED_DIAMETER_OF_CIRCLE = 11;
            const int SEARCH_DISTANCE = (ASSUMED_DIAMETER_OF_CIRCLE/2)+1;
            const KnownColor SEARCH_COLOR = KnownColor.Red;

            // Get a directBitmap wrapper around the video data for fast pixel access
            // remember this is a copy, not the original so writing on it will have no effect
            // we must Dispose() at the end, hence the "using" call
            using (DirectBitmap dBitmap = new DirectBitmap(m_imageWidthInPixels, m_imageHeightInPixels, pDest))
            {
                // this sets up an enumerator which uses the yield pattern. It is much faster for us to look at pixels in a spiral
                // from the last known centroid location and stop when we found one. This implies that we are only looking for one
                // circle of known color if we are looking for multiple circles or multiple colors we can use the alternate code
                // which has been commented out. This scans each frame in its entireity. It is fast enough actually
                IEnumerable<Point> pixels = Utils.GetSpiralGrid(centerLastCircleFound, new Size(m_imageWidthInPixels - 1, (m_imageHeightInPixels - BottomOfScreenSkipHeight) - 1));
                foreach (Point i in pixels)
                {
                    // get the pixel. not very efficient
                    Color pixelColor = dBitmap.GetPixel(i.X, i.Y);
                    // is it the proper color
                    byte[] bytes = new byte[] { pixelColor.R, pixelColor.G, pixelColor.B };
                    KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                    if (kc == SEARCH_COLOR)
                    {
                        KnownColor xxc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                        Point centerPoint = DetectCentroidOfObject(dBitmap, SEARCH_DISTANCE, ASSUMED_DIAMETER_OF_CIRCLE, KnownColor.Red, i.X, i.Y, m_imageWidthInPixels, (m_imageHeightInPixels - BottomOfScreenSkipHeight));
                        if (centerPoint.IsEmpty == true) continue;
                        // is good 
                        ColoredRotatedObject tmp = new ColoredRotatedCircle(new CircleF(centerPoint, ASSUMED_DIAMETER_OF_CIRCLE/2));
                        tmp.CenterPixelBGRValue = Utils.RBGPixelToBGR(pixelColor);
                        tmp.ObjColor = kc;
                        circles.Add(tmp);
                        centerLastCircleFound = centerPoint;
                        break;
                    }
                }

                //// this also works but is slower as it does the whole frame (potentially)
                //// loop through every pixel, remember the 0,0 point is at the top left of the frame at this 
                //// point. In other words, the Y axis is inverted
                //for (int i = 0; i < m_imageWidthInPixels; i++)
                //{
                //    if (circles.Count > 0) break;
                //    // we do not look in the bar at the bottom, hence the BottomOfScreenSkipHeight 
                //    for (int j = 0; j < (m_imageHeightInPixels- BottomOfScreenSkipHeight); j++)
                //    {
                //        if (circles.Count > 0) break;
                //        Color c = dBitmap.GetPixel(i, j);
                //        byte[] bytes = new byte[] { c.R, c.G, c.B };
                //        KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                //        if (kc == SEARCH_COLOR)
                //        {
                //            Point centerPoint = DetectCentroidOfObject(dBitmap, SEARCH_DISTANCE, ASSUMED_DIAMETER_OF_CIRCLE, KnownColor.Red, i, j, m_imageWidthInPixels, (m_imageHeightInPixels - BottomOfScreenSkipHeight));
                //            if (centerPoint.IsEmpty == true) continue;
                //            // is good 
                //            ColoredRotatedObject tmp = new ColoredRotatedObject(centerPoint);
                //            tmp.CenterPixelBGRValue = Utils.RBGPixelToBGR(c);
                //            tmp.ObjColor = kc;
                //            circles.Add(tmp);
                //        }
                //    }
                //}
                //// if doing multiple circles don't forget to do something like this
                //circles = DeDuplicateCircles(circles, 10);

                // this code draws a little cross on the top of each found object. It is for diagnostics mostly. Note that
                // we cannot use the SetPixel() call on the DirectBitmap above. That data is a copy and updating it will 
                // not affect the screen. So we use pDest and build a writeable bitmap from that. It is reasonably fast
                if (circles.Count > 0)
                {
                    using (Bitmap bitmapFrame = new Bitmap(m_imageWidthInPixels, m_imageHeightInPixels, m_lStrideIfContiguous, PixelFormat.Format32bppRgb, pDest))
                    {
                        // get a graphics object and Mat() object
                        using (Graphics graphicsObj = Graphics.FromImage(bitmapFrame))
                        {
                            // draw the cross
                            Utils.DrawCrossOnPoint(graphicsObj, new Point(Convert.ToInt32(circles[0].CenterPoint.X), Convert.ToInt32(circles[0].CenterPoint.Y)), CENTROID_CROSS_BAR_LEN, blackPen);
                        }
                    }
                }
            }
            // return what we got
            return circles;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Finds the centroid of a circular-ish object if the color and diameter is known.
        /// 
        /// Will detect objects of a larger diameter but the found centroid will be off center
        /// </summary>
        /// <param name="dBitmap">the direct bitmap object we look in. This has fast pixel access</param>
        /// <param name="minDiameter">the minimum diameter we look for usually slightly larger than detection threshold</param>
        /// <param name="detectionThreshold">the size of contiguous colored in X and Y we have to have before we consider 
        /// the point to be the centriod of the found object. Usually slighty smaller than the diameter</param>
        /// <param name="expectedColor">the colour we expect all pixels to be</param>
        /// <param name="maxX">the maximum X can ever be (for bounds checking)</param>
        /// <param name="maxY">the maximum Y can ever be (for bounds checking)</param>
        /// <param name="startX">the starting X point. Should be of the expectedColor and hence on the object</param>
        /// <param name="startY">the starting Y point. Should be of the expectedColor and hence on the object</param>
        /// <returns>true - in range, false - is not</returns>
        private Point DetectCentroidOfObject(DirectBitmap dBitmap, int minDiameter, int detectionThreshold, KnownColor expectedColor, int startX, int startY, int maxX, int maxY)
        {
            int count_X_Pos = 0;
            int count_X_Neg = 0;
            int count_Y_Pos = 0;
            int count_Y_Neg = 0;

            // no need to be clever here. Just roll through 4 loops, one in each direction

            // count how many we have of the expected color in a positive X direction
            for (int i = startX+1; i < startX + minDiameter; i++)
            {
                if(i> maxX) break;
                Color c = dBitmap.GetPixel(i, startY);
                byte[] bytes = new byte[] { c.R, c.G, c.B };
                KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                if (kc != expectedColor) break; // we are done
                // count it
                count_X_Pos++;
            }
            // count how many we have of the expected color in a negative X direction
            for (int i = startX-1; i >= 0; i--)
            {
                if (i < 0) break;
                Color c = dBitmap.GetPixel(i, startY);
                byte[] bytes = new byte[] { c.R, c.G, c.B };
                KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                if (kc != expectedColor) break; // we are done
                // count it
                count_X_Neg++;
            }

            // count how many we have of the expected color jn a posjtive Y direction
            for (int j = startY+1; j < startY + minDiameter; j++)
            {
                if (j > maxY) break;
                Color c = dBitmap.GetPixel(startX, j);
                byte[] bytes = new byte[] { c.R, c.G, c.B };
                KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                if(kc != expectedColor) break; // we are done
                // count jt
                count_Y_Pos++;
            }
            // count how many we have of the expected color jn a negative Y direction
            for (int j = startY - 1; j >= 0; j--)
            {
                if (j <0 ) break;
                Color c = dBitmap.GetPixel(startX, j);
                byte[] bytes = new byte[] { c.R, c.G, c.B };
                KnownColor kc = colorDetectorObj.GetClosestKnownColorRGB(bytes);
                if(kc != expectedColor) break; // we are done
                // count jt
                count_Y_Neg++;
            }

            // now figure out wether the point we found seems to be in the middle (or at least far enough in)
            // to be considered the centroid
            int halfDetectionThreshold = detectionThreshold / 2;
            if ((count_X_Pos >= halfDetectionThreshold) && (count_X_Neg >= halfDetectionThreshold) && (count_Y_Pos >= halfDetectionThreshold) && (count_Y_Neg >= halfDetectionThreshold))
            {
                return new Point(startX + (count_X_Pos - count_X_Neg+1), startY + (count_Y_Pos - count_Y_Neg+1));
            }
            else
            {
                // return empty point
                return new Point();
            }

            // these also work with varying degrees of success
            //  if ((count_X_Pos > THRESHOLD) || (count_X_Neg > THRESHOLD) || (count_Y_Pos > THRESHOLD) || (count_Y_Neg > THRESHOLD))
            //if ((count_X_Pos >= detectionThreshold / 2) && (count_X_Neg >= detectionThreshold / 2) && (count_Y_Pos >= detectionThreshold / 2) && (count_Y_Neg >= detectionThreshold / 2))
            //{
            //    return new Point(startX, startY);
            //}
            //else
            //{
            //    // return empty point
            //    return new Point();
            //}

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
