using UnityEngine;
using MidiJack;

public class DelegateTester : MonoBehaviour
{
    void KeyOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log("KeyOn: " + channel + "," + note + "," + velocity);
    }

    void KeyOff(MidiChannel channel, int note)
    {
        Debug.Log("KeyOff: " + channel + "," + note);
    }

    void Knob(MidiChannel channel, int knobNumber, float knobValue)
    {
        Debug.Log("Knob: " + knobNumber + "," + knobValue);
    }

    void OnEnable()
    {
        MidiMaster.keyOnDelegate += KeyOn;
        MidiMaster.keyOffDelegate += KeyOff;
        MidiMaster.knobDelegate += Knob;
    }

    void OnDisable()
    {
        MidiMaster.keyOnDelegate -= KeyOn;
        MidiMaster.keyOffDelegate -= KeyOff;
        MidiMaster.knobDelegate -= Knob;
    }
}
