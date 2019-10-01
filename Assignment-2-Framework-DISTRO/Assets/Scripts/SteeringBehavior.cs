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
        DynamicArrive da = new DynamicArrive(agent.k, target.k, maxAcceleration, maxSpeed, targetRadiusL, slowRadiusL);
        agent.DrawCircle(target.k.position, slowRadiusL);
        SteeringOutput so = da.getSteering();
        return so;
    }

    public SteeringOutput PursueArrive() {
        float dis = (agent.k.position - target.k.position).magnitude;
        if(dis <= slowRadiusL) {
            return Arrive();
        }
        DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        agent.DrawCircle(dp.predictPos, targetRadiusL);
        return dp.getSteering();
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
        DynamicWander dw = new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, wanderOrientation, f);
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

    public SteeringOutput ObstacleAvoidance()
    {
        DynamicSeek s = new DynamicSeek(agent.k, target.k, maxAcceleration);
        DynamicObstacleAvoidance doa = new DynamicObstacleAvoidance(5f, 5f, s);
        SteeringOutput so = doa.getSteering();
        agent.DrawLine(agent.k.position,doa.targetPos);
        return so;
    }

    public SteeringOutput PlayerObstacleAvoidance()
    {
        DynamicSeek s = new DynamicSeek(agent.k, target.k, maxAcceleration);
        DynamicObstacleAvoidance doa = new DynamicObstacleAvoidance(5f, 5f, s);
        SteeringOutput so = doa.getSteering();
        agent.DrawLine(agent.k.position, doa.targetPos);
        return so;
    }

    public SteeringOutput ObstacleFlee()
    {
        DynamicFlee f = new DynamicFlee(agent.k, target.k, maxAcceleration);
        DynamicObstacleFleeAvoidance dofa = new DynamicObstacleFleeAvoidance(5f, 5f, f);
        SteeringOutput so = dofa.getSteering();
        agent.DrawLine(agent.k.position, dofa.targetPos);
        return so;
    }



}

