using UnityEngine;
using MidiJack;
using UnityEngine.UI;

public class DelegateSendTester : MonoBehaviour
{
    public SendTestMIDIManager midiManager;
    public Dropdown midiOutSelector;

    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);

        uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
        MidiMaster.SendNoteOn(id, channel, note, velocity);
    }

    void NoteOff(MidiChannel channel, int note)
    {
        Debug.Log("NoteOff: " + channel + "," + note);

        uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
        MidiMaster.SendNoteOff(id, channel, note, 0.0f);
    }

    void Knob(MidiChannel channel, int knobNumber, float knobValue)
    {
        Debug.Log("Knob: " + knobNumber + "," + knobValue);

        uint id = midiManager.MidiOutDevices[ midiOutSelector.value ].Id;
        MidiMaster.SendCC(id, channel, knobNumber, knobValue);

    }

    void OnEnable()
    {
        MidiMaster.noteOnDelegate += NoteOn;
        MidiMaster.noteOffDelegate += NoteOff;
        MidiMaster.knobDelegate += Knob;
    }

    void OnDisable()
    {
        MidiMaster.noteOnDelegate -= NoteOn;
        MidiMaster.noteOffDelegate -= NoteOff;
        MidiMaster.knobDelegate -= Knob;
    }
}
