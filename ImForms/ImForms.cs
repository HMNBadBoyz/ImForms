using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Interop = System.Runtime.InteropServices;
using CmplTime = System.Runtime.CompilerServices;
using WForms = System.Windows.Forms;
using WFControlList = System.Windows.Forms.Control.ControlCollection;
using System.Diagnostics;

namespace ImForms
{
    public  class GenIDAttribute : Attribute
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
        public ulong? ID { get; set; }

        public ImControl(WForms.Control control) { WfControl = control; }
    }

    public partial class ImFormsMgr
    {
        [Interop.DllImport("user32.dll", CharSet = Interop.CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(Interop.HandleRef hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        public string GetOwnFunctionName([CmplTime.CallerMemberName]string s="") => s;

        public ulong? Hash(string a ,int b, string c)
        {
            unchecked
            {
                ulong hash0 = (ulong)(a, b, c).GetHashCode();
                ulong hash1 = (ulong)(a, b, c).GetHashCode();
                return hash0 | (hash1 << 32);
            }
        }

        private static void EnableRepaint(Interop.HandleRef handle, bool enable)
        {
            const int WM_SETREDRAW = 0x000B;
            SendMessage(handle, WM_SETREDRAW, new IntPtr(enable ? 1 : 0), IntPtr.Zero);
        }
        
        private int RemainingRedraws = 0;
        private TaskCompletionSource<bool> TCS;
        private Dictionary<ulong?, ImControl> ImControls;
        public WFControlList DisplayedControls;
        private int CurrentSortKey;
        private ulong? InteractedElementId;
        private ImControl LastCalledControl;

        // OH NOTE This could be configurable by the user in the _distant_ future
        private  int RedrawsPerInteraction = 1;

        private ulong? PrevInteractedElementId;

        public ImFormsMgr(WForms.Panel panel)
        {
            InteractedElementId = null;
            ImControls = new Dictionary<ulong?, ImControl>();
            TCS = new TaskCompletionSource<bool>();
            CurrentSortKey = 0;
            DisplayedControls = panel.Controls;
        }

        public void QueueRedraws(int numRedraws) { RemainingRedraws += numRedraws; }

        private void LetImGuiHandleIt(object sender, EventArgs args)
        {
            if (sender is WForms.TreeNode)
            {
                InteractedElementId = (ulong)(((WForms.TreeNode)sender).Tag);
            }
            else
            {
                InteractedElementId = (ulong)(((WForms.Control)sender).Tag);
            }
            QueueRedraws(RedrawsPerInteraction);
            Refresh();
        }

        public bool ControlExists(ulong? id)
        {
            return ImControls.ContainsKey(id);
        }

        public ImControl ProcureControl(ulong? id, ImFormsCtrlMaker maker )
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

        public delegate WForms.Control ImFormsCtrlMaker(ulong? id);

        public WForms.Control InitControlForClicking(WForms.Control wfCtrl, ulong? id)
        {
            wfCtrl.Name = id?.ToString();
            wfCtrl.Tag = id;
            wfCtrl.Click += LetImGuiHandleIt;
            wfCtrl.TabStopChanged += LetImGuiHandleIt;
            wfCtrl.AutoSize = true;
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
                foreach (var ctrl in undrawnControls.Take(ctrlsToRemoveForCleanup))
                {
                    ImControls.Remove(ctrl.ID);
                }
            }

            InteractedElementId = null;
            WForms.Control[] sortedControls = ImControls.Values.Where(x => x.State == ImDraw.Drawn)
                .OrderBy(c => c.SortKey).Select(c => c.WfControl).ToArray();
            var controlsChanged = DisplayedControls.Count != sortedControls.Length
                || !Enumerable.Zip(
                        DisplayedControls.OfType<WForms.Control>(),
                        sortedControls,
                        (c1, c2) => c1 == c2).All(b => b);
            
            if (controlsChanged)
            {
                var handle = new Interop.HandleRef(DisplayedControls.Owner, DisplayedControls.Owner.Handle);
                EnableRepaint(handle, false);
                DisplayedControls.Clear();
                DisplayedControls.AddRange(sortedControls);
                EnableRepaint(handle, true);
                DisplayedControls.Owner.Refresh();
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