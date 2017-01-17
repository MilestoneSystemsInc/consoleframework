using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using System.ComponentModel;
using Xaml;

namespace ConsoleFramework.Controls
{
    [ContentProperty("Caption")]
    public class KeyGestureButton : Button
    {
        public KeyGesture ShortcutKey
        {
            get; set;
        } = new KeyGesture(VirtualKeys.Space, ModifierKeys.None);

        protected override void Button_KeyDown(object sender, KeyEventArgs args)
        {
            if (Disabled) return;
            var kg = KeyGesture.FromKeyEventArgs(args);
            if (args.wVirtualKeyCode == VirtualKeys.Space
                || args.wVirtualKeyCode == VirtualKeys.Return
                || kg.Equals(ShortcutKey))
            {
                RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                if (Command != null && Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                }
                pressedUsingKeyboard = true;
                Invalidate();
                ConsoleApplication.Instance.Post(() => {
                    pressedUsingKeyboard = false;
                    Invalidate();
                }, TimeSpan.FromMilliseconds(300));
                args.Handled = true;
            }
        }
    }

    public class KeyGestureOption
    {
        public KeyGesture OptionKey { get; set; }
        public string FieldName { get; set; }
        public bool RequiresConsoleInput { get; set; }
        public Func<string, bool> FieldSetter { get; set; }
        public Func<string> FieldGetter { get; set; }
        public Func<object> FieldDefaultVal { get; set; }
        public int Page { get; internal set; }
    }

    public class KeyGestureOption<U> : KeyGestureOption
    {
        Func<U> _genericFieldGetter = null;
        Func<U, bool> _genericFieldSetter = null;
        public Func<U, bool> GenericFieldSetter
        {
            get { return _genericFieldSetter; }
            set
            {
                _genericFieldSetter = value;
                var conv = TypeDescriptor.GetConverter(typeof(U));
                FieldSetter = s => _genericFieldSetter((U)conv.ConvertFromString(s));
            }
        }
        public Func<U> GenericFieldGetter
        {
            get { return _genericFieldGetter; }
            set
            {
                _genericFieldGetter = value;
                var conv = TypeDescriptor.GetConverter(typeof(U));
                FieldGetter = () => conv.ConvertToString(_genericFieldGetter());
            }
        }
    }

    public static class KeyGestureOptionExtensions
    {
        public static readonly int ItemsPerPage = 10;

        public static void AddField(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, int nPage, KeyGesture oOptionKey, string strField, Func<string, bool> setter, Func<string> getter, Func<object> defaultVal)
        {
            if (!Options.ContainsKey(nPage))
                Options.Add(nPage, new Dictionary<KeyGesture, KeyGestureOption>());
            var cteOptions = new KeyGestureOption<object>()
            {
                OptionKey = oOptionKey,
                FieldName = strField,
                RequiresConsoleInput = true,
                FieldSetter = setter,
                FieldGetter = getter,
                FieldDefaultVal = defaultVal,
                Page = nPage,
            };
            if (!Options[nPage].ContainsKey(oOptionKey))
                Options[nPage].Add(oOptionKey, cteOptions);
            else
                Options[nPage][oOptionKey] = cteOptions;
        }

        public static void AddOp(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, KeyGesture oOptionKey, string strField, Func<string, bool> setter, Func<string> getter, Func<object> defaultVal)
        {
            AddOp(Options, -1, oOptionKey, strField, setter, getter, defaultVal);
        }

        public static void AddOp(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, int nPage, KeyGesture oOptionKey, string strField, Func<string, bool> setter, Func<string> getter, Func<object> defaultVal)
        {
            if (!Options.ContainsKey(nPage))
                Options.Add(nPage, new Dictionary<KeyGesture, KeyGestureOption>());
            var cteOptions = new KeyGestureOption<object>()
            {
                OptionKey = oOptionKey,
                FieldName = strField,
                RequiresConsoleInput = false,
                FieldSetter = setter,
                FieldGetter = getter,
                FieldDefaultVal = defaultVal,
                Page = nPage,
            };
            if (!Options[nPage].ContainsKey(oOptionKey))
                Options[nPage].Add(oOptionKey, cteOptions);
            else
                Options[nPage][oOptionKey] = cteOptions;
        }

        public static void AddField<U>(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, int nPage, KeyGesture oOptionKey, string strField, Func<U, bool> setter, Func<U> getter, Func<object> defaultVal)
        {
            if (!Options.ContainsKey(nPage))
                Options.Add(nPage, new Dictionary<KeyGesture, KeyGestureOption>());
            var cteOptions = new KeyGestureOption<U>()
            {
                OptionKey = oOptionKey,
                FieldName = strField,
                RequiresConsoleInput = true,
                GenericFieldSetter = setter,
                GenericFieldGetter = getter,
                FieldDefaultVal = defaultVal,
                Page = nPage,
            };
            if (!Options[nPage].ContainsKey(oOptionKey))
                Options[nPage].Add(oOptionKey, cteOptions);
            else
                Options[nPage][oOptionKey] = cteOptions;
        }

        public static void AddOp<U>(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, KeyGesture oOptionKey, string strField, Func<U, bool> setter, Func<U> getter, Func<object> defaultVal)
        {
            AddOp<U>(Options, -1, oOptionKey, strField, setter, getter, defaultVal);
        }

        public static void AddOp<U>(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, int nPage, KeyGesture oOptionKey, string strField, Func<U, bool> setter, Func<U> getter, Func<object> defaultVal)
        {
            if (!Options.ContainsKey(nPage))
                Options.Add(nPage, new Dictionary<KeyGesture, KeyGestureOption>());
            var cteOptions = new KeyGestureOption<U>()
            {
                OptionKey = oOptionKey,
                FieldName = strField,
                RequiresConsoleInput = false,
                GenericFieldSetter = setter,
                GenericFieldGetter = getter,
                FieldDefaultVal = defaultVal,
                Page = nPage,
            };
            if (!Options[nPage].ContainsKey(oOptionKey))
                Options[nPage].Add(oOptionKey, cteOptions);
            else
                Options[nPage][oOptionKey] = cteOptions;
        }

        static bool IsPrimitiveType(Type oType)
        {
            return oType.IsPrimitive || oType.IsValueType ||
                   (String.Compare(oType.FullName, "System.String", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(oType.FullName, "System.DateTime", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(oType.FullName, "System.Timespan", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(oType.FullName, "System.Guid", StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(oType.FullName, "System.Net.IPAddress", StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static void AddTypeProperty(this Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options, 
                                           int nPage, 
                                           KeyGesture oOptionKey, 
                                           Type propType, 
                                           string strPropName, 
                                           Func<object, bool> fnPropSetter, 
                                           Func<object> fnPropGetter, 
                                           Func<object> fnPropDefaultVal, 
                                           Action<string, Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>>> fnEditSubTypeAction)
        {
            if (!IsPrimitiveType(propType))
            {
                var propVal = fnPropGetter();
                // Only create child type if the existing value is null
                if (null == propVal)
                {
                    //var typeName = String.Concat(prop.PropertyType.FullName, ", ", prop.PropertyType.Assembly.FullName);
                    //var type = Type.GetType(typeName);
                    var child = Activator.CreateInstance(propType);
                    fnPropSetter(child); //prop.SetValue(instance, child, null);
                    propVal = child;
                }
                AddOp(Options,
                      nPage,
                      oOptionKey,
                      $"{strPropName} [{propType.ToString()}]",
                      op =>
                      {
                          //FromType(propVal, true).Show();
                          fnEditSubTypeAction(strPropName, FromType(propVal,fnEditSubTypeAction));
                          return true;
                      },
                      () => strPropName,
                      null);
            }
            else
            {
                var conv2 = TypeDescriptor.GetConverter(propType);
                //var propFunc = prop;
                //var propAttr = prop.GetCustomAttributes(typeof(DefaultValueAttribute), false).Cast<DefaultValueAttribute>().SingleOrDefault();
                AddField(Options, 
                         nPage,
                         oOptionKey,
                         $"{strPropName} [{propType.ToString()}]",
                         s =>
                         {
                             bool fRet = false;
                             try
                             {
                                 var obj = conv2.ConvertFromString(s);
                                 fRet = fnPropSetter(obj);
                             }
                             catch
                             {
                                 fRet = false;
                             }
                             return fRet;
                         },
                         () =>
                         {
                             var str = (String)null;
                             try
                             {
                                 str = conv2.ConvertToString(fnPropGetter());
                             }
                             catch { }
                             return str;
                         },
                         fnPropDefaultVal);
            }
        }

        public static T MaxBy<T>(this IEnumerable<T> self, Func<T, decimal> selector)
        {
            var max     = Decimal.MinValue;
            var maxItem = default(T);
            foreach(var item in self)
            {
                var val = selector(item);
                if (val > max)
                {
                    max = val;
                    maxItem = item;
                }
            }
            return maxItem;
        }

        public static Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> FromType(object instance, Action<string, Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>>> fnEditSubTypeAction = null)
        {
            int nIndex = 0;
            var instanceType = instance.GetType();
            var oRet = new Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>>();

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(instanceType))
            {
                var dict = instance as System.Collections.IDictionary;
                foreach (System.Collections.DictionaryEntry prop in dict)
                {
                    var nPage = nIndex / ItemsPerPage;
                    var nPageIndex = nIndex % ItemsPerPage;
                    var conv2 = TypeDescriptor.GetConverter(prop.Key.GetType());
                    oRet.AddTypeProperty(nPage,
                                         FromInt32(nPageIndex),
                                         prop.Value.GetType(),
                                         $"KeyValue [{conv2.ConvertToString(prop.Key)}]",
                                         o => { dict[prop.Key] = o; return true; },
                                         () => prop.Value,
                                         null,
                                         fnEditSubTypeAction);
                    nIndex++;
                }
            }
            else if (typeof(System.Collections.IList).IsAssignableFrom(instanceType))
            {
                var list = instance as System.Collections.IList;
                for (nIndex = 0; nIndex < list.Count; nIndex++)
                {
                    var nPage = nIndex / ItemsPerPage;
                    var nPageIndex = nIndex % ItemsPerPage;
                    oRet.AddTypeProperty(nPage,
                                         FromInt32(nPageIndex),
                                         list[nIndex].GetType(),
                                         $"Item [{nIndex}]",
                                         o => { list[nIndex] = o; return true; },
                                         () => list[nIndex],
                                         null,
                                         fnEditSubTypeAction);
                }
            }
            else if (typeof(System.Array).IsAssignableFrom(instanceType))
            {
                var list = instance as System.Array;
                for (nIndex = 0; nIndex < list.Length; nIndex++)
                {
                    var nPage = nIndex / ItemsPerPage;
                    var nPageIndex = nIndex % ItemsPerPage;
                    oRet.AddTypeProperty(nPage,
                                         FromInt32(nPageIndex),
                                         list.GetValue(nIndex).GetType(),
                                         $"Item [{nIndex}]",
                                         o => { list.SetValue(o, nIndex); return true; },
                                         () => list.GetValue(nIndex),
                                         null,
                                         fnEditSubTypeAction);
                    nIndex++;
                }
            }
            // Handle all objects primitive or classes that have "normal" properties
            // that should be enumerated by reflection
            else
            {
                var props = instanceType.GetProperties().OrderBy(p => p.Name);
                var propCount = props.Count();
                foreach (var prop in props)
                {
                    var nPage = nIndex / ItemsPerPage;
                    var nPageIndex = nIndex % ItemsPerPage;
                    oRet.AddTypeProperty(nPage,
                                         FromInt32(nPageIndex),
                                         prop.PropertyType,
                                         prop.Name,
                                         o => { prop.SetValue(instance, o); return true; },
                                         () => prop.GetValue(instance),
                                         () => prop.GetCustomAttributes(typeof(DefaultValueAttribute), false).Cast<DefaultValueAttribute>().SingleOrDefault()?.Value,
                                         fnEditSubTypeAction);
                    nIndex++;
                }
            }
            return oRet;
        }

        public static KeyGesture FromInt32(int nIndex)
        {
            var key = new KeyGesture(Native.VirtualKeys.Noname, ModifierKeys.None);
            if (nIndex >= 0 && nIndex < 10)
                key = new KeyGesture(Native.VirtualKeys.N0 + (ushort)nIndex, ModifierKeys.None);
            if (nIndex >= 10)
                key = new KeyGesture(Native.VirtualKeys.A + ((ushort)(nIndex - 10)));
            return key;
        }

    }

    public enum KeyGestureActionEscapeBehavior
    {
        Ignore,
        CloseWindow,
        CloseApplication,
    }

    public class KeyGestureActionWindow : Window
    {
        //private readonly TextBlock _textBlock;

        public Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> Options { get; private set; }
        int m_nPage = 0;
        public int CurrentPage
        {
            get { return m_nPage; }
            set
            {
                if (value < 0)
                    m_nPage = 0;
                else if (value >= Options.Count)
                    m_nPage = Math.Max(0, Options.Count - 1);
                else
                    m_nPage = value;
            }
        }

        public KeyGestureActionEscapeBehavior EscapeBehavior { get; set; } = KeyGestureActionEscapeBehavior.Ignore;

        public Dictionary<KeyGesture, KeyGestureButton> ShortcutButtons { get; private set; } = new Dictionary<KeyGesture, KeyGestureButton>();



        KeyGestureActionWindow(Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> items, KeyGestureActionEscapeBehavior eEscapeBehavior = KeyGestureActionEscapeBehavior.Ignore)
            :base()
        {
            AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown));
            Options = items;
            EscapeBehavior = eEscapeBehavior;
            //Panel panel = new Panel();
            //_textBlock = new TextBlock();
            //_textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            //_textBlock.VerticalAlignment = VerticalAlignment.Center;
            //_textBlock.Margin = new Thickness(1);
            //panel.XChildren.Add(_textBlock);

            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(GridUnitType.Star, 1) });
            var longestItem = Options[CurrentPage].MaxBy(o => $"{o.Key.DisplayString} -  {o.Value.FieldName}".Length);
            var nMaxBtnWidth = $"{longestItem.Key.DisplayString} -  {longestItem.Value.FieldName}".Length;
            foreach (var opt in Options[CurrentPage])
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(GridUnitType.Star, 1) });
                var button = new KeyGestureButton();
                //button.Margin = new Thickness(4, 0, 4, 0);
                button.Width = nMaxBtnWidth + 10;
                button.HorizontalAlignment = HorizontalAlignment.Left;
                button.Caption = $"{opt.Key.DisplayString} -  {opt.Value.FieldName}";
                //button.Command = new RelayCommand(o => ButtonKeyPressed(o as KeyGesture));
                //button.CommandParameter = opt.Key;
                button.ShortcutKey = opt.Key;
                button.OnClick += (o, e) => ButtonKeyPressed(opt.Key);
                grid.Controls.Add(button);
                ShortcutButtons.Add(opt.Key, button);
            }

             //panel.XChildren.Add(grid);
            //panel.HorizontalAlignment = HorizontalAlignment.Center;
            //panel.VerticalAlignment = VerticalAlignment.Bottom;
            this.Content = grid;
        }

        void ButtonKeyPressed(/*KeyEventArgs args*/KeyGesture kg)
        {
            //var kg = KeyGesture.FromKeyEventArgs(args);
            if (Options.ContainsKey(CurrentPage) && Options[CurrentPage].ContainsKey(kg))
            {
                if (Options[CurrentPage][kg].RequiresConsoleInput)
                {

                }
                else
                {
                    Options[CurrentPage][kg].FieldSetter(String.Empty);
                }
            }
            else if (Options.ContainsKey(-1) && Options[-1].ContainsKey(kg))
            {
                if (Options[-1][kg].RequiresConsoleInput)
                {

                }
                else
                {
                    Options[-1][kg].FieldSetter(String.Empty);
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            var kg = KeyGesture.FromKeyEventArgs(args);
            if (ShortcutButtons.ContainsKey(kg))
            {
                var ctl = ShortcutButtons[kg];
                ConsoleApplication.Instance.FocusManager.SetFocus(ctl);
                var kevargs = new KeyEventArgs(ctl, Button.KeyDownEvent)
                              {
                                  bKeyDown          = args.bKeyDown,
                                  dwControlKeyState = args.dwControlKeyState,
                                  Handled           = args.Handled,
                                  UnicodeChar       = args.UnicodeChar,
                                  wRepeatCount      = args.wRepeatCount,
                                  wVirtualKeyCode   = args.wVirtualKeyCode,
                                  wVirtualScanCode  = args.wVirtualScanCode,
                              };
                ctl.RaiseEvent(Button.KeyDownEvent, kevargs);
                args.Handled = true;
            }
            else if(args.wVirtualKeyCode == VirtualKeys.Escape)
            {
                if ((EscapeBehavior == KeyGestureActionEscapeBehavior.CloseWindow) || (EscapeBehavior == KeyGestureActionEscapeBehavior.CloseApplication))
                {
                    Close();
                    args.Handled = true;
                }
                //else if(EscapeBehavior == KeyGestureActionEscapeBehavior.CloseApplication)
                //{
                //    ConsoleApplication.Instance.Exit();
                //    args.Handled = true;
                //}
            }
        }

        //public string Text
        //{
        //    get { return _textBlock.Text; }
        //    set { _textBlock.Text = value; }
        //}

        public static void Show(string title/*, string text*/, Dictionary<int, Dictionary<KeyGesture, KeyGestureOption>> items, KeyGestureActionEscapeBehavior eEscapeClosesWindow = KeyGestureActionEscapeBehavior.Ignore)
        {
            Control rootControl = ConsoleApplication.Instance.RootControl;
            if (!(rootControl is WindowsHost))
                throw new InvalidOperationException("Default windows host not found, create MessageBox manually");
            WindowsHost windowsHost = (WindowsHost)rootControl;
            KeyGestureActionWindow wndPaged = new KeyGestureActionWindow(items, eEscapeClosesWindow);
            wndPaged.Title = title;
//            wndPaged.
            //wndPaged.Text = text;
            
            wndPaged.AddHandler(ClosedEvent, new EventHandler((sender, args) =>
            {
                if (wndPaged.EscapeBehavior == KeyGestureActionEscapeBehavior.CloseApplication)
                {
                    ConsoleApplication.Instance.Exit();
                }
            }));
            
            windowsHost.ShowModal(wndPaged);
        }

    }
}
