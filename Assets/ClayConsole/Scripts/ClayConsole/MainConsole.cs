// Copyright 2021 The Aha001 Team.
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
using UnityEngine;

namespace ClayConsole {
  // TODO: support more kinds of screens.
  public enum ScreenType {
    FlatScreen,
  }

  // The base class of all kinds of Consoles.
  public class MainConsole : MonoBehaviour {
    private readonly IKeyboardInput _keyboard = new EnUsKeyboardInput();
    private InputManager _inputManager = null;

    public ScreenType ScreenType = ScreenType.FlatScreen;

    public BaseScreen Screen { get; private set; }

    public void Write(string s) {
      Write(s, BaseScreen._defaultColor);
    }

    public void Write(string s, Color color) {
      if (!string.IsNullOrEmpty(s)) {
        foreach (char c in s) {
          Screen.WriteChar(c, color, out int _);
        }
      }
    }

    public void WriteLine(string s) {
      WriteLine(s, BaseScreen._defaultColor);
    }

    public void WriteLine(string s, Color color) {
      Write(s + '\n', color);
    }

    public void StartReadLineLoop(Func<string, bool> readLineCallback) {
      _inputManager.StartReadLineLoop(readLineCallback);
    }

    public void StopReadLineLoop() {
      _inputManager.StopReadLineLoop();
    }

    void Awake() {
      switch (ScreenType) {
        case ScreenType.FlatScreen:
          Screen = new FlatScreen(gameObject);
          break;
        default:
          throw new NotSupportedException($"Screen type {ScreenType} is not supported yet.");
      }
      _inputManager = new InputManager(Screen);
    }

    void OnGUI() {
      if (_inputManager.Active && Event.current.isKey && Event.current.type == EventType.KeyDown &&
          _keyboard.TryConvertKeyCode(Event.current, out char c, out ControlKey controlKey)) {
        _inputManager.OnKeyInput(c, controlKey);
      }
    }
  }
}
