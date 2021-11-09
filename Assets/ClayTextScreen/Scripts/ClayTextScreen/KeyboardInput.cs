using UnityEngine;

namespace ClayTextScreen {
  public enum ControlKey
  {
    None = 0,
    Backspace,
    Delete,
    Insert,
    Escape,
    Left,
    Right,
    Up,
    Down,
    Home,
    End,
    PageUp,
    PageDown,
    CtrlA = 'A',
    CtrlB,
    CtrlC,
    CtrlD,
    CtrlE,
    CtrlF,
    CtrlG,
    CtrlH,
    CtrlI,
    CtrlJ,
    CtrlK,
    CtrlL,
    CtrlM,
    CtrlN,
    CtrlO,
    CtrlP,
    CtrlQ,
    CtrlR,
    CtrlS,
    CtrlU,
    CtrlV,
    CtrlW,
    CtrlX,
    CtrlY,
    CtrlZ,
  }

  internal interface IKeyboardInput {
    bool TryConvertKeyCode(Event e, out char c, out ControlKey controlKey);
  }

  internal class EnUsKeyboardInput : IKeyboardInput {
    private static readonly char[] _shiftedAlphaKeys =
        new char[] { ')', '!', '@', '#', '$', '%', '^', '&', '*', '('  };

    public bool TryConvertKeyCode(Event e, out char c, out ControlKey controlKey) {
      Debug.Assert(e.isKey);
      bool ret = true;
      c = '\0';
      controlKey = ControlKey.None;
      switch (e.keyCode) {
        case KeyCode n when (n >= KeyCode.Keypad0 && n <= KeyCode.Keypad9):
          c = (char)('0' + (int)e.keyCode - (int)KeyCode.Keypad0);
          break;
        case KeyCode n when (n >= KeyCode.Alpha0 && n <= KeyCode.Alpha9):
          {
            int i = (int)e.keyCode - (int)KeyCode.Alpha0;
            c = e.shift ? _shiftedAlphaKeys[i] : (char)('0' + i);
          }
          break;
        case KeyCode n when (n >= KeyCode.A && n <= KeyCode.Z):
          {
            int i = (int)e.keyCode - (int)KeyCode.A;
            c = (e.capsLock ^ e.shift) ? (char)('A' + i) : (char)('a' + i);
            if (e.control) {
              controlKey = (ControlKey)((int)ControlKey.CtrlA + i);
            }
          }
          break;
        case KeyCode.KeypadPeriod:
          c = '.';
          break;
        case KeyCode.KeypadDivide:
          c = '/';
          break;
        case KeyCode.KeypadMultiply:
          c = '*';
          break;
        case KeyCode.KeypadMinus:
          c = '-';
          break;
        case KeyCode.KeypadPlus:
          c = '+';
          break;
        case KeyCode.KeypadEnter:
        case KeyCode.Return:
          c = '\n';
          break;
        case KeyCode.BackQuote:
          c = e.shift ? '~' : '`';
          break;
        case KeyCode.Minus:
          c = e.shift ? '_' : '-';
          break;
        case KeyCode.KeypadEquals:
          c = '=';
          break;
        case KeyCode.LeftBracket:
          c = e.shift ? '{' : '[';
          break;
        case KeyCode.RightBracket:
          c = e.shift ? '}' : ']';
          break;
        case KeyCode.Backslash:
          c = e.shift ? '|' : '\\';
          break;
        case KeyCode.Semicolon:
          c = e.shift ? ':' : ';';
          break;
        case KeyCode.Quote:
          c = e.shift ? '\"' : '\'';
          break;
        case KeyCode.Equals:
          c = e.shift ? '+' : '=';
          break;
        case KeyCode.Comma:
          c = e.shift ? '<' : ',';
          break;
        case KeyCode.Period:
          c = e.shift ? '>' : '.';
          break;
        case KeyCode.Slash:
          c = e.shift ? '?' : '/';
          break;
        case KeyCode.Tab:
          c = '\t';
          break;
        case KeyCode.Space:
          c = ' ';
          break;
        case KeyCode.Escape:
          controlKey = ControlKey.Escape;
          break;
        case KeyCode.Backspace:
          controlKey = ControlKey.Backspace;
          break;
        case KeyCode.Delete:
          controlKey = ControlKey.Delete;
          break;
        case KeyCode.UpArrow:
          controlKey = ControlKey.Up;
          break;
        case KeyCode.DownArrow:
          controlKey = ControlKey.Down;
          break;
        case KeyCode.RightArrow:
          controlKey = ControlKey.Right;
          break;
        case KeyCode.LeftArrow:
          controlKey = ControlKey.Left;
          break;
        case KeyCode.Insert:
          controlKey = ControlKey.Insert;
          break;
        case KeyCode.Home:
          controlKey = ControlKey.Home;
          break;
        case KeyCode.End:
          controlKey = ControlKey.End;
          break;
        case KeyCode.PageUp:
          controlKey = ControlKey.PageUp;
          break;
        case KeyCode.PageDown:
          controlKey = ControlKey.PageDown;
          break;
        default:
          ret = false;
          break;
      }
      return ret;
    }
  }
}
