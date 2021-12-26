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

using System.Collections;

namespace ClayConsole {
  internal abstract class BaseCharset {
    private const string _glyphObjectNamePattern = "U{0:X4}";

    public abstract IEnumerable VisibleCharCodes { get; }

    public bool IsValid(char c) {
      return IsValid((uint)c);
    }

    public abstract bool IsValid(uint charCode);

    // Returns if the character is visible on screen.
    //
    // Visible characters:
    // * Letters
    // * Digits
    // * Punctuations
    // * Other visible symbols excluding spaces
    //
    // Invisible characters:
    // * Spaces
    // * Newlines
    // * Control characters
    public bool IsVisible(char c) {
      return IsVisible((uint)c);
    }

    public abstract bool IsVisible(uint charCode);

    public bool IsSpace(char c) {
      return IsSpace((uint)c);
    }

    public abstract bool IsSpace(uint charCode);

    public bool IsNewline(char c) {
      return IsNewline((uint)c);
    }

    public abstract bool IsNewline(uint charCode);

    public bool IsTab(char c) {
      return IsTab((uint)c);
    }

    public abstract bool IsTab(uint charCode);

    public string GetGlyphName(char c) {
      return GetGlyphName((uint)c);
    }

    public string GetGlyphName(uint charCode) {
      return string.Format(_glyphObjectNamePattern, charCode);
    }
  }

  internal class AsciiCharset : BaseCharset {
    internal class VisibleCharCodesEnumerable : IEnumerable, IEnumerator {
      private uint _current = _minVisibleCharCode - 1;
      public object Current => _current;

      public IEnumerator GetEnumerator() {
        return (IEnumerator)this;
      }

      public bool MoveNext() {
        _current++;
        if (_current > _maxVisibleCharCode) {
          return false;
        }
        return true;
      }

      public void Reset() {
        _current = _minVisibleCharCode - 1;
      }
    }

    private const uint _minVisibleCharCode = 0x21;
    private const uint _maxVisibleCharCode = 0x7e;

    private readonly VisibleCharCodesEnumerable _visibleCharCodes =
        new VisibleCharCodesEnumerable();

    public override IEnumerable VisibleCharCodes => _visibleCharCodes;

    public AsciiCharset() {
    }

    public override bool IsValid(uint charCode) {
      return IsSpace(charCode) || IsNewline(charCode) ||
        (charCode >= _minVisibleCharCode && charCode <= _maxVisibleCharCode);
    }

    public override bool IsSpace(uint charCode) {
      // Only U0020 is supported as space characters in the this charset.
      return charCode == 0x20;
    }

    public override bool IsNewline(uint charCode) {
      return charCode == 0x0a || charCode == 0x0d;
    }

    public override bool IsTab(uint charCode) {
      return charCode == '\t';
    }

    public override bool IsVisible(uint charCode) {
      return IsValid(charCode) && !IsSpace(charCode) && !IsNewline(charCode);
    }
  }
}
