using UnityEngine;
using MidiJack;

public class ChannelAfterTouchIndicator : MonoBehaviour
{
    void Update()
    {
        var pressure = MidiMaster.GetChannelAfterTouch();
        transform.localScale = new Vector3(0.1f + pressure, 0.1f + pressure * 8.0f, 0.1f + pressure);
        var color = new Color(pressure, 0.0f, 0.0f, 1.0f);
        GetComponent<Renderer>().material.color = color;
    }
}
