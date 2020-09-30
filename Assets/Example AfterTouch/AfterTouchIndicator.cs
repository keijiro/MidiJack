using UnityEngine;
using MidiJack;

public class AfterTouchIndicator : MonoBehaviour
{
    public int noteNumber;

    void Update()
    {
        transform.localScale = Vector3.one * (0.1f + MidiMaster.GetPolyAfterTouch(noteNumber));

        var pressure = MidiMaster.GetPolyAfterTouch(noteNumber);
        var color = new Color(pressure, 0.0f, 0.0f, 1.0f);
        GetComponent<Renderer>().material.color = color;
    }
}
