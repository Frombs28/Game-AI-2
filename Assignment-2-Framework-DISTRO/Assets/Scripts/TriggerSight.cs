using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSight : MonoBehaviour
{
    // This goes on the hunter, and is used to start phase 3.

    NPCController thisObject;
    MapStateManager msm;
    LineRenderer line;
    bool found = false;
    int radius = 9;
    // Start is called before the first frame update
    void Start()
    {
        thisObject = transform.parent.gameObject.GetComponent<NPCController>();
        msm = FindObjectOfType<MapStateManager>();
        line = GetComponent<LineRenderer>();
        if (msm.CurState() == 2)
        {
            line.positionCount = 51;
            line.useWorldSpace = false;
            float x;
            float z;
            float angle = 20f;

            for (int i = 0; i < 51; i++)
            {
                x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                line.SetPosition(i, new Vector3(x, 0, z));
                angle += (360f / 51);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Wolf" && !found && msm.CurState() == 2)
        {
            msm.Sight();
            found = true;
        }
    }
}
