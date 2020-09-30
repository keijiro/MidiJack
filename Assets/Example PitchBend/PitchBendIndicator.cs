using UnityEngine;
using MidiJack;

public class PitchBendIndicator : MonoBehaviour
{
    void Awake()
    {
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        var p = MidiMaster.GetBend();
        transform.localPosition = new Vector3(0, p, 0);
    }
}
