using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Drawing;

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

namespace Walnut
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to persist the settings of the Walnut app.
    /// </summary>
    /// <remarks>
    /// The ApplicationSettingsBase class does not write to the registry. It 
    /// persists the configuration settings by leaving an XML file on disk in the location 
    /// below.
    /// 
    /// <document and settings folder>\<userfolder>\Local Settings\Application
    /// Data\<companyname>\<applicationname>_StrongName\<applicationversion>\user.config
    /// 
    /// This code uses the techniques discussed here
    /// http://msdn.microsoft.com/en-us/library/system.configuration.applicationsettingsbase%28VS.80%29.aspx
    /// </remarks>
    public sealed class ApplicationImplicitSettings : ApplicationSettingsBase
    {

        // ####################################################################
        // ##### Config Items Explicitly Set By the User
        // ####################################################################
        #region Config Items Explicitly Set By the User

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("INCHES")]
        //public ApplicationUnitsEnum DefaultApplicationUnits
        //{
        //    get { return (ApplicationUnitsEnum)this["DefaultApplicationUnits"]; }
        //    set { this["DefaultApplicationUnits"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("INCHES")]
        //public ApplicationUnitsEnum OutputApplicationUnits
        //{
        //    get { return (ApplicationUnitsEnum)this["OutputApplicationUnits"]; }
        //    set { this["OutputApplicationUnits"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("2000")]
        //public int IsoPlotPointsPerAppUnitIN
        //{
        //    get { return (int)this["IsoPlotPointsPerAppUnitIN"]; }
        //    set { this["IsoPlotPointsPerAppUnitIN"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("78")]
        //public int IsoPlotPointsPerAppUnitMM
        //{
        //    get { return (int)this["IsoPlotPointsPerAppUnitMM"]; }
        //    set { this["IsoPlotPointsPerAppUnitMM"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowGerberApertures
        //{
        //    get { return (bool)this["ShowGerberApertures"]; }
        //    set { this["ShowGerberApertures"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowGerberCenterLines
        //{
        //    get { return (bool)this["ShowGerberCenterLines"]; }
        //    set { this["ShowGerberCenterLines"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowOrigin
        //{
        //    get { return (bool)this["ShowOrigin"]; }
        //    set { this["ShowOrigin"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowFlipAxis
        //{
        //    get { return (bool)this["ShowFlipAxis"]; }
        //    set { this["ShowFlipAxis"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowGCodeOrigin
        //{
        //    get { return (bool)this["ShowGCodeOrigin"]; }
        //    set { this["ShowGCodeOrigin"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool ShowGerberOnGCodePlots
        //{
        //    get { return (bool)this["ShowGerberOnGCodePlots"]; }
        //    set { this["ShowGerberOnGCodePlots"] = value; }
        //}

        //[UserScopedSetting()]
        //[DefaultSettingValueAttribute("false")]
        //public bool OKWithDisclaimer
        //{
        //    get { return (bool)this["OKWithDisclaimer"]; }
        //    set { this["OKWithDisclaimer"] = value; }
        //}

        #endregion

        // ####################################################################
        // ##### Config Items Implicitly Set By the User
        // ####################################################################
        #region Config Items Implicitly Set By the User

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("971, 566")]
        public Size FormSize
        {
            get { return (Size)this["FormSize"]; }
            set { this["FormSize"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastCaptureDirectory
        {
            get { return (String)this["LastCaptureDirectory"]; }
            set { this["LastCaptureDirectory"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastCaptureFileName
        {
            get { return (String)this["LastCaptureFileName"]; }
            set { this["LastCaptureFileName"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LastRunName
        {
            get { return (String)this["LastRunName"]; }
            set { this["LastRunName"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LastRunNumber
        {
            get { return (int)this["LastRunNumber"]; }
            set { this["LastRunNumber"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int DrawGreenCircleRadius
        {
            get { return (int)this["DrawGreenCircleRadius"]; }
            set { this["DrawGreenCircleRadius"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int DrawGreenOutlineCircleLineWidth
        {
            get { return (int)this["DrawGreenOutlineCircleLineWidth"]; }
            set { this["DrawGreenOutlineCircleLineWidth"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int DrawGreenCircleDrawMouseClicks
        {
            get { return (int)this["DrawGreenCircleDrawMouseClicks"]; }
            set { this["DrawGreenCircleDrawMouseClicks"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String StepperControlNumSteps
        {
            get { return (String)this["StepperControlNumSteps"]; }
            set { this["StepperControlNumSteps"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String StepperControlStepsPerSecond
        {
            get { return (String)this["StepperControlStepsPerSecond"]; }
            set { this["StepperControlStepsPerSecond"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public bool StepperControlDirIsCW
        {
            get { return (bool)this["StepperControlDirIsCW"]; }
            set { this["StepperControlDirIsCW"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public double CalibratedPixelsPerMicron
        {
            get { return (double)this["CalibratedPixelsPerMicron"]; }
            set { this["CalibratedPixelsPerMicron"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorHorizTop
        {
            get { return (String)this["LineDetectColorHorizTop"]; }
            set { this["LineDetectColorHorizTop"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorHorizBot
        {
            get { return (String)this["LineDetectColorHorizBot"]; }
            set { this["LineDetectColorHorizBot"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorMinPixelsHoriz
        {
            get { return (String)this["LineDetectColorMinPixelsHoriz"]; }
            set { this["LineDetectColorMinPixelsHoriz"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorVertTop
        {
            get { return (String)this["LineDetectColorVertTop"]; }
            set { this["LineDetectColorVertTop"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorVertBot
        {
            get { return (String)this["LineDetectColorVertBot"]; }
            set { this["LineDetectColorVertBot"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public String LineDetectColorMinPixelsVert
        {
            get { return (String)this["LineDetectColorMinPixelsVert"]; }
            set { this["LineDetectColorMinPixelsVert"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int GridCountX
        {
            get { return (int)this["GridCountX"]; }
            set { this["GridCountX"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int GridCountY
        {
            get { return (int)this["GridCountY"]; }
            set { this["GridCountY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int GridBarSizeX
        {
            get { return (int)this["GridBarSizeX"]; }
            set { this["GridBarSizeX"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int GridBarSizeY
        {
            get { return (int)this["GridBarSizeY"]; }
            set { this["GridBarSizeY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int GridSpacingInMicrons
        {
            get { return (int)this["GridSpacingInMicrons"]; }
            set { this["GridSpacingInMicrons"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public Color? GridColor
        {
            get { return (Color?)this["GridColor"]; }
            set { this["GridColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public LineRecognitionModeEnum HorizLineRecognitionMode
        {
            get { return (LineRecognitionModeEnum)this["HorizLineRecognitionMode"]; }
            set { this["HorizLineRecognitionMode"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public LineRecognitionModeEnum VertLineRecognitionMode
        {
            get { return (LineRecognitionModeEnum)this["VertLineRecognitionMode"]; }
            set { this["VertLineRecognitionMode"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LineDetectHoriz_Floor
        {
            get { return (int)this["LineDetectHoriz_Floor"]; }
            set { this["LineDetectHoriz_Floor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LineDetectHoriz_Offset
        {
            get { return (int)this["LineDetectHoriz_Offset"]; }
            set { this["LineDetectHoriz_Offset"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LineDetectVert_Offset
        {
            get { return (int)this["LineDetectVert_Offset"]; }
            set { this["LineDetectVert_Offset"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LineDetectHoriz_PreDrop
        {
            get { return (int)this["LineDetectHoriz_PreDrop"]; }
            set { this["LineDetectHoriz_PreDrop"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int LineDetectHoriz_PostDrop
        {
            get { return (int)this["LineDetectHoriz_PostDrop"]; }
            set { this["LineDetectHoriz_PostDrop"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int MoveRedOntoTargetSpeedX
        {
            get { return (int)this["MoveRedOntoTargetSpeedX"]; }
            set { this["MoveRedOntoTargetSpeedX"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int MoveRedOntoTargetSpeedY
        {
            get { return (int)this["MoveRedOntoTargetSpeedY"]; }
            set { this["MoveRedOntoTargetSpeedY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int MoveRedOntoTargetClearanceRadius
        {
            get { return (int)this["MoveRedOntoTargetClearanceRadius"]; }
            set { this["MoveRedOntoTargetClearanceRadius"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public Color MoveRedToTargetColor
        {
            get { return (Color)this["MoveRedToTargetColor"]; }
            set { this["MoveRedToTargetColor"] = value; }
        }
        
        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public uint Motor0GlobalPositiveDir
        {
            get { return (uint)this["Motor0GlobalPositiveDir"]; }
            set { this["Motor0GlobalPositiveDir"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public uint Motor1GlobalPositiveDir
        {
            get { return (uint)this["Motor1GlobalPositiveDir"]; }
            set { this["Motor1GlobalPositiveDir"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public uint Motor2GlobalPositiveDir
        {
            get { return (uint)this["Motor2GlobalPositiveDir"]; }
            set { this["Motor2GlobalPositiveDir"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public uint Motor3GlobalPositiveDir
        {
            get { return (uint)this["Motor3GlobalPositiveDir"]; }
            set { this["Motor3GlobalPositiveDir"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int WASDSpeedX
        {
            get { return (int)this["WASDSpeedX"]; }
            set { this["WASDSpeedX"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int WASDSpeedY
        {
            get { return (int)this["WASDSpeedY"]; }
            set { this["WASDSpeedY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute(null)]
        public int WASDSpeedZ
        {
            get { return (int)this["WASDSpeedZ"]; }
            set { this["WASDSpeedZ"] = value; }
        }

        //// the most recently used file list
        //[UserScopedSettingAttribute()]
        //public List<string> MRUFileList
        //{
        //    get { return (List<string>)this["MRUFileList"]; }
        //    set { this["MRUFileList"] = value; }
        //}

        //[UserScopedSettingAttribute()]
        //[DefaultSettingValueAttribute(null)]
        //public String LastGCodeDirectory
        //{
        //    get { return (String)this["LastGCodeDirectory"]; }
        //    set { this["LastGCodeDirectory"] = value; }
        //}

        //[UserScopedSettingAttribute()]
        //[DefaultSettingValueAttribute(null)]
        //public String LastOpenFileName
        //{
        //    get { return (String)this["LastOpenFileName"]; }
        //    set { this["LastOpenFileName"] = value; }
        //}

        #endregion

    }
}

