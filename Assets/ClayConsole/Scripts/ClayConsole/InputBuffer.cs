using System.Text;
using UnityEngine;

namespace ClayConsole {
  internal class InputBuffer {
    private StringBuilder _textBuffer = new StringBuilder();

    public void Clear() {
      _textBuffer.Clear();
    }

    public void Push(char c, ControlKey controlKey) {
    }
  }
}
