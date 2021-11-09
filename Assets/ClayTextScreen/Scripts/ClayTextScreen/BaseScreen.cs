using System.Collections.Generic;
using UnityEngine;

namespace ClayTextScreen {
  // The base class of all kinds of screens.
  public abstract class BaseScreen : MonoBehaviour {
    private const string _screenObjectName = "Screen";
    private const string _cursorObjectName = "Cursor";
    private const string _glyphsObjectNamePrefix = "Glyphs/";
    private const int _minRows = 10;
    private const int _maxRows = 80;
    private const int _minCols = 10;
    private const int _maxCols = 160;
    private const int _defaultRows = 15;
    private const int _defaultCols = 40;

    private protected GameObject _screen = null;
    private protected GameObject _cursor = null;
    private protected BaseCharset _charset = null;

    private readonly Dictionary<uint, GameObject> _glyphs =
      new Dictionary<uint, GameObject>();
    private readonly Dictionary<(int row, int col), GameObject> _buffer =
      new Dictionary<(int row, int col), GameObject>();

    private int _rows = _defaultRows;
    private int _cols = _defaultCols;
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

    public void PutChar(int row, int col, char c) {
      uint charCode = (uint)c;
      if (_glyphs.TryGetValue(charCode, out var glyphRefObject)) {
        // The new object is cloned from the reference object. The life cycle of the new object is
        // maintained by _buffer.
        if (_buffer.TryGetValue((row, col), out var existingGlyphObject)) {
          Destroy(existingGlyphObject);
        }
        var glyphObject = Instantiate(glyphRefObject);
        glyphObject.transform.parent = transform;
        _buffer.Add((row, col), glyphObject);
        PlaceGlyphObject(row, col, glyphObject);
        Show(row, col);
      }
    }

    public void Show(int row, int col) {
      if (_buffer.TryGetValue((row, col), out var glyphObject)) {
        glyphObject.SetActive(true);
      }
    }

    public void Hide(int row, int col) {
      if (_buffer.TryGetValue((row, col), out var glyphObject)) {
        glyphObject.SetActive(false);
      }
    }

    public void ShowCursor() {
      _cursor.SetActive(true);
    }

    public void HideCursor() {
      _cursor.SetActive(false);
    }

    void Awake() {
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

    void Start() {
    }

    void Update() {
    }

    protected abstract void OnUpdateSize(int row, int col);

    protected abstract void OnUpdateCursorPos(int row, int col);

    protected abstract void PlaceGlyphObject(int row, int col, GameObject glyphObject);
  }
}
