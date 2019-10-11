using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSight : MonoBehaviour
{
    // This goes on the hunter, and is used to start phase 3.

    NPCController thisObject;
    MapStateManager msm;
    bool found = false;
    // Start is called before the first frame update
    void Start()
    {
        thisObject = transform.parent.gameObject.GetComponent<NPCController>();
        msm = FindObjectOfType<MapStateManager>();
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
