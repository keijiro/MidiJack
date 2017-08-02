//
// MidiJack - MIDI Input Plugin for Unity
//
// Copyright (C) 2013-2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

namespace MidiJack
{
    class MidiJackWindow : EditorWindow
    {
        #region Custom Editor Window Code

        [MenuItem("Window/MIDI Jack")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<MidiJackWindow>("MIDI Jack");
        }

        void OnGUI()
        {
            var endpointCount = CountEndpoints();
            var endpointCountSend = CountSendEndpoints();

            // Endpoints
            var temp = "Detected MIDI IN devices:";
            for (var i = 0; i < endpointCount; i++)
            {
                var id = GetEndpointIdAtIndex(i);
                var name = GetEndpointName(id);
                temp += "\n" + id.ToString("X8") + ": " + name;
            }

            temp += "\n\n" + "Detected MIDI OUT devices:";
            for (var i = 0; i < endpointCountSend; i++)
            {
                var id = GetSendEndpointIdAtIndex(i);
                var name = GetSendEndpointName(id);
                temp += "\n" + id.ToString("X8") + ": " + name;
            }

            EditorGUILayout.HelpBox(temp, MessageType.None);

            // Message history
            temp = "Recent MIDI IN messages:";
            foreach (var message in MidiDriver.Instance.History)
                temp += "\n" + message.ToString();
            EditorGUILayout.HelpBox(temp, MessageType.None);

            // Send Message history
            temp = "Recent MIDI OUT messages:";
            foreach (var message in MidiDriver.Instance.HistorySend)
                temp += "\n" + message.ToString();
            EditorGUILayout.HelpBox(temp, MessageType.None);
        }

        #endregion

        #region Update And Repaint

        const int _updateInterval = 15;
        int _countToUpdate;
        int _lastMessageCount;

        void Update()
        {
            if (--_countToUpdate > 0) return;

            var mcount = MidiDriver.Instance.TotalMessageCount;
            if (mcount != _lastMessageCount) {
                Repaint();
                _lastMessageCount = mcount;
            }

            _countToUpdate = _updateInterval;
        }

        #endregion

        #region Native Plugin Interface

        // MIDI IN
        [DllImport("MidiJackPlugin", EntryPoint="MidiJackCountEndpoints")]
        static extern int CountEndpoints();

        [DllImport("MidiJackPlugin", EntryPoint="MidiJackGetEndpointIDAtIndex")]
        static extern uint GetEndpointIdAtIndex(int index);

        [DllImport("MidiJackPlugin")]
        static extern System.IntPtr MidiJackGetEndpointName(uint id);

        static string GetEndpointName(uint id) {
            return Marshal.PtrToStringAnsi(MidiJackGetEndpointName(id));
        }

        // MIDI OUT
        [DllImport("MidiJackPlugin", EntryPoint="MidiJackCountSendEndpoints")]
        static extern int CountSendEndpoints();

        [DllImport("MidiJackPlugin", EntryPoint="MidiJackGetSendEndpointIDAtIndex")]
        static extern uint GetSendEndpointIdAtIndex(int index);

        [DllImport("MidiJackPlugin")]
        static extern System.IntPtr MidiJackGetSendEndpointName(uint id);

        static string GetSendEndpointName(uint id) {
            return Marshal.PtrToStringAnsi(MidiJackGetSendEndpointName(id));
        }

        #endregion
    }
}
