using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Globalization;
using UnityEngine.UI;
using MidiJack;

public class SendTest : MonoBehaviour {
	public SendTestMIDIManager midiManager;
	public Dropdown midiOutSelector;
	public InputField message;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnValueChanged(int result){
	}

	public void SendMIDIOut() {
		uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
		uint msg = (uint)int.Parse(message.text, NumberStyles.HexNumber);
		// MidiMaster.SendMessage(id, 0x0090637f);
		MidiMaster.SendMessage(id, msg);
	}
}
