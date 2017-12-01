using System.Windows.Forms;
using ImForms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ImFormsUser
{
    public partial class ImFormsUser : Form
    {
        public int y = 0;
        public int t = 0;
        ImFormsMgr leftPanelMgr;

        public ImFormsUser()
        {
            InitializeComponent();
            Load += async (o, e) => await Main(Panel1);
            Load += async (o, e) => await Main2(Panel2);

            Task.Run(async () =>
            {
                while (true)
                {
                    t++;
                    await Task.Delay(1);
                }
            });
        }

        public async Task Main(Panel panel)
        {
            ImFormsMgr mgr = new ImFormsMgr(panel);

            IList<int> list = new List<int>();

            int x = 0;
            bool displayList = false;
            bool reverseList = false;

            while (true)
            {
                mgr.Label("PANEL A");
                mgr.Space(CompileTime.ID());
                mgr.Label("This ImForms panel refreshes only when there is user interaction");
                mgr.Space(CompileTime.ID());
                mgr.Label("ImForms makes it easy to display and modify one value with multiple controls");
                mgr.Label("x =");
                mgr.RadioButton("0", ref x, 0);
                mgr.RadioButton("1", ref x, 1);

                int valueToAssignX = (x == 1) ? 0 : 1;
                if (mgr.Button("x <- " + valueToAssignX, CompileTime.ID()))
                {
                    x = valueToAssignX;
                }

                bool xIs1 = (x == 1);
                mgr.Checkbox("X == 1", ref xIs1);
                x = xIs1 ? 1 : 0;

                mgr.Space(CompileTime.ID());
                mgr.Label("Just like with other ImGui implementations, if a function isn't called for it," +
                    " a control isn't displayed.");
                mgr.Checkbox("Show list", ref displayList);

                if (displayList)
                {
                    var seq = reverseList ? list.Reverse() : list;

                    foreach (var n in seq) { mgr.Label("[" + n + "]"); }

                    if (mgr.Button("Add to end")) { list.Add(list.LastOrDefault() + 1); }

                    if (mgr.Button("Remove from front")) { list.RemoveAt(0); }

                    mgr.Checkbox("Display reversed", ref reverseList);
                }

                mgr.Space(CompileTime.ID());
                mgr.Label("Values from other threads can be displayed when a panel refreshes.");
                mgr.LinkLabel("Try it!");
                mgr.Label("y = " + y, CompileTime.ID());

                await mgr.NextFrame();
            }
        }

        public async Task Main2(Panel panel)
        {
            leftPanelMgr = new ImFormsMgr(panel);

            var timer = new Timer();
            timer.Tick += (o, e) => leftPanelMgr.Refresh();
            int updateRate = 1000;

            while (true)
            {
                leftPanelMgr.Label("PANEL B");
                leftPanelMgr.Space(CompileTime.ID());
                leftPanelMgr.Label("This ImForms panel auto-updates once every:");
                leftPanelMgr.RadioButton("Second", ref updateRate, 1000);
                leftPanelMgr.RadioButton("100ms", ref updateRate, 100);
                leftPanelMgr.RadioButton("10ms", ref updateRate, 10);
                leftPanelMgr.RadioButton("Never", ref updateRate, -1);
                timer.Interval = (updateRate > 0) ? updateRate : int.MaxValue;
                timer.Enabled = (updateRate > 0);
                leftPanelMgr.Space(CompileTime.ID());
                leftPanelMgr.Label("Auto-updating is an easy way to display values from other threads");
                leftPanelMgr.Label("y = " + y, CompileTime.ID());
                leftPanelMgr.Label("t = " + t, CompileTime.ID());
                await leftPanelMgr.NextFrame();
            }
        }

        private void yIncrButton_Click(object sender, System.EventArgs e)
        {
            y++;
        }

        private void refreshBtn_Click(object sender, System.EventArgs e)
        {
            leftPanelMgr.Refresh();
        }
    }
}
