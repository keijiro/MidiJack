using UnityEngine;
using System.Collections.Generic;

public class KnobIndicatorGroup : MonoBehaviour
{
    public GameObject prefab;
    List<KnobIndicator> indicators;

    void Start ()
    {
        indicators = new List<KnobIndicator> ();
    }

    void Update ()
    {
        var channels = MidiJack.GetKnobNumbers ();

        // If a new chennel was added...
        if (indicators.Count != channels.Length)
        {
            // Instantiate the new indicator.
            var go = Instantiate (prefab, Vector3.right * indicators.Count, Quaternion.identity) as GameObject;

            // Initialize the indicator.
            var indicator = go.GetComponent<KnobIndicator> ();
            indicator.knobNumber = channels [indicators.Count];

            // Add it to the indicator list.
            indicators.Add (indicator);
        }
    }
}
