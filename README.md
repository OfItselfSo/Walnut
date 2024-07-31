# Walnut
The path planning and control software for the FPath project.

Walnut is a Windows Media Foundation application with integrated image recognition functionality from the EmguCV library (a C# OpenCV Interface). It also incorporates the ability to send a typed
object to remote software (WalnutClient) running on the Beaglebone Black via TCP/IP.

The Prism6 application in the OfItselfSo Prism project [http://www.OfItselfSo.com/Prism](http://www.OfItselfSo.com/Prism) was used as the initial structure and has been much modified since then. 

The Walnut software is intended to handle on the CNC planning and control aspects for the FPath project [http://www.OfItselfSo.com/FPath](http://www.OfItselfSo.com/FPath). FPath is an ongoing exploration of the Feynman
Path to Nanotechnology and since the FPath project is so open-ended, the Walnut  software does not have a fixed endpoint. It will be modified, rewritten and forked repeatedly as each iteration is reconfigured for a different
goal. A list of the iterations and their main features is provided below. If you wish to have a specific version you will need to clone based off of the commit number. 

The home page for the Walnut software can be found at: [http://www.OfItselfSo.com/Walnut](http://www.OfItselfSo.com/Walnut)

The Walnut code is released as open source under the MIT License.

## The Walnut Application Versions

- **00.02.03** Commit ID: 4902917
    - This version of the Walnut Server/Client software supports FPath Experiment 001. The purpose of this experiment was to provide a workout and test of the software and hardware tool chain. 
    The actual experimental goal was to 
    move a colored square on a rotating platform as close as possible to a colored square off that platform. See the FPath_Ex001 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex001_PIDControlAndSoftwareTesting.php](http://www.ofitselfso.com/FPath/FPath_Ex001_PIDControlAndSoftwareTesting.php).
    
- **00.02.02** Commit ID: e4fef89
    - Contains all the functionality of v00.02.01, but adds the ability of the WalnutClient to control stepper motors via a assembly language program running in the 
    Programmable Realtime Units (PRU's) of the Beaglebone Black. As a test, the Walnut Server software can send control signals to activate a stepper motor. This 
    functionality has largely been derived from the [http://www.OfItselfSo.com/Tilo](http://www.OfItselfSo.com/Tilo) project and is now fully operational.

- **00.02.01** Commit ID: c6d0e6f
    - Contains all the functionality of v00.01.01 but also implements client software for the Beaglebone Black. The functionality of the 
    RemCon ([http://www.OfItselfSo.com/RemCon](RemCon) project has been incorporated into the Walnut Server and Client to provide a mechanism
    to exchange typed objects over a TCP/IP link in a server/client relationship. At the moment the typed object just consists of the screen coordinates and colors of 
    rectangles picked up by the image recognition system of v00.01.01. Elements of the [http://www.OfItselfSo.com/Tilo](http://www.OfItselfSo.com/Tilo) project have been incorporated 
    to provide eventual control over the FPath Waldos however this code is largely non-functional as yet.

- **00.01.01** Commit ID: 9e5b761
    - Displays webcam images on the screen via Windows Media Foundation and, optionally, saves the stream to an mp4 file. Walnut also contains EmguCV library image recognition code imbedded 
    in a WMF Transform (operating in an independent thread) which can recognise
rectangles of Red, Green and Blue color and mark them on the display with a black cross. Other code accesses the data in the recognition transform and displays the color and center of each rectangle
in a text box on the main form once per second. A further
WMF Transform writes timing, frame count and other information in white text on a black background on the bottom of each frame. Please note that the primary purpose of this version
is to implement the image display, save, text overlay and object center detection functionality. As such the image recognition algorythms have not been particularly well tuned. For example,
although the EmguCV code will recognise colored rectangles in the image, it will also recognise circles. In reality, it seems be just finding the centroid of various colored blobs. This is 
sufficient for the purposes of this version.

