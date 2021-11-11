using System.Collections.Generic;
using UnityEngine;

namespace ClayConsole {
  // The base class of all kinds of screens.
  public abstract class BaseScreen {
    internal static readonly Color _defaultColor = Color.yellow;

    private const string _screenObjectName = "Screen";
    private const string _cursorObjectName = "Cursor";
    private const string _glyphsObjectNamePrefix = "Glyphs/";
    private const string _mainColor = "_Color";
    private const int _minRows = 10;
    private const int _maxRows = 50;
    private const int _minCols = 10;
    private const int _maxCols = 100;

    internal BaseCharset _charset = null;
    internal IKeyboardInput _keyboard = null;

    protected GameObject _mainConsole = null;
    protected GameObject _screen = null;
    protected GameObject _cursor = null;
    protected readonly Dictionary<uint, GameObject> _glyphs =
      new Dictionary<uint, GameObject>();
    protected readonly Dictionary<(int row, int col), (char c, GameObject glyphObject)>
        _buffer = new Dictionary<(int row, int col), (char c, GameObject glyphObject)>();

    private int _rows = 0;
    private int _cols = 0;
    private int _cursorRow = 0;
    private int _cursorCol = 0;

    public int Rows {
      get {
        return _rows;
      }
      set {
        if (value >= _minRows && value <= _maxRows) {
          _rows = value;
          OnUpdateSize(_rows, _cols);
        }
      }
    }

    public int Cols {
      get {
        return _cols;
      }
      set {
        if (value >= _minCols && value <= _maxCols) {
          _cols = value;
          OnUpdateSize(_rows, _cols);
        }
      }
    }

    public bool CursorVisible {
      get {
        return _cursor.activeSelf;
      }
      set {
        _cursor.SetActive(value);
      }
    }

    public int CursorRow {
      get {
        return _cursorRow;
      }
      set {
        if (value >= 0 && value < Rows) {
          _cursorRow = value;
          OnUpdateCursorPos(_cursorRow, _cursorCol);
        }
      }
    }

    public int CursorCol {
      get {
        return _cursorCol;
      }
      set {
        if (value >= 0 && value < Cols) {
          _cursorCol = value;
          OnUpdateCursorPos(_cursorRow, _cursorCol);
        }
      }
    }

    public int SpacesPerTab { get; set; } = 4;

    public bool Scrollable { get; set; } = true;

    public BaseScreen(GameObject mainConsole, int rows, int cols) {
      this._mainConsole = mainConsole;
      _rows = rows;
      _cols = cols;
      _charset = InitCharset();
      // Associates local references to child objects.
      _screen = this._mainConsole.transform.Find(_screenObjectName).gameObject;
      _cursor = this._mainConsole.transform.Find(_cursorObjectName).gameObject;
      foreach (uint charCode in _charset.VisibleCharCodes) {
        string glyphName = _glyphsObjectNamePrefix + _charset.GetGlyphName(charCode);
        var glyphRefObject = this._mainConsole.transform.Find(glyphName).gameObject;
        if (!(glyphRefObject is null)) {
          _glyphs.Add(charCode, glyphRefObject);
        }
      }
      OnUpdateSize(_rows, _cols);
      OnUpdateCursorPos(CursorRow, CursorCol);
    }

    // Only visible and space characters are accepted. The cursor position is not affected by this
    // method.
    public void PutChar(int row, int col, char c) {
      PutChar(row, col, c, _defaultColor);
    }

    public void PutChar(int row, int col, char c, Color color) {
      uint charCode = (uint)c;
      if (TryCreateGlyphObject(charCode, out GameObject glyphObject)) {
        glyphObject.GetComponent<Renderer>().material.SetColor(_mainColor, color);
        glyphObject.transform.parent = _mainConsole.transform;
        PutToBuffer(row, col, c, glyphObject);
      } else if (_charset.IsSpace(charCode)) {
        // Since spaces have no corresponding 3D glyphs, _buffer simply keeps a null reference for
        // every one of them.
        PutToBuffer(row, col, c, null);
      } else {
        Debug.LogError(
            $"The character U+{charCode:X4} is not supported by PutChar.");
      }
    }

    // Visible characters, spaces, enter characters and tabs are accepted. The cursor position is
    // affected by this method.
    public void WriteChar(char c, out int lineScrolled) {
      WriteChar(c, _defaultColor, out lineScrolled);
    }

    public void WriteChar(char c, Color color, out int lineScrolled) {
      lineScrolled = 0;
      if (_charset.IsVisible(c) || _charset.IsSpace(c)) {
        PutChar(CursorRow, CursorCol, c, color);
        MoveCursorToNext(out lineScrolled);
      } else if (_charset.IsNewline(c)) {
        MoveCursorToNewline(out lineScrolled);
      } else if (_charset.IsTab(c)) {
        for (int i = 0; i < SpacesPerTab; i++) {
          PutChar(CursorRow, CursorCol, ' ');
          MoveCursorToNext(out lineScrolled);
        }
      }
    }

    public void DeleteChar(int row, int col) {
      if (_buffer.TryGetValue((row, col), out var oldValue)) {
        if (oldValue.glyphObject) {
          Object.Destroy(oldValue.glyphObject);
        }
        _buffer.Remove((row, col));
      }
    }

    public bool TryGetChar(int row, int col, out char c) {
      return TryGetChar(row, col, out c, out Color _);
    }

    public bool TryGetChar(int row, int col, out char c, out Color color) {
      if (_buffer.TryGetValue((row, col), out var glyph)) {
        c = glyph.c;
        color = glyph.glyphObject is null ? _defaultColor :
            glyph.glyphObject.GetComponent<Renderer>().material.GetColor(_mainColor);
        return true;
      } else {
        c = '\0';
        color = _defaultColor;
        return false;
      }
    }

    public void MoveCursorToNext(out int lineScrolled) {
      lineScrolled = 0;
      if (CursorCol < Cols - 1) {
        CursorCol++;
      } else if (CursorRow < Rows - 1) {
        CursorCol = 0;
        CursorRow++;
      } else {
        Scroll(1);
        lineScrolled = 1;
        CursorCol = 0;
      }
    }

    public void MoveCursorToPrev() {
      if (CursorCol > 0) {
        CursorCol--;
      } else if (CursorRow > 0) {
        CursorCol = Cols - 1;
        CursorRow--;
      }
    }

    public void MoveCursorToNewline(out int lineScrolled) {
      lineScrolled = 0;
      if (CursorRow < Rows - 1) {
        CursorCol = 0;
        CursorRow++;
      } else {
        Scroll(1);
        lineScrolled = 1;
        CursorCol = 0;
      }
    }

    public void Scroll(int lines) {
      if (!Scrollable) {
        return;
      }
      for (int i = 0; i < lines; i++) {
        for (int row = 0; row < Rows; row++) {
          for (int col = 0; col < Cols; col++) {
            if (row < Rows - 1) {
              MoveInBuffer(row + 1, col, row, col);
            } else {
              DeleteChar(row, col);
            }
          }
        }
      }
    }

    internal bool TryCreateGlyphObject(char c, out GameObject glyphObject, bool active = true) {
      return TryCreateGlyphObject((uint)c, out glyphObject);
    }

    internal bool TryCreateGlyphObject(
        uint charCode, out GameObject glyphObject, bool active = true) {
      glyphObject = null;
      if (_glyphs.TryGetValue(charCode, out var glyphRefObject)) {
        // The new object is cloned from the reference object. The life cycle of the new object is
        // maintained by _buffer.
        glyphObject = Object.Instantiate(glyphRefObject);
        glyphObject.SetActive(active);
        return true;
      } else {
        return false;
      }
    }

    private protected abstract void OnUpdateSize(int row, int col);

    private protected abstract void OnUpdateCursorPos(int row, int col);

    private protected abstract void PlaceGlyphObject(int row, int col, GameObject glyphObject);

    private protected abstract BaseCharset InitCharset();

    private void PutToBuffer(int row, int col, char c, GameObject glyphObject) {
      if (_buffer.TryGetValue((row, col), out var oldValue) && oldValue.glyphObject) {
        Object.Destroy(oldValue.glyphObject);
      }
      _buffer[(row, col)] = (c, glyphObject);
      if (glyphObject) {
        PlaceGlyphObject(row, col, glyphObject);
      }
    }

    private void MoveInBuffer(int fromRow, int fromCol, int toRow, int toCol) {
      if (_buffer.TryGetValue((fromRow, fromCol), out var glyph)) {
        PutToBuffer(toRow, toCol, glyph.c, glyph.glyphObject);
        _buffer.Remove((fromRow, fromCol));
      } else {
        DeleteChar(toRow, toCol);
      }
    }
  }
}
