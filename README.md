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
- **00.02.09** Commit ID: 38341fb
   - This code is designed to support the FPath Experiment 008 which demonstrate that the use of small, common off the shelf (COTS), stepper motor driven linear actuators can form a viable positioning mechanism down to about the 50 micron level - if closed loop feedback is used. See the FPath_Ex008 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex008_BorrowedPrecision.php](http://www.ofitselfso.com/FPath/FPath_Ex008_BorrowedPrecision.php).

- **00.02.07** Commit ID: 6a96848
   - This code is intended to support experiment 006 - the goal of which was to demonstrate that closed loop control is a viable error reduction option when having large machines make smaller machines which then make smaller machines. An interesting application of a transparent panel control has also been implemented. This allows mouse events to be intercepted on third party controls. See the FPath_Ex006 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex006_ThePantograph.php](http://www.ofitselfso.com/FPath/FPath_Ex006_ThePantograph.php).
    
- **00.02.07** Commit ID: 1fb9950
   - This code is designed to support the 2D area filling using a technique dubbed Stigmergic Fill. A new WMF transform has been implemented and this code uses various fast bitmaps to overlay, in real-time, an image frame with a virtual area which clearly shows which pixels of a certain color underlie it. Since the pixels on the areas which have not been filled are clearly a different color it is very simple algorithmically to determine which areas remain to be processed. See the FPath_Ex005 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex005_StigmergicFill.php](http://www.ofitselfso.com/FPath/FPath_Ex005_StigmergicFill.php).
   

- **00.02.06** Commit ID: 424bddc
    - This experiment demonstrates the controlled motion of a movable red circle over a virtual linear path drawn in the color green. The green path is removed as the circle moves over it. As the red circle moves along, it can lay down a colored "trail" which forms a return path. When there is no more of the green path to follow, the red circle will follow the return path back to the start. See the FPath_Ex004 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex004_GraphicalStigmergy.php](http://www.ofitselfso.com/FPath/FPath_Ex004_GraphicalStigmergy.php).
    
- **00.02.05** Commit ID: ae0b457
    - This experiment uses an XY stage to demonstrate 2D control of DC Gear Motors via pulse width modulation. It is designed to identify red and green squares via image recognition and move the red square onto the position of the virtual green square. See the FPath_Ex003 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex003_2DMotion.php](http://www.ofitselfso.com/FPath/FPath_Ex003_2DMotion.php).

- **00.02.04** Commit ID: d9a1b38
    - This version builds on the results of the Walnut version created for FPath Experiment 001 and uses a virtual static target entity. Also introduced was the ability to overlay the webcam stream with a semi-transparent image. See the FPath_Ex002 web page for more details: 
    [http://www.ofitselfso.com/FPath/FPath_Ex002_VirtualTargets.php](http://www.ofitselfso.com/FPath/FPath_Ex002_VirtualTargets.php).
    
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

