//
// MidiJack - MIDI Input Plugin for Unity
//
// Copyright (C) 2013-2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MidiJack
{
    // MIDI channel names
    public enum MidiChannel
    {
        Ch1,    // 0
        Ch2,    // 1
        Ch3,
        Ch4,
        Ch5,
        Ch6,
        Ch7,
        Ch8,
        Ch9,
        Ch10,
        Ch11,
        Ch12,
        Ch13,
        Ch14,
        Ch15,
        Ch16,
        All     // 16
    }

    // MIDI master class
    public class MidiMaster : MonoBehaviour
    {
        #region Public Interface

        // Returns the key state (on: velocity, off: zero).
        public static float GetKey(MidiChannel channel, int noteNumber)
        {
            var v = instance.channelArray[(int)channel].noteArray [noteNumber];
            if (v > 1) return v - 1;
            if (v > 0) return v;
            return 0.0f;
        }

        public static float GetKey(int noteNumber)
        {
            return GetKey(MidiChannel.All, noteNumber);
        }

        // Returns true if the key was pressed down in the current frame.
        public static bool GetKeyDown(MidiChannel channel, int noteNumber)
        {
            return instance.channelArray[(int)channel].noteArray[noteNumber] > 1;
        }

        public static bool GetKeyDown(int noteNumber)
        {
            return GetKeyDown(MidiChannel.All, noteNumber);
        }

        // Returns true if the key was released in the current frame.
        public static bool GetKeyUp(MidiChannel channel, int noteNumber)
        {
            return instance.channelArray[(int)channel].noteArray[noteNumber] < 0;
        }

        public static bool GetKeyUp(int noteNumber)
        {
            return GetKeyUp(MidiChannel.All, noteNumber);
        }

        // Provides the CC (knob) list.
        public static int[] GetKnobNumbers(MidiChannel channel)
        {
            var cs = instance.channelArray[(int)channel];
            var numbers = new int[cs.knobMap.Count];
            cs.knobMap.Keys.CopyTo(numbers, 0);
            return numbers;
        }

        public static int[] GetKnobNumbers()
        {
            return GetKnobNumbers(MidiChannel.All);
        }

        // Returns the CC (knob) value.
        public static float GetKnob(MidiChannel channel, int knobNumber, float defaultValue = 0)
        {
            var cs = instance.channelArray[(int)channel];
            if (cs.knobMap.ContainsKey(knobNumber)) return cs.knobMap[knobNumber];
            return defaultValue;
        }

        public static float GetKnob(int knobNumber, float defaultValue = 0)
        {
            return GetKnob(MidiChannel.All, knobNumber, defaultValue);
        }

        #endregion

        #region Internal data structure

        public struct MidiMessage
        {
            public uint source; // MIDI source (endpoint) ID
            public byte status; // MIDI status byte
            public byte data1;  // MIDI data bytes
            public byte data2;

            public MidiMessage(ulong data)
            {
                source = (uint)(data & 0xffffffffUL);
                status = (byte)((data >> 32) & 0xff);
                data1 = (byte)((data >> 40) & 0xff);
                data2 = (byte)((data >> 48) & 0xff);
            }

            public override string ToString()
            {
                const string fmt = "s({0:X2}) d({1:X2},{2:X2}) from {3:X8}";
                return string.Format(fmt, status, data1, data2, source);
            }
        }

        class ChannelState
        {
            // Note state array
            // X<0    : Released on this frame
            // X=0    : Off
            // 0<X<=1 : On (X represents velocity)
            // 1<X<=2 : Triggered on this frame
            //          (X-1 represents velocity)
            public float[] noteArray;

            // Knob number to knob value mapping
            public Dictionary<int, float> knobMap;

            public ChannelState()
            {
                noteArray = new float[128];
                knobMap = new Dictionary<int, float>();
            }
        }

        // Channel state array
        ChannelState[] channelArray;

        #endregion

        #region Editor supports

        #if UNITY_EDITOR

        // Incoming message history.
        Queue<MidiMessage> messageHistory;
        public Queue<MidiMessage> History {
            get { return messageHistory; }
        }

        #endif

        #endregion

        #region Monobehaviour functions

        void Awake()
        {
            channelArray = new ChannelState[17];
            for (var i = 0; i < 17; i++)
                channelArray[i] = new ChannelState();

            #if UNITY_EDITOR
            messageHistory = new Queue<MidiMessage>();
            #endif
        }

        void Update()
        {
            // Update the note state array.
            foreach (var cs in channelArray)
            {
                for (var i = 0; i < 128; i++)
                {
                    var x = cs.noteArray [i];
                    if (x > 1)
                        cs.noteArray [i] = x - 1; // Key down -> Hold.
                    else if (x < 0)
                        cs.noteArray [i] = 0; // Key up -> Off.
                }
            }

            // Process the message queue.
            while (true)
            {
                // Pop from the queue.
                var data = DequeueIncomingData();
                if (data == 0) break;

                // Parse the message.
                var message = new MidiMessage(data);

                // Split the first byte.
                var statusCode = message.status >> 4;
                var channelNumber = message.status & 0xf;

                // Note on message?
                if (statusCode == 9)
                {
                    var velocity = 1.0f / 127 * message.data2 + 1;
                    channelArray[channelNumber].noteArray[message.data1] = velocity;
                    channelArray[(int)MidiChannel.All].noteArray[message.data1] = velocity;
                }

                // Note off message?
                if (statusCode == 8 || (statusCode == 9 && message.data2 == 0))
                {
                    channelArray[channelNumber].noteArray[message.data1] = -1;
                    channelArray[(int)MidiChannel.All].noteArray[message.data1] = -1;
                }

                // CC message?
                if (statusCode == 0xb)
                {
                    // Normalize the value.
                    var level = 1.0f / 127 * message.data2;
                    // Update the channel if it already exists, or add a new channel.
                    channelArray[channelNumber].knobMap[message.data1] = level;
                    // Do again for All-ch.
                    channelArray[(int)MidiChannel.All].knobMap[message.data1] = level;
                }

                #if UNITY_EDITOR
                // Record the message history.
                messageHistory.Enqueue(message);
                #endif
            }

            #if UNITY_EDITOR
            // Truncate the history.
            while (messageHistory.Count > 8)
                messageHistory.Dequeue();
            #endif
        }

        #endregion

        #region Native module interface

        [DllImport ("MidiJackPlugin", EntryPoint="MidiJackCountEndpoints")]
        public static extern int CountEndpoints();

        [DllImport ("MidiJackPlugin", EntryPoint="MidiJackGetEndpointIDAtIndex")]
        public static extern uint GetEndpointIdAtIndex(int index);

        [DllImport ("MidiJackPlugin", EntryPoint="MidiJackDequeueIncomingData")]
        public static extern ulong DequeueIncomingData();

        [DllImport ("MidiJackPlugin")]
        private static extern System.IntPtr MidiJackGetEndpointName(uint id);

        public static string GetEndpointName(uint id) {
            return Marshal.PtrToStringAnsi(MidiJackGetEndpointName(id));
        }

        #endregion

        #region Singleton class handling

        static MidiMaster _instance;

        public static MidiMaster instance {
            get {
                if (_instance == null)
                {
                    var previous = FindObjectOfType(typeof(MidiMaster));
                    if (previous)
                    {
                        Debug.LogWarning("Initialized twice. Don't use MidiInput in the scene hierarchy.");
                        _instance = (MidiMaster)previous;
                    }
                    else
                    {
                        var go = new GameObject("MidiJack");
                        _instance = go.AddComponent<MidiMaster>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion
    }
}
