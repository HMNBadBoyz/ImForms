using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;
using Interop = System.Runtime.InteropServices;
using CmplTime = System.Runtime.CompilerServices;
using WForms = System.Windows.Forms;
using WFControlList = System.Windows.Forms.Control.ControlCollection;
using IDType = System.ValueTuple<string,int,string>;

namespace ImForms
{
    public struct IDType :IEquatable<IDType>
    {
        public string CallerFilePath;
        public int CallerLineNumber;
        public string CallerMemberName;

        public IDType(string callerfilepath, int callerlinenumber, string callermembername)
        {
            CallerFilePath = callerfilepath;
            CallerLineNumber = callerlinenumber;
            CallerMemberName = callermembername;
        }

        public bool Equals(IDType other)
        {
            return ((CallerFilePath == other.CallerFilePath) && 
                   (CallerLineNumber == other.CallerLineNumber) &&
                (CallerMemberName == other.CallerMemberName));
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(CallerFilePath, new CustomStringComparer());
            hash.Add(CallerLineNumber);
            hash.Add(CallerMemberName, new CustomStringComparer());
            return hash.ToHashCode();
        }

        public override bool Equals(object obj)
         {
            return base.Equals(obj);
         }

        public static bool operator ==(IDType a,IDType b)
        {
            return ((a.CallerFilePath == b.CallerFilePath) &&
                    (a.CallerLineNumber == b.CallerLineNumber) &&
                    (a.CallerMemberName == b.CallerMemberName));
        }

        public static bool operator !=(IDType a, IDType b)
        {
            return !(a == b);
        }

    }
    public class CustomStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetDeterministicHashCode();
        }
    }


    public class IdTypeComparer : IEqualityComparer<IDType>
    {
        private IEqualityComparer<string> Cache;

        public IdTypeComparer()
        {
            Cache = new CustomStringComparer();
        }
        public bool Equals(IDType x, IDType y)
        {
            return x.CallerFilePath == y.CallerFilePath &&
                   x.CallerMemberName == y.CallerMemberName &&
                   x.CallerLineNumber == y.CallerLineNumber;
        }

        public int GetHashCode(IDType id)
        {
            var hash = new HashCode();
            hash.Add(id.CallerFilePath,Cache);
            hash.Add(id.CallerLineNumber);
            hash.Add(id.CallerMemberName,Cache);
            return hash.ToHashCode();
        }
    }

    class BufferedTreeView : WForms.TreeView
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        
        [Interop.DllImport("user32.dll", CharSet = Interop.CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }


    class BufferedTrackBar : WForms.TrackBar
    {
        private const int WM_ERASEBKGND = 20;

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Unrestricted = true)]
        protected override void WndProc(ref WForms.Message m)
        {
            //NOTE(shazan) : Only do this for System Drawn (common controls) do not do this to
            //               user drawn controls or it might cause artifacts
            if (m.Msg == WM_ERASEBKGND)
            {
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }


    public static class HashingExtensions
    {
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }


    public class GenIDAttribute : Attribute
    {

    }

    public class CheckIDAttribute : Attribute
    {

    }

    public class ImFormsIDException : Exception
    {
        public ImFormsIDException(string message) : base(message)
        {

        }
    }

    /// <summary>
    /// All control types in ImForms
    /// Todo(shazan) : Look into adding custom controls and fill every default control in this
    /// </summary>
    public enum ImControlType
    {
        Textbox,
        Label,
        Spinner,
        TrackBar,
        MultilineTextbox,
        Treeview,

    }

    public enum ImDraw
    {
        NotDrawn,
        Drawn
    }

    public class ImControl
    {
        public readonly WForms.Control WfControl;
        public ImDraw State { get; set; }
        public int SortKey { get; set; }
        public IDType ID { get; set; }
        public object TempData { get; set; }
        public ImControl(WForms.Control control) { WfControl = control; }
    }


    public partial class ImFormsMgr
    {
        [Interop.DllImport("user32.dll", CharSet = Interop.CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(Interop.HandleRef hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        
        [Interop.DllImport("user32.dll", CharSet = Interop.CharSet.Auto, SetLastError = false)]
        public static extern bool RedrawWindow(Interop.HandleRef hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);
        public string GetOwnFunctionName([CmplTime.CallerMemberName]string s="") => s;

        public enum RedrawWindowFlags : uint
        {
            ///<summary>Invalidates lprcUpdate or hrgnUpdate (only one may be non-NULL). 
            ///If both are NULL, the entire window is invalidated.</summary>
            Invalidate = 0x1,

            ///<summary>Causes a WM_PAINT message to be posted to the window regardless of 
            ///whether any portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            ///<summary>Causes the window to receive a WM_ERASEBKGND message when the window 
            ///is repainted. The <b>Invalidate</b> flag must also be specified; otherwise, 
            ///<b>Erase</b> has no effect.</summary>
            Erase = 0x4,

            ///<summary>Validates lprcUpdate or hrgnUpdate (only one may be non-NULL). If both 
            ///are NULL, the entire window is validated. This flag does not affect internal 
            ///WM_PAINT messages.</summary>
            Validate = 0x8,

            ///<summary>Suppresses any pending internal WM_PAINT messages. This flag does not 
            ///affect WM_PAINT messages resulting from a non-NULL update area.</summary>
            NoInternalPaint = 0x10,

            ///<summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            ///<summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            ///<summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            ///<summary>Causes the affected windows (as specified by the <b>AllChildren</b> and <b>NoChildren</b> flags) to 
            ///receive WM_NCPAINT, WM_ERASEBKGND, and WM_PAINT messages, if necessary, before the function returns.</summary>
            UpdateNow = 0x100,

            ///<summary>Causes the affected windows (as specified by the <b>AllChildren</b> and <b>NoChildren</b> flags) 
            ///to receive WM_NCPAINT and WM_ERASEBKGND messages, if necessary, before the function returns. 
            ///WM_PAINT messages are received at the ordinary time.</summary>
            EraseNow = 0x200,

            ///<summary>Causes any part of the nonclient area of the window that intersects the update region 
            ///to receive a WM_NCPAINT message. The <b>Invalidate</b> flag must also be specified; otherwise, 
            ///<b>Frame</b> has no effect. The WM_NCPAINT message is typically not sent during the execution of 
            ///RedrawWindow unless either <b>UpdateNow</b> or <b>EraseNow</b> is specified.</summary>
            Frame = 0x400,

            ///<summary>Suppresses any pending WM_NCPAINT messages. This flag must be used with <b>Validate</b> and 
            ///is typically used with <b>NoChildren</b>. <b>NoFrame</b> should be used with care, as it could cause parts 
            ///of a window to be painted improperly.</summary>
            NoFrame = 0x800
        }
        private static void EnableRepaint(Interop.HandleRef handle, bool enable)
        {
            const int WM_SETREDRAW = 0x000B;
            SendMessage(handle, WM_SETREDRAW, new IntPtr(enable ? 1 : 0), IntPtr.Zero);
        }

        private int RemainingRedraws = 0;
        private TaskCompletionSource<bool> TCS;
        private readonly Dictionary<IDType, ImControl> ImControls;
        public WFControlList DisplayedControls;
        private int CurrentSortKey;
        private IDType InteractedElementId;
        private ImControl LastCalledControl;

        // OH NOTE This could be configurable by the user in the _distant_ future
        private  int RedrawsPerInteraction = 1;

        private IDType PrevInteractedElementId;

        public ImFormsMgr(WForms.Panel panel)
        {
            InteractedElementId = default(IDType);
            ImControls = new Dictionary<IDType, ImControl>(new IdTypeComparer());
            TCS = new TaskCompletionSource<bool>();
            CurrentSortKey = 0;
            DisplayedControls = panel.Controls;
        }

        public void QueueRedraws(int numRedraws) { RemainingRedraws += numRedraws; }

        private void LetImGuiHandleIt(object sender, EventArgs args)
        {
            if (sender is WForms.TreeNode)
            {
                InteractedElementId = (IDType)(((WForms.TreeNode)sender).Tag);
            }
            else
            {
                InteractedElementId = (IDType)(((WForms.Control)sender).Tag);
            }
            QueueRedraws(RedrawsPerInteraction);
            Refresh();
        }

        public bool ControlExists(IDType id)
        {
            return ImControls.ContainsKey(id);
        }

        private string GenerateNameFromID(IDType id)
        {
            return $"{id.CallerFilePath}#{id.CallerLineNumber}@{id.CallerMemberName}";
        }

        public ImControl ProcureControl(IDType id, ImFormsCtrlMaker maker )
        {
            ImControl ctrl;
            if (!ImControls.TryGetValue(id, out ctrl))
            {
                ctrl = new ImControl(maker(id));
                ctrl.ID = id;
                ImControls.Add(id, ctrl);
            }
            LastCalledControl = ctrl;
            ctrl.State = ImDraw.Drawn;
            ctrl.SortKey = CurrentSortKey;
            ctrl.WfControl.Visible = true;
            CurrentSortKey++;
            return ctrl;
        }

        public delegate WForms.Control ImFormsCtrlMaker(IDType id);

        public WForms.Control InitControlForClicking(WForms.Control wfCtrl, IDType id)
        {
            wfCtrl.Name = GenerateNameFromID(id);
            wfCtrl.Tag = id;
            wfCtrl.Click += LetImGuiHandleIt;
            wfCtrl.TabStopChanged += LetImGuiHandleIt;
            wfCtrl.AutoSize = true;
            return wfCtrl;
        }

        public WForms.Control InitControlForClickingAndTyping(WForms.Control wfCtrl, IDType id)
        {
            wfCtrl.Name = GenerateNameFromID(id);
            wfCtrl.Tag = id;
            wfCtrl.Click += LetImGuiHandleIt;
            wfCtrl.TabStopChanged += LetImGuiHandleIt;
            wfCtrl.AutoSize = true;
            wfCtrl.TextChanged += LetImGuiHandleIt;
            return wfCtrl;
        }

        public ImControl GetLastCalledControl() => LastCalledControl;

        public void Refresh()
        {
            if (!TCS.Task.IsCompleted)
            {
                TCS.SetResult(true);
            }
        }

       
        public async Task NextFrame()
        {
            PrevInteractedElementId = InteractedElementId;
            const int ctrlsToTriggerCleanup = 100;
            const int ctrlsToRemoveForCleanup = 50;

            var undrawnControls = ImControls.Values.Where(ctrl => ctrl.State == ImDraw.NotDrawn)
                .Take(ctrlsToTriggerCleanup).ToList();

            if (undrawnControls.Count == ctrlsToTriggerCleanup)
            {
                var controlstaken = undrawnControls.Take(ctrlsToRemoveForCleanup);
                foreach (var ctrl in controlstaken)
                {
                    if(!ctrl.WfControl.IsDisposed) ctrl.WfControl.Dispose();
                    ImControls.Remove(ctrl.ID);
                }
            }

            InteractedElementId = default(IDType);
            WForms.Control[] sortedControls = ImControls.Values.Where(x => x.State == ImDraw.Drawn)
                .OrderBy(c => c.SortKey).Select(c => c.WfControl).ToArray();
            var controlsChanged = DisplayedControls.Count != sortedControls.Length
                || !Enumerable.Zip(
                        DisplayedControls.OfType<WForms.Control>(),
                        sortedControls,
                        (c1, c2) => c1 == c2).All(b => b);
            
            if (controlsChanged)
            {
                DisplayedControls.Owner.SuspendLayout();
                var handle = new Interop.HandleRef(DisplayedControls.Owner, DisplayedControls.Owner.Handle);
                EnableRepaint(handle, false);
                
                DisplayedControls.Clear();
                DisplayedControls.AddRange(sortedControls); 
                
                EnableRepaint(handle, true);
                var isContainer = true;
                RedrawWindow(handle, IntPtr.Zero, IntPtr.Zero, isContainer ? RedrawWindowFlags.Erase | RedrawWindowFlags.Frame | RedrawWindowFlags.Invalidate | RedrawWindowFlags.AllChildren :
                    RedrawWindowFlags.NoErase | RedrawWindowFlags.Invalidate | RedrawWindowFlags.InternalPaint);
                DisplayedControls.Owner.ResumeLayout();
            }

            // Automatically go to next frame for each requested redraw
            if (RemainingRedraws <= 0)
            {
                RemainingRedraws = 0;
                await TCS.Task;
                TCS = new TaskCompletionSource<bool>();
            }
            else
            {
                RemainingRedraws--;
            }

            foreach (var ctrl in ImControls.Values)
            {
                ctrl.State = ImDraw.NotDrawn;
                ctrl.SortKey = 999999;
            }
        }
    }
}