using System;
using ConsoleFramework.Native;
using Xaml;

namespace ConsoleFramework.Events
{
    [TypeConverter( typeof ( KeyGestureConverter ) )]
    public class KeyGesture : IEquatable<KeyGesture>
    {
        private readonly string _displayString;
        private readonly VirtualKeys _key;
        private static readonly ITypeConverter _keyGestureConverter = new KeyGestureConverter( );
        private readonly ModifierKeys _modifiers;

        public KeyGesture( VirtualKeys key ) : this( key, ModifierKeys.None ) {
        }

        public KeyGesture( VirtualKeys key, ModifierKeys modifiers ) : this( key, modifiers, string.Empty ) {
        }

        public KeyGesture( VirtualKeys key, ModifierKeys modifiers, string displayString ) {
            if ( displayString == null ) throw new ArgumentNullException( "displayString" );
            if ( !IsValid( key, modifiers ) )
                throw new InvalidOperationException( "KeyGesture is invalid" );
            this._modifiers = modifiers;
            this._key = key;
            this._displayString = displayString;
        }

        string GetDisplayString( ) {
            if ( !string.IsNullOrEmpty( this._displayString ) ) {
                return this._displayString;
            }
            return ( string ) _keyGestureConverter.ConvertTo( this, typeof ( string ) );
        }

        // todo : check incompatible combinations
        internal static bool IsValid( VirtualKeys key, ModifierKeys modifiers ) {
            return true;
        }

        public bool Matches( KeyEventArgs args ) {
            VirtualKeys wVirtualKeyCode = args.wVirtualKeyCode;
            if ( this.Key != wVirtualKeyCode ) return false;
            ControlKeyState controlKeyState = args.dwControlKeyState;
            ModifierKeys modifierKeys = this.Modifiers;

            // Проверяем все возможные модификаторы по очереди

            if ( ( modifierKeys & ModifierKeys.Alt ) != 0 ) {
                if ( ( controlKeyState & ( ControlKeyState.LEFT_ALT_PRESSED
                                           | ControlKeyState.RIGHT_ALT_PRESSED ) ) == 0 ) {
                    // Должен быть взведён один из флагов, показывающих нажатие Alt, а его нет
                    return false;
                }
            } else {
                if ( ( controlKeyState & ( ControlKeyState.LEFT_ALT_PRESSED
                                           | ControlKeyState.RIGHT_ALT_PRESSED ) ) != 0 ) {
                    // Не должно быть взведено ни одного флага, показывающего нажатие Alt,
                    // а на самом деле - флаг стоит
                    return false;
                }
            }

            if ( ( modifierKeys & ModifierKeys.Control ) != 0 ) {
                if ( ( controlKeyState & ( ControlKeyState.LEFT_CTRL_PRESSED
                                           | ControlKeyState.RIGHT_CTRL_PRESSED ) ) == 0 ) {
                    return false;
                }
            } else {
                if ( ( controlKeyState & ( ControlKeyState.LEFT_CTRL_PRESSED
                                           | ControlKeyState.RIGHT_CTRL_PRESSED ) ) != 0 ) {
                    return false;
                }
            }

            if ( ( modifierKeys & ModifierKeys.Shift ) != 0 ) {
                if ( ( controlKeyState & ( ControlKeyState.SHIFT_PRESSED ) ) == 0 ) {
                    return false;
                }
            } else {
                if ( ( controlKeyState & ( ControlKeyState.SHIFT_PRESSED ) ) != 0 ) {
                    return false;
                }
            }

            return true;
        }

        public static KeyGesture FromKeyEventArgs(KeyEventArgs args)
        {
            VirtualKeys  key   = args.wVirtualKeyCode;
            ModifierKeys modif = ModifierKeys.None;
            if(((args.dwControlKeyState & ControlKeyState.LEFT_ALT_PRESSED) == ControlKeyState.LEFT_ALT_PRESSED) ||
               ((args.dwControlKeyState & ControlKeyState.RIGHT_ALT_PRESSED) == ControlKeyState.RIGHT_ALT_PRESSED))
            {
                modif |= ModifierKeys.Alt;
            }
            if (((args.dwControlKeyState & ControlKeyState.LEFT_CTRL_PRESSED) == ControlKeyState.LEFT_CTRL_PRESSED) ||
               ((args.dwControlKeyState & ControlKeyState.RIGHT_CTRL_PRESSED) == ControlKeyState.RIGHT_CTRL_PRESSED))
            {
                modif |= ModifierKeys.Control;
            }
            if ((args.dwControlKeyState & ControlKeyState.SHIFT_PRESSED) == ControlKeyState.SHIFT_PRESSED)
            {
                modif |= ModifierKeys.Shift;
            }
            return new KeyGesture(key, modif);
        }

        public string DisplayString {
            get { return GetDisplayString(); }
        }

        public VirtualKeys Key {
            get { return this._key; }
        }

        public ModifierKeys Modifiers {
            get { return this._modifiers; }
        }

        public override bool Equals(object obj)
        {
            bool fRet = false;
            if ((obj != null) && (obj is KeyGesture))
            {
                fRet = this.Equals(obj as KeyGesture);
            }
            return fRet;
        }

        public bool Equals(KeyGesture other)
        {
            bool fRet = false;
            if ((other != null) &&
                (other.Key == this.Key) &&
                (other.Modifiers == this.Modifiers))
            {
                fRet = true;
            }
            return fRet;
        }
        /*
        public static bool operator==(KeyGesture x, KeyGesture y)
        {
            if ((x == null) && (y == null))
                return true;
            else if ((x == null) || (y == null))
                return false;
            else             
                return x.Equals(y);
        }

        public static bool operator !=(KeyGesture x, KeyGesture y)
        {
            if ((x == null) && (y == null))
                return false;
            else if ((x == null) || (y == null))
                return true;
            else
                return !x.Equals(y);
        }
        */
        static int MergeHashCodes(params int[] hashCodes)
        {
            unchecked
            {
                int hash = 17;
                foreach (var h in hashCodes)
                {
                    hash = hash * 31 + h;
                }
                return hash;
            }

        }
        public override int GetHashCode()
        {
            //return base.GetHashCode();
            return MergeHashCodes(Key.GetHashCode(), Modifiers.GetHashCode());
        }

        public override string ToString()
        {
            return GetDisplayString();
        }
    }
}