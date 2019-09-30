using System.Collections;
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

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start()
    {
        agent = GetComponent<NPCController>();
    }

    public void SetTarget(NPCController newTarget)
    {
        target = newTarget;
    }

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

    public SteeringOutput Arrive()
    {
        return new DynamicArrive(agent.k, target.k, maxAcceleration, maxSpeed, targetRadiusL, slowRadiusL).getSteering();
    }

    public SteeringOutput PursueArrive() {
        float dis = (agent.k.position - target.k.position).magnitude;
        if(dis <= slowRadiusL) {
            return Arrive();
        }

        return Pursue();
    }
    public SteeringOutput Evade()
    {
        DynamicEvade de = new DynamicEvade(agent.k, target.k, maxAcceleration, maxPrediction);
        SteeringOutput so = de.getSteering();
        agent.DrawCircle(de.predictPos, targetRadiusL);
        return so;
    }
    private float randomBinomial()
    {
        return Random.value - Random.value;
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

        return new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, wanderOrientation, f).getSteering();


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



}

