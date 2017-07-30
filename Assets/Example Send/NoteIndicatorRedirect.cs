using UnityEngine;
using MidiJack;
using UnityEngine.UI;

public class NoteIndicatorRedirect : MonoBehaviour
{
    public SendTestMIDIManager midiManager;
	public Dropdown midiOutSelector;
    public int noteNumber;

    void Update()
    {
        var key = MidiMaster.GetKey(noteNumber);
        var keydown = MidiMaster.GetKeyDown(noteNumber);
        var keyup = MidiMaster.GetKeyUp(noteNumber);

        if(keydown) {
            uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
            uint msg = 0x00900000;
            msg |= (uint)noteNumber << 8;
            msg |= (uint)(key * 127f);
            // MidiMaster.SendMessage(id, 0x0090637f);
            MidiMaster.SendMessage(id, msg);
        }

        if(keyup) {
            uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
            uint msg = 0x00800000;
            msg |= (uint)noteNumber << 8;
            msg |= (uint)(key * 127f);
            // MidiMaster.SendMessage(id, 0x0090637f);
            MidiMaster.SendMessage(id, msg);
        }


        transform.localScale = Vector3.one * (0.1f + key);
        var color = keydown ? Color.red : Color.white;
        GetComponent<Renderer>().material.color = color;
    }
}
