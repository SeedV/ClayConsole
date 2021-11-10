using System.Collections.Generic;
using UnityEngine;

namespace ClayConsole {
  // The base class of all kinds of Consoles.
  public abstract class BaseConsole : MonoBehaviour {
    private const string _screenObjectName = "Screen";
    private const string _cursorObjectName = "Cursor";
    private const string _glyphsObjectNamePrefix = "Glyphs/";
    private const int _minRows = 10;
    private const int _maxRows = 80;
    private const int _minCols = 10;
    private const int _maxCols = 160;
    private const int _defaultRows = 15;
    private const int _defaultCols = 40;
    private static readonly Color _defaultColor = Color.yellow;

    private protected BaseCharset _charset = null;
    private protected IKeyboardInput _keyboard = null;
    private protected GameObject _screen = null;
    private protected GameObject _cursor = null;
    private protected readonly Dictionary<uint, GameObject> _glyphs =
      new Dictionary<uint, GameObject>();
    private protected readonly Dictionary<(int row, int col), (char c, GameObject glyphObject)>
        _buffer = new Dictionary<(int row, int col), (char c, GameObject glyphObject)>();

    private int _rows = _defaultRows;
    private int _cols = _defaultCols;
    private int _cursorRow = 0;
    private int _cursorCol = 0;

    public int SpacesPerTab { get; set; } = 4;

    public bool Scrollable { get; set; } = true;

    public bool CursorVisible {
      get {
        return _cursor.activeSelf;
      }
      set {
        _cursor.SetActive(value);
      }
    }

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

    // Only visible and space characters are accepted. The cursor position is not affected by this
    // method.
    public void PutChar(int row, int col, char c) {
      PutChar(row, col, c, _defaultColor);
    }

    public void PutChar(int row, int col, char c, Color color) {
      uint charCode = (uint)c;

      if (_glyphs.TryGetValue(charCode, out var glyphRefObject)) {
        // The new object is cloned from the reference object. The life cycle of the new object is
        // maintained by _buffer.
        var glyphObject = Instantiate(glyphRefObject);
        glyphObject.SetActive(true);
        glyphObject.GetComponent<Renderer>().material.SetColor("_Color", color);
        glyphObject.transform.parent = transform;
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

    public void DeleteChar(int row, int col) {
      if (_buffer.TryGetValue((row, col), out var oldValue)) {
        if (oldValue.glyphObject) {
          Destroy(oldValue.glyphObject);
        }
        _buffer.Remove((row, col));
      }
    }

    public bool TryGetChar(int row, int col, out char c) {
      if (_buffer.TryGetValue((row, col), out var glyph)) {
        c = glyph.c;
        return true;
      } else {
        c = '\0';
        return false;
      }
    }

    public void Write(string s) {
      Write(s, _defaultColor);
    }

    public void Write(string s, Color color) {
      if (!string.IsNullOrEmpty(s)) {
        foreach (char c in s) {
          if (_charset.IsVisible(c) || _charset.IsSpace(c)) {
            PutChar(CursorRow, CursorCol, c, color);
            MoveCursorToNext();
          } else if (_charset.IsNewline(c)) {
            MoveCursorToNewline();
          } else if (_charset.IsTab(c)) {
            for (int i = 0; i < SpacesPerTab; i++) {
              PutChar(CursorRow, CursorCol, ' ');
              MoveCursorToNext();
            }
          }
        }
      }
    }

    public void WriteLine(string s) {
      WriteLine(s, _defaultColor);
    }

    public void WriteLine(string s, Color color) {
      Write(s + '\n', color);
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

    void Awake() {
      _charset = InitCharset();
      _keyboard = InitKeyboard();
      // Associates local references to child objects.
      _screen = transform.Find(_screenObjectName).gameObject;
      _cursor = transform.Find(_cursorObjectName).gameObject;
      foreach (uint charCode in _charset.VisibleCharCodes) {
        string glyphName = _glyphsObjectNamePrefix + _charset.GetGlyphName(charCode);
        var glyphRefObject = transform.Find(glyphName).gameObject;
        if (!(glyphRefObject is null)) {
          _glyphs.Add(charCode, glyphRefObject);
        }
      }
      OnUpdateSize(Rows, Cols);
      OnUpdateCursorPos(CursorRow, CursorCol);
    }

    void OnGUI() {
      if (Event.current.type == EventType.KeyDown &&
          _keyboard.TryConvertKeyCode(Event.current, out char c, out ControlKey controlKey)) {
        if (c != '\0') {
          Write(c.ToString());
        }
      }
    }

    private protected abstract void OnUpdateSize(int row, int col);

    private protected abstract void OnUpdateCursorPos(int row, int col);

    private protected abstract void PlaceGlyphObject(int row, int col, GameObject glyphObject);

    private protected abstract BaseCharset InitCharset();

    private protected abstract IKeyboardInput InitKeyboard();

    private void PutToBuffer(int row, int col, char c, GameObject glyphObject) {
      if (_buffer.TryGetValue((row, col), out var oldValue) && oldValue.glyphObject) {
        Destroy(oldValue.glyphObject);
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

    private void MoveCursorToNext() {
      if (CursorCol < Cols - 1) {
        CursorCol++;
      } else if (CursorRow < Rows - 1) {
        CursorCol = 0;
        CursorRow++;
      } else {
        Scroll(1);
        CursorCol = 0;
      }
    }

    private void MoveCursorToNewline() {
      if (CursorRow < Rows - 1) {
        CursorCol = 0;
        CursorRow++;
      } else {
        Scroll(1);
        CursorCol = 0;
      }
    }
  }
}
