﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour
{

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;




    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed = 1.0f;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;
    public float startTime;
    Vector3 targetPos;

    // Holds the path to follow
    public List<GameObject> Path;
    GameObject pathsManager;
    public int current = 0;
    bool pathFollow = false;
    bool change = false;

    [SerializeField]
    public List<NPCController> targets;

    protected void Start()
    {

        agent = GetComponent<NPCController>();
        pathsManager = GameObject.FindGameObjectWithTag("Paths");
        if (!pathsManager) {
            return;
        }
        foreach(Transform child in pathsManager.transform)
        {
            Path.Add(child.gameObject);
        }
    }

    public void SetTarget(NPCController newTarget)
    {
        target = newTarget;
    }
    /*
    public SteeringOutput Seek()
    {
        return new DynamicSeek(agent.k, target.k, maxAcceleration).getSteering();
    }
    public SteeringOutput Flee()
    {
        return new DynamicFlee(agent.k, target.k, maxAcceleration).getSteering();
    }

    public SteeringOutput Pursue()
    {
        DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        SteeringOutput so = dp.getSteering();
        agent.DrawCircle(dp.predictPos, targetRadiusL);
        return so;
    }
    */
    public SteeringOutput Arrive()
    {
        DynamicArrive da = new DynamicArrive(agent.k, target.k, maxAcceleration, maxSpeed, targetRadiusL, slowRadiusL);
        agent.DrawCircle(target.k.position, slowRadiusL);
        SteeringOutput so = da.getSteering();
        if (pathFollow && !change && current < 5)
        {
            current++;
            change = true;
        }
        else if (!agent.hit)
        {
            agent.CaughtTarget();
        }
        return so;
    }

    public SteeringOutput PathFollow()
    {
        pathFollow = true;
        change = false;
        SetTarget(Path[current].GetComponent<NPCController>());
        DynamicSeek sb = new DynamicSeek(agent.k, target.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }

    public SteeringOutput PursueArrive() {
        
        float dis = (agent.k.position - target.k.position).magnitude;
        if(dis <= slowRadiusL) {
            return Arrive();
        }
        DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        //agent.DrawCircle(dp.predictPos, targetRadiusL);
        agent.DrawLine(agent.transform.position, dp.predictPos);
        return dp.getSteering();
    }
    /*
    public SteeringOutput Evade()
    {
        DynamicEvade de = new DynamicEvade(agent.k, target.k, maxAcceleration, maxPrediction);
        SteeringOutput so = de.getSteering();
        agent.DrawCircle(de.predictPos, targetRadiusL);
        return so;
    }
    */
    private float randomBinomial()
    {
        return Random.value - Random.value;
    }


    private Vector3 asVector(float _orientation)
    {
        return new Vector3(Mathf.Sin(_orientation), 0f, Mathf.Cos(_orientation));
    }

    
    public SteeringOutput Wander()
    {
        if(startTime > wanderRate) {

            wanderOrientation += randomBinomial() * wanderRate;
            startTime = 0f;
        }
        startTime += Time.deltaTime;
        DynamicAlign a = new DynamicAlign(agent.k, new Kinematic(), maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA);
        DynamicFace f = new DynamicFace(new Kinematic(), a);
        DynamicSeek ds = new DynamicSeek(agent.k, new Kinematic(), maxAcceleration);
        DynamicWander dw = new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, wanderOrientation, ds);
        SteeringOutput so = dw.getSteering();
        agent.DrawCircle(dw.targetPos, wanderRadius);
        //agent.DrawLine(agent.k.position, asVector(wanderOrientation));
        return so;


    }
    

    public SteeringOutput Face()
    {

        DynamicAlign a = new DynamicAlign(agent.k, target.k, maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA);
        return new DynamicFace(target.k, a).getSteering();
    }

    public SteeringOutput Align()
    {
        return new DynamicAlign(agent.k, target.k, maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA).getSteering();
    }

    // Pursue with Obstacle Avoidance and Arrival

    private float stationaryTime = 0f;
    float theta = 0.005f;
    private Vector3 deltaPos = Vector3.zero;
    private Vector3 lastFramePos = Vector3.zero;
    bool stationaryTimeIncrimented = false;
    bool seekingUnstuckPoint = false;
    Kinematic unstuckTarget = new Kinematic();

    public SteeringOutput ObstacleAvoidance(SteeringBehaviour behaviourWhenNotAvoiding)
    {
        Kinematic currentTarget = target.k;

        float dis = (agent.k.position - currentTarget.position).magnitude;
        if (dis <= slowRadiusL && agent.mapState != 7)
        {
            return Arrive();
        }
        stationaryTimeIncrimented = false;

        //trigger, sets unstuck position
        if (stationaryTime > 5f) {
            seekingUnstuckPoint = true;
            stationaryTime = 0f;
            //unstuckTarget.position = agent.k.position - (Quaternion.Euler(0f, Random.Range(0,360f), 0f) * Vector3.forward)*15f;
            unstuckTarget.position = agent.k.position + getEscapeVector(agent.k.position, 20).normalized * 10f;
        }

        SteeringBehaviour s;
        if (seekingUnstuckPoint)
        {
            currentTarget = unstuckTarget;
            if ((agent.k.position - unstuckTarget.position).magnitude < slowRadiusL) {
                seekingUnstuckPoint = false;
            }
            s = new DynamicSeek(agent.k, currentTarget, maxAcceleration);
        }
        else {
            s = behaviourWhenNotAvoiding;
        }


        
        //DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        deltaPos = lastFramePos - agent.k.position;
        //check if x is stagnant

        //check if it is heading in the direction of the target

        if (s.isStuck())
        {
            stationaryTime += Time.deltaTime;
        }
        else {
            stationaryTime -= Time.deltaTime;
            stationaryTime = Mathf.Max(0, stationaryTime);
        }
        //if (Vector3.Dot(agent.k.velocity.normalized, (currentTarget.position - agent.k.position).normalized) < 0.8f)
        //{
            
        //    if (deltaPos.x < theta)
        //    {
        //        stationaryTime += Time.deltaTime;
        //        stationaryTimeIncrimented = true;
        //    }

        //    //check for z
        //    else if (deltaPos.z < theta)
        //    {
        //        if (!stationaryTimeIncrimented)
        //        {
        //            stationaryTime += Time.deltaTime;
        //        }
        //    }
        //}
        //else {
            
            
        //    //stationaryTime = 0;
        //}
        DynamicObstacleAvoidance doa = new DynamicObstacleAvoidance(3f, 2f, s, maxAcceleration);
        SteeringOutput so = doa.getSteering();
        if(agent.mapState == 7)
        {
            agent.DrawCircle(targetPos, wanderRadius);
        }
        else
        {
            agent.DrawLine(agent.k.position, doa.targetPos);
        }
        lastFramePos = agent.k.position;
        return so;
    }

    public Vector3 getEscapeVector(Vector3 pos, int rays) {
        Vector3 returnVector = pos;
        float increment = 360f / rays;
        RaycastHit hit;
        for (int i = 0; i < rays; i++) {

            if (Physics.Raycast(pos, Quaternion.Euler(0f, (increment * i), 0f) * Vector3.forward, out hit, Mathf.Infinity)) {
                if ((hit.point - pos).magnitude > returnVector.magnitude) {
                    returnVector = (hit.point - pos);
                }
            }
            //didn't hit anything
            else {
                return Quaternion.Euler(0f, (increment * i), 0f) * Vector3.forward;
            }
            
        }

        return returnVector;

    }

    public SteeringOutput ObstacleSeek() {
        DynamicSeek sb = new DynamicSeek(agent.k, target.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }
    public SteeringOutput ObstacleFlee()
    {
        DynamicFlee sb = new DynamicFlee(agent.k, target.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }

    /*
    public SteeringOutput ObstaclePursue()
    {
        DynamicPursue sb = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        agent.DrawLine(agent.transform.position, sb.predictPos);
        return ObstacleAvoidance(sb);
    }
    */
    
    public SteeringOutput ObstacleWander()
    {
        //if (startTime > wanderRate)
        //{
        //    wanderOrientation += randomBinomial() * wanderRate;
        //    startTime = 0f;
        //}
        startTime += Time.deltaTime;
        DynamicSeek ds = new DynamicSeek(agent.k, new Kinematic(), maxAcceleration);
        DynamicWander dw = new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, 100f, ds);
        targetPos = dw.targetPos;
        //agent.DrawCircle(dw.targetPos, wanderRadius);
        return ObstacleAvoidance(dw);
    }


    public SteeringOutput CollisionAvoidance() {

        float radius = 1f;
        SteeringOutput so = new SteeringOutput();
        SteeringOutput dca = new DynamicCollisionAvoidance(agent.k, radius, targets, maxAcceleration).getSteering();
        so.linear = ObstacleSeek().linear + dca.linear;
        so.angular = ObstacleSeek().angular + dca.angular;
        return so;
    }

    /*
    public SteeringOutput PathFollowing() {
        Path p = new Path(path);
        Transform currentNode = path[current].transform;
        DynamicSeek s = new DynamicSeek(agent.k, target.k, maxAcceleration);
        DynamicPathFollowing pf = new DynamicPathFollowing(p, 10f, path[current].transform, s);
        pf.path.getClosestPointOnPath(path[current].transform, path[current + 1].transform, agent.k.position);
        return new SteeringOutput();
    }
    */



}

