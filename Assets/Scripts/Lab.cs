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

using System;
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
  private const int _fireOriginX = -20;
  private const int _fireOriginY = 10;
  private const int _fireOriginZ = -20;
  private const int _maxCharactersPerAction = 12;
  private const int _maxChildCount = 128;
  private const float _secondsBeforeDestroy = 20f;
  private const float _fireIntervalInSeconds = .1f;

  public BaseScreen Screen { get; set; }

  public void Rain(string s) {
    Action<GameObject, int, int> action = (glyphObject, length, index) => {
      glyphObject.transform.localScale = RandomLocalScale();
      glyphObject.transform.localPosition = RandomSequencePosition(length, index);
    };
    CreateGlyphsWithAction(s, action);
  }

  public void Fire(string s) {
    Action<GameObject, int, int> action = (glyphObject, length, index) => {
      glyphObject.transform.localScale = RandomLocalScale();
      glyphObject.transform.localPosition = RandomFirePosition(length, index);
      StartCoroutine(WaitAndFire(_fireIntervalInSeconds * index, glyphObject));
    };
    CreateGlyphsWithAction(s, action);
  }

  private void CreateGlyphsWithAction(
      string s, Action<GameObject, int, int> onEveryGlyphCreatedCallback) {
    if (!string.IsNullOrEmpty(s) && transform.childCount < _maxChildCount) {
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
            var collider = glyphObject.AddComponent<BoxCollider>();
            collider.material.bounciness = 0.85f;
            var rigidbody = glyphObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            glyphs.Add(glyphObject);
            if (!(onEveryGlyphCreatedCallback is null)) {
              onEveryGlyphCreatedCallback(glyphObject, length, count);
            }
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

  private IEnumerator WaitAndFire(float seconds, GameObject gameObject) {
    yield return new WaitForSeconds(seconds);
    gameObject.GetComponent<Rigidbody>().AddForce(RandomForce(), ForceMode.Impulse);
  }

  private IEnumerator WaitAndDestroy(float seconds, IReadOnlyList<GameObject> gameObjects) {
    yield return new WaitForSeconds(seconds);
    foreach (var gameObject in gameObjects) {
      UnityEngine.Object.Destroy(gameObject);
    }
  }

  private Color RandomColor() {
    float h = UnityEngine.Random.Range(0f, 1f);
    float s = UnityEngine.Random.Range(0.9f, 1f);
    float v = UnityEngine.Random.Range(0.7f, 1f);
    return Color.HSVToRGB(h, s, v);
  }

  private Vector3 RandomSequencePosition(int length, int index) {
    float y = UnityEngine.Random.Range(_maxY - 3, _maxY);
    float z = UnityEngine.Random.Range(-3, 3);
    float xUnit = (_maxX -_minX) / _maxCharactersPerAction;
    float x = xUnit * (index - length / 2.0f);
    return new Vector3(x, y, z);
  }

  private Vector3 RandomFirePosition(int length, int index) {
    float x = _fireOriginX + UnityEngine.Random.Range(-5, 5);
    float y = _fireOriginY + UnityEngine.Random.Range(-5, 5);
    float z = _fireOriginZ + UnityEngine.Random.Range(-5, 5);
    return new Vector3(x, y, z);
  }

  private Vector3 RandomLocalScale() {
    float scale = UnityEngine.Random.Range(300, 400);
    return new Vector3(scale, scale, scale * 4);
  }

  private Vector3 RandomForce() {
    float x = UnityEngine.Random.Range(40, 100);
    float y = UnityEngine.Random.Range(50, 200);
    float z = UnityEngine.Random.Range(40, 100);
    return new Vector3(x, y, z);
  }
}
