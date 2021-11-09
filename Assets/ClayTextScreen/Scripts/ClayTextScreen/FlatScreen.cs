using UnityEngine;

namespace ClayTextScreen {
  public class FlatScreen : BaseScreen {
    private const float _width = 40f;
    private const float _height = 30f;
    private const float _margin = 1f;

    private Vector2 _origin = new Vector2();
    private Vector2 _padding = new Vector2();
    private Vector2 _charSize = new Vector2();
    private Vector3 _charScale = new Vector3();
    private Vector3 _cursorScale = new Vector3();
    private float _baseLineHeight = 0;

    private protected override BaseCharset InitCharset() {
      return new AsciiCharset();
    }

    private protected override IKeyboardInput InitKeyboard() {
      return new EnUsKeyboardInput();
    }

    private protected override void PlaceGlyphObject(int row, int col, GameObject glyphObject) {
      var o = GetCharTopLeft(row, col);
      glyphObject.transform.position = new Vector3(o.x, o.y - _charSize.y + _baseLineHeight, 0f);
      glyphObject.transform.localScale = _charScale;
    }

    private protected override void OnUpdateSize(int row, int col) {
      _origin.x = -_width / 2f + _margin;
      _origin.y = _height / 2f - _margin;
      _padding.x = (_width - 2 * _margin) / Cols / 20f;
      _padding.y = (_height - 2 * _margin) / Rows / 5f;
      _charSize.x = (_width - 2 * _margin - (_padding.x * (Cols + 1))) / Cols;
      _charSize.y = (_height - 2 * _margin - (_padding.y * (Rows + 1))) / Rows;
      _charScale.x = 100f * _charSize.x;
      _charScale.y = (_height / _width) * 100f * _charSize.y;
      _charScale.z = 200f;
      _baseLineHeight = _charSize.y * .25f;
      _cursorScale.x = _charSize.x;
      _cursorScale.y = _charSize.y;
      _cursorScale.z = .3f;
      _cursor.transform.localScale = _cursorScale;
    }

    private protected override void OnUpdateCursorPos(int row, int col) {
      var o = GetCharTopLeft(row, col);
      _cursor.transform.position =
          new Vector3(o.x + _charSize.x / 2f, o.y - _charSize.y / 2.5f, 0f);
    }

    private Vector2 GetCharTopLeft(int row, int col) {
      float x = _origin.x + _padding.x + col * (_charSize.x + _padding.x);
      float y = _origin.y - _padding.y - row * (_charSize.y + _padding.y);
      return new Vector2(x, y);
    }
  }
}
