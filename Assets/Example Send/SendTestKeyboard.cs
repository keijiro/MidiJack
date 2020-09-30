using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Globalization;
using UnityEngine.UI;
using MidiJack;
using System;

public class SendTestKeyboard : MonoBehaviour {
	public SendTestMIDIManager midiManager;
	public Dropdown midiOutSelector;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SendMIDINote(String note) {
		uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
		int noteParsed = int.Parse(note, NumberStyles.HexNumber);
		MidiMaster.SendNoteOn(id, MidiJack.MidiChannel.Ch1, noteParsed, 0.8f);
		StartCoroutine(waitNoteOff(id, noteParsed));
	}

	private IEnumerator waitNoteOff(uint id, int note) {
		 yield return new WaitForSeconds (0.2f);
		 MidiMaster.SendNoteOff(id, MidiJack.MidiChannel.Ch1, note, 0.8f);
	}
}
