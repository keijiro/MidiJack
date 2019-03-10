using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMIDIDevice : MonoBehaviour {
	public SendTestMIDIManager midiManager;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RefreshMIDIOutList();
	}
	

	public void RefreshMIDIOutList() {
		Dropdown dd = GetComponent<Dropdown>();
		dd.ClearOptions();

		for (var i = 0; i < midiManager.midiOutDeviceCount; i++)
        {
			dd.options.Add(new Dropdown.OptionData(midiManager.MidiOutDevices[i].ToString()) );
		}

		dd.RefreshShownValue();
	}
}
