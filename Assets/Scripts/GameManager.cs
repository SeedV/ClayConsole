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
    MainConsole.WriteLine("Welcome to Interactive Small Basic", Color.gray);
    MainConsole.Write("] ");
    MainConsole.StartReadLineLoop(onReadLine);
  }

  bool onReadLine(string line) {
    if (!_inMultilineMode && line == "clear") {
      _engine.Reset();
      ShowPromot();
    } else if (!_inMultilineMode && line == "help") {
      MainConsole.WriteLine(_engine.LibsHelpString);
      ShowPromot();
    } else if (!_inMultilineMode && line == "list") {
      MainConsole.WriteLine(string.Join("\n", _engine.CodeLines));
      ShowPromot();
    } else {
      string code = _inMultilineMode ? string.Join("\n", _multiLineCode) + "\n" + line : line;
      if (!_engine.Compile(code, false) && _engine.ErrorInfo.Contents.Count > 0) {
        if (_engine.ErrorInfo.Contents[_engine.ErrorInfo.Contents.Count - 1].Code ==
            Diagnostic.ErrorCode.UnexpectedEndOfStream) {
          _multiLineCode.Add(line);
          ShowPromot();
        } else {
          ReportErrors(_engine);
          ShowPromot();
        }
      } else {
        _multiLineCode.Clear();

        Action<bool> doneCallback = (isSuccess) => {
          if (!isSuccess) {
            ReportErrors(_engine);
            _engine.Reset();
          } else if (_engine.StackCount > 0) {
            string ret = _engine.StackTop.ToDisplayString();
            MainConsole.WriteLine(ret, Color.green);
          }
          ShowPromot();
        };
        // Prevents the scripting engine from being stuck in an infinite loop.
        const int maxInstructionsToExecute = 10000;
        Func<int, bool> canContinueCallback =
            (counter) => counter >= maxInstructionsToExecute ? false : true;
        StartCoroutine(_engine.RunAsCoroutine(doneCallback, canContinueCallback, false));
      }
    }
    return true;
  }

  private void ReportErrors(Engine engine) {
    var buffer = new List<string>();
    foreach (var content in engine.ErrorInfo.Contents) {
      buffer.Add(content.ToDisplayString());
    }
    string message = string.Join("\n", buffer);
    Debug.Log(message);
    MainConsole.WriteLine(message, new Color(1f, .5f, 0f));
  }

  private void ShowPromot() {
    MainConsole.Write(_inMultilineMode ? "> " : "] ");
  }
}
