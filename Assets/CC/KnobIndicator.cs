using UnityEngine;
using System.Collections;

public class KnobIndicator : MonoBehaviour
{
    public int knobNumber;

    void Update ()
    {
        var position = transform.localPosition;
        position.y = (MidiJack.GetKnob (knobNumber) - 0.5f) * 10.0f;
        transform.localPosition = position;
    }
}
