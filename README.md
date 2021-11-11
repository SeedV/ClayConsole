# ClayConsole

A Unity in-game console with 3D text glyphs.

No UI toolkit is needed. The console is simply a group of 3D game objects that
handles basic text console APIs such as `Write`, `WriteLine`, `ReadLine`, etc.

## Usage

* Copy the entire dir `Assets/ClayConsole` into your Unity project's `Asset`
  dir.
* Use `Prefabs/FlatConsole.prefab` to create a new console.
* In your script, control the console via the interface of
  [ClayConsole/MainConsole.c](./Assets/ClayConsole/Scripts/ClayConsole/MainConsole.cs).

## Demo Project: In-game ISB Shell

This repo is also a Unity project to demonstrate ClayConsole. It implements an
in-game shell of the [ISB language](https://github.com/wixette/isb).

## About the Fonts

The 3D glyphs used by ClayConsole are derived from the [Roboto Mono
Font](https://fonts.google.com/specimen/Roboto+Mono), which is licensed under
the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0).
