// Copyright 2021-2022 The SeedV Lab.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text;
using UnityEngine;

namespace ClayConsole {
  internal class InputManager {
    private const int _maxBufferSize = 65536;
    private BaseScreen _screen;
    private (int row, int col) _lineStart = (0, 0);
    private StringBuilder _lineBuffer = new StringBuilder();
    Func<string, bool> _readLineCallback = null;

    public bool Active { get; set; } = false;

    public InputManager(BaseScreen screen) {
      _screen = screen;
      _screen.CursorVisible = false;
    }

    public void ClearBuffer() {
      _lineBuffer.Clear();
    }

    public void StartReadLineLoop(Func<string, bool> readLineCallback) {
      if (!Active) {
        _screen.CursorVisible = true;
        _readLineCallback = readLineCallback;
        _lineBuffer.Clear();
        _lineStart = (_screen.CursorRow, _screen.CursorCol);
        Active = true;
      }
    }

    public void StopReadLineLoop() {
      _screen.CursorVisible = false;
      Active = false;
      _lineBuffer.Clear();
    }

    public void OnKeyInput(char c, ControlKey controlKey) {
      Debug.Assert(Active);
      if (c != '\0' && controlKey == ControlKey.None) {
        if (_lineStart.row == 0 &&
            _screen.CursorRow == _screen.Rows - 1 &&
            _screen.CursorCol == _screen.Cols - 1) {
          return;
        }
        _screen.WriteChar(c, out int lineScrolled);
        _lineStart = (_lineStart.row - lineScrolled, _lineStart.col);
        if (_screen._charset.IsNewline(c)) {
          if (!(_readLineCallback is null) && !_readLineCallback(_lineBuffer.ToString())) {
            StopReadLineLoop();
          } else {
            _lineStart = (_screen.CursorRow, _screen.CursorCol);
            _lineBuffer.Clear();
          }
        } else {
          _lineBuffer.Append(c);
        }
      } else if (c == '\0') {
        switch (controlKey) {
          case ControlKey.Backspace:
            Backspace();
            break;
        }
      }
    }

    private void Backspace() {
      if (_lineBuffer.Length > 0) {
        _lineBuffer.Remove(_lineBuffer.Length - 1, 1);
        _screen.MoveCursorToPrev();
        _screen.DeleteChar(_screen.CursorRow, _screen.CursorCol);
      }
    }
  }
}
