using UnityEngine;

namespace ClayConsole {
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
      bool shiftPressed = e.shift || e.modifiers == EventModifiers.Shift;
      bool ctrlPressed = e.control || e.modifiers == EventModifiers.Control;
      bool capsPressed = e.capsLock || e.modifiers == EventModifiers.CapsLock;
      switch (e.keyCode) {
        case KeyCode n when (n >= KeyCode.Keypad0 && n <= KeyCode.Keypad9):
          c = (char)('0' + (int)e.keyCode - (int)KeyCode.Keypad0);
          break;
        case KeyCode n when (n >= KeyCode.Alpha0 && n <= KeyCode.Alpha9):
          {
            int i = (int)e.keyCode - (int)KeyCode.Alpha0;
            c = shiftPressed ? _shiftedAlphaKeys[i] : (char)('0' + i);
          }
          break;
        case KeyCode n when (n >= KeyCode.A && n <= KeyCode.Z):
          {
            int i = (int)e.keyCode - (int)KeyCode.A;
            c = (capsPressed || shiftPressed) ? (char)('A' + i) : (char)('a' + i);
            if (ctrlPressed) {
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
        case KeyCode.Asterisk:
          c = '*';
          break;
        case KeyCode.KeypadMinus:
          c = '-';
          break;
        case KeyCode.KeypadPlus:
        case KeyCode.Plus:
          c = '+';
          break;
        case KeyCode.KeypadEnter:
        case KeyCode.Return:
          c = '\n';
          break;
        case KeyCode.BackQuote:
          c = shiftPressed ? '~' : '`';
          break;
        case KeyCode.Minus:
          c = shiftPressed ? '_' : '-';
          break;
        case KeyCode.KeypadEquals:
          c = '=';
          break;
        case KeyCode.LeftBracket:
          c = shiftPressed ? '{' : '[';
          break;
        case KeyCode.RightBracket:
          c = shiftPressed ? '}' : ']';
          break;
        case KeyCode.Backslash:
          c = shiftPressed ? '|' : '\\';
          break;
        case KeyCode.Semicolon:
          c = shiftPressed ? ':' : ';';
          break;
        case KeyCode.Quote:
          c = shiftPressed ? '\"' : '\'';
          break;
        case KeyCode.Equals:
          c = shiftPressed ? '+' : '=';
          break;
        case KeyCode.Comma:
          c = shiftPressed ? '<' : ',';
          break;
        case KeyCode.Period:
          c = shiftPressed ? '>' : '.';
          break;
        case KeyCode.Slash:
          c = shiftPressed ? '?' : '/';
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
        case KeyCode.None:
          ret = false;
          break;
        default:
          Debug.Log($"Unsupported keyCode ${e.keyCode}");
          ret = false;
          break;
      }
      return ret;
    }
  }
}
