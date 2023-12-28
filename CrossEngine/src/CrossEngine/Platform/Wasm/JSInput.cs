using CrossEngine.Inputs;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CrossEngine.Platform.Wasm
{
    public class JSInput
    {
        public static Key TranslateKey(string code)
        {
            switch (code)
            {
                case "Pause": return Key.Pause;
                case "Backspace": return Key.Backspace;
                case "Tab": return Key.Tab;
                case "NumLock": return Key.NumLock;
                case "Enter": return Key.Enter;
                case "ShiftLeft": return Key.LeftShift;
                case "ShiftRight": return Key.RightShift;
                case "ControlLeft": return Key.LeftControl;
                case "ControlRight": return Key.RightControl;
                case "AltLeft": return Key.LeftAlt;
                case "AltRight": return Key.RightAlt;
                case "CapsLock": return Key.CapsLock;
                //case "Lang1": return Key.Unknown;
                //case "Lang2": return Key.Unknown;
                case "Escape": return Key.Escape;
                case "Space": return Key.Space;
                case "ArrowLeft": return Key.ArrowLeft;
                case "ArrowUp": return Key.ArrowUp;
                case "ArrowRight": return Key.ArrowRight;
                case "ArrowDown": return Key.ArrowDown;
                case "NumpadDecimal": return Key.NumpadDecimal;
                case "Digit0": return Key.Alpha0;
                case "Digit1": return Key.Alpha1;
                case "Digit2": return Key.Alpha2;
                case "Digit3": return Key.Alpha3;
                case "Digit4": return Key.Alpha4;
                case "Digit5": return Key.Alpha5;
                case "Digit6": return Key.Alpha6;
                case "Digit7": return Key.Alpha7;
                case "Digit8": return Key.Alpha8;
                case "Digit9": return Key.Alpha9;
                case "Period": return Key.Period;
                case "Semicolon": return Key.Semicolon;
                //case "Backquote": return Key.Unknown;
                case "Equal": return Key.Equal;
                case "Minus": return Key.Minus;
                case "KeyA": return Key.A;
                case "KeyB": return Key.B;
                case "KeyC": return Key.C;
                case "KeyD": return Key.D;
                case "KeyE": return Key.E;
                case "KeyF": return Key.F;
                case "KeyG": return Key.G;
                case "KeyH": return Key.H;
                case "KeyI": return Key.I;
                case "KeyJ": return Key.J;
                case "KeyK": return Key.K;
                case "KeyL": return Key.L;
                case "KeyM": return Key.M;
                case "KeyN": return Key.N;
                case "KeyO": return Key.O;
                case "KeyP": return Key.P;
                case "KeyQ": return Key.Q;
                case "KeyR": return Key.R;
                case "KeyS": return Key.S;
                case "KeyT": return Key.T;
                case "KeyU": return Key.U;
                case "KeyV": return Key.V;
                case "KeyW": return Key.W;
                case "KeyX": return Key.X;
                case "KeyY": return Key.Y;
                case "KeyZ": return Key.Z;
                case "MetaLeft": return Key.LeftSuper;
                case "MetaRight": return Key.RightSuper;
                case "ContextMenu": return Key.Menu;
                case "Numpad0": return Key.Numpad0;
                case "Numpad1": return Key.Numpad1;
                case "Numpad2": return Key.Numpad2;
                case "Numpad3": return Key.Numpad3;
                case "Numpad4": return Key.Numpad4;
                case "Numpad5": return Key.Numpad5;
                case "Numpad6": return Key.Numpad6;
                case "Numpad7": return Key.Numpad7;
                case "Numpad8": return Key.Numpad8;
                case "Numpad9": return Key.Numpad9;
                case "NumpadMultiply": return Key.NumpadMultiply;
                case "NumpadAdd": return Key.NumpadAdd;
                case "NumpadSubtract": return Key.NumpadSubtract;
                case "NumpadDivide": return Key.NumpadDivide;
                case "F1":  return Key.F1;
                case "F2":  return Key.F2;
                case "F3":  return Key.F3;
                case "F4":  return Key.F4;
                case "F5":  return Key.F5;
                case "F6":  return Key.F6;
                case "F7":  return Key.F7;
                case "F8":  return Key.F8;
                case "F9":  return Key.F9;
                case "F10": return Key.F10;
                case "F11": return Key.F11;
                case "F12": return Key.F12;
                case "F13": return Key.F13;
                case "F14": return Key.F14;
                case "F15": return Key.F15;
                case "F16": return Key.F16;
                case "F17": return Key.F17;
                case "F18": return Key.F18;
                case "F19": return Key.F19;
                case "F20": return Key.F20;
                case "F21": return Key.F21;
                case "F22": return Key.F22;
                case "F23": return Key.F23;
                case "F24": return Key.F24;
                case "F25": return Key.F25;
                //case "F26": return Key.F26;
                //case "F27": return Key.F27;
                //case "F28": return Key.F28;
                //case "F29": return Key.F29;
                //case "F30": return Key.F30;
                //case "F31": return Key.F31;
                //case "F32": return Key.F32;
                case "ScrollLock": return Key.ScrollLock;
                case "BracketLeft": return Key.LeftBracket;
                case "BracketRight": return Key.RightBracket;
                case "Backslash": return Key.Backslash;
                case "Quote": return Key.Unknown;
                //case "MediaTrackNext": return Key.Unknown;
                //case "MediaTrackPrevious": return Key.Unknown;
                //case "VolumeMute": return Key.Unknown;
                //case "VolumeDown": return Key.Unknown;
                //case "VolumeUp": return Key.Unknown;
                case "Comma": return Key.Comma;
                case "Slash": return Key.Slash;
                //case "IntlBackslash": return Key.Unknown;
                //case "IntlRo": return Key.Unknown;
                //case "NumpadComma": return Key.Unknown;
                //case "OSLeft": return Key.Unknown;
                //case "WakeUp": return Key.Unknown;
                default:
                    {
                        Logging.Log.Default.Warn($"Unknown key code '{code}'");
                        Debug.Assert(false, $"Unknown key code '{code}'");
                        return Key.Unknown;
                    }
            }
        }

        public static Button TranslateMouse(int button)
        {
            switch (button)
            {
                case 0: return Button.Left;
                case 1: return Button.Middle;
                case 2: return Button.Right;
                case 3: return Button.Back;
                case 4: return Button.Forward;
                default:
                    {
                        Logging.Log.Default.Warn($"Unknown mouse button '{button}'");
                        Debug.Assert(false, $"Unknown mouse button '{button}'");
                        return Button.Unknown;
                    }
            }
        }
    }
}
