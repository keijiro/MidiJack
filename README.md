**Note: MidiJack is superseded by a new MIDI plugin [Minis]. Although you can
still use MidiJack with the latest Unity version, it's recommended to migrate
to Minis for further updates.**

[Minis]: https://github.com/keijiro/Minis

MIDI Jack
=========

MIDI Jack is a MIDI input plugin for Unity.

![sample](http://keijiro.github.io/MidiJack/sample.gif)

System Requirements
-------------------

- Unity 5
- Windows or Mac OS X

Installation
------------

Download and import [MidiJack.unitypackage][unitypackage] into your project.
That’s it!

See [the troubleshooting topics][troubleshooting] if you encounter any problems.

[unitypackage]:
  https://github.com/keijiro/MidiJack/raw/master/MidiJack.unitypackage
[troubleshooting]:
  https://github.com/keijiro/MidiJack/wiki/Troubleshooting

API Reference
-------------

The basic functions of MIDI Jack are provided in the MidiMaster class.

The channel arguments in the following functions can be omitted.
In that case, the functions return the values in the All-Channel slot, which stores
mixed status of all active channels.

- MidiMaster.GetKey (channel, noteNumber)
  
  Returns the velocity value while the key is pressed, or zero while the
  key is released. The value ranges from 0.0 (note-off) to 1.0 (maximum
  velocity).

- MidiMaster.GetKeyDown (channel, noteNumber)

  Returns true during the frame the user starts pressing down the key.

- MidiMaster.GetKeyUp (channel, noteNumber)

  Returns true during the frame the user releases the key.

- MidiMaster.GetKnob (channel, knobNumber, defaultValue)

  Returns the controller value (CC). The value ranges from 0.0 to 1.0.

- MidiMaster.GetKnobNumbers (channel)

  Returns the list of active controllers.

There are also delegates for the each type of MIDI event.

- MidiMaster.noteOnDelegate (channel, noteNumber, velocity)
- MidiMaster.noteOffDelegate (channel, noteNumber)
- MidiMaster.knobDelegate (channel, knobNumber, konbValue)

MIDI Monitor Window
-------------------

MIDI Jack provides the MIDI Monitor window, which shows the list of
active devices and incoming MIDI messages.

![monitor](http://keijiro.github.io/MidiJack/monitor.png)

The MIDI Monitor window is available from the menu `Window` -> `MIDI Jack`.

Current Limitations
-------------------

- Currently MIDI Jack only supports Windows and OS X. No iOS support yet.
- Only supports note and CC messages. No support for program changes nor
  SysEx.
- The MIDI Jack plugin always tries to capture all available MIDI devices.
  On Windows this behavior may conflict with other MIDI applications.

License
-------

Copyright (C) 2013-2015 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
