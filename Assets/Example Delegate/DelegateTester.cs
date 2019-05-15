using UnityEngine;
using MidiJack;

public class DelegateTester : MonoBehaviour
{
    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);
    }

    void NoteOff(MidiChannel channel, int note)
    {
        Debug.Log("NoteOff: " + channel + "," + note);
    }

    void Knob(MidiChannel channel, int knobNumber, float knobValue)
    {
        Debug.Log("Knob: " + knobNumber + "," + knobValue);
    }
    void PolyAfterTouch(MidiChannel channel, int note, float pressure)
    {
        Debug.Log("PolyAfterTouch: " + channel + "," + note + "," + pressure);
    }
    void ChannelAfterTouch(MidiChannel channel, float pressure)
    {
        Debug.Log("ChannelAfterTouch: " + channel + "," + pressure);
    }

    void OnEnable()
    {
        MidiMaster.noteOnDelegate += NoteOn;
        MidiMaster.noteOffDelegate += NoteOff;
        MidiMaster.knobDelegate += Knob;
        MidiMaster.polyAfterTouchDelegate += PolyAfterTouch;
        MidiMaster.channelAfterTouchDelegate += ChannelAfterTouch;
    }

    void OnDisable()
    {
        MidiMaster.noteOnDelegate -= NoteOn;
        MidiMaster.noteOffDelegate -= NoteOff;
        MidiMaster.knobDelegate -= Knob;
        MidiMaster.polyAfterTouchDelegate -= PolyAfterTouch;
        MidiMaster.channelAfterTouchDelegate -= ChannelAfterTouch;
    }
}
