using System;
using System.Text;

namespace ClayConsole {
  internal class InputManager {
    private const int _maxBufferSize = 65536;
    private BaseScreen _screen;
    private StringBuilder _buffer = new StringBuilder();
    private bool _active = false;
    Func<string, bool> _readLineCallback = null;

    public InputManager(BaseScreen screen) {
      _screen = screen;
      _screen.CursorVisible = false;
    }

    public void ClearBuffer() {
      _buffer.Clear();
    }

    public void StartReadLineLoop(Func<string, bool> readLineCallback) {
      if (!_active) {
        _screen.CursorVisible = true;
        _readLineCallback = readLineCallback;
        _buffer.Clear();
        _active = true;
      }
    }

    public void StopReadLineLoop() {
      _screen.CursorVisible = false;
      _active = false;
      _buffer.Clear();
    }

    public void OnKeyInput(char c, ControlKey controlKey) {
      if (_active && c != '\0' && controlKey == ControlKey.None) {
        _screen.WriteChar(c);
        if (_screen._charset.IsNewline(c)) {
          if (!(_readLineCallback is null) && !_readLineCallback(_buffer.ToString())) {
            StopReadLineLoop();
          } else {
            _buffer.Clear();
          }
        } else {
          _buffer.Append(c);
        }
      }
    }
  }
}
