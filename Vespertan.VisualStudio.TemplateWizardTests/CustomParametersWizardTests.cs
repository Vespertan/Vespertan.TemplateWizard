using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TemplateWizard;
using System.Globalization;
using System.IO;
using Vespertan.VisualStudio.TemplateWizard;

namespace Vespertan.VisualStudio.TemplateWizardTests
{
    [TestClass()]
    public class CustomParametersWizardTests
    {
        [TestMethod()]
        public void RunStartedTest()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#ShowTextBox -prompt WarośćA -defaultValue XXX";
            dic["$b$"] = "#ShowTextBox -prompt WarośćB -defaultValue \"a b c\"";
            dic["$c$"] = "#ShowCheckBox -prompt \"Warość C\" -defaultValue true -isThreeState";
            dic["$d$"] = "#ShowComboBox -prompt WarośćD -defaultValue 1 -values 1`2`3`4 -displayValues \"Jeden 1`Dwa 2`Trzy 3`Cztery 4\" -allowCustomText";
            dic["$d2$"] = "#ShowComboBox -prompt WarośćD -defaultValue 2 -values 1`2`3`4 -allowCustomText";
            dic["$e$"] = "#ShowLabel -prompt WarośćE -defaultValue \"opis \\nwielo\\nLinowy\"";
            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("XXX", dic["$a$"]);
            Assert.AreEqual("a b c", dic["$b$"]);
            Assert.AreEqual("true", dic["$c$"]);
            Assert.AreEqual("1", dic["$d$"]);
            Assert.AreEqual("2", dic["$d2$"]);
        }

        [TestMethod()]
        public void RunStartedTest2()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#ShowTextBox -prompt WarośćA -defaultValue XXX";
            dic["$b$"] = "#ShowTextBox -prompt WarośćB -defaultValue \"a b c\"";
            dic["$c$"] = "#ShowCheckBox -prompt \"Warość C\" -defaultValue true -isThreeState";
            dic["$d$"] = "#ShowComboBox -prompt WarośćD -defaultValue 1 -values 1`2`3`4 -displayValues \"Jeden 1`Dwa 2`Trzy 3`Cztery 4\" -allowCustomText";
            dic["$d2$"] = "#ShowComboBox -prompt WarośćD -defaultValue 2 -values 1`2`3`4 -allowCustomText";
            dic["$e$"] = "#ShowLabel -prompt WarośćE -defaultValue \"opis \\nwielo\\nLinowy\"";
            dic["$f$"] = "#EndEditorsGroup";

            dic["$a1$"] = "#ShowLabel -defaultValue $a$ -hide";
            dic["$b2$"] = "#ShowLabel -defaultValue \"$b$\"";
            dic["$c3$"] = "#ShowLabel -defaultValue $c$";
            dic["$d4$"] = "#ShowLabel -defaultValue $d$";
            dic["$d25$"] = "#ShowLabel -defaultValue $d2$";
            dic["$e6$"] = "#ShowLabel -defaultValue $e$";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("XXX", dic["$a$"]);
            Assert.AreEqual("a b c", dic["$b$"]);
            Assert.AreEqual("true", dic["$c$"]);
            Assert.AreEqual("1", dic["$d$"]);
            Assert.AreEqual("2", dic["$d2$"]);
        }

        [TestMethod()]
        public void RunStartedTest3()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"Baca`Alfa``Flaga\" ";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("Baca\r\nAlfa\r\nFlaga", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest4()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"baca`Alfa``Flaga\" -sort Asc";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("Alfa\r\nbaca\r\nFlaga", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest5()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"Baca`Alfa` `Flaga\" -sort Asc -oneLineResult -leaveEmptyItems";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual(" AlfaBacaFlaga", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest6()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"Baca`Alfa`Flaga\" -sort Asc -oneLineResult -format \"{1},\"";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("alfa,baca,flaga,", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest7()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"Baca`Alfa`Flaga\" -sort Asc -oneLineResult -regexPattern [aA] -regexReplacment \"x\"";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("xlfxBxcxFlxgx", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest8()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#Foreach -list \"Baca`Alfa`Flaga\" -sort Asc -oneLineResult -regexPattern ([aA]).+ -regexReplacment \"$1\"";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
            Assert.AreEqual("ABaFla", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest9()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#ShowListBox -prompt WarośćAXXXXXX -defaultValue 1`2 -values 1`2`3`4 -displayValues \"Jeden 1`Dwa 2`Trzy 3`Cztery 4\" -selectionMode Multiple";
            dic["$b$"] = "#ShowListBox -prompt WarośćB -defaultValue 2 -values 1`2`3`4";
            dic["$f$"] = "#EndEditorsGroup";

            dic["$a1$"] = "#ShowLabel -defaultValue \"$a$\"";
            dic["$b2$"] = "#ShowLabel -defaultValue \"$b$\"";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);
        }

        [TestMethod()]
        public void RunStartedTest10()
        {
            var cpw = new CustomParametersWizard();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#GetValue -assemblyName mscorlib -className System.String -isStatic -methodName IsNullOrEmpty -methodParams";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("True", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest11()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#GetValue -assemblyName Vespertan.CustomParametersWizardTests -className Vespertan.CustomParametersWizardTests.TestData -isStatic -propertyName List -resultPropertyName Key -separator |";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("1|2", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest12()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#GetValue -assemblyName Vespertan.CustomParametersWizardTests -className Vespertan.CustomParametersWizardTests.TestData -methodName Nothing -separator |";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest13()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#GetValue -assemblyName Vespertan.CustomParametersWizardTests -className Vespertan.CustomParametersWizardTests.TestData -methodName DoubleValue -separator |";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("1.3", dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest14()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$a$"] = "#GetValue -assemblyName mscorlib -className System.DateTime -isStatic -propertyName Today";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual(DateTime.Today.ToString(CultureInfo.InvariantCulture), dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest15()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#SetDateTimeFormat yyyy-MM-dd";
            dic["$a$"] = "#GetValue -assemblyName mscorlib -className System.DateTime -isStatic -propertyName Today";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), dic["$a$"]);
        }

        [TestMethod()]
        public void RunStartedTest16()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#Execute -fileName cmd -args \"/C echo xxx xxx\" -waitTime 0 -returnOutput";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("xxx xxx", dic["$1$"]);
        }

        [TestMethod()]
        public void RunStartedTest17()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#EncodeSpecialChars Line\nLine2\"\r\t\\";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("Line\\nLine2\\\"\\r\\t\\\\", dic["$1$"]);
        }

        [TestMethod()]
        public void RunStartedTest18()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#DecodeSpecialChars Line\\nLine2\\\"\\r\\t\\\\";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("Line\nLine2\"\r\t\\", dic["$1$"]);
        }


        [TestMethod()]
        public void RunStartedTest19()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#GetDateTime";
            dic["$2$"] = "#SetDateTimeFormat yyyy-MM-dd";
            dic["$3$"] = "#GetDateTime";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual(DateTime.Now.ToString(CultureInfo.InvariantCulture), dic["$1$"]);
            Assert.AreEqual(DateTime.Now.ToString("yyyy-MM-dd"), dic["$3$"]);
        }

        [TestMethod()]
        public void RunStartedTest20()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "Get Date Time\nXXXXXX\n";
            dic["$2$"] = "#SaveText -fileName test.txt -append -text \"$1$\"";


            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.IsTrue(File.Exists(dic["$2$"]));
        }

        [TestMethod()]
        public void RunStartedTest21()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#SaveText -fileName test1.txt -text XXXX";

            dic["$2$"] = "#LoadText -fileName test1.txt";


            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("XXXX", dic["$2$"]);
        }

        [TestMethod()]
        public void RunStartedTest22()
        {
            var cpw = new CustomParametersWizard();
            var t = cpw.GetType();
            var dic = new Dictionary<string, string>();
            dic["$1$"] = "#SaveText -fileName test1.txt -text XXXX";

            dic["$2$"] = "#LoadText -fileName test1.txt";
            dic["$3$"] = "#DeleteFile -fileName test1.txt";

            cpw.RunStarted(null, dic, WizardRunKind.AsNewItem, null);

            Assert.AreEqual("XXXX", dic["$2$"]);
            Assert.AreEqual(Path.GetFullPath("test1.txt"), dic["$3$"]);
        }
    }
}