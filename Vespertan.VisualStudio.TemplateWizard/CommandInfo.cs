using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vespertan.VisualStudio.TemplateWizard
{
    public class CommandInfo
    {
        private string _command;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Switches { get; set; } = new Dictionary<string, string>();
        public string GetSwitchValue(string switchName)
        {
            if (Switches.ContainsKey(switchName))
            {
                return Switches[switchName];
            }
            else
            {
                return null;
            }
        }
        public bool HasSwitch(string switchName)
        {
            return Switches.ContainsKey(switchName);
        }
        public string Data { get; set; }
        public string Key { get; set; }

        public CommandInfo(string command)
        {
            _command = command.Trim();
            ExtractData();
        }

        private void ExtractData()
        {
            int index = 0;
            var strb = new StringBuilder();
            for (; index < _command.Length; index++)
            {
                if (_command[index] == ' ')
                {
                    break;
                }
                else
                {
                    strb.Append(_command[index]);
                }
            }
            Name = strb.ToString();
            if (index > _command.Length)
            {
                Data = string.Empty;
            }
            else
            {
                Data = _command.Substring(index).Trim();
            }

            strb.Clear();

            var isSwitchName = false;
            var isQuote = false;
            var isSwitchValue = false;
            var isSpace = false;
            var waitForSwitchValue = false;
            var isSlash = false;
            for (; index < _command.Length; index++)
            {
                isSpace = _command[index] == ' ';

                if (_command[index] == '\\' && !isSlash && isSwitchValue)
                {
                    isSlash = true;
                    if (index + 1 < _command.Length)
                    {
                        continue;
                    }
                    else
                    {
                        strb.Append(_command[index]);
                    }
                }

                if (isSpace && !isSwitchName && !isSwitchValue && !isQuote)
                {
                    continue;
                }

                if (_command[index] == '-' && !isQuote && !isSwitchName && !isSwitchValue)
                {
                    strb.Append(_command[index]);
                    isSwitchName = true;
                    continue;
                }


                if (isSwitchName)
                {
                    if (isSpace)
                    {
                        isSwitchName = false;
                        waitForSwitchValue = true;
                        Switches[strb.ToString()] = string.Empty;
                        strb.Clear();
                        continue;
                    }
                    else
                    {
                        strb.Append(_command[index]);
                        continue;
                    }
                }

                isSwitchValue = true;

                if (_command[index] == '"' && !isQuote)
                {
                    isQuote = true;
                    continue;
                }
                else if ((_command[index] == '"' && isQuote && !isSlash) || (_command[index] == ' ' && !isQuote))
                {
                    if (waitForSwitchValue)
                    {
                        Switches[Switches.Keys.Last()] = strb.ToString();
                        strb.Clear();
                        waitForSwitchValue = false;
                    }
                    else
                    {
                        Switches[string.Empty] = strb.ToString();
                    }
                    isQuote = false;
                    isSwitchValue = false;
                }
                else if (isSlash)
                {
                    if (_command[index] == 'n')
                    {
                        strb.Append("\n");
                    }
                    else if (_command[index] == 'r')
                    {
                        strb.Append("\r");
                    }
                    else if (_command[index] == 't')
                    {
                        strb.Append("\t");
                    }
                    else if (_command[index] == '\\')
                    {
                        strb.Append("\\");
                    }
                    else if (_command[index] == '"')
                    {
                        strb.Append("\"");
                    }
                    else
                    {
                        strb.Append('\\');
                        strb.Append(_command[index]);
                    }
                    isSlash = false;
                    continue;
                }
                else
                {
                    strb.Append(_command[index]);
                }
            }

            if (strb.Length > 0)
            {
                if (waitForSwitchValue)
                {
                    Switches[Switches.Keys.Last()] = strb.ToString();
                }
                else
                {
                    if (strb[0] == '-')
                    {
                        Switches[strb.ToString()] = string.Empty;
                    }
                    else
                    {
                        Switches[string.Empty] = strb.ToString();
                    }
                }
            }
        }
    }
}
