using UnityEngine;
using System.Collections;

public class NoteIndicator : MonoBehaviour
{
    public int noteNumber;

    void Update ()
    {
        transform.localScale = Vector3.one * (0.1f + MidiJack.GetKey (noteNumber));
        GetComponent<Renderer>().material.color = MidiJack.GetKeyDown (noteNumber) ? Color.red : Color.white;
    }
}
