using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simonDebugScript : MonoBehaviour
{

    public NPCController hunter;
    public NPCController wolf;
    // Start is called before the first frame update
    void Start()
    {
        if(!hunter || !wolf) {
            Debug.Log("wolf and hunter do not exist");
            return;
        }
        hunter.mapState = 9;
        wolf.mapState = 0;
        //dumb but sets the new target in NPC Controller script
        hunter.GetComponent<NPCController>().NewTarget(wolf);
        wolf.GetComponent<NPCController>().NewTarget(hunter);

    }

    // Update is called once per frame
    void Update()
    {
        if (!hunter || !wolf)
        {
            Debug.Log("wolf and hunter do not exist");
            return;
        }
    }
}
