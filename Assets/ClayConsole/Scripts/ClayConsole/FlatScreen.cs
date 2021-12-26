// Copyright 2021-2022 The SeedV Lab.
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

using UnityEngine;

namespace ClayConsole {
  public class FlatScreen : BaseScreen {
    private const int _defaultRows = 15;
    private const int _defaultCols = 40;
    private const float _width = 40f;
    private const float _height = 30f;
    private const float _margin = 1f;

    private Vector2 _origin = new Vector2();
    private Vector2 _padding = new Vector2();
    private Vector2 _charSize = new Vector2();
    private Vector3 _charScale = new Vector3();
    private Vector3 _cursorScale = new Vector3();
    private float _baseLineHeight = 0;

    public FlatScreen(GameObject mainConsole, int rows = _defaultRows, int cols = _defaultCols)
        : base(mainConsole, rows, cols) {
    }

    private protected override BaseCharset InitCharset() {
      return new AsciiCharset();
    }

    private protected override void PlaceGlyphObject(int row, int col, GameObject glyphObject) {
      var o = GetCharTopLeft(row, col);
      glyphObject.transform.localPosition = new Vector3(o.x, o.y - _charSize.y + _baseLineHeight, 0f);
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
      _cursor.transform.localPosition =
          new Vector3(o.x + _charSize.x / 2f, o.y - _charSize.y / 2.5f, 0f);
    }

    private Vector2 GetCharTopLeft(int row, int col) {
      float x = _origin.x + _padding.x + col * (_charSize.x + _padding.x);
      float y = _origin.y - _padding.y - row * (_charSize.y + _padding.y);
      return new Vector2(x, y);
    }
  }
}
