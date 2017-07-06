using UnityEngine;
using UnityEngine.UI;

public class NoteIndicatorRedirectGroup : MonoBehaviour
{
    public SendTestMIDIManager midiManager;
	public Dropdown midiOutSelector;
    public GameObject prefab;

    void Start()
    {
        for (var i = 0; i < 128; i++)
        {
            var go = Instantiate<GameObject>(prefab);
            go.transform.position = new Vector3(i % 12, i / 12, 0);
            go.GetComponent<NoteIndicatorRedirect>().noteNumber = i;
            go.GetComponent<NoteIndicatorRedirect>().midiManager = midiManager;
            go.GetComponent<NoteIndicatorRedirect>().midiOutSelector = midiOutSelector;
        }
    }
}
