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

    //public GameObject spawner1;
    public List<GameObject> spawns;
    public Text SpawnText1;
    //public GameObject spawner2;
    public Text SpawnText2;
    //public GameObject spawner3;
    public Text SpawnText3;
 
    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.

    private int currentPhase = -1;           // This stores where in the "phases" the game is.
    private int previousPhase = -1;          // The "phases" we were just in
    private int numAtHouse = 0;              // This keeps track of the number of NPCs who have went to the house in this scene

    //public int Phase => currentPhase;

    LineRenderer line;                 
    public GameObject[] Path;
    public Text narrator;

    //bool runningThrough = false;

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start() {
        Camera.main.GetComponent<CameraController>().player = spawns[0];
        StartCoroutine("NextPhase", 0.02f);
        spawnedNPCs = new List<GameObject>();
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
            //Debug.Log(inputstring);

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
                case 9:
                    EnterMapStateNine();
                    break;

        }
    }

    // Used to switch between phases/scenes in the story
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
        numAtHouse = 0;
        // Now set the correct phase
        if (currentPhase < 9)
        {
            currentPhase++;
        }
        else
        {
            currentPhase = 0;
        }
    }

    // Used to change the behavoir of the NPC's midscene; used only in Scene 5
    IEnumerator NextHalfPhase(float length)
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
        numAtHouse = 0;
        StateFiveAndAHalf();
    }

    // Used for switching scene when Hunter and Wolf see each other
    public void Sight()
    {
        if (currentPhase == 2)
        {
            StartCoroutine("NextPhase",0.02f);
        }
    }

    public void CaughtCharacter()
    {
        if(currentPhase == 3)
        {
            StartCoroutine("NextPhase", 2.0f);
        }
        if(currentPhase == 5)
        {
            StartCoroutine("NextHalfPhase", 0.2f);
        }
    }


    public void ReachedHouse()
    {
        if(currentPhase == 6)
        {
            numAtHouse++;
            if (numAtHouse == 2)
            {
                StartCoroutine("NextPhase", 1.2f);
                return;
            }
        }
        if(currentPhase == 8)
        {
            StartCoroutine("NextPhase", 1.2f);
        }
    }

    public int CurState()
    {
        return currentPhase;
    }

    // $ NEED MORE SPAWNERS FOR BETTER SCENE CONTROL

    // Scene 0: Start scene. nothing happens
    private void EnterMapStateZero()
    {
        narrator.text = "We begin our story in the woods, a place full of mystery and intrigue.";
        StartCoroutine("NextPhase",5.0f);
    }

    // Scene 1: Triggered by time. Hunter appears, wanders randomly.
    private void EnterMapStateOne()
    {
        narrator.text = "The Hunter wanders through the woods, expertly navigating its dense foliage.";
        spawnedNPCs.Add(SpawnItem(spawns[0], HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 7;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 25.0f);
    }

    // Scene 2: Triggered by time. Wolf appears and wanders randomly. Hunter should still be wandering.
    private void EnterMapStateTwo()
    {
        narrator.text = "Likewise, the Big Bad Wolf moves through the forest like a predator, able to move quickly and precisely.";
        spawnedNPCs.Add(SpawnItem(spawns[1], HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 7;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs.Add(SpawnItem(spawns[2], WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 7;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
    }

    // Scene 3: Triggered when the Hunter and Wolf get too close. The Hunter chases the Wolf.
    private void EnterMapStateThree() {
        narrator.text = "The Hunter spots the Wolf walking through the woods, and "+
                        "begins pursuing it, intent on hunting the Wolf so it cannot hurt anyone.";
        spawnedNPCs.Add(SpawnItem(spawns[3], HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs.Add(SpawnItem(spawns[4], WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[0].GetComponent<NPCController>().NewTarget(spawnedNPCs[1].GetComponent<NPCController>());
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(spawnedNPCs[0].GetComponent<NPCController>());
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 3;
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 4;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
    }
   
    // Scene 4: Triggered when the Hunter and Wolf "collide", or get too close. Hunter and Wolf are gone, Red appears and
    //              follows path to House.
    private void EnterMapStateFour()
    {
        narrator.text = "After the Hunter catches up to the Wolf and scares it off, Little Red Riding Hood appears, "+
                        "happily following the road to her Grandmother's house.";
        spawnedNPCs.Add(SpawnItem(spawns[5], RedPrefab, null, SpawnText3, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 9;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        CreatePath();
        StartCoroutine("NextPhase", 15.0f);
    }

    // Scene 5: Triggered by time. Wolf appears and pursues Red until he catches her. Then, they stand still, before the Wolf
    //              pursues House and Red continues path. Triggers 5.5 when Wolf catches Red.
    private void EnterMapStateFive()
    {
        narrator.text = "The Wolf spots Red, and runs toward her to intercept her and figure out where she is going.";
        spawnedNPCs.Add(SpawnItem(spawns[6], RedPrefab, house, SpawnText3, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 9;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[0].GetComponent<SteeringBehavior>().current = 4;
        CreatePath();
        spawnedNPCs.Add(SpawnItem(spawns[7], WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(spawnedNPCs[0].GetComponent<NPCController>());
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 3;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
    }

    // Scene 5.5: Triggered by Wolf catching up to Red. The two stand still for awhile.
    private void StateFiveAndAHalf()
    {
        narrator.text = "After catching up to Red, the Wolf talks to Red, figuring out where she is headed.";
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 0;
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 0;
        StartCoroutine("NextPhase", 5.0f);
    }

    // Scene 6: Triggered by time. Red follows path towards, the House, while Wolf pursues the House, avoiding obstacles.
    private void EnterMapStateSix()
    {
        narrator.text = "The Wolf takes off towards Grandmother's house, using his " +
                        "knowledge of the forest to get the House before her.";
        spawnedNPCs.Add(SpawnItem(spawns[8], RedPrefab, house, SpawnText3, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 9;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        spawnedNPCs[0].GetComponent<SteeringBehavior>().current = 4;
        CreatePath();
        spawnedNPCs.Add(SpawnItem(spawns[9], WolfPrefab, null, SpawnText2, 0));
        spawnedNPCs[1].GetComponent<NPCController>().NewTarget(house);
        spawnedNPCs[1].GetComponent<NPCController>().mapState = 3;
        spawnedNPCs[1].GetComponent<NPCController>().label.enabled = true;
    }

    // Scene 7: Triggered by BOTH characters reaching their destination - both characters dissapear when they arrive.
    //              Hunter appears, but doesn't do anything.
    private void EnterMapStateSeven()
    {
        narrator.text = "The Hunter reappears, resting as he wonders where the Wolf went.";
        spawnedNPCs.Add(SpawnItem(spawns[10], HunterPrefab, null, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 0;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
        StartCoroutine("NextPhase", 7.5f);
    }
    
    // Scene 8: Triggered by time. Hunter pursues House, but faster.
    private void EnterMapStateEight()
    {
        narrator.text = "The Hunter realizes that the Wolf has gone towards the nearby House, and " +
            "runs as fast as he can towards the house in an attempt to save the people living there.";
        spawnedNPCs.Add(SpawnItem(spawns[11], HunterPrefab, house, SpawnText1, 0));
        spawnedNPCs[0].GetComponent<NPCController>().mapState = 3;
        spawnedNPCs[0].GetComponent<NPCController>().label.enabled = true;
    }

    // Scene 9: Triggered when Hunter arrives to house. Nothing happens but the twist ending.
    private void EnterMapStateNine()
    {
        narrator.text = "When the Hunter arrives, he finds the Wolf and Grandmother have been the victims in a murder. " +
            "He wonders who exactly he should have been trying to hunt this whole time.\nEND";
        StartCoroutine("NextPhase", 7.5f);
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
        Vector3 position = spawner.transform.position;
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
        Gizmos.DrawCube(spawns[0].transform.position, spawns[0].transform.localScale);
        Gizmos.DrawCube(spawns[1].transform.position, spawns[1].transform.localScale);
        Gizmos.DrawCube(spawns[2].transform.position, spawns[2].transform.localScale);
    }
}
