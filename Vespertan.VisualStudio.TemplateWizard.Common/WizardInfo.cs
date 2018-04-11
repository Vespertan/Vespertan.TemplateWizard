using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vespertan.VisualStudio.TemplateWizard.Common
{
    public static class WizardInfo
    {
        public static string Name { get; set; }
        public static Dictionary<string, string> InputReplacementDictionary { get; set; }
        public static Dictionary<string, string> EvaluatedReplacementDictionary { get; set; }
        public static Dictionary<string, object> Data { get; set; }
        public static List<string> Log { get; set; }
    }
}
