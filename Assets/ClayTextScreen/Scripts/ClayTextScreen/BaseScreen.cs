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

    private protected BaseCharset _charset = null;
    private GameObject _screen = null;
    private GameObject _cursor = null;
    private readonly Dictionary<uint, GameObject> _glyphs = new Dictionary<uint, GameObject>();
    private readonly Dictionary<(int row, int col), GameObject> _buffer =
      new Dictionary<(int row, int col), GameObject>();

    private int _rows = 25;
    private int _cols = 80;

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
    }

    void Start() {
    }

    void Update() {
    }

    protected virtual void OnUpdateSize(int row, int col) {
    }

    protected abstract void PlaceGlyphObject(int row, int col, GameObject glyphObject);
  }
}
