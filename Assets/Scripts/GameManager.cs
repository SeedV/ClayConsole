using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using ClayConsole;
using ISB.Runtime;
using ISB.Utilities;

[Preserve]
public class Game
{
    [Doc("Print to shell.")]
    [Preserve]
    public void Print(StringValue s)
    {
      string text = s.ToString();
      GameManager.Instance.MainConsole.WriteLine(text, Color.cyan);
      GameManager.Instance.Lab.Rain(text);
    }
}

public class GameManager : MonoBehaviour {
  public static GameManager Instance = null;

  public MainConsole MainConsole;
  public Lab Lab;

  private Engine _engine;
  private List<string> _multiLineCode;
  private bool _inMultilineMode => _multiLineCode.Count > 0;

  void Awake() {
    if (Instance == null) {
      Instance = this;
    } else if (Instance != this) {
      Destroy(gameObject);
    }
  }

  void Start() {
    Lab.Screen = MainConsole.Screen;
    _engine = new Engine("ClayProgram", new Type[] { typeof(Game) });
    _multiLineCode = new List<string>();
    MainConsole.WriteLine("Welcome to Interactive Small Basic");
    MainConsole.Write("] ");
    MainConsole.StartReadLineLoop(onReadLine);
  }

  bool onReadLine(string line) {
    if (!_inMultilineMode && line == "clear") {
      _engine.Reset();
    } else if (!_inMultilineMode && line == "help") {
      MainConsole.WriteLine(_engine.LibsHelpString);
    } else if (!_inMultilineMode && line == "list") {
      MainConsole.WriteLine(string.Join("\n", _engine.CodeLines));
    } else {
      string code = _inMultilineMode ? string.Join("\n", _multiLineCode) + "\n" + line : line;
      if (!_engine.Compile(code, false) && _engine.ErrorInfo.Contents.Count > 0) {
        if (_engine.ErrorInfo.Contents[_engine.ErrorInfo.Contents.Count - 1].Code ==
            Diagnostic.ErrorCode.UnexpectedEndOfStream) {
          _multiLineCode.Add(line);
        } else {
          MainConsole.WriteLine(_engine.ErrorInfo.Contents[0].ToDisplayString());
        }
      } else {
        _multiLineCode.Clear();
        if (!_engine.Run(false) && _engine.ErrorInfo.Contents.Count > 0) {
          MainConsole.WriteLine(_engine.ErrorInfo.Contents[0].ToDisplayString());
        }
        if (_engine.StackCount > 0) {
          BaseValue value = _engine.StackPop();
          MainConsole.WriteLine(value.ToDisplayString(), Color.green);
        }
      }
    }
    MainConsole.Write(_inMultilineMode ? "> " : "] ");
    return true;
  }
}
