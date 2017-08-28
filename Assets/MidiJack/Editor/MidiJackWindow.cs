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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MidiJack
{
    class MidiJackWindow : EditorWindow
    {

        List<string> allDevices = new List<string>();
        Dictionary<string, bool> allDevicesBound = new Dictionary<string, bool>();
        bool _showDeviceManagement = true;

        #region Custom Editor Window Code

        [MenuItem("Window/MIDI Jack")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<MidiJackWindow>("MIDI Jack");
        }

        private void OnEnable()
        {
            Refresh();
        }

        void Refresh()
        {
            RefreshDevices();
            GetDeviceNames();
            RestoreDeviceValues();
        }

        void RestoreDeviceValues()
        {
            // Restore state from EditorPrefs
            var keys = new List<string>();
            foreach (var item in allDevicesBound.Keys)
            {
                keys.Add(item);
            }
            foreach (var key in keys)
            {
                allDevicesBound[key] = EditorPrefs.GetBool(GetPrefsKeyFromName(key), false);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Device Management
            _showDeviceManagement = EditorGUILayout.Foldout(_showDeviceManagement, "Device Management", true);
            if (_showDeviceManagement)
            {
                var endpointCount = CountEndpoints();

                EditorGUILayout.Space();

                if (GUILayout.Button("Refresh"))
                {
                    Refresh();
                }

                // Device Buttons
                for (uint i = 0; i < allDevices.Count; i++)
                {
                    string name = allDevices[(int)i];
                    bool newValue = (GUILayout.Toggle(allDevicesBound[name], name));
                    if (newValue != allDevicesBound[name])
                    {
                        if (newValue)
                        {
                            CloseAllDevices();
                            OpenDevice(i);
                        }
                        else
                        {
                            CloseAllDevices();
                        }

                        SetDeviceState(name, newValue);
                    }
                }

                // Close All Button
                var closeButtonStyle = new GUIStyle(GUI.skin.button);
                closeButtonStyle.normal.textColor = Color.red;
                if (GUILayout.Button("Close All Devices", closeButtonStyle))
                {
                    CloseAllDevices();
                    Repaint();
                }

            }

            EditorGUILayout.Space();


            // Message history
            var temp = "Recent MIDI messages:";
            foreach (var message in MidiDriver.Instance.History)
                temp += "\n" + message.ToString();
            EditorGUILayout.HelpBox(temp, MessageType.None);
        }

        void SetDeviceState(string deviceName, bool value)
        {
            EditorPrefs.SetBool(GetPrefsKeyFromName(deviceName), value);
            allDevicesBound[deviceName] = value;
        }

        string GetPrefsKeyFromName(string name)
        {
            return string.Format("MidiJackDeviceEnabled-{0}", name);
        }

        void CloseAllDevices()
        {
            List<string> keys = new List<string>(allDevicesBound.Keys);
            foreach (string key in keys)
            {
                SetDeviceState(key, false);
            }
            CloseDevices();
        }

        void GetDeviceNames()
        {
            allDevices = new List<string>();
            var endpointCount = CountEndpoints();
            for (uint i = 0; i < endpointCount; i++)
            {
                string name = GetEndpointName(i);
                allDevices.Add(name);
                if (!allDevicesBound.ContainsKey(name))
                {
                    allDevicesBound.Add(name, EditorPrefs.GetBool(GetPrefsKeyFromName(name), false));
                }
            }
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

        [DllImport("MidiJackPlugin", EntryPoint="MidiJackCountEndpoints")]
        static extern int CountEndpoints();

        [DllImport("MidiJackPlugin", EntryPoint="MidiJackGetEndpointIDAtIndex")]
        static extern uint GetEndpointIdAtIndex(int index);

        [DllImport("MidiJackPlugin")]
        static extern System.IntPtr MidiJackGetEndpointName(uint id);

        static string GetEndpointName(uint id) {
            return Marshal.PtrToStringAnsi(MidiJackGetEndpointName(id));
        }

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackCloseAllDevices")]
        static extern void CloseDevices();

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackCloseDevice")]
        static extern void CloseDevice(uint index);

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackOpenDevice")]
        static extern void OpenDevice(uint index);

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackRefreshDevices")]
        static extern void RefreshDevices();

        #endregion
    }
}
