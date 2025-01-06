using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE) MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions)                                                                   ¦
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
    /// A control to place a transparent area on the screen. Useful for mouse 
    /// hit testing etc
    /// 
    /// Credit: https://www.codeguru.com/dotnet/creating-a-net-transparent-panel/ 
    /// except I built it as a control not as a component inherited from another 
    /// control
    /// 
    /// USAGE: In form designer
    ///             this.ctlTransparentControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ctlTransparentControl1_MouseClick);
    ///
    ///        In form code
    ///              private void ctlTransparentControl1_MouseClick(object sender, MouseEventArgs e) {}
    /// 
    /// </summary>
    public partial class ctlTransparentControl : UserControl
    {
        private const int DEFAULT_OPACITY = 50;
        private int opacity = DEFAULT_OPACITY;
        private const int WS_EX_TRANSPARENT = 0x20;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        public ctlTransparentControl()
        {
            InitializeComponent();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Scales a point on the surface to an image size. This control is used to 
        /// intercept mouse clicks on controls that cannot do this. Say the control
        /// is overlaying an image which is supposed to be 640x480 but the user
        /// has stretched it. If the user clicks on the control we need to convert 
        /// the actual click point to a coordinate in the 640x480 space.
        /// 
        /// </summary>
        /// <param name="ptIn">the point to convert</param>
        /// <param name="imageSize">the image size </param>
        /// <param name="wantYInversion">if true we invert the Y coord so 0 is at bottom</param>
        public Point ConvertPoint(Point ptIn, Size imageSize, bool wantYInversion)
        {
            if (wantYInversion == true)
            {
                return new Point(((ptIn.X * imageSize.Width) / this.Width), (imageSize.Height-((ptIn.Y * imageSize.Height) / this.Height)));
            }
            else
            {
                return new Point(((ptIn.X * imageSize.Width) / this.Width), ((ptIn.Y * imageSize.Height) / this.Height));
            }
        }

        [DefaultValue(50)]
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the opacity of the control
        /// 
        /// </summary>
        public int Opacity
        {
            get
            {
                return this.opacity;
            }

            set
            {

                if (value < 0 || value > 100)
                    throw new ArgumentException("value must be between 0 and 100");
                    this.opacity = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The CreateParams property gets all the required creation parameters 
        /// when a control handle is created
        /// 
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cpar = base.CreateParams;
                cpar.ExStyle = cpar.ExStyle | WS_EX_TRANSPARENT;
                return cpar;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Override onpaint so we can draw the control transparent
        /// 
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb (this.opacity * 255 / 100, this.BackColor)))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}
