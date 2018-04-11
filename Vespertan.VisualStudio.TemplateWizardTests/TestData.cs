using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vespertan.CustomParametersWizardTests
{
    public class TestData
    {
        public string Key { get; set; }
        public string Name { get; set; }

        public static List<TestData> List => new List<TestData> { new TestData { Key = "1", Name = "One" }, new TestData { Key = "2", Name = "Two" } };
        public void Nothing() { }
        public double DoubleValue() => 1.3;
    }
}
