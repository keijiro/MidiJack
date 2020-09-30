using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMIDIDevice : MonoBehaviour {
	public SendTestMIDIManager midiManager;
	private int midiOutDeviceCount = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RefreshMIDIOutList();
	}
	

	public void RefreshMIDIOutList() {
		var deviceCount = midiManager.midiOutDeviceCount;
		if(deviceCount == midiOutDeviceCount) {
			return;
		}

		Dropdown dd = GetComponent<Dropdown>();
		dd.ClearOptions();

		for (var i = 0; i < deviceCount; i++)
        {
			dd.options.Add(new Dropdown.OptionData(midiManager.MidiOutDevices[i].ToString()) );
		}

		this.midiOutDeviceCount = deviceCount;

		dd.RefreshShownValue();
	}
}
