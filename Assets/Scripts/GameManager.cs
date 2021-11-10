using System;
using UnityEngine;
using ClayConsole;

public class GameManager : MonoBehaviour {
  public MainConsole MainConsole;

  void Start() {
    MainConsole.WriteLine("Hello, World!");
    MainConsole.Write("] ");
    MainConsole.StartReadLineLoop(onReadLine);
  }

  bool onReadLine(string line) {
    Debug.Log(line);
    if (line == "quit") {
      return false;
    } else {
      MainConsole.Write("] ");
      return true;
    }
  }
}
