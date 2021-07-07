using System;
using ImForms;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImForms;
using System.Windows.Forms;

namespace ImFormsTests
{
    public class Tests
    {
        [Fact]
        void IDTypeGenerationTest()
        {
            var panel = new Panel();
            var mgr = new ImFormsMgr(panel);
            var id = new IDType();
            var name = mgr.GenerateNameFromID(id);
        }
    }
}
