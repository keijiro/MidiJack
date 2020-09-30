using UnityEngine;

public class AfterTouchIndicatorGroup : MonoBehaviour
{
    public GameObject prefab;

    void Start()
    {
        for (var i = 0; i < 128; i++)
        {
            var go = Instantiate<GameObject>(prefab);
            go.transform.position = new Vector3(i % 12, i / 12, 0);
            go.GetComponent<AfterTouchIndicator>().noteNumber = i;
        }
    }
}
