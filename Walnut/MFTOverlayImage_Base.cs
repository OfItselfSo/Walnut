using Emgu.CV.Structure;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.OPM;
using MediaFoundation.Transform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TantaCommon;
using WalnutCommon;
using static Emgu.CV.Fuzzy.FuzzyInvoke;

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

/// Large parts of this code are derived from the samples which ship with the MF.Net dll. These are 
/// in turn derived from the original Microsoft samples. These have been placed in the public domain 
/// without copyright. The original copyright statement is below

/// *****************************************************************************
/// Original Copyright Statement - Released to public domain
/// While the underlying library is covered by LGPL or BSD, this sample is released
/// as public domain.  It is distributed in the hope that it will be useful, but
/// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
/// or FITNESS FOR A PARTICULAR PURPOSE.
/// ******************************************************************************

/// This file implements a Synchronous Media Foundation Transform (MFT)
/// which provides a base class to write bitmaps onto the video frames as they pass 
/// through the transform.
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
    /// An MFT to provide base class support of Overlay image transforms
    /// 
    /// This MFT can handle 1 media types (ARGB). You will also note that it
    /// hard codes the support for this type 
    /// 
    /// </summary>
    public abstract class MFTOverlayImage_Base : TantaMFTBase_Sync
    {
        // Format information
        private int m_imageWidthInPixels;
        private int m_imageHeightInPixels;
        private int m_cbImageSize;              // Image size, in bytes.
        private int m_lStrideIfContiguous;

        // this list of the guids of the media subtypes we support. The input format must be the same
        // as the output format 
        private readonly Guid[] m_MediaSubtypes = new Guid[] { MFMediaType.RGB32 };

        // we do not support interlacing. If the Media Type proposed by the client says
        // it "might be interlaced" we set this flag. If interlaced frames are passe in, we will reject them 
        private bool m_MightBeInterlaced;

        protected DirectBitmap overlayImage;
        protected DirectBitmap trackerImage;

        // we use this to draw on the bitmaps
        protected Graphics overlayGraphicsObj = null;
        protected Graphics trackerGraphicsObj = null;

        private bool displayTrackerOnImage = true;
        private bool displayOnlyOverlayOnImage = false;

        // when we mask off areas we record the lowest alpha value we have seen. 
        // this can be useful for some algorythms
        private byte lowestAlphaFoundOnMask = 255;

        // grid stuff
        private Color? lastGridColor = null;
        private bool gridEnabled=false;
        private Color gridColor = Color.Yellow;
        private int gridCountX = 2;
        private int gridCountY = 2;
        private int gridBarSizeX = 10;
        private int gridBarSizeY = 10;
        private int gridSpacingPixels = 50;

        // this is the region at the bottom of the screen we do not do object recognition in
        // it is the area of chyron text - so there is no point
        private int bottomOfScreenSkipHeight = 0;

        private Color transparentColor = Color.FromArgb(0, 255, 255, 255);
        private SolidBrush whiteTransparentBrush = new SolidBrush(Color.FromArgb(0, 255, 255, 255));

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public MFTOverlayImage_Base() : base()
        {
            // create some default overlay bitmaps, these can be overridden later
            InitOverlayAndTrackerImages();
            // DebugMessage("MFTOverlayImage_Base Constructor");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Destructor
        /// </summary>
        ~MFTOverlayImage_Base()
        {
            // DebugMessage("MFTOverlayImage_Base Destructor");
        }

        // ########################################################################
        // ##### TantaMFTBase_Sync Overrides, all child classes must implement these
        // ########################################################################

        #region Overrides

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns a value indicating if the proposed input type is acceptable to 
        /// this MFT.
        /// 
        /// An override of the abstract version in TantaMFTBase_Sync. 
        /// </summary>
        /// <param name="pmt">The type to check.  Should never be null.</param>
        /// <returns>S_Ok if the type is valid or MF_E_INVALIDTYPE.</returns>
        override protected HResult OnCheckInputType(IMFMediaType pmt)
        {
            HResult hr;

            // We assume the input type will get checked first
            if (OutputType == null)
            {
                // we do not have an output type, check that the proposed
                // input type is acceptable
                hr = OnCheckMediaType(pmt);
            }
            else
            {
                // we have an output type
                hr = TantaWMFUtils.IsMediaTypeIdentical(pmt, OutputType);
            }

            return hr;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Return settings to describe input stream. This should get the buffer 
        /// requirements and other information for an input stream. 
        /// (see IMFTransform::GetInputStreamInfo).
        /// 
        /// An override of the abstract version in TantaMFTBase_Sync. 
        /// </summary>
        /// <param name="pStreamInfo">The struct where the parameters get set.</param>
        override protected void OnGetInputStreamInfo(ref MFTInputStreamInfo pStreamInfo)
        {
            // return the image size
            pStreamInfo.cbSize = m_cbImageSize;
            // MFT_INPUT_STREAM_WHOLE_SAMPLES - Each media sample(IMFSample interface) of 
            //    input data from the MFT contains complete, unbroken units of data. 
            // MFT_INPUT_STREAM_SINGLE_SAMPLE_PER_BUFFER -  Each input sample contains 
            //    exactly one unit of data 
            // MFT_INPUT_STREAM_FIXED_SAMPLE_SIZE - All input samples are the same size.
            // MFT_INPUT_STREAM_PROCESSES_IN_PLACE - The MFT can perform in-place processing.
            //     In this mode, the MFT directly modifies the input buffer. When the client calls 
            //     ProcessOutput, the same sample that was delivered to this stream is returned in 
            //     the output stream that has a matching stream identifier. This flag implies that 
            //     the MFT holds onto the input buffer, so this flag cannot be combined with the 
            //     MFT_INPUT_STREAM_DOES_NOT_ADDREF flag. If this flag is present, the MFT must 
            //     set the MFT_OUTPUT_STREAM_PROVIDES_SAMPLES or MFT_OUTPUT_STREAM_CAN_PROVIDE_SAMPLES 
            //     flag for the output stream that corresponds to this input stream. 
            pStreamInfo.dwFlags = MFTInputStreamInfoFlags.WholeSamples |
                                  MFTInputStreamInfoFlags.FixedSampleSize |
                                  MFTInputStreamInfoFlags.SingleSamplePerBuffer |
                                  MFTInputStreamInfoFlags.ProcessesInPlace;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Return settings to describe output stream. This should get the buffer 
        /// requirements and other information for an output stream. 
        /// (see IMFTransform::GetOutputStreamInfo).
        /// 
        /// An override of the abstract version in TantaMFTBase_Sync. 
        /// </summary>
        /// <param name="pStreamInfo">The struct where the parameters get set.</param>
        override protected void OnGetOutputStreamInfo(ref MFTOutputStreamInfo pStreamInfo)
        {
            // return the image size
            pStreamInfo.cbSize = m_cbImageSize;

            // MFT_OUTPUT_STREAM_WHOLE_SAMPLES - Each media sample(IMFSample interface) of 
            //    output data from the MFT contains complete, unbroken units of data. 
            // MFT_OUTPUT_STREAM_SINGLE_SAMPLE_PER_BUFFER -  Each output sample contains 
            //    exactly one unit of data 
            // MFT_OUTPUT_STREAM_FIXED_SAMPLE_SIZE - All output samples are the same size.
            // MFT_OUTPUT_STREAM_PROVIDES_SAMPLES - The MFT provides the output samples 
            //    for this stream, either by allocating them internally or by operating 
            //    directly on the input samples. The MFT cannot use output samples provided 
            //    by the client for this stream. If this flag is not set, the MFT must 
            //    set cbSize to a nonzero value in the MFT_OUTPUT_STREAM_INFO structure, 
            //    so that the client can allocate the correct buffer size. For more information,
            //    see IMFTransform::GetOutputStreamInfo. This flag cannot be combined with 
            //    the MFT_OUTPUT_STREAM_CAN_PROVIDE_SAMPLES flag.
            pStreamInfo.dwFlags = MFTOutputStreamInfoFlags.WholeSamples |
                                  MFTOutputStreamInfoFlags.SingleSamplePerBuffer |
                                  MFTOutputStreamInfoFlags.FixedSampleSize |
                                  MFTOutputStreamInfoFlags.ProvidesSamples;
        }

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

                // now that we have an output buffer, do the work to draw the overlay and tracker on it.
                // Writing into outputMediaBuffer will write to the approprate location in the outputSample
                DrawOverlayImageOnBuffer(outputMediaBuffer);

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
        /// The MFT defines a list of available media types for each input stream
        /// and orders them by preference. This method enumerates the available
        /// media types for an input stream. 
        /// 
        /// Many clients will just "try it on" with their preferred media type
        /// and if/when that gets rejected will start enumerating the types the
        /// transform prefers in order to see if they have one in common
        ///
        /// An override of the virtual version in TantaMFTBase_Sync. 
        /// </summary>
        /// <param name="dwTypeIndex">The (zero-based) index of the type.</param>
        /// <param name="pInputType">The input type supported by the MFT.</param>
        /// <returns>S_Ok unless error.</returns>
        protected override HResult OnEnumInputTypes(int dwTypeIndex, out IMFMediaType pInputType)
        {
            return TantaWMFUtils.CreatePartialMediaType(dwTypeIndex, MFMediaType.Video, m_MediaSubtypes, out pInputType);
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Called when the output type should be set. Since our output type must be the 
        ///  same as the input type we just create the output type as a copy of 
        ///  the input type here
        ///  
        ///  Expects the InputType variable to have been set.
        ///
        ///  An override of the virtual stub in TantaMFTBase_Sync. 
        /// </summary>
        protected override void OnSetOutputType()
        {
            // If the output type is null or is being reset to null (by 
            // dynamic format change), pre-populate it.
            if (InputType != null && OutputType == null)
            {
                OutputType = CreateOutputFromInput();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Processes the input. Most of the transformation happens in OnProcessOutput
        /// so all we need to do here is check to see if the sample is interlaced and, if
        /// it is, discard it.
        /// 
        /// Expects InputSample to be set.
        /// </summary>
        /// <returns>S_Ok or E_FAIL.</returns>
        protected override HResult OnProcessInput()
        {
            HResult hr = HResult.S_OK;

            // While we accept types that *might* be interlaced, if we actually receive
            // an interlaced sample, reject it.
            if (m_MightBeInterlaced == true)
            {
                int ix;

                // Returns a bool: true = interlaced, false = progressive
                hr = InputSample.GetUINT32(MFAttributesClsid.MFSampleExtension_Interlaced, out ix);
                if (hr != HResult.S_OK || ix != 0)
                {
                    hr = HResult.E_FAIL;
                }
            }

            return hr;
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Validates a media type for this transform. Since both input and output types must be
        /// the same, they both call this routine.
        /// </summary>
        /// <param name="pmt">The media type to validate.</param>
        /// <returns>S_Ok or MF_E_INVALIDTYPE.</returns>
        private HResult OnCheckMediaType(IMFMediaType pmt)
        {
            int interlace;
            HResult hr = HResult.S_OK;

            // see if the media type is one of our list of acceptable subtypes
            hr = TantaWMFUtils.CheckMediaType(pmt, MFMediaType.Video, m_MediaSubtypes);
            if (hr != HResult.S_OK)
            {
                // return this - we will try something else (probably)
                return HResult.MF_E_INVALIDTYPE;
                //throw new Exception("OnCheckMediaType call to TantaWMFUtils.CheckMediaType failed. Err=" + hr.ToString());
            }

            // Video must be progressive frames. Set this now
            m_MightBeInterlaced = false;

            // get the interlace mode
            hr = pmt.GetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, out interlace);
            if (hr != HResult.S_OK)
            {
                throw new Exception("OnCheckMediaType call to getting the interlace mode failed. Err=" + hr.ToString());
            }
            // set it now
            MFVideoInterlaceMode im = (MFVideoInterlaceMode)interlace;

            // Mostly we only accept Progressive.
            if (im == MFVideoInterlaceMode.Progressive) return HResult.S_OK;
            // If the type MIGHT be interlaced, we'll accept it.
            if (im == MFVideoInterlaceMode.MixedInterlaceOrProgressive)
            {
                // But we will check to see if any samples actually
                // are interlaced, and reject them.
                m_MightBeInterlaced = true;

                return HResult.S_OK;
            }
            // not a valid option
            return HResult.MF_E_INVALIDTYPE;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Called when the input type gets set. We record the basic image 
        ///  size and format information here.
        ///  
        ///  Expects the InputType variable to have been set. This will have been
        ///  done in the base class immediately before this routine gets called
        ///
        ///  An override of the virtual stub in TantaMFTBase_Sync. 
        /// </summary>
        override protected void OnSetInputType()
        {
            HResult hr;

            // init some things
            m_imageWidthInPixels = 0;
            m_imageHeightInPixels = 0;
            m_cbImageSize = 0;
            m_lStrideIfContiguous = 0;

            // get this now, the type can be null to clear
            IMFMediaType pmt = InputType;
            if (pmt == null)
            {
                // Since the input must be set before the output, nulling the 
                // input must also clear the output.  Note that nulling the 
                // input is only valid if we are not actively streaming.
                OutputType = null;
                return;
            }

            // get the image width and height in pixels. These will become 
            // very important later when the time comes to size and center the 
            // text we will write on the screen.

            // Note that changing the size of the image on the screen, by resizing
            // the EVR control does not change this image size. The EVR will 
            // remove various rows and columns as necssary for display purposes
            hr = MFExtern.MFGetAttributeSize(pmt, MFAttributesClsid.MF_MT_FRAME_SIZE, out m_imageWidthInPixels, out m_imageHeightInPixels);
            if (hr != HResult.S_OK)
            {
                throw new Exception("OnSetInputType call to get the image size failed failed. Err=" + hr.ToString());
            }

            // get the image size
            hr = MFExtern.MFGetAttributeSize(pmt, MFAttributesClsid.MF_MT_FRAME_SIZE, out m_imageWidthInPixels, out m_imageHeightInPixels);
            if (hr != HResult.S_OK)
            {
                throw new Exception("OnSetInputType call to get MFGetAttributeSize failed failed. Err=" + hr.ToString());
            }

            // get the image stride. The stride is the number of bytes from one row of pixels in memory 
            // to the next row of pixels in memory. If padding bytes are present, the stride is wider 
            // than the width of the image.
            hr = pmt.GetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, out m_lStrideIfContiguous);
            if (hr != HResult.S_OK)
            {
                throw new Exception("OnSetInputType call to MF_MT_DEFAULT_STRIDE failed failed. Err=" + hr.ToString());
            }

            // Calculate the image size (including padding)
            m_cbImageSize = m_imageHeightInPixels * m_lStrideIfContiguous;

            // If the output type isn't set yet, we can pre-populate it, 
            // since output must always exactly equal input.  This can 
            // save a (tiny) bit of time in negotiating types.

            OnSetOutputType();

        }

        #endregion

        // ########################################################################
        // ##### Image draw and display code
        // ########################################################################
        #region DrawCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw the overlay image on the output buffer
        /// </summary>
        /// <param name="outputMediaBuffer">Output buffer</param>
        private void DrawOverlayImageOnBuffer(IMFMediaBuffer outputMediaBuffer)
        {
            IntPtr destRawDataPtr = IntPtr.Zero;			// Destination buffer.
            int destStride = 0;	                            // Destination stride.
            bool destIs2D = false;

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

                // We could eventually offer the ability to write on other formats depending on the 
                // current media type. We have this hardcoded to ARGB for now
                DrawOverlayImageOnFrame(destRawDataPtr,
                                destStride,
                                m_imageWidthInPixels,
                                m_imageHeightInPixels);

                // Set the data size on the output buffer. It probably is already there
                // since the output buffer is the input buffer
                HResult hr = outputMediaBuffer.SetCurrentLength(m_cbImageSize);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("DrawOverlayImageOnBuffer call to outputMediaBuffer.SetCurrentLength failed. Err=" + hr.ToString());
                }
            }
            finally
            {
                // we MUST unlock
                if (destIs2D == false) TantaWMFUtils.UnLockIMFMediaBuffer(outputMediaBuffer);
                else TantaWMFUtils.UnLockIMF2DBuffer((outputMediaBuffer as IMF2DBuffer));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw the overlay image on an ARGB formatted frame
        /// </summary>
        /// <param name="pDest">Pointer to the destination buffer.</param>
        /// <param name="lDestStride">Stride of the destination buffer, in bytes.</param>
        /// <param name="dwWidthInPixels">Frame width in pixels.</param>
        /// <param name="dwHeightInPixels">Frame height, in pixels.</param>
        private void DrawOverlayImageOnFrame(
            IntPtr pDest,
            int lDestStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {

            // Although the actual data is down in unmanaged memory
            // we do not need to use "unsafe" access to get at it. 
            // The new Bitmap() call does this for us. This is probably
            // only useful in this sort of rare circumstance. Normally
            // you have to copy it about. See the MFTTantaGrayscale_Sync code.


            // A wrapper around the video data.
            //using (Bitmap v = new Bitmap(m_imageWidthInPixels, m_imageHeightInPixels, m_lStrideIfContiguous, PixelFormat.Format32bppRgb, pDest))
            using (Bitmap v = new Bitmap(m_imageWidthInPixels, m_imageHeightInPixels, m_lStrideIfContiguous, PixelFormat.Format32bppPArgb, pDest))
            {
                using (Graphics g = Graphics.FromImage(v))
                {
                    // if we have a overlay image draw it on the frame
                    if (overlayImage != null)
                    {
                        // image overlay compositing is really quite simple. See
                        // https://chrisbitting.com/2013/11/08/overlaying-compositing-images-using-c-system-drawing/
                        if (DisplayOnlyOverlayOnImage == true)
                        {
                            g.CompositingMode = CompositingMode.SourceCopy;
                            g.DrawImage(overlayImage.Bitmap, 0, 0);
                        }
                        else
                        {
                            g.CompositingMode = CompositingMode.SourceOver;
                            g.DrawImage(overlayImage.Bitmap, 0, 0);
                            // if we have a tracker image draw it as well
                            if ((displayTrackerOnImage == true) && (trackerImage != null))
                            {
                                g.DrawImage(trackerImage.Bitmap, 0, 0);
                            }
                        }
                    }
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the height of the region at the bottom of the screen we do not do object recognition
        /// </summary>
        public int BottomOfScreenSkipHeight { get => bottomOfScreenSkipHeight; set => bottomOfScreenSkipHeight = value; }

        #endregion

        // ########################################################################
        // ##### Image processing and manipulation code
        // ########################################################################

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Creates the base overlay and tracker bitmaps 
        ///  
        /// </summary>
        public void InitOverlayAndTrackerImages()
        {
            // reset this
            overlayImage = null;
            trackerImage = null;

            // set our bitmap graphics object, dispose first if it already exists
            if (overlayGraphicsObj != null)
            {
                overlayGraphicsObj.Dispose();
                overlayGraphicsObj = null;
            }

            // set our bitmap graphics object, dispose first if it already exists
            if (trackerGraphicsObj != null)
            {
                trackerGraphicsObj.Dispose();
                trackerGraphicsObj = null;
            }

            // just build a default 640x480 transparent bitmap and use that
            overlayImage = new DirectBitmap(640, 480);
            overlayGraphicsObj = Graphics.FromImage(overlayImage.Bitmap);
            overlayGraphicsObj.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            // we have to flip the Y axis or the graphics draw calls will be inverted
            overlayGraphicsObj.ScaleTransform(1.0F, -1.0F);
            overlayGraphicsObj.TranslateTransform(0.0F, -(float)overlayImage.Height);

            // just build a default 640x480 transparent bitmap and use that
            trackerImage = new DirectBitmap(640, 480);
            trackerGraphicsObj = Graphics.FromImage(trackerImage.Bitmap);
            trackerGraphicsObj.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            // we have to flip the Y axis or the graphics draw calls will be inverted
            trackerGraphicsObj.ScaleTransform(1.0F, -1.0F);
            trackerGraphicsObj.TranslateTransform(0.0F, -(float)trackerImage.Height);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///    Saves the overlay image to disk as a PNG. We do not check if already 
        ///    exists. These sort of checks should be already done
        ///  
        /// </summary>
        /// <param name="overlayImagePathAndFilename">the path and filename</param>
        public void SaveOverlayImageAsPNG(string overlayImagePathAndFilename)
        {
            if((overlayImagePathAndFilename==null) || (overlayImagePathAndFilename.Length<10)) throw new Exception("Invalid overlay path and filename");
            if (overlayImage == null) throw new Exception("Overlay image is null");
            // just do it
            overlayImage.SaveAsPNG(overlayImagePathAndFilename);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///    Saves the overlay image to disk as a binary file. We do not check if already 
        ///    exists. These sort of checks should be already done
        ///  
        /// </summary>
        /// <param name="overlayImagePathAndFilename">the path and filename</param>
        public void SaveOverlayImageAsBinary(string overlayImagePathAndFilename)
        {
            if ((overlayImagePathAndFilename == null) || (overlayImagePathAndFilename.Length < 10)) throw new Exception("Invalid overlay path and filename");
            if (overlayImage == null) throw new Exception("Overlay image is null");
            // just do it
            overlayImage.SaveAsBinary(overlayImagePathAndFilename);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///    Load the overlay image from disk as a binary file. This file should
        ///    have been written by SaveOverlayImageAsBinary
        ///  
        /// </summary>
        /// <param name="overlayImagePathAndFilename">the path and filename</param>
        public void LoadOverlayImageAsBinary(string overlayImagePathAndFilename)
        {
            if ((overlayImagePathAndFilename == null) || (overlayImagePathAndFilename.Length < 10)) throw new Exception("Invalid overlay path and filename");
            if (overlayImage == null) throw new Exception("Overlay image is null");
            // just do it
            overlayImage.LoadAsBinary(overlayImagePathAndFilename);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets overlay graphics object. There is no set this is done with the 
        /// transform is loaded. Will return Null. 
        /// </summary>
        public Graphics OverlayGraphicsObj { get => overlayGraphicsObj; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last grid color
        /// </summary>
        protected Color? LastGridColor { get => lastGridColor; set => lastGridColor = value; }
        public bool GridEnabled { get => gridEnabled; set => gridEnabled = value; }
        public bool DisplayTrackerOnImage { get => displayTrackerOnImage; set => displayTrackerOnImage = value; }
        public bool DisplayOnlyOverlayOnImage { get => displayOnlyOverlayOnImage; set => displayOnlyOverlayOnImage = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the overlay bitmap between two points. 
        ///  
        ///  Mostly for diagnostics because they do not get erased
        ///  
        /// </summary>
        /// <param name="endPoint">the end point</param>
        /// <param name="startPoint">the start point</param>
        /// <param name="workingPen">pen to use</param>
        public void DrawLineBetweenPointsOnOverlay(Pen workingPen, Point startPoint, Point endPoint)
        {
            if (overlayImage == null) return;
            if (overlayGraphicsObj == null) return;
            // draw the line with the default pen
            overlayGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the overlay bitmap between two points. 
        ///  
        ///  Mostly for diagnostics because they do not get erased
        ///  
        /// </summary>
        /// <param name="endPoint">the end point</param>
        /// <param name="startPoint">the start point</param>
        /// <param name="workingPen">pen to use</param>
        public void DrawLineAtCenterPointOnOverlay(Pen workingPen, Point centerPoint, int lineLen, bool wantVert)
        {
            Point? startPoint = null;
            Point? endPoint = null;
            if (overlayImage == null) return;
            if (overlayGraphicsObj == null) return;
            if (lineLen <= 0) return;

            // calc the start and end points
            if (wantVert == false)
            {
                // we want horizontal line
                startPoint = new Point(centerPoint.X - (lineLen/2), centerPoint.Y);
                endPoint = new Point(centerPoint.X + (lineLen / 2), centerPoint.Y);
            }
            else
            {
                // we want vertical
                startPoint = new Point(centerPoint.X, centerPoint.Y - (lineLen / 2));
                endPoint = new Point(centerPoint.X , centerPoint.Y + (lineLen / 2));
            }

            // draw the line with the pen
            overlayGraphicsObj.DrawLine(workingPen, (Point)startPoint, (Point)endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the tracker bitmap between two points. 
        ///  
        ///  Mostly for diagnostics because they do not get erased
        ///  
        /// </summary>
        /// <param name="endPoint">the end point</param>
        /// <param name="startPoint">the start point</param>
        /// <param name="workingPen">pen to use</param>
        public void DrawLineAtCenterPointOnTracker(Pen workingPen, Point centerPoint, int lineLen, bool wantVert)
        {
            Point? startPoint = null;
            Point? endPoint = null;
            if (trackerImage == null) return;
            if (trackerGraphicsObj == null) return;
            if (lineLen <= 0) return;

            // calc the start and end points
            if (wantVert == false)
            {
                // we want horizontal line
                startPoint = new Point(centerPoint.X - (lineLen / 2), centerPoint.Y);
                endPoint = new Point(centerPoint.X + (lineLen / 2), centerPoint.Y);
            }
            else
            {
                // we want vertical
                startPoint = new Point(centerPoint.X, centerPoint.Y - (lineLen / 2));
                endPoint = new Point(centerPoint.X, centerPoint.Y + (lineLen / 2));
            }

            // draw the line with the pen
            trackerGraphicsObj.DrawLine(workingPen, (Point)startPoint, (Point)endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the tracker bitmap between two points. 
        ///  
        ///  Mostly for diagnostics because they do not get erased
        ///  
        /// </summary>
        /// <param name="endPoint">the end point</param>
        /// <param name="startPoint">the start point</param>
        /// <param name="workingPen">pen to use</param>
        public void DrawLineBetweenPointsOnTracker(Pen workingPen, Point startPoint, Point endPoint)
        {
            if (trackerImage == null) return;
            if (trackerGraphicsObj == null) return;
            // draw the line with the default pen
            trackerGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the color of a point on the overlay image
        ///  
        /// </summary>
        /// <param name="pointNonInverted">the point to look at, should be non-inverted</param>
        public Color GetColorOnOverlayAtPoint(Point pointNonInverted)
        {
            if (overlayImage == null) return Color.Empty;
            if(pointNonInverted == null) return Color.Empty;
            if(pointNonInverted.IsEmpty) return Color.Empty;

            return overlayImage.GetPixel(pointNonInverted.X, pointNonInverted.Y);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a circle at the specfied point of the specified radius of the 
        ///  specified line width using the specified brush. 
        ///  
        /// </summary>
        /// <param name="centerPoint">the centerpoint of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        /// <param name="workingBrush">the brush to use</param>
        public void DrawCircleOnOverlayAsOutlineOnOverlay(SolidBrush workingBrush, Point centerPoint, int radius, int lineThickness)
        {
            if (overlayImage == null) return;

            // some sanity checks
            if (centerPoint.IsEmpty == true) return;
            if (lineThickness <= 0) return;
            if (radius <= 0) return;
            if (radius - lineThickness <= 0) return;


            // Create global graphics object for alteration.
            try
            {
                if (overlayGraphicsObj != null)
                {
                    // Draw a filled circle on screen.
                    overlayGraphicsObj.FillEllipse(workingBrush, centerPoint.X - radius, centerPoint.Y - radius, radius * 2, radius * 2);
                    // transparent a smaller filled circle
                    overlayGraphicsObj.FillEllipse(whiteTransparentBrush, centerPoint.X - radius + lineThickness, centerPoint.Y - radius + lineThickness, (radius - lineThickness) * 2, (radius - lineThickness) * 2);
                }
            }
            catch { }
            finally
            {
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Makes a circular part of the overlay image a specified color. If using 
        ///  a color equivalent to the transparent color, the overlay region will be
        ///  rendered transparent. If already transparent there will be no change. 
        ///  
        /// </summary>
        /// <param name="centerPoint">the centerpoint of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        /// <param name="workingBrush">the brush to use</param>
        public void FillCircularRegionOnOverlay(SolidBrush workingBrush, Point centerPoint, int radius)
        {
            if (overlayImage == null) return;

            // some sanity checks
            if (centerPoint.IsEmpty == true) return;

            // make sure we have a global grahics object for alteration.
            try
            {
                if (overlayGraphicsObj != null)
                {
                    // fill circle on screen.
                    overlayGraphicsObj.FillEllipse(workingBrush, centerPoint.X - radius, centerPoint.Y - radius, radius * 2, radius * 2);
                }
            }
            catch { }
            finally
            {
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Makes a rectangular part of the overlay image a specified color. If using 
        ///  a color equivalent to the transparent color, the overlay region will be
        ///  rendered transparent. If already transparent there will be no change. 
        ///  
        /// </summary>
        /// <param name="rectIn">the rectangle on the overlay we check</param>
        /// <param name="workingBrush">the brush to use</param>
        public void FillRectangularRegionOnOverlay(SolidBrush workingBrush, Rectangle rectIn)
        {
            if (overlayImage == null) return;

            // some sanity checks
            if (rectIn == null) return;
            if (rectIn.Width == 0) return;
            if (rectIn.Height == 0) return;

            try
            {
                if (overlayGraphicsObj != null)
                {
                    // Transparent fill circle on screen.
                    overlayGraphicsObj.FillRectangle(workingBrush, rectIn);
                }
            }
            catch { }
            finally
            {
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Sets the alpha value for a specific color in a rectangle on the overlay 
        ///  
        /// </summary>
        /// <param name="color">the color we check</param>
        /// <param name="alphaValue">should be between 0-255 only the bottom 8 bits will be used</param>
        /// <param name="rectIn">the rectangle on the overlay we check (inverted coordinates)</param>
        public void SetAlphaValueForColorInRectOnOverlay(Color color, int alphaValue, Rectangle rectIn)
        {
            if (overlayImage == null) return;

            // some sanity checks
            if (rectIn == null) return;
            if (rectIn.Width == 0) return;
            if (rectIn.Height == 0) return;
            if (m_imageHeightInPixels <= 0) return;

            // Make the call into the direct bitmap.
            try
            {
                // convert the rectangle to nonInverted. The call below needs it this way
                Rectangle invertedRect = Utils.ConvertRectangleToInvertedRectangle(rectIn, m_imageHeightInPixels);
                // set the alpha channel now
                overlayImage.SetAlphaValueForColorInRect(color, alphaValue, invertedRect);
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the min effective screen width of the overlay. This is the min width we can draw on
        ///   
        ///  Will return -ve for fail
        /// 
        /// </summary>
        public int GetMinEffectiveScreenWidth
        {
            get
            {
                if (overlayImage == null) return -1;
                return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the min effective screen height of the overlay. This is the min height we can draw on
        ///   
        ///  Will return -ve for fail
        /// 
        /// </summary>
        public int GetMinEffectiveScreenHeight
        {
            get
            {
                if (overlayImage == null) return -1;
                return BottomOfScreenSkipHeight;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the max effective screen width of the overlay. This is the max width we can draw on
        ///   
        ///  Will return -ve for fail
        /// 
        /// </summary>
        public int GetMaxEffectiveScreenWidth
        {
            get
            {
                if (overlayImage == null) return -1;
                return overlayImage.Width;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the max effective screen height of the overlay. This is the max height we can draw on
        ///   
        ///  Will return -ve for fail
        /// 
        /// </summary>
        public int GetMaxEffectiveScreenHeight
        {
            get
            {
                if (overlayImage == null) return -1;
                return overlayImage.Height;
            }
        }

        public byte LowestAlphaFoundOnMask { get => lowestAlphaFoundOnMask; set => lowestAlphaFoundOnMask = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the overlay bitmap through a point. Always does full
        ///  width or height
        ///  
        /// </summary>
        /// <param name="throughPoint">the through point</param>
        /// <param name="workingPen">pen to use, contains the width</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        /// 
        public void DrawLineThroughPointOnOverlay(Pen workingPen, Point throughPoint, bool wantVert)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);

            if (overlayImage == null) return;
            if (overlayGraphicsObj == null) return;
            if (wantVert == true)
            {
                // we want vertical line, full width, set up our points
                startPoint.X = throughPoint.X;
                endPoint.X = throughPoint.X;
                startPoint.Y = bottomOfScreenSkipHeight;
                endPoint.Y = m_imageHeightInPixels - 1;

            }
            else
            {
                // we want horizontal line, full width, set up our points
                startPoint.Y = throughPoint.Y;
                endPoint.Y = throughPoint.Y;
                startPoint.X = 0;
                endPoint.X = m_imageWidthInPixels - 1;
            }

            // draw the line with the default pen
            overlayGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line on the tracker bitmap through a point. Always does full
        ///  width or height
        ///  
        /// </summary>
        /// <param name="throughPoint">the through point</param>
        /// <param name="workingPen">pen to use, contains the width</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        /// 
        public void DrawLineThroughPointOnTracker(Pen workingPen, Point throughPoint, bool wantVert)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);

            if (trackerImage == null) return;
            if (trackerGraphicsObj == null) return;
            if (wantVert == true)
            {
                // we want vertical line, full width, set up our points
                startPoint.X = throughPoint.X;
                endPoint.X = throughPoint.X;
                startPoint.Y = bottomOfScreenSkipHeight;
                endPoint.Y = m_imageHeightInPixels - 1;

            }
            else
            {
                // we want horizontal line, full width, set up our points
                startPoint.Y = throughPoint.Y;
                endPoint.Y = throughPoint.Y;
                startPoint.X = 0;
                endPoint.X = m_imageWidthInPixels - 1;
            }

            // draw the line with the default pen
            trackerGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line of specified length on the overlay bitmap through a point. 
        ///  We always center the line on the point. We will not exceed the boundarys
        ///  
        /// </summary>
        /// <param name="throughPoint">the through point</param>
        /// <param name="workingPen">pen to use, contains the width</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        /// <param name="length">the length of the line to draw, -ve means full width or height</param>
        /// 
        public void DrawLineThroughPointOnOverlay(Pen workingPen, Point throughPoint, int length, bool wantVert)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);

            if (overlayImage == null) return;
            if (overlayGraphicsObj == null) return;
            if (length < 0)
            {
                // just draw full width or height
                DrawLineThroughPointOnOverlay(workingPen, throughPoint, wantVert);
                return;
            }

            // if we get here we are only drawing a specific length

            // calc the half length
            int halfLength = length / 2;
            if (halfLength <= 0) return;

            if (wantVert == true)
            {
                // we want vertical line, never more than full height, set up our points
                startPoint.X = throughPoint.X;
                endPoint.X = throughPoint.X;
                startPoint.Y = throughPoint.Y - halfLength;
                endPoint.Y = throughPoint.Y + halfLength;
                if (endPoint.Y < bottomOfScreenSkipHeight) endPoint.Y = bottomOfScreenSkipHeight;
                if (endPoint.Y > m_imageHeightInPixels - 1) endPoint.Y = m_imageHeightInPixels - 1;
            }
            else
            {
                // we want horizontal line, never more than full width, set up our points
                startPoint.Y = throughPoint.Y;
                endPoint.Y = throughPoint.Y;
                startPoint.X = throughPoint.X - halfLength;
                endPoint.X = throughPoint.X + halfLength;
                if (endPoint.X < 0) endPoint.X = 0;
                if (endPoint.X > m_imageWidthInPixels - 1) endPoint.X = m_imageWidthInPixels - 1;
            }

            // draw the line with the default pen
            overlayGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Draws a line of specified length on the tracker bitmap through a point. 
        ///  We always center the line on the point. We will not exceed the boundarys
        ///  
        /// </summary>
        /// <param name="throughPoint">the through point</param>
        /// <param name="workingPen">pen to use, contains the width</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        /// <param name="length">the length of the line to draw, -ve means full width or height</param>
        /// 
        public void DrawLineThroughPointOnTracker(Pen workingPen, Point throughPoint, int length, bool wantVert)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);

            if (trackerImage == null) return;
            if (trackerGraphicsObj == null) return;
            if (length < 0)
            {
                // just draw full width or height
                DrawLineThroughPointOnTracker(workingPen, throughPoint, wantVert);
                return;
            }

            // if we get here we are only drawing a specific length

            // calc the half length
            int halfLength = length / 2;
            if (halfLength <= 0) return;

            if (wantVert == true)
            {
                // we want vertical line, never more than full height, set up our points
                startPoint.X = throughPoint.X;
                endPoint.X = throughPoint.X;
                startPoint.Y = throughPoint.Y - halfLength;
                endPoint.Y = throughPoint.Y + halfLength;
                if (endPoint.Y < bottomOfScreenSkipHeight) endPoint.Y = bottomOfScreenSkipHeight;
                if (endPoint.Y > m_imageHeightInPixels - 1) endPoint.Y = m_imageHeightInPixels - 1;
            }
            else
            {
                // we want horizontal line, never more than full width, set up our points
                startPoint.Y = throughPoint.Y;
                endPoint.Y = throughPoint.Y;
                startPoint.X = throughPoint.X - halfLength;
                endPoint.X = throughPoint.X + halfLength;
                if (endPoint.X < 0) endPoint.X = 0;
                if (endPoint.X > m_imageWidthInPixels - 1) endPoint.X = m_imageWidthInPixels - 1;
            }

            // draw the line with the default pen
            trackerGraphicsObj.DrawLine(workingPen, startPoint, endPoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Makes a circular part of the tracker image a specified color. If using 
        ///  a color equivalent to the transparent color, the tracker region will be
        ///  rendered transparent. If already transparent there will be no change. 
        ///  
        /// </summary>
        /// <param name="centerPoint">the centerpoint of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        /// <param name="workingBrush">the brush to use</param>
        public void FillCircularRegionOnTracker(SolidBrush workingBrush, Point centerPoint, int radius)
        {
            if (trackerImage == null) return;

            // some sanity checks
            if (centerPoint.IsEmpty == true) return;

            // make sure we have a global grahics object for alteration.
            try
            {
                if (trackerGraphicsObj != null)
                {
                    // fill circle on screen.
                    trackerGraphicsObj.FillEllipse(workingBrush, centerPoint.X - radius, centerPoint.Y - radius, radius * 2, radius * 2);
                }
            }
            catch { }
            finally
            {
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Copys the tracker bitmap contents onto the overlay bitmap
        ///  
        /// </summary>
        public void CopyTrackerOntoOverlay()
        {
            if (trackerImage == null) return;
            if (overlayImage == null) return;
            overlayImage.CopyFrom(trackerImage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Converts a color on the Overlay to another color
        ///  
        /// </summary>
        /// <param name="color">the source color</param>
        /// <param name="toColor">the color to convert to</param>
        public void ConvertColorToColorOnOverlay(Color color, Color toColor)
        {
            if (overlayImage == null) return;
            overlayImage.ConvertColorToColor(color, toColor);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Converts a color in a rect on the Overlay to another color
        ///  
        /// </summary>
        /// <param name="rectIn">the rect in which we draw</param>
        /// <param name="color">the source color</param>
        /// <param name="preserveAlpha">if true we ignore alpha when comparing and preserve it over the change</param>
        /// <param name="toColor">the color to convert to</param>
        public void ConvertColorToColorInRectOnOverlay(Color color, Color toColor, Rectangle rectIn, bool preserveAlpha)
        {
            // some sanity checks
            if (rectIn == null) return;
            if (rectIn.Width == 0) return;
            if (rectIn.Height == 0) return;
            if (m_imageHeightInPixels <= 0) return;
            if (overlayImage == null) return;

            try
            {
                // convert the rectangle to nonInverted. The call below needs it this way
                Rectangle invertedRect = Utils.ConvertRectangleToInvertedRectangle(rectIn, m_imageHeightInPixels);
                lowestAlphaFoundOnMask = overlayImage.ConvertColorToColorInRect(color, toColor, invertedRect, preserveAlpha);
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Finds all occurances of a specific color and erases them (makes them
        ///  transparent)
        ///  
        /// </summary>
        /// <param name="color">the color to remove</param>
        /// <param name="toColor">the color to convert to</param>
        public void ClearColorFromOverlay(Color color)
        {
            if (overlayImage == null) return;
            if (color == null) return;
            overlayImage.ConvertColorToColor(color, transparentColor);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Finds all occurances of a specific color and erases them (makes them
        ///  transparent)
        ///  
        /// </summary>
        /// <param name="color">the color to remove</param>
        /// <param name="toColor">the color to convert to</param>
        public void ClearColorFromTracker(Color color)
        {
            if (trackerImage == null) return;
            if (color == null) return;
            trackerImage.ConvertColorToColor(color, transparentColor);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Clears the tracker DirectBitmap, uses a transparent brush
        ///  
        /// </summary>
        public void ClearTracker()
        {
            if (trackerImage == null) return;
            if (trackerGraphicsObj != null)
            {
                Rectangle fullRectagle = new Rectangle(new Point(0, 0), new Size(trackerImage.Width, trackerImage.Height));
                trackerGraphicsObj.FillRectangle(whiteTransparentBrush, fullRectagle);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Clears the overlay DirectBitmap, uses a transparent brush
        ///  
        /// </summary>
        public void ClearOverlay()
        {
            if (overlayImage == null) return;
            if (overlayGraphicsObj != null)
            {
                Rectangle fullRectagle = new Rectangle(new Point(0, 0), new Size(overlayImage.Width, overlayImage.Height));
                overlayGraphicsObj.FillRectangle(whiteTransparentBrush, fullRectagle);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Gets the nearest colored point from the specified origin point. 
        ///  
        ///  Note this uses a spiral algorythm to find the point 
        ///  
        /// </summary>
        /// <param name="originPoint">the origin we start from</param>
        /// <param name="colorIgnoreAlphaChannel">the value of the color being looked for - we ignore the alpha channel</param
        /// <param name="minConsecutivePointsNeeded">the minimum number of consecutive points needed in order to consider a returned point valid</param>
        /// <returns>the nearest colored point or an empty point for fail</returns>
        public Point GetNearestColorPointFromOrigin(Point originPoint, Color colorIgnoreAlphaChannel, int minConsecutivePointsNeeded)
        {
            if (originPoint.IsEmpty == true) return new Point();
            if (overlayImage == null) return new Point();

            try
            {
                // this sets up an enumerator which uses the yield pattern
                IEnumerable<Point> pixels = Utils.GetSpiralGrid(originPoint, new Size(overlayImage.Width - 1, overlayImage.Height - 1));
                foreach (Point i in pixels)
                {

                    // get the pixel. not very efficient
                    Color pixelColor = overlayImage.GetPixelInvertedY(i.X, i.Y);
                    // believe it or not this is actually one of the more efficent ways of comparing colors
                    if ((pixelColor.R == colorIgnoreAlphaChannel.R) && (pixelColor.G == colorIgnoreAlphaChannel.G) && (pixelColor.B == colorIgnoreAlphaChannel.B))
                    {
                        Point foundPoint = new Point(i.X, i.Y);
                        return foundPoint;
                    }
                }
            }
            catch { }
            // not found 
            return new Point();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the grid from the image overlay. Uses the last known grid color to 
        /// do so.
        /// </summary>
        public void ClearGrid()
        {
            if (LastGridColor == null) return;

            // just clear it
            ClearColorFromOverlay((Color)LastGridColor);
            LastGridColor = null;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the grid on the overlay. All parameters are supposed to have been checked
        /// before we get them. The grid will not draw if these are not right
        /// </summary>
        /// <param name="gridEnabledIn">the grid is enabled</param>
        /// <param name="gridBarSizeXIn">the size in pixels of the x bar of the grid</param>
        /// <param name="gridBarSizeYIn">the size in pixels of the y bar of the grid</param>
        /// <param name="gridColorIn">the color we draw the grid in</param>
        /// <param name="gridCountXIn">the number of grid points in the X direction</param>
        /// <param name="gridCountYIn">the number of grid points in the Y direction</param>
        /// <param name="gridSpacingPixelIn">the number of pixels between the grid point spacing</param>
        public void SetGrid(bool gridEnabledIn, Color gridColorIn, int gridCountXIn, int gridCountYIn, int gridBarSizeXIn, int gridBarSizeYIn, int gridSpacingPixelsIn)
        {
            gridColor = gridColorIn;
            gridCountX = gridCountXIn;
            gridCountY = gridCountYIn;
            gridBarSizeX = gridBarSizeXIn;
            gridBarSizeY = gridBarSizeYIn;
            gridSpacingPixels = gridSpacingPixelsIn;
            GridEnabled = gridEnabledIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws the grid on the tracker. All parameters are supposed to have been checked
        /// before we get them. If we find something we don't like we just leave. We also 
        /// do not clear the old grid. Note this is a symmetric grid. The spacing in the X
        /// direction is the same as in the Y direction.
        /// </summary>
        public void DrawGridOnTracker()
        {
            int lowerLeftXOrigin = 0;
            int lowerLeftYOrigin = 0;

            // sanity checks
            if (gridEnabled == false) return;   // any existing grid must be cleared elsewhere

            if (gridBarSizeX <=0) return;
            if (gridBarSizeY <=0) return;
            if (gridBarSizeX <= 0) return;
            if (gridBarSizeY <= 0) return;
            if (gridSpacingPixels <= 0) return;
            if (gridCountX <= 0) return; 
            if (gridCountY <= 0) return; 
            if (gridColor == null) return;
            if (m_imageWidthInPixels<=0) return;
            if (m_imageHeightInPixels<=0) return;

            // set up our empty grid
            Point[,] gridArray = new Point[gridCountX, gridCountY];

            // now figure out our start point X and Y. This differs if the X or Y is even or odd
            if(gridCountX%2==0)
            {
                // we are even, LLX is ...
                lowerLeftXOrigin = (m_imageWidthInPixels/2) - (((gridCountX / 2) * gridSpacingPixels) - (gridSpacingPixels / 2));
            }
            else
            {
                // we are odd, LLX is ....
                lowerLeftXOrigin = (m_imageWidthInPixels/2) - (((gridCountX / 2) * gridSpacingPixels));
            }
            if (gridCountY % 2 == 0)
            {
                // we are even, LLY is ...
                lowerLeftYOrigin = (m_imageHeightInPixels/2) - (((gridCountY / 2) * gridSpacingPixels) - (gridSpacingPixels / 2));
            }
            else
            {
                // we are odd, LLY is ....
                lowerLeftYOrigin = (m_imageHeightInPixels/2) - (((gridCountY / 2) * gridSpacingPixels));
            }

            // now we have our LowerLeft start point in both X and Y we can just roll through the array and 
            // set each grid center point appropriately
            for (int col = 0; col < gridArray.GetLength(0); col++)
            {
                for (int row = 0; row < gridArray.GetLength(1); row++)
                {
                    gridArray[col, row] = new Point(((col* gridSpacingPixels)+ lowerLeftXOrigin), ((row * gridSpacingPixels) + lowerLeftYOrigin));
                }
            }

            // now we draw the grid bars
            using (Pen workingPen = new Pen(gridColor))
            {
                for (int col = 0; col < gridArray.GetLength(0); col++)
                {
                    for (int row = 0; row < gridArray.GetLength(1); row++)
                    {
                        DrawLineAtCenterPointOnTracker(workingPen, gridArray[col, row], gridBarSizeX, false);
                        DrawLineAtCenterPointOnTracker(workingPen, gridArray[col, row], gridBarSizeY, true);
                    }
                }
            }

            // remember to set this
            LastGridColor = gridColor;
        }

    }

}
