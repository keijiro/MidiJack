using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class SendTestMIDIManager : MonoBehaviour {
    private List<MidiOutDevice> midiOutDevices;
    public List<MidiOutDevice> MidiOutDevices{ get{ return midiOutDevices; } }
    public int midiOutDeviceCount;


    public class MidiOutDevice {
        private uint id;
        public uint Id{ get{ return id; } }

        private string name;
        public string Name{ get{ return name;} }

        public MidiOutDevice(uint id, string name) {
            this.id = id;
            this.name = name;
        }

        public override string ToString() {
            return id.ToString("X8") + ": " + name;
        }
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        midiOutDevices = new List<MidiOutDevice>();
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        var endpointCountSend = CountSendEndpoints();
        midiOutDeviceCount = endpointCountSend;
        midiOutDevices.Clear();

        for (var i = 0; i < endpointCountSend; i++)
        {
            var id = GetSendEndpointIdAtIndex(i);
            var name = GetSendEndpointName(id);
            MidiOutDevice mo = new MidiOutDevice(id, name);
            midiOutDevices.Add(mo);
        }
	}


       #region Native Plugin Interface

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
