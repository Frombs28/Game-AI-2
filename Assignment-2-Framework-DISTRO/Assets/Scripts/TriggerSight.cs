using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSight : MonoBehaviour
{
    NPCController thisObject;
    // Start is called before the first frame update
    void Start()
    {
        thisObject = transform.parent.gameObject.GetComponent<NPCController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Red" || other.gameObject.tag == "Wolf" || other.gameObject.tag == "Hunter" || other.gameObject.tag == "House")
        {

        }
    }
}
