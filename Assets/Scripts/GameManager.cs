using UnityEngine;
using ClayTextScreen;

public class GameManager : MonoBehaviour {
  public FlatScreen Screen;

  void Start() {
    Screen.Write("Hello, World!");
  }

  void Update() {
  }
}
