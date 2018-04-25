using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Vespertan.TemplateWizard.Common;

namespace Vespertan.TemplateWizard
{
    public class CustomParametersWizard : IWizard
    {

        private List<FrameworkElement> _controlsEditor = new List<FrameworkElement>();
        private string DateTimeFormat = string.Empty;
        Dictionary<string, string> _replacementsDictionary;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(Project project)
        {

        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {

        }

        public void RunFinished()
        {

        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            WizardInfo.Name = nameof(CustomParametersWizard);
            WizardInfo.InputReplacementDictionary = replacementsDictionary;
            _replacementsDictionary = replacementsDictionary;
            var lastEditorKey = GetLastShowEditorKeyName(replacementsDictionary);
            foreach (var key in replacementsDictionary.Keys.ToList())
            {
                var value = replacementsDictionary[key];
                var maches = Regex.Matches(value, @"\$[^$]+\$");
                if (maches.Count > 0)
                {
                    foreach (Match m in maches)
                    {
                        if (replacementsDictionary.ContainsKey(m.Value))
                        {
                            value = value.Replace(m.Value, replacementsDictionary[m.Value]);
                        }
                    }
                }
                try
                {
                    replacementsDictionary[key] = GetEvaluatedValue(key, value);
                }
                catch (Exception ex)
                {
                    var strb = new StringBuilder();
                    int i = 1;

                    while (ex != null)
                    {
                        strb.AppendLine($"-----Exception {1} {ex.GetType()}-------\n" +
                            $"----------Message {i} --------------\n" +
                            $"{ex.Message}\n" +
                            $"----------StackTrace {i} -------------\n" +
                            $"{ex.StackTrace}");
                        i++;
                        ex = ex.InnerException;
                    }
                    replacementsDictionary[key] = strb.ToString();
                }
                if (key == lastEditorKey)
                {
                    GetValueFromEditors(replacementsDictionary);
                    _controlsEditor = new List<FrameworkElement>();
                    lastEditorKey = GetLastShowEditorKeyName(replacementsDictionary, key);
                }
            }
            WizardInfo.EvaluatedReplacementDictionary = replacementsDictionary;
        }

        public string GetLastShowEditorKeyName(Dictionary<string, string> replacementsDictionary, string startKey = null)
        {
            string key = null;
            bool watching = startKey == null;
            foreach (var item in replacementsDictionary)
            {
                if (item.Key == startKey)
                {
                    watching = true;
                    continue;
                }

                if (!watching)
                {
                    continue;
                }

                if (item.Value.StartsWith("#Show"))
                {
                    key = item.Key;
                }

                if (item.Value == "#EndEditorsGroup")
                {
                    return item.Key;
                }
            }
            return key;
        }

        public void GetValueFromEditors(Dictionary<string, string> replacementsDictionary)
        {
            if (_controlsEditor.Count == 0)
            {
                return;
            }
            var wnd = new ParamEditorsWindow();

            int i = 0;
            foreach (var control in _controlsEditor)
            {
                wnd.grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                var promptTextBlock = new TextBlock { Text = (((CommandInfo)control.Tag).GetSwitchValue("-prompt") ?? control.Name) + ": ", Padding = new Thickness(2, 5, 2, 5) };
                Grid.SetColumn(promptTextBlock, 0);
                Grid.SetRow(promptTextBlock, i);
                wnd.grid.Children.Add(promptTextBlock);

                control.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(control, 1);
                Grid.SetRow(control, i);
                wnd.grid.Children.Add(control);
                i++;
            }

            wnd.grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            var btn = new Button() { Content = "OK", HorizontalAlignment = HorizontalAlignment.Left, Width = 40, Padding = new Thickness(2, 3, 2, 3) };
            btn.Click += (s, e) => wnd.Close();
            Grid.SetColumn(btn, 0);
            Grid.SetColumnSpan(btn, 2);
            Grid.SetRow(btn, i);
            wnd.grid.Children.Add(btn);
            wnd.grid.RowDefinitions.Add(new RowDefinition());

            wnd.ShowDialog();

            foreach (var control in _controlsEditor)
            {
                if (control is TextBox textBox)
                {
                    replacementsDictionary[$"${control.Name}$"] = textBox.Text;
                }
                else if (control is CheckBox checkBox)
                {
                    var cmdInfo = ((CommandInfo)control.Tag);
                    string value = string.Empty;
                    if (checkBox.IsChecked == true)
                    {
                        replacementsDictionary[$"${control.Name}$"] = cmdInfo.Switches["-trueValue"];
                    }
                    else if (checkBox.IsChecked == true)
                    {
                        replacementsDictionary[$"${control.Name}$"] = cmdInfo.Switches["-falseValue"];
                    }
                    else
                    {
                        replacementsDictionary[$"${control.Name}$"] = cmdInfo.Switches["-nullValue"];
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    if (comboBox.SelectedValue == null)
                    {
                        replacementsDictionary[$"${control.Name}$"] = comboBox.Text;
                    }
                    else
                    {
                        replacementsDictionary[$"${control.Name}$"] = (string)comboBox.SelectedValue;
                    }
                }
                else if (control is ListBox listBox)
                {
                    var cmdInfo = ((CommandInfo)control.Tag);
                    var separator = DecodeSpecialChars(cmdInfo.GetSwitchValue("-separator")) ?? "`";
                    var strb = new StringBuilder();
                    var j = 1;
                    foreach (var item in listBox.SelectedItems)
                    {
                        if (item is KeyName keyName)
                        {
                            strb.Append(keyName.Key);
                        }
                        else
                        {
                            strb.Append(item);
                        }
                        if (j < listBox.SelectedItems.Count)
                        {
                            strb.Append(separator);
                        }
                        j++;
                    }
                    replacementsDictionary[$"${control.Name}$"] = strb.ToString();
                }
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public string GetEvaluatedValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.StartsWith("##"))
            {
                return value.Substring(1);
            }
            else if (value.StartsWith("#"))
            {
                return ExecuteCommand(new CommandInfo(value.Substring(1)) { Key = key });
            }
            else
            {
                return value;
            }
        }

        public string ExecuteCommand(CommandInfo commandInfo)
        {
            var method = GetType().GetMethod(commandInfo.Name, new[] { typeof(CommandInfo) });
            if (method == null)
            {
                return "Command not found: " + commandInfo.Name;
            }
            else
            {
                return (string)method.Invoke(this, new object[] { commandInfo });
            }
        }

        public string LowerFirstLetter(CommandInfo commandInfo)
        {
            return LowerFirstLetter(commandInfo.Data);
        }

        private string LowerFirstLetter(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            else
            {
                if (value.Length == 1)
                {
                    return value.ToLower();
                }
                else
                {
                    return value.Substring(0, 1).ToLower() + value.Substring(1);
                }
            }
        }

        public string UpperFirstLetter(CommandInfo commandInfo)
        {
            return UpperFirstLetter(commandInfo.Data);
        }

        public string UpperFirstLetter(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            else
            {
                if (value.Length == 1)
                {
                    return value.ToUpper();
                }
                else
                {
                    return value.Substring(0, 1).ToUpper() + value.Substring(1);
                }
            }
        }

        public string Lower(CommandInfo commandInfo)
        {
            if (string.IsNullOrWhiteSpace(commandInfo.Data))
            {
                return commandInfo.Data;
            }
            else
            {
                return commandInfo.Data.ToLower();
            }
        }

        public string Upper(CommandInfo commandInfo)
        {
            if (string.IsNullOrWhiteSpace(commandInfo.Data))
            {
                return commandInfo.Data;
            }
            else
            {
                return commandInfo.Data.ToUpper();
            }
        }

        public string Replace(CommandInfo commandInfo)
        {
            var value = commandInfo.GetSwitchValue("-value");
            var oldValue = commandInfo.GetSwitchValue("-oldValue");
            var newValue = commandInfo.GetSwitchValue("-newValue");

            return value.Replace(oldValue, newValue);
        }

        public string SubString(CommandInfo commandInfo)
        {
            var value = commandInfo.GetSwitchValue("-value");
            var startIndex = commandInfo.GetSwitchValue("-startIndex") ?? "0";
            var length = commandInfo.GetSwitchValue("-length");
            if (length == null)
            {
                return value.Substring(int.Parse(startIndex));
            }
            else
            {
                return value.Substring(int.Parse(startIndex), int.Parse(length));
            }
        }

        public string Length(CommandInfo commandInfo)
        {
            return commandInfo.Data.Length.ToString();
        }

        public string RemoveLastWord(CommandInfo commandInfo)
        {
            if (string.IsNullOrWhiteSpace(commandInfo.Data))
            {
                return commandInfo.Data;
            }
            else
            {
                for (int i = commandInfo.Data.Length - 1; i >= 0; i--)
                {
                    var ch = commandInfo.Data.Substring(i, 1);
                    if (ch == ch.ToUpper() && ch.ToUpper() != ch.ToLower() && i > 0)
                    {
                        return commandInfo.Data.Substring(0, i);
                    }
                }
                return commandInfo.Data;
            }
        }

        public string SplitPascalCase(CommandInfo commandInfo)
        {
            if (string.IsNullOrWhiteSpace(commandInfo.Data))
            {
                return commandInfo.Data;
            }
            else
            {
                var strb = new StringBuilder();
                for (int i = 0; i < commandInfo.Data.Length; i++)
                {
                    var c = commandInfo.Data.Substring(i, 1);
                    var cU = c.ToUpper();
                    var cL = c.ToLower();
                    var isUpper = c == cU;
                    if (i == 0)
                    {
                        strb.Append(commandInfo.Data[i]);
                    }
                    else if (isUpper)
                    {
                        strb.Append(" ").Append(cL);
                    }
                    else
                    {
                        strb.Append(c);
                    }
                }
                return strb.ToString();
            }
        }

        public string SetDateTimeFormat(CommandInfo commandInfo)
        {
            DateTimeFormat = commandInfo.Data;
            return DateTimeFormat;
        }

        public string Foreach(CommandInfo commandInfo)
        {
            var list = commandInfo.GetSwitchValue("-list");
            var format = DecodeSpecialChars(commandInfo.GetSwitchValue("-format")) ?? "{0}";
            var separator = DecodeSpecialChars(commandInfo.GetSwitchValue("-separator")) ?? "`";
            var leaveEmptyItems = commandInfo.HasSwitch("-leaveEmptyItems");
            var oneLineResult = commandInfo.HasSwitch("-oneLineResult");
            var sort = commandInfo.GetSwitchValue("-sort");
            var regexPattern = commandInfo.GetSwitchValue("-regexPattern");
            var regexReplacment = commandInfo.GetSwitchValue("-regexReplacment");
            var strb = new StringBuilder();
            var items = list.Split(new string[] { separator }, leaveEmptyItems ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries).ToList();

            if (sort != null)
            {
                if (sort == "Asc")
                {
                    items.Sort((x, y) => string.Compare(x, y));
                }
                else if (sort == "Desc")
                {
                    items.Sort((x, y) => -string.Compare(x, y));
                }
            }
            var i = 1;
            foreach (var item in items)
            {
                string valueString;
                if (regexPattern == null)
                {
                    valueString = item;
                }
                else
                {
                    valueString = Regex.Replace(item, regexPattern, regexReplacment);
                }
                strb.Append(string.Format(format, valueString, LowerFirstLetter(valueString), UpperFirstLetter(valueString), valueString.ToLower(), valueString.ToUpper()));

                if (!oneLineResult && i < items.Count)
                {
                    strb.AppendLine();
                }
                i++;
            }
            return strb.ToString();
        }

        public string ShowLabel(CommandInfo commandInfo)
        {
            if (!commandInfo.HasSwitch("-hide"))
            {
                var textBlock = new TextBlock();
                textBlock.Text = commandInfo.GetSwitchValue("-defaultValue");
                textBlock.Name = commandInfo.Key.Replace("$", null);
                textBlock.Tag = commandInfo;
                _controlsEditor.Add(textBlock);
            }
            return string.Empty;
        }

        public string ShowTextBox(CommandInfo commandInfo)
        {
            if (!commandInfo.HasSwitch("-hide"))
            {
                var textBox = new TextBox();
                textBox.Text = commandInfo.GetSwitchValue("-defaultValue");
                textBox.Name = commandInfo.Key.Replace("$", null);
                textBox.Tag = commandInfo;
                _controlsEditor.Add(textBox);
            }
            return string.Empty;
        }

        public string ShowCheckBox(CommandInfo commandInfo)
        {
            if (!commandInfo.HasSwitch("-hide"))
            {
                var checkBox = new CheckBox();
                if (!commandInfo.HasSwitch("-trueValue"))
                {
                    commandInfo.Switches["-trueValue"] = "true";
                }

                if (!commandInfo.HasSwitch("-falseValue"))
                {
                    commandInfo.Switches["-falseValue"] = "false";
                }

                if (!commandInfo.HasSwitch("-nullValue"))
                {
                    commandInfo.Switches["-nullValue"] = "null";
                }

                if (commandInfo.HasSwitch("-isThreeState"))
                {
                    checkBox.IsThreeState = true;
                }

                if (commandInfo.HasSwitch("-defaultValue"))
                {
                    if (commandInfo.Switches["-defaultValue"] == commandInfo.Switches["-trueValue"])
                    {
                        checkBox.IsChecked = true;
                    }
                    else if (commandInfo.Switches["-defaultValue"] == commandInfo.Switches["-falseValue"])
                    {
                        checkBox.IsChecked = false;
                    }
                    else if (commandInfo.Switches["-defaultValue"] == commandInfo.Switches["-nullValue"] && checkBox.IsThreeState)
                    {
                        checkBox.IsChecked = null;
                    }
                }
                checkBox.Name = commandInfo.Key.Replace("$", null);
                checkBox.Tag = commandInfo;
                _controlsEditor.Add(checkBox);
            }
            return string.Empty;
        }

        public string ShowComboBox(CommandInfo commandInfo)
        {
            if (!commandInfo.HasSwitch("-hide"))
            {

                var comboBox = new ComboBox();
                if (!commandInfo.HasSwitch("-separator"))
                {
                    commandInfo.Switches["-separator"] = "`";
                }
                else
                {
                    commandInfo.Switches["-separator"] = DecodeSpecialChars(commandInfo.Switches["-separator"]);
                }

                if (!commandInfo.HasSwitch("-values"))
                {
                    commandInfo.Switches["-values"] = string.Empty;
                }


                if (commandInfo.HasSwitch("-displayValues"))
                {
                    var values = commandInfo.Switches["-values"].Split(new string[] { commandInfo.Switches["-separator"] }, StringSplitOptions.None);
                    var displayValues = commandInfo.Switches["-displayValues"].Split(new string[] { commandInfo.Switches["-separator"] }, StringSplitOptions.None);
                    var lst = new List<KeyName>();
                    for (int i = 0; i < values.Length; i++)
                    {
                        lst.Add(new KeyName { Key = values[i], Name = displayValues.ElementAtOrDefault(i) ?? values[i] });
                    }
                    comboBox.SelectedValuePath = "Key";
                    comboBox.DisplayMemberPath = "Name";
                    comboBox.ItemsSource = lst;
                }
                else
                {
                    var values = commandInfo.Switches["-values"].Split(new string[] { commandInfo.Switches["-separator"] }, StringSplitOptions.None);
                    for (int i = 0; i < values.Length; i++)
                    {
                        comboBox.Items.Add(values[i]);
                    }
                }

                if (commandInfo.HasSwitch("-allowCustomText"))
                {
                    comboBox.IsEditable = true;
                }

                comboBox.SelectedValue = commandInfo.GetSwitchValue("-defaultValue");
                comboBox.Name = commandInfo.Key.Replace("$", null);
                comboBox.Tag = commandInfo;
                _controlsEditor.Add(comboBox);
            }
            return string.Empty;
        }

        public string ShowListBox(CommandInfo commandInfo)
        {
            if (!commandInfo.HasSwitch("-hide"))
            {
                var listBox = new ListBox();
                var separator = DecodeSpecialChars(commandInfo.GetSwitchValue("-separator")) ?? "`";
                var values = commandInfo.GetSwitchValue("-values") ?? string.Empty;

                
                if (commandInfo.HasSwitch("-displayValues"))
                {
                    var valuesItems = values.Split(new string[] { separator }, StringSplitOptions.None);
                    var displayValues = commandInfo.Switches["-displayValues"].Split(new string[] { separator }, StringSplitOptions.None);
                    var lst = new List<KeyName>();
                    for (int i = 0; i < valuesItems.Length; i++)
                    {
                        lst.Add(new KeyName { Key = valuesItems[i], Name = displayValues.ElementAtOrDefault(i) ?? valuesItems[i] });
                    }
                    listBox.SelectedValuePath = "Key";
                    listBox.DisplayMemberPath = "Name";
                    listBox.ItemsSource = lst;
                }
                else
                {
                    var valuesItems = values.Split(new string[] { separator }, StringSplitOptions.None);
                    for (int i = 0; i < valuesItems.Length; i++)
                    {
                        listBox.Items.Add(valuesItems[i]);
                    }
                }

                if (Enum.TryParse(commandInfo.GetSwitchValue("-selectionMode"), out SelectionMode selectionMode))
                {
                    listBox.SelectionMode = selectionMode;
                }
                var defaultValue = commandInfo.GetSwitchValue("-defaultValue");
                if (selectionMode == SelectionMode.Single)
                {
                    listBox.SelectedValue = defaultValue;
                }
                else
                {
                    var defaultValueList = defaultValue.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listBox.Items)
                    {
                        if (item is KeyName keyName)
                        {
                            if (defaultValueList.Contains(keyName.Key))
                            {
                                listBox.SelectedItems.Add(item);
                            }
                        }
                        else
                        {
                            if (defaultValueList.Contains((string)item))
                            {
                                listBox.SelectedItems.Add(item);
                            }
                        }
                    }

                }
                if (double.TryParse(commandInfo.GetSwitchValue("-maxHeight"), NumberStyles.Float, CultureInfo.InvariantCulture, out double maxHeight))
                {
                    listBox.MaxHeight = maxHeight;
                }
                else
                {
                    listBox.MaxHeight = 500;
                }
                listBox.Name = commandInfo.Key.Replace("$", null);
                listBox.Tag = commandInfo;
                _controlsEditor.Add(listBox);
            }
            return string.Empty;
        }

        public string GetValue(CommandInfo commandInfo)
        {
            var className = commandInfo.GetSwitchValue("-className");
            var assemblyName = commandInfo.GetSwitchValue("-assemblyName");
            var assemblyLocation = commandInfo.GetSwitchValue("-assemblyLocation");
            var ctorParams = commandInfo.GetSwitchValue("-ctorParams");
            var ctorParamsType = commandInfo.GetSwitchValue("-ctorParamsType");
            var methodName = commandInfo.GetSwitchValue("-methodName");
            var methodParams = commandInfo.GetSwitchValue("-methodParams");
            var methodParamsType = commandInfo.GetSwitchValue("-methodParamsType");
            var propertyName = commandInfo.GetSwitchValue("-propertyName");
            var isStatic = commandInfo.HasSwitch("-isStatic");
            var separator = DecodeSpecialChars(commandInfo.GetSwitchValue("-separator")) ?? "`";
            var resultPropertyName = commandInfo.GetSwitchValue("-resultPropertyName");

            Assembly assembly;
            if (assemblyLocation == null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                assembly = Assembly.LoadWithPartialName(assemblyName) ?? throw new ArgumentNullException(nameof(assemblyName));
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                assembly = Assembly.LoadFile(assemblyLocation) ?? throw new ArgumentNullException(nameof(assemblyLocation));
            }

            var type = assembly.GetType(className) ?? throw new Exception($"Type not found: {className}");
            //Static
            object instance = null;
            if (!isStatic)
            {
                var paramsInfo = GetParamInfo(ctorParams, ctorParamsType, separator);
                var ctor = type.GetConstructor(paramsInfo.Item1);
                instance = ctor.Invoke(paramsInfo.Item2);
            }
            //Method
            IEnumerable<object> result;
            object invokeResult;
            if (methodName != null)
            {
                var methodParamsInfo = GetParamInfo(methodParams, methodParamsType, separator);
                var method = type.GetMethod(methodName, methodParamsInfo.Item1) ?? throw new ArgumentNullException(nameof(methodName));
                invokeResult = method.Invoke(instance, methodParamsInfo.Item2);
            }
            //Property
            else
            {
                var property = type.GetProperty(propertyName) ?? throw new ArgumentNullException(nameof(propertyName));
                invokeResult = property.GetValue(instance);
            }

            //Transform result
            if (invokeResult is System.Collections.IEnumerable enumerable)
            {
                result = enumerable.Cast<object>();
            }
            else
            {
                result = new object[] { invokeResult };
            }

            // Prepare result
            var strb = new StringBuilder();
            foreach (var item in result)
            {
                if (item != null)
                {
                    if (resultPropertyName != null)
                    {
                        var prop = item.GetType().GetProperty(resultPropertyName);
                        if (prop != null)
                        {
                            strb.Append(ObjectToString(prop.GetValue(item)));
                        }
                    }
                    else
                    {
                        strb.Append(ObjectToString(item));
                    }
                }
                strb.Append(separator);
            }

            //Remove last separator
            if (strb.Length > 0)
            {
                strb.Remove(strb.Length - separator.Length, separator.Length);
            }
            return strb.ToString();
        }

        public string Execute(CommandInfo commandInfo)
        {
            var fileName = commandInfo.GetSwitchValue("-fileName");
            var args = commandInfo.GetSwitchValue("-args");
            var waitTime = commandInfo.GetSwitchValue("-waitTime");
            var returnOutput = commandInfo.HasSwitch("-returnOutput");
            var useShellExecute = commandInfo.HasSwitch("-useShellExecute");
            var proc = new System.Diagnostics.Process();
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo(fileName, args);
            if (returnOutput)
            {
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
            }
            else
            {
                proc.StartInfo.UseShellExecute = useShellExecute;
            }

            proc.Start();
            if (waitTime == null)
            {
                return $"no wait: {fileName} {args}";
            }
            else
            {
                if (waitTime == string.Empty || waitTime == "0")
                {
                    proc.WaitForExit();
                    return returnOutput ? RemoveLastNewLine(proc.StandardOutput.ReadToEnd()) : string.Empty;
                }
                else
                {
                    if (proc.WaitForExit(int.Parse(waitTime)))
                    {
                        return returnOutput ? RemoveLastNewLine(proc.StandardOutput.ReadToEnd()) : string.Empty;
                    }
                    else
                    {
                        return "timeOut! waitTime [ms]: " + waitTime;
                    }
                }
            }

        }

        private string RemoveLastNewLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            else if (value.EndsWith("\r\n"))
            {
                return value.Substring(0, value.Length - 2);
            }
            else if (value.EndsWith("\n"))
            {
                return value.Substring(0, value.Length - 1);
            }
            else
            {
                return value;
            }
        }

        private Tuple<Type[], object[]> GetParamInfo(string values, string types, string separator)
        {
            var valueList = values?.Split(new string[] { separator }, StringSplitOptions.None) ?? new string[0];
            var typeList = types?.Split(new string[] { separator }, StringSplitOptions.None) ?? new string[0];
            var resultTypeList = new Type[valueList.Count()];
            var resultValueList = new object[valueList.Count()];

            for (int i = 0; i < resultTypeList.Length; i++)
            {
                var typeText = typeList.ElementAtOrDefault(i)?.Trim();
                var valueText = valueList.ElementAtOrDefault(i);
                switch (typeText)
                {
                    case "char":
                        resultTypeList[i] = typeof(char);
                        resultValueList[i] = char.Parse(valueText);
                        break;
                    case "char?":
                        resultTypeList[i] = typeof(char?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : char.Parse(valueText);
                        break;
                    case "byte":
                        resultTypeList[i] = typeof(byte);
                        resultValueList[i] = byte.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "byte?":
                        resultTypeList[i] = typeof(byte?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : byte.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "int":
                        resultTypeList[i] = typeof(int);
                        resultValueList[i] = int.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "int?":
                        resultTypeList[i] = typeof(int?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : int.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "long":
                        resultTypeList[i] = typeof(long);
                        resultValueList[i] = long.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "long?":
                        resultTypeList[i] = typeof(long?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : long.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "double":
                        resultTypeList[i] = typeof(double);
                        resultValueList[i] = double.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "double?":
                        resultTypeList[i] = typeof(double?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : double.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "decimal":
                        resultTypeList[i] = typeof(decimal);
                        resultValueList[i] = decimal.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "decimal?":
                        resultTypeList[i] = typeof(decimal?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : decimal.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "bool":
                        resultTypeList[i] = typeof(bool);
                        resultValueList[i] = byte.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "bool?":
                        resultTypeList[i] = typeof(bool?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : byte.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "DateTime":
                        resultTypeList[i] = typeof(DateTime);
                        resultValueList[i] = DateTime.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "DateTime?":
                        resultTypeList[i] = typeof(DateTime?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : DateTime.Parse(valueText, CultureInfo.InvariantCulture);
                        break;
                    case "Guid":
                        resultTypeList[i] = typeof(Guid);
                        resultValueList[i] = Guid.Parse(valueText);
                        break;
                    case "Guid?":
                        resultTypeList[i] = typeof(Guid?);
                        resultValueList[i] = string.IsNullOrEmpty(valueText) ? (object)null : Guid.Parse(valueText);
                        break;
                    default:
                        resultTypeList[i] = typeof(string);
                        resultValueList[i] = valueText;
                        break;
                }

            }
            return new Tuple<Type[], object[]>(resultTypeList, resultValueList);
        }

        public string DecodeSpecialChars(CommandInfo commandInfo)
        {
            return DecodeSpecialChars(commandInfo.Data);
        }

        public string DecodeSpecialChars(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            var strb = new StringBuilder();
            var isBackSlash = false;
            foreach (var ch in value)
            {
                if (ch == '\\' && !isBackSlash)
                {
                    isBackSlash = true;
                    continue;
                }

                if (isBackSlash)
                {
                    if (ch == 'n')
                    {
                        strb.Append("\n");
                    }
                    else if (ch == 'r')
                    {
                        strb.Append("\r");
                    }
                    else if (ch == 't')
                    {
                        strb.Append("\t");
                    }
                    else if (ch == '\\')
                    {
                        strb.Append("\\");
                    }
                    else if (ch == '"')
                    {
                        strb.Append("\"");
                    }
                    else
                    {
                        strb.Append("\\").Append(ch);
                    }
                    isBackSlash = false;
                }
                else
                {
                    strb.Append(ch);
                }
            }
            if (isBackSlash)
            {
                strb.Append("\\");
            }

            return strb.ToString();
        }

        public string EncodeSpecialChars(CommandInfo commandInfo)
        {
            return EncodeSpecialChars(commandInfo.Data);
        }

        public string EncodeSpecialChars(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            var strb = new StringBuilder();

            foreach (var ch in value)
            {
                if (ch == '\n')
                {
                    strb.Append("\\n");
                }
                else if (ch == '\r')
                {
                    strb.Append("\\r");
                }
                else if (ch == '\t')
                {
                    strb.Append("\\t");
                }
                else if (ch == '\\')
                {
                    strb.Append("\\\\");
                }
                else if (ch == '"')
                {
                    strb.Append("\\\"");
                }
                else
                {
                    strb.Append(ch);
                }
            }
            return strb.ToString();
        }

        public string GetDateTime(CommandInfo commandInfo)
        {
            return ObjectToString(DateTime.Now);
        }

        public string SaveText(CommandInfo commandInfo)
        {
            var fileName = commandInfo.GetSwitchValue("-fileName");
            var text = commandInfo.GetSwitchValue("-text");
            var append = commandInfo.HasSwitch("-append");

            if (append)
            {
                File.AppendAllText(fileName, text);
            }
            else
            {
                File.WriteAllText(fileName, text);
            }
            return Path.GetFullPath(fileName);
        }

        public string LoadText(CommandInfo commandInfo)
        {
            var fileName = commandInfo.GetSwitchValue("-fileName");
            return File.ReadAllText(fileName);
        }

        public string DeleteFile(CommandInfo commandInfo)
        {
            var fileName = commandInfo.GetSwitchValue("-fileName");
            File.Delete(fileName);
            return Path.GetFullPath(fileName);

        }

        public string GetAllParams(CommandInfo commandInfo)
        {
            var strb = new StringBuilder();
            foreach (var item in _replacementsDictionary)
            {
                strb.AppendLine($"{item.Key}: {item.Value}");
            }
            strb.Remove(strb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            return strb.ToString();
        }

        private string ObjectToString(object value)
        {
            if (value == null)
            {
                return null;
            }
            var vt = value.GetType();
            if (vt == typeof(double))
            {
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (vt == typeof(decimal))
            {
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (vt == typeof(DateTime))
            {
                if (string.IsNullOrWhiteSpace(DateTimeFormat))
                {
                    return ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return ((DateTime)value).ToString(DateTimeFormat);
                }
            }
            else
            {
                return value.ToString();
            }


        }

    }
}
