using UnityEngine;

namespace ClayConsole {
  // TODO: support more kinds of screens.
  public enum ScreenType {
    FlatScreen,
  }

  // The base class of all kinds of Consoles.
  public class MainConsole : MonoBehaviour {
    private readonly IKeyboardInput _keyboard = new EnUsKeyboardInput();

    public ScreenType ScreenType = ScreenType.FlatScreen;

    public BaseScreen Screen { get; private set; }

    public int SpacesPerTab { get; set; } = 4;

    public void Write(string s) {
      Write(s, BaseScreen._defaultColor);
    }

    public void Write(string s, Color color) {
      if (!string.IsNullOrEmpty(s)) {
        foreach (char c in s) {
          if (Screen._charset.IsVisible(c) || Screen._charset.IsSpace(c)) {
            Screen.PutChar(Screen.CursorRow, Screen.CursorCol, c, color);
            MoveCursorToNext();
          } else if (Screen._charset.IsNewline(c)) {
            MoveCursorToNewline();
          } else if (Screen._charset.IsTab(c)) {
            for (int i = 0; i < SpacesPerTab; i++) {
              Screen.PutChar(Screen.CursorRow, Screen.CursorCol, ' ');
              MoveCursorToNext();
            }
          }
        }
      }
    }

    public void WriteLine(string s) {
      WriteLine(s, BaseScreen._defaultColor);
    }

    public void WriteLine(string s, Color color) {
      Write(s + '\n', color);
    }

    public char Read() {
      return '\0';
    }

    public string ReadLine() {
      return null;
    }

    void Awake() {
      switch (ScreenType) {
        case ScreenType.FlatScreen:
          Screen = new FlatScreen(gameObject);
          break;
        default:
          throw new System.NotSupportedException($"Screen type {ScreenType} is not supported yet.");
      }
    }

    void OnGUI() {
      if (Event.current.type == EventType.KeyDown &&
          _keyboard.TryConvertKeyCode(Event.current, out char c, out ControlKey controlKey)) {
        if (c != '\0') {
          Write(c.ToString());
        }
      }
    }

    private void MoveCursorToNext() {
      if (Screen.CursorCol < Screen.Cols - 1) {
        Screen.CursorCol++;
      } else if (Screen.CursorRow < Screen.Rows - 1) {
        Screen.CursorCol = 0;
        Screen.CursorRow++;
      } else {
        Screen.Scroll(1);
        Screen.CursorCol = 0;
      }
    }

    private void MoveCursorToNewline() {
      if (Screen.CursorRow < Screen.Rows - 1) {
        Screen.CursorCol = 0;
        Screen.CursorRow++;
      } else {
        Screen.Scroll(1);
        Screen.CursorCol = 0;
      }
    }
  }
}
