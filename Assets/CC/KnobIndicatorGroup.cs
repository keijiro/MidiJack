using UnityEngine;
using System.Collections.Generic;
using MidiJack;

public class KnobIndicatorGroup : MonoBehaviour
{
    public GameObject prefab;

    List<KnobIndicator> indicators;

    void Start()
    {
        indicators = new List<KnobIndicator>();
    }

    void Update()
    {
        var channels = MidiMaster.GetKnobNumbers();

        // If a new chennel was added...
        if (indicators.Count != channels.Length)
        {
            // Instantiate the new indicator.
            var go = Instantiate<GameObject>(prefab);
            go.transform.position = Vector3.right * indicators.Count;

            // Initialize the indicator.
            var indicator = go.GetComponent<KnobIndicator>();
            indicator.knobNumber = channels[indicators.Count];

            // Add it to the indicator list.
            indicators.Add(indicator);
        }
    }
}
