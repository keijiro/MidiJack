MIDI Jack
=========

MIDI Jack is a MIDI input plugin for Unity.

System Requirements
-------------------

- Unity 5
- Windows or Mac OS X

Installation
------------

Download and import MidiJack.unitypackage into your project. That’s it!

API Reference
-------------

The basic functions of MIDI jack are provided in the MidiMaster class.

The *channel* arguments in the following functions can be omitted.
In that case, it returns the value from the All-Channel slot, which stores
the mixed status of all available channels.

- MidiMaster.GetKey (channel, noteNumber)
  
  Returns the ststate of a key. If the key is "on", it returns the velocity
  value (more than zero, up to 1.0). If the key is "off", it returns zero.

- MidiMaster.GetKeyDown (channel, noteNumber)

  Returns true during the frame the user starts pressing down the key.

- MidiMaster.GetKeyUp (channel, noteNumber)

  Returns true during the frame the user releases the key.

- MidiMaster.GetKnob (channel, knobNumber, defaultValue)

  Returns the controller value (CC). The value range is 0.0 to 1.0.

- MidiMaster.GetKnobNumbers (channel)

  Returns the list of active controllers.

Current Limitations
-------------------

- Currently MIDI Jack only supports Windows and OS X. No iOS support yet.
- Only supports MIDI in. No MIDI out support yet.
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
