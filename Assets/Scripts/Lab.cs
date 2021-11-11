using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClayConsole;

public class Lab : MonoBehaviour {
  private const int _minX = -25;
  private const int _maxX = 25;
  private const int _minZ = -25;
  private const int _maxZ = 25;
  private const int _minY = 5;
  private const int _maxY = 55;
  private const int _maxCharactersPerAction = 12;
  private const float _secondsBeforeDestroy = 20f;

  public BaseScreen Screen { get; set; }

  public void Rain(string s) {
    if (!string.IsNullOrEmpty(s)) {
      int i = 0;
      int count = 0;
      int length = System.Math.Min(s.Length, _maxCharactersPerAction);
      List<GameObject> glyphs = new List<GameObject>();
      while (i < s.Length && count < _maxCharactersPerAction) {
        char c = s[i];
        if (Screen._charset.IsVisible(c)) {
          if (Screen.TryCreateGlyphObject(c, out GameObject glyphObject)) {
            glyphObject.transform.SetParent(transform);
            glyphObject.GetComponent<Renderer>().material.SetColor("_Color", RandomColor());
            glyphObject.transform.localScale = RandomLocalScale();
            glyphObject.transform.localPosition = RandomLocalPosition(length, i);
            var collider = glyphObject.AddComponent<BoxCollider>();
            collider.material.bounciness = 0.85f;
            var physics = glyphObject.AddComponent<Rigidbody>();

            glyphs.Add(glyphObject);
          }
          count++;
        }
        i++;
      }
      if (glyphs.Count > 0) {
        StartCoroutine(WaitAndDestroy(_secondsBeforeDestroy, glyphs));
      }
    }
  }

  private IEnumerator WaitAndDestroy(float seconds, IReadOnlyList<GameObject> gameObjects) {
    yield return new WaitForSeconds(seconds);
    foreach (var gameObject in gameObjects) {
      Object.Destroy(gameObject);
    }
  }

  private Color RandomColor() {
    float h = Random.Range(0f, 1f);
    float s = Random.Range(0.9f, 1f);
    float v = Random.Range(0.5f, 1f);
    return Color.HSVToRGB(h, s, v);
  }

  private Vector3 RandomLocalPosition(int length, int index) {
    float y = Random.Range(_maxY - 3, _maxY);
    float z = Random.Range(-3, 3);
    float xUnit = (_maxX -_minX) / _maxCharactersPerAction;
    float x = xUnit * (index - length / 2.0f);
    return new Vector3(x, y, z);
  }

  private Vector3 RandomLocalScale() {
    float scale = Random.Range(300, 400);
    return new Vector3(scale, scale, scale * 4);
  }
}
