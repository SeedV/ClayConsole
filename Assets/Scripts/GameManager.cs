using UnityEngine;
using ClayConsole;

public class GameManager : MonoBehaviour {
  public MainConsole MainConsole;

  void Start() {
    MainConsole.Write("Hello, World!");
  }
}
