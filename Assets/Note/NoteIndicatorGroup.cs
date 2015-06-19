using UnityEngine;
using System.Collections;

public class NoteIndicatorGroup : MonoBehaviour
{
    public GameObject prefab;

    void Start ()
    {
        for (var i = 0; i < 128; i++)
        {
            var position = new Vector3 (i % 12, i / 12, 0);
            var indicator = Instantiate (prefab, position, Quaternion.identity) as GameObject;
            indicator.GetComponent<NoteIndicator> ().noteNumber = i;
        }
    }
}
