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
    [Doc("Print to both the shell and the lab.")]
    [Preserve]
    public void Print(StringValue s)
    {
      string text = s.ToDisplayString();
      GameManager.Instance.MainConsole.WriteLine(text, Color.cyan);
      GameManager.Instance.Lab.Rain(text);
    }

    [Doc("Same as Print().")]
    [Preserve]
    public void Rain(StringValue s)
    {
      Print(s);
    }

    [Doc("Fire a string to the lab.")]
    [Preserve]
    public void Fire(StringValue s)
    {
      string text = s.ToDisplayString();
      GameManager.Instance.MainConsole.WriteLine(text, Color.cyan);
      GameManager.Instance.Lab.Fire(text);
    }
}

public class GameManager : MonoBehaviour {
  private const string _helpString =
@"See https://github.com/wixette/isb for more info. Or, try the following statements:

Game.Print(""Hello, World!"");

Game.Fire(123);

For i = 1 to 5
  Game.Print(Math.Sin(i))
EndFor
";
  private bool _quitting = false;

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
    MainConsole.Screen.Rows = 30;
    Lab.Screen = MainConsole.Screen;
    _engine = new Engine("ClayProgram", new Type[] { typeof(Game) });
    _multiLineCode = new List<string>();
    MainConsole.WriteLine("Welcome to Interactive Small Basic", Color.gray);
    MainConsole.Write("] ");
    MainConsole.StartReadLineLoop(onReadLine);
  }

  void Update() {
    if (_quitting) {
      Application.Quit();
    }
  }

  bool onReadLine(string line) {
    if (!_inMultilineMode && line == "clear") {
      _engine.Reset();
      ShowPromot();
    } else if (!_inMultilineMode && line == "help") {
      MainConsole.WriteLine(_helpString, Color.green);
      ShowPromot();
    } else if (!_inMultilineMode && line == "list") {
      MainConsole.WriteLine(string.Join("\n", _engine.CodeLines), Color.white);
      ShowPromot();
    } else if (!_inMultilineMode && line == "quit") {
      Debug.Log("Quiting");
      _quitting = true;
    } else {
      string code = _inMultilineMode ? string.Join("\n", _multiLineCode) + "\n" + line : line;
      if (!_engine.Compile(code, false) && _engine.ErrorInfo.Contents.Count > 0) {
        if (_engine.ErrorInfo.Contents[_engine.ErrorInfo.Contents.Count - 1].Code ==
            Diagnostic.ErrorCode.UnexpectedEndOfStream) {
          _multiLineCode.Add(line);
          ShowPromot();
        } else {
          ReportErrors(_engine);
          _multiLineCode.Clear();
          ShowPromot();
        }
      } else {
        _multiLineCode.Clear();

        Action<bool> doneCallback = (isSuccess) => {
          if (!isSuccess) {
            ReportErrors(_engine);
            _engine.Reset();
          } else if (_engine.StackCount > 0) {
            BaseValue value = _engine.StackPop();
            MainConsole.WriteLine(value.ToDisplayString(), Color.cyan);
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
