//#define DEBUGGING
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace OSResWinForms
{
    /// <summary>
    /// Provides initialization methods for WinForms responsiveness.
    /// </summary>
    public static class RWF
    {
        /// <summary>
        /// Screen resolution at design time.
        /// </summary>
        public static Size DesignTimeResolution { get; private set; }
        /// <summary>
        /// Screen resolution of the user.
        /// </summary>
        public static Rectangle UserResolution { get; private set; }
        /// <summary>
        /// Multiplication factor for resizing controls.
        /// </summary>
        public static SizeF MulFac { get; private set; }
        /// <summary>
        /// Flags for additional fixes.
        /// </summary>
        public static ResFlags Fixes = ResFlags.None;
        internal static ResFlags[] Flags;
        /// <summary>
        /// Collection of responsive controls.
        /// </summary>
        public static Dictionary<Form, RForm> ResponsiveObjects = new Dictionary<Form, RForm>();
        /// <summary>
        /// Responsive WinForms flags for additional effects.
        /// </summary>
        [Flags]
        public enum ResFlags
        {
            /// <summary>
            /// No effect.
            /// </summary>
            None = 0,
            /// <summary>
            /// Automatically allow scroll bars for every control which requires it.
            /// </summary>
            AutoScroll = 1,
            /// <summary>
            /// Automatically allow ellipsis for longer texts with "..." instead of just cutting it.
            /// </summary>
            AutoEllipsis = 2,
            /// <summary>
            /// By default, controls are resized only when the user finished resizing the window.
            /// If AlwaysResize is flagged, the controls will resize on real-time.
            /// Note that this process can be slow sometimes, especially on forms containing a lot of controls.
            /// </summary>
            AlwaysResize = 4,
            /// <summary>
            /// Disables the entire window when resizing.
            /// </summary>
            DisableWhenResize = 8
        }
        /// <summary>
        /// Initialize OsRwf with screen resolution info.
        /// </summary>
        /// <param name="devRes">Resolution at design-time (developer's screen resolution).</param>
        /// <param name="cliRes">Current software user resolution.</param>
        public static void Initialize(Size devRes, Rectangle cliRes)
        {
            DesignTimeResolution = devRes;
            UserResolution = cliRes;
            InitializeStep2();
        }
        /// <summary>
        /// Initialize OsRwf with screen resolution info.
        /// User resolution will be checked automatically.
        /// </summary>
        /// <param name="devRes">Resolution at design-time (developer's screen resolution).</param>
        public static void Initialize(Size devRes)
        {
            DesignTimeResolution = devRes;
            UserResolution = Screen.PrimaryScreen.Bounds;
            InitializeStep2();
        }
        /// <summary>
        /// Initialize OsRwf with screen resolution info.
        /// Developer resolution will be checked from the app config file, from appSettings keys: DESIGN_TIME_WIDTH, DESIGN_TIME_HEIGHT.
        /// </summary>
        /// <param name="cliRes">Current software user resolution.</param>
        public static void Initialize(Rectangle cliRes)
        {
            DesignTimeResolution = new Size(Convert.ToInt32(ConfigurationManager.AppSettings["DESIGN_TIME_WIDTH"]), Convert.ToInt32(ConfigurationManager.AppSettings["DESIGN_TIME_HEIGHT"]));
            UserResolution = cliRes;
            InitializeStep2();
        }
        /// <summary>
        /// Initialize OsRwf with screen resolution info.
        /// Developer resolution will be checked from the app config file, from appSettings keys: DESIGN_TIME_WIDTH, DESIGN_TIME_HEIGHT.
        /// User resolution will be checked automatically.
        /// </summary>
        public static void Initialize()
        {
            DesignTimeResolution = new Size(Convert.ToInt32(ConfigurationManager.AppSettings["DESIGN_TIME_WIDTH"]), Convert.ToInt32(ConfigurationManager.AppSettings["DESIGN_TIME_HEIGHT"]));
            UserResolution = Screen.PrimaryScreen.Bounds;
            InitializeStep2();
        }
        /// <summary>
        /// Generates the multiplication factor and the flags array.
        /// </summary>
        private static void InitializeStep2()
        {
            MulFac = new SizeF(UserResolution.Width / (float)DesignTimeResolution.Width, UserResolution.Height / (float)DesignTimeResolution.Height);
            Flags = Enum.GetValues(typeof(ResFlags)).Cast<ResFlags>().ToArray();
        }
        private enum MetricsType
        {
            Width,
            Height
        }
        /// <summary>
        /// Get modified control size based on the multiplication factor.
        /// </summary>
        /// <param name="val">The original value.</param>
        /// <param name="type">Width / Height.</param>
        /// <returns>The modified value.</returns>
        private static int GetMetrics(int val, MetricsType type)
        {
            if (type == MetricsType.Width)
                return (int)Math.Floor(val * MulFac.Width);
            else if (type == MetricsType.Height)
                return (int)Math.Floor(val * MulFac.Height);
            return 0;
        }
        /// <summary>
        /// Flag additional fixes.
        /// </summary>
        /// <param name="flags">Fixes to use.</param>
        public static void UseResFixFlags(ResFlags flags) =>
            Fixes = flags;
        internal static bool HasProperty(Type type, string name) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => p.Name == name);
        /// <summary>
        /// Applies the responsive form effect.
        /// </summary>
        /// <param name="f">Form reference.</param>
        /// <returns>Responsive form object, bounded to the given form.</returns>
        public static RForm ApplyResponsiveness(Form f)
        {
            var r = new RForm(f);
            ResponsiveObjects.Add(f, r);
            return r;
        }
        /// <summary>
        /// Gets a responsive form object bounded to a form.
        /// </summary>
        /// <param name="f">Form reference.</param>
        /// <returns>Responsive form object.</returns>
        public static RForm GetReponsivenessObject(this Form f) =>
            ResponsiveObjects.ContainsKey(f) ? ResponsiveObjects[f] : null;
    }
    /// <summary>
    /// Object used to save responsive control original data.
    /// </summary>
    public struct ResponsiveControlData
    {
        /// <summary>
        /// The original control to base this object on.
        /// </summary>
        public Control Base;
        /// <summary>
        /// Original control size.
        /// </summary>
        public Size OriginalSize;
        /// <summary>
        /// Original control font data.
        /// </summary>
        public Font OriginalFont;
        /// <summary>
        /// Initializes a struct of responsive control data.
        /// </summary>
        /// <param name="b">The base control.</param>
        public ResponsiveControlData(Control b)
        {
            Base = b;
            OriginalSize = b.Size;
            OriginalFont = b.Font;
        }
        /// <summary>
        /// Constructs a responsive control data object based on a given control.
        /// </summary>
        /// <param name="c">The control.</param>
        public static implicit operator ResponsiveControlData(Control c) =>
            new ResponsiveControlData(c);
    }
    /// <summary>
    /// Responsive form data object.
    /// </summary>
    public class RForm
    {
        /// <summary>
        /// The base form.
        /// </summary>
        public Form Form;
        /// <summary>
        /// Responsive control data object for the form itself.
        /// </summary>
        public ResponsiveControlData ResponsiveFormData;
        /// <summary>
        /// Original form data.
        /// </summary>
        public FormWindowState OriginalWindowState = default(FormWindowState), CurrentWindowState = default(FormWindowState);
        /// <summary>
        /// Collection of responsive control data objects for any control within the form.
        /// </summary>
        public Dictionary<Control, ResponsiveControlData> ResponsiveControlsData;
        /// <summary>
        /// Constructs an responsive form object bounded to a form.
        /// </summary>
        /// <param name="form">The form to base the object on.</param>
        public RForm(Form form)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            Form = form;
            ResponsiveControlsData = new Dictionary<Control, ResponsiveControlData>();
            DbgMsg("Loading Responsive Form");
            ResponsiveFormData = new ResponsiveControlData(Form as Control);
            DbgMsg("Form size is " + ResponsiveFormData.OriginalSize);
            OriginalWindowState = CurrentWindowState = Form.WindowState;
            DbgMsg("Window state is " + Form.WindowState);
            ResponsiveFormData.Base.Scale(new SizeF(RWF.MulFac.Width, RWF.MulFac.Height));
            DbgMsg("Scaling with factors " + RWF.MulFac);
            SaveControls(Form.Controls);
            DbgMsg("Triggering events");
            Form.ControlAdded += RForm_ControlAdded;
            Form.ControlRemoved += RForm_ControlRemoved;
            if (RWF.Fixes.HasFlag(RWF.ResFlags.AlwaysResize))
                Form.Resize += RForm_Resize;
            Form.SizeChanged += Form_SizeChanged;
            Form.ResizeEnd += RForm_ResizeEnd;
            Form.ResizeBegin += RForm_ResizeBegin;
            //Form.Load += RForm_Load;
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (Form.WindowState != CurrentWindowState)
            {
                CurrentWindowState = Form.WindowState;
                ResizeControls(Form.Controls, new SizeF((Form.Size.Width / (float)ResponsiveFormData.OriginalSize.Width), (Form.Size.Height / (float)ResponsiveFormData.OriginalSize.Height)));
            }
        }
        private void DbgMsg(string msg)
        {
#if DEBUGGING
            Debug.WriteLine(msg);
            MessageBox.Show(msg);
#endif
        }
        private void RForm_ResizeBegin(object sender, EventArgs e)
        {
            if (RWF.Fixes.HasFlag(RWF.ResFlags.DisableWhenResize))
                Form.Enabled = false;
        }
        private void RForm_ResizeEnd(object sender, EventArgs e)
        {
            if (RWF.Fixes.HasFlag(RWF.ResFlags.DisableWhenResize))
                Form.Enabled = true;
            ResizeControls(Form.Controls, new SizeF((Form.Size.Width / (float)ResponsiveFormData.OriginalSize.Width), (Form.Size.Height / (float)ResponsiveFormData.OriginalSize.Height)));
        }
        private void RForm_ControlRemoved(object sender, ControlEventArgs e) =>
            ResponsiveControlsData.Remove(e.Control);
        private void RForm_ControlAdded(object sender, ControlEventArgs e) =>
            Add(e.Control);
        private void RForm_Resize(object sender, EventArgs e)
        {
            if (RWF.Fixes.HasFlag(RWF.ResFlags.AlwaysResize))
                ResizeControls(Form.Controls, new SizeF((Form.Size.Width / (float)ResponsiveFormData.OriginalSize.Width), (Form.Size.Height / (float)ResponsiveFormData.OriginalSize.Height)));
        }
        private void Add(Control c)
        {
            ResponsiveControlsData.Add(c, c);
            DbgMsg("Added " + c.GetHashCode() + "; " + c.Name + " - " + c.Size);
            c.ControlAdded += RForm_ControlAdded;
            c.ControlRemoved += RForm_ControlRemoved;
            if (c is ScrollableControl)
                (c as ScrollableControl).Scroll += RForm_Scroll;
            if (c.HasChildren && c.Controls.Count > 0)
            {
                DbgMsg("Getting into " + c.GetHashCode() + "; " + c.Name + ", with " + c.Controls.Count + " child controls");
                SaveControls(c.Controls);
                DbgMsg("Finished " + c.GetHashCode() + "; " + c.Name);
            }
            foreach (var i in RWF.Flags)
                if (i == RWF.ResFlags.AutoEllipsis || i == RWF.ResFlags.AutoScroll)
                {
                    try
                    {
                        if (RWF.Fixes.HasFlag(RWF.ResFlags.AutoEllipsis) ||
                        (RWF.Fixes.HasFlag(RWF.ResFlags.AutoScroll) && c is ScrollableControl))
                        {
                            var pr = c.GetType().GetProperty(i.ToString());
                            if (pr != null && pr.GetValue(c) is bool && !(bool)pr.GetValue(c) && !Attribute.IsDefined(pr, typeof(BrowsableAttribute)))
                                pr.SetValue(c, true);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                }
        }
        private void RForm_Scroll(object sender, ScrollEventArgs e) =>
            (sender as Control).Refresh();
        /// <summary>
        /// Saves the original data of the controls.
        /// </summary>
        /// <param name="cc">Control collection.</param>
        public void SaveControls(Control.ControlCollection cc)
        {
            DbgMsg("Saving " + cc.Count + " controls");
            foreach (Control c in cc)
                Add(c);
        }
        /// <summary>
        /// Resizes any control within the collection based on a given ratio.
        /// </summary>
        /// <param name="cc">Control collection.</param>
        /// <param name="ratio">The ratio to base the new size on.</param>
        public void ResizeControls(Control.ControlCollection cc, SizeF ratio)
        {
            foreach (Control c in cc)
                if (!IsAutoSize(c))
                {
                    c.Size = new Size((int)(Math.Floor(ResponsiveControlsData[c].OriginalSize.Width * ratio.Width)), (int)(Math.Floor(ResponsiveControlsData[c].OriginalSize.Height * ratio.Height)));
                    c.Font = new Font(ResponsiveControlsData[c].OriginalFont.Name, ResponsiveControlsData[c].OriginalFont.SizeInPoints * ratio.Height, ResponsiveControlsData[c].OriginalFont.Style, ResponsiveControlsData[c].OriginalFont.Unit);
                    if (c.Controls.Count > 0)
                        ResizeControls(c.Controls, ratio);
                }
        }
        /// <summary>
        /// Resets control sizes.
        /// </summary>
        /// <param name="cc">Control collection.</param>
        public void ResetControls(Control.ControlCollection cc)
        {
            Form.Size = ResponsiveFormData.OriginalSize;
            Form.WindowState = OriginalWindowState;
            foreach (Control c in cc)
                if (!IsAutoSize(c))
                {
                    c.Size = ResponsiveControlsData[c].OriginalSize;
                    c.Font = ResponsiveControlsData[c].OriginalFont;
                    if (c.Controls.Count > 0)
                        ResetControls(c.Controls);
                }
        }
        private bool IsAutoSize(Control c)
        {
            var @as = c.GetType().GetProperty("AutoSize");
            return @as != null && @as.GetValue(c) is bool && (bool)@as.GetValue(c);
        }
        /*protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xa4)
            {
                ResetControls(Form.Controls);
                return;
            }
            base.WndProc(ref m);
        }*/
    }
}