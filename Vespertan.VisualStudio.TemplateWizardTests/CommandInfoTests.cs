using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vespertan.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vespertan.VisualStudio.TemplateWizardTests
{
    [TestClass()]
    public class CommandInfoTests
    {
        [TestMethod()]
        public void CommandInfoTest()
        {
            var command = "Command1 -t -r kkk";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("Command1", cmdInfo.Name);
            Assert.AreEqual("", cmdInfo.Switches["-t"]);
            Assert.AreEqual("kkk", cmdInfo.Switches["-r"]);
        }

        [TestMethod()]
        public void CommandInfoTest2()
        {
            var command = "Command1  -t  -r kkk -x ";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("Command1", cmdInfo.Name);
            Assert.AreEqual("", cmdInfo.Switches["-t"]);
            Assert.AreEqual("kkk", cmdInfo.Switches["-r"]);
            Assert.AreEqual("", cmdInfo.Switches["-x"]);
        }

        [TestMethod()]
        public void CommandInfoTest3()
        {
            var command = "Command1 -t -r \"kkk ddd\"";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("Command1", cmdInfo.Name);
            Assert.AreEqual("", cmdInfo.Switches["-t"]);
            Assert.AreEqual("kkk ddd", cmdInfo.Switches["-r"]);

        }

        [TestMethod()]
        public void CommandInfoTest4()
        {
            var command = "Command1 -t -r \"kkk \\n ddd\"";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("Command1", cmdInfo.Name);
            Assert.AreEqual("", cmdInfo.Switches["-t"]);
            Assert.AreEqual("kkk \n ddd", cmdInfo.Switches["-r"]);

        }

        [TestMethod()]
        public void CommandInfoTest5()
        {
            var command = "Command1 \"kkk \\n ddd\"";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("kkk \n ddd", cmdInfo.Switches[""]);

        }

        [TestMethod()]
        public void CommandInfoTest6()
        {
            var command = "Command1 \"kkk -ddd\"";
            var cmdInfo = new CommandInfo(command);
            Assert.AreEqual("kkk -ddd", cmdInfo.Switches[""]);
        }
    }
}