# Walnut
The path planning and control software for the FPath project.

Walnut is a Windows Media Foundation application with integrated image recognition functionality from the EmguCV library (a C# OpenCV Interface).

The Prism6 application in the OfItselfSo Prism project [http://www.OfItselfSo.com/Prism](http://www.OfItselfSo.com/Prism) was used as the initial structure and has been much modified since then. 

The Walnut software is intended to handle on the CNC planning and control aspects for the FPath project [http://www.OfItselfSo.com/FPath](http://www.OfItselfSo.com/FPath). FPath is an ongoing exploration of the Feynman
Path to Nanotechnology and since the FPath project is so open-ended, the Walnut  software does not have a fixed endpoint. It will be modified, rewritten and forked repeatedly as each iteration is reconfigured for a different
goal. A list of the iterations and their main features is provided below. If you wish to have a specific version you will need to clone based off of the commit number. 

The home page for the Walnut software can be found at: [http://www.OfItselfSo.com/Walnut](http://www.OfItselfSo.com/Walnut)

The Walnut code is released as open source under the MIT License.

## The Walnut Application Versions

- **00.01.01** Commit ID: 9e5b761
    - Displays webcam images on the screen via Windows Media Foundation and, optionally, saves the stream to an mp4 file. Walnut also contains EmguCV library image recognition code imbedded 
    in a WMF Transform (operating in an independent thread) which can recognise
rectangles of Red, Green and Blue color and mark them on the display with a black cross. Other code accesses the data in the recognition transform and displays the color and center of each rectangle
in a text box on the main form once per second. A further
WMF Transform writes timing, frame count and other information in white text on a black background on the bottom of each frame. Please note that the primary purpose of this version
is to implement the image display, save, text overlay and object center detection functionality. As such the image recognition algorythms have not been particularly well tuned. For example,
although the EmguCV code will recognise colored rectangles in the image, it will also recognise circles. In reality, it seems be just finding the centroid of various colored blobs. This is 
sufficient for the purposes of this version.

