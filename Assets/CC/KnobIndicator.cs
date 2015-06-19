using UnityEngine;
using MidiJack;

public class KnobIndicator : MonoBehaviour
{
    public int knobNumber;

    void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        var s = MidiMaster.GetKnob(knobNumber);
        transform.localScale = new Vector3(1, s, 1);
    }
}
