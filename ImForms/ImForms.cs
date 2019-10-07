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

    public class ImFormsMgr
    {
        [Interop.DllImport("user32.dll", CharSet = Interop.CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(Interop.HandleRef hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        public string GetOwnFunctionName([CmplTime.CallerMemberName]string s) => s;
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
                InteractedElementId = ulong.Parse(((WForms.TreeNode)sender).ImageKey);
            }
            else
            {
                InteractedElementId = ulong.Parse(((WForms.Control)sender).Name);
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
            wfCtrl.Click += LetImGuiHandleIt;
            wfCtrl.TabStopChanged += LetImGuiHandleIt;
            wfCtrl.AutoSize = true;
            return wfCtrl;
        }

        public ImControl GetLastCalledControl() => LastCalledControl;

        [CheckID]
        public void Space([GenID] ulong? id = null)
        {
            Label("",id);
        }
        [CheckID]
        public void Label(string text, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id , id1 => new WForms.Label { Name = id1?.ToString(), AutoSize = true });
            ctrl.WfControl.Text = text;
        }

        [CheckID]
        public bool Button(string text, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id,x => InitControlForClicking(new WForms.Button(),x));
            ctrl.WfControl.Text = text;
            return InteractedElementId == ctrl.ID;
        }

        [CheckID]
        public bool LinkLabel(string text, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.LinkLabel(), x));
            ctrl.WfControl.Text = text;
            return InteractedElementId == ctrl.ID;
        }


        [CheckID]
        public bool Checkbox(string text, ref bool checkBoxChecked, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.CheckBox(), x));
            var checkBox = ctrl.WfControl as WForms.CheckBox;
            checkBox.Text = text;
            checkBox.AutoCheck = false;
            var wasInteracted = InteractedElementId == ctrl.ID;

            if (wasInteracted) { checkBox.Checked = ! checkBox.Checked; checkBoxChecked = checkBox.Checked;  }
            else { checkBox.Checked = checkBoxChecked; }

            return wasInteracted;
        }


        [CheckID]
        public bool RadioButton(string text, ref int value, int checkAgainst, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.RadioButton(), x));
            var radioButton = ctrl.WfControl as WForms.RadioButton;
            radioButton.Text = text;
            radioButton.AutoCheck = false;
            var wasInteracted = InteractedElementId == ctrl.ID ;

            if (wasInteracted) { value = checkAgainst; }
            else { radioButton.Checked = (value == checkAgainst); }

            return wasInteracted ;
        }


        [CheckID]
        public bool SliderInt(string text,ref int value ,int minval = 0, int maxval = 1, [GenID] ulong? id = null)
        {
            var FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.TrackBar(), x));
            var trackbar = ctrl.WfControl as WForms.TrackBar;
            trackbar.Text = text;
            trackbar.Minimum = minval;
            trackbar.Maximum = maxval;
            if(FirstPass) trackbar.ValueChanged += LetImGuiHandleIt;
            var wasInteracted = InteractedElementId == ctrl.ID;
            if (wasInteracted) { value = trackbar.Value; }
            else { trackbar.Value = value; }
            return wasInteracted;
        }


        [CheckID]
        public bool SliderFloat(string text, ref float value, float minval = 0.0f, float maxval = 1.0f, [GenID] ulong? id = null)
        {
            var FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id , x => InitControlForClicking(new WForms.TrackBar(), x));
            var trackbar = ctrl.WfControl as WForms.TrackBar;
            trackbar.Text = text;
            var unitscale = (maxval - minval)*100; 
            trackbar.Minimum = (int)(minval*unitscale);
            trackbar.Maximum = (int)(maxval*unitscale);
            trackbar.Orientation = WForms.Orientation.Vertical;
            if (FirstPass) trackbar.ValueChanged += LetImGuiHandleIt;
            var wasInteracted = InteractedElementId == ctrl.ID;
            if (wasInteracted) { value = trackbar.Value/100.0f; }
            else { trackbar.Value = (int)(value*unitscale); }
            return wasInteracted;
        }


        [CheckID]
        public bool ProgressInt(string text, ref int value, int minval = 0, int maxval = 1, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id , x => InitControlForClicking(new WForms.ProgressBar(), x));
            var trackbar = ctrl.WfControl as WForms.ProgressBar;
            trackbar.Text = text;
            trackbar.Minimum = minval;
            trackbar.Maximum = maxval;
            trackbar.Value = value;
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            return wasInteracted;
        }


        [CheckID]
        public bool ProgressFloat(string text, ref float value, float minval = 0.0f, float maxval = 1.0f, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.ProgressBar(), x));
            var trackbar = ctrl.WfControl as WForms.ProgressBar;
            trackbar.Text = text;
            var unitscale = (maxval - minval) * 100;
            trackbar.Minimum = (int)(minval * unitscale);
            trackbar.Maximum = (int)(maxval * unitscale);
            trackbar.Value = (int)(value * unitscale);
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            return wasInteracted;
        }


        [CheckID]
        public bool InputText(string text,ref string output, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.TextBox(), x));
            var textbox = ctrl.WfControl as WForms.TextBox;
            textbox.Text = text;
            textbox.Multiline = false;
            output = textbox.Text;
            var wasInteracted = InteractedElementId == ctrl.ID;
            return wasInteracted;
        }


        [CheckID]
        public bool InputMultilineText(string text, ref string output, [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.TextBox(), x));
            var multilinetextbox = ctrl.WfControl as WForms.TextBox;
            if (FirstPass)
            {
                multilinetextbox.Text = text;
                multilinetextbox.WordWrap = false;
                multilinetextbox.Multiline = true;
                multilinetextbox.ScrollBars = WForms.ScrollBars.Both;
                multilinetextbox.Size = new System.Drawing.Size(multilinetextbox.Size.Width, multilinetextbox.Size.Height * 3);
                multilinetextbox.TextChanged += (o,e) => {
                    multilinetextbox.SelectionStart = multilinetextbox.Text.Length;
                    multilinetextbox.ScrollToCaret();
                };
            }
            output = multilinetextbox.Text;
            var wasInteracted = InteractedElementId == ctrl.ID;
            return wasInteracted;
        }


        [CheckID]
        public bool TreeView(IList<string> texts, ref int selectedIndex, [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.TreeView(), x));
            var treeview = ctrl.WfControl as WForms.TreeView;
            treeview.Text = texts[0];
            if (FirstPass)
            {
                foreach (var text in texts)
                {
                    treeview.Nodes.Add(id.ToString(), text);
                }
            }
            var wasInteracted = InteractedElementId == ctrl.ID;
            if (wasInteracted)
            {
                selectedIndex = treeview.Nodes.IndexOf(treeview.SelectedNode);
            }
            return wasInteracted;
        }

        [CheckID]
        public bool TreeView(IList<string> texts,  [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.TreeView(), x));
            var treeview = ctrl.WfControl as WForms.TreeView;
            treeview.Text = texts[0];
            if (FirstPass)
            {
                foreach (var text in texts)
                {
                    treeview.Nodes.Add(id.ToString(), text);
                }
            }
            var wasInteracted = InteractedElementId == ctrl.ID;
            return wasInteracted;
        }



        [CheckID]
        public bool ComboBox(string text,ref string selecteditem ,string[] items , [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.ComboBox(), x));
            var combobox = ctrl.WfControl as WForms.ComboBox;
            combobox.Text = text;
            if (FirstPass) {
                combobox.SelectedIndexChanged += LetImGuiHandleIt;
                combobox.Click -= LetImGuiHandleIt;
                combobox.Items.AddRange(items);
                combobox.DropDownStyle = WForms.ComboBoxStyle.DropDownList;
            }
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            selecteditem = combobox.SelectedItem as string;
            selecteditem = selecteditem == null ? "" : selecteditem;
            return wasInteracted;
        }



        [CheckID]
        public bool ListBox(string text, ref string selecteditem,string[] items, [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.ListBox(), x));
            var listbox = ctrl.WfControl as WForms.ListBox;
            listbox.Text = text;
            if (FirstPass)
            {
                listbox.Click -= LetImGuiHandleIt;
                listbox.SelectedValueChanged += LetImGuiHandleIt;
                listbox.Items.AddRange(items);
            }
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            selecteditem = listbox.SelectedItem as string;
            selecteditem = selecteditem == null ? "" : selecteditem;
            return wasInteracted;
        }


        [CheckID]
        public bool CheckedListBox(string text, ref string selecteditem, string[] items, [GenID] ulong? id = null)
        {
            bool FirstPass = !ControlExists(id);
            var ctrl = ProcureControl(id, x => InitControlForClicking(new WForms.CheckedListBox(), x));
            var checkedlistbox = ctrl.WfControl as WForms.CheckedListBox;
            checkedlistbox.Text = text;
            if (FirstPass)
            {
                checkedlistbox.Click -= LetImGuiHandleIt;
                checkedlistbox.SelectedValueChanged += LetImGuiHandleIt;
                checkedlistbox.Items.AddRange(items);
            }
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            selecteditem = checkedlistbox.SelectedItem as string;
            selecteditem = selecteditem == null ? "" : selecteditem;
            return wasInteracted;
        }

        [CheckID]
        public bool Spinner(string text, ref int value, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, id1 => new WForms.NumericUpDown { Name = id1?.ToString(), AutoSize = true });
            var spinner = ctrl.WfControl as WForms.NumericUpDown;
            spinner.Text = text;
            spinner.Value = value;
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            return wasInteracted;
        }

        [CheckID]
        public bool Image(string text,System.Drawing.Image image , [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, id1 => new WForms.PictureBox { Name = id1?.ToString(), AutoSize = true });
            var picturebox = ctrl.WfControl as WForms.PictureBox;
            picturebox.Text = text;
            picturebox.Image = image;
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            return wasInteracted;
        }

        [CheckID]
        public bool DateTime(string text,  ref DateTime value, [GenID] ulong? id = null)
        {
            var ctrl = ProcureControl(id, id1 => new WForms.DateTimePicker { Name = id1?.ToString(), AutoSize = true });
            var spinner = ctrl.WfControl as WForms.DateTimePicker;
            spinner.Text = text;
            spinner.Value = value;
            var wasInteracted = InteractedElementId == ctrl.ID || PrevInteractedElementId == ctrl.ID;
            return wasInteracted;
        }

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