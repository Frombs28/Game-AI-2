using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MapStateManager is the place to keep a succession of events or "states" when building 
/// a multi-step AI demo. Note that this is a way to manage 
/// 
/// State changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>

public class MapStateManager : MonoBehaviour {
    // Set prefabs
    public GameObject PlayerPrefab;     // You, the player
    public GameObject HunterPrefab;     // Agent doing chasing
    public GameObject WolfPrefab;       // Agent getting chased
    public GameObject RedPrefab;        // Red Riding Hood, or just "team red"
    public GameObject BluePrefab;       // "team blue"

    public NPCController house;         // for future use

    public NPCController player;

    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;
 
    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.

    private int currentPhase = -1;           // This stores where in the "phases" the game is.
    private int previousPhase = -1;          // The "phases" we were just in

    //public int Phase => currentPhase;

    LineRenderer line;                 
    public GameObject[] Path;
    public Text narrator;

    //bool runningThrough = false;

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start() {
        narrator.text = "Here we see the Hunter, resting outdoors.";
        spawnedNPCs = new List<GameObject>();
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 0));
        StartCoroutine("NextPhase", 5.0f);
    }

    /// <summary>
    /// This is where you put the code that places the level in a particular phase.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    private void Update()
    {
        int num;

        string inputstring = Input.inputString;
        if (inputstring.Length > 0)
        {
            Debug.Log(inputstring);

            // Look for a number key click
            if (inputstring.Length > 0)
            {
                if (Int32.TryParse(inputstring, out num))
                {
                    if (num != currentPhase)
                    {
                        previousPhase = currentPhase;
                        currentPhase = num;
                        StopAllCoroutines();
                        for (int i = spawnedNPCs.Count - 1; i >= 0; i--)
                        {
                            GameObject character = spawnedNPCs[i];
                            character.GetComponent<NPCController>().label.enabled = false;
                            character.GetComponent<NPCController>().DestroyPoints();
                            character.SetActive(false);
                            //Debug.Log("Deleted");
                        }
                        spawnedNPCs.Clear();
                    }
                }
            }
        }
        // Check if a game event had caused a change of phase.
        if (currentPhase == previousPhase)
        {
            return;
        }
        previousPhase = currentPhase;
        // Switch just goes to the correct state, which are all in order and call each other after one is started.
        switch (currentPhase)
            {
                case 0:
                    EnterMapStateZero();
                    break;
                case 1:
                    EnterMapStateOne();
                    break;
                case 2:
                    EnterMapStateTwo();
                    break;
                case 3:
                    EnterMapStateThree();
                    break;
                case 4:
                    EnterMapStateFour();
                    break;
                case 5:
                    EnterMapStateFive();
                    break;
                case 6:
                    EnterMapStateSix();
                    break;
                case 7:
                    EnterMapStateSeven();
                    break;
                case 8:
                    EnterMapStateEight();
                    break;

        }
    }

    IEnumerator NextPhase(float length)
    {
        // After the input 'length' number of seconds, move on to the next state
        float startTime = Time.deltaTime;
        float currentTime = startTime;
        while (currentTime - startTime <= length)
        {
            //Debug.Log(Time.deltaTime - startTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        // First delete all NPCs
        for (int i = spawnedNPCs.Count - 1; i >= 0; i--)
        {
            GameObject character = spawnedNPCs[i];
            character.GetComponent<NPCController>().label.enabled = false;
            character.GetComponent<NPCController>().DestroyPoints();
            character.SetActive(false);
            //Debug.Log("Deleted");
        }
        spawnedNPCs.Clear();
        // Now set the correct phase
        if (currentPhase < 8)
        {
            currentPhase++;
        }
        else
        {
            currentPhase = 0;
        }
    }

    private void EnterMapStateZero()
    {
        narrator.text = "We begin our story in the woods, a place full of mystery and intrigue.";
        StartCoroutine("NextPhase",5.0f);
    }

    private void EnterMapStateOne()
    {
        narrator.text = "The Hunter wanders through the woods, expertly navigating its dense foliage.";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 7;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 25.0f);
    }

    private void EnterMapStateTwo()
    {
        narrator.text = "Likewise, the Big Bad Wolf moves through the forest like a predator, able to move quickly and precisely.";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 7;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        //spawnedNPCs.Add(SpawnItem(spawner3, RedPrefab, null, SpawnText3, 0));
        //spawnedNPCs[0].GetComponent<NPCController>().NewTarget(house);
        //spawnedNPCs[0].GetComponent<NPCController>().mapState = 8;
        //spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        //CreatePath();
        StartCoroutine("NextPhase", 25.0f);
    }

    private void EnterMapStateThree() {
        narrator.text = "The Hunter spots the Wolf walking through the woods, and "+
                        "begins pursuing it, intent on hunting the Wolf so it cannot hurt anyone.";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(spawnedNPCs[1].GetComponent<NPCController>());
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(spawnedNPCs[0].GetComponent<NPCController>());
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 8;
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 4;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 15.0f);
    }
    // $ PICK UP HERE. NEED A NEW WAY TO CHANGE SCENES BASED ON EXTERNAL TRIGGERS
    private void EnterMapStateFour()
    {
        narrator.text = "The Wolf looks for shelter, and spots little Red.";
        spawnedNPCs.Add(SpawnItem(spawner3, RedPrefab, null, SpawnText3, 0));
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(spawnedNPCs[1].GetComponent<NPCController>());
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(spawnedNPCs[0].GetComponent<NPCController>());
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 8;
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 8;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 15.0f);
    }

    private void EnterMapStateFive()
    {
        narrator.text = "The two converse, and little Red directs the Wolf to her house.";
        spawnedNPCs.Add(SpawnItem(spawner3, RedPrefab, null, SpawnText3, 0));
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(house);
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(house);
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 11;
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 11;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 30.0f);
    }

    private void EnterMapStateSix()
    {
        narrator.text = "The Hunter arrives, determined to catch the killer. He spots a house and moves accordingly.";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, house, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(house);
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 8;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 30.0f);
    }

    private void EnterMapStateSeven()
    {
        narrator.text = "The Hunter sees YOU (Player) moving aroudn the woods and descides to give chase, ignoring the house.";
        spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, house, SpawnText1, 0));
        //spawnedNPCs.Add(SpawnItem(spawner3, PlayerPrefab, house, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(player);
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 8;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        player.mapState = 10;
        Camera.main.GetComponent<CameraController>().player = player.gameObject;
        StartCoroutine("NextPhase", 30.0f);
    }

    private void EnterMapStateEight()
    {
        narrator.text = "Days later, reports come in. The killer is still at large, but police have found one clue on its identity. "
            + "A little red hood. END";
        StartCoroutine("NextPhase", 10.0f);
    }

    /// <summary>
    /// SpawnItem placess an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="phase"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText, int phase)
    {
        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2), 0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity);
        if (target)
        {
            temp.GetComponent<NPCController>().NewTarget(target);
        }
        temp.GetComponent<NPCController>().label = spawnText;
        temp.GetComponent<NPCController>().mapState = phase;
        Camera.main.GetComponent<CameraController>().player = temp;
        return temp;
    }

    private void SetArrive(GameObject character) {

        character.GetComponent<NPCController>().mapState = 3;
        character.GetComponent<NPCController>().DrawConcentricCircle(character.GetComponent<SteeringBehavior>().slowRadiusL);
    }

    private void CreatePath()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = Path.Length;
        for (int i = 0; i < Path.Length; i++)
        {
            line.SetPosition(i, Path[i].transform.position);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
