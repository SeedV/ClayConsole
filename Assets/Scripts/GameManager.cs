using UnityEngine;
using ClayTextScreen;

public class GameManager : MonoBehaviour {
  public FlatScreen Screen;

  void Start() {
    Screen.Rows = 25;
    Screen.Cols = 80;
    for (int row = 0; row < Screen.Rows; row++) {
      for (int col = 0; col < Screen.Cols; col++) {
        Screen.PutChar(row, col, col % 2 == 0 ? 'W' : 'y');
      }
    }
  }

  void Update() {
  }
}
