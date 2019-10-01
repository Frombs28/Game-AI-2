using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MillingtonImplementation : MonoBehaviour
{
    // Use this for initialization
    Kinematic playerK;
    Kinematic enemyK;
    SteeringOutput playerSO;
    SteeringOutput enemySO;
    public GameObject player;
    public GameObject target;
    public float maxAcceleration;
    public float maxSpeed;
    public float targetRadius;
    public float slowRadius;


    public float maxAngularAcceleration;
    public float maxRotation;


    public float maxPrediction;


    public float wanderOffset;
    public float wanderRadius;

    public float wanderRate;
    private float wanderOrientation = 0f;

    void Start()
    {
        playerK = new Kinematic();
        enemyK = new Kinematic();

        playerK.maxSpeed = maxSpeed;
        enemyK.maxSpeed = maxSpeed;

        playerK.position = player.GetComponent<Rigidbody>().position;
        playerK.velocity = Vector3.zero;
        playerK.orientation = Mathf.Deg2Rad * player.GetComponent<Rigidbody>().rotation.eulerAngles.y;

        enemyK.position = target.GetComponent<Rigidbody>().position;
        enemyK.velocity = Vector3.zero;
        enemyK.orientation = Mathf.Deg2Rad * target.GetComponent<Rigidbody>().rotation.eulerAngles.y;

        playerSO = new SteeringOutput();
        enemySO = new SteeringOutput();
        
    }

    // Update is called once per frame
    SteeringOutput empty;

    void Update()
    {
        playerK.position = player.gameObject.GetComponent<Rigidbody>().position;
        enemyK.position = target.gameObject.GetComponent<Rigidbody>().position;


        DynamicAlign a = new DynamicAlign(playerK, enemyK, maxAcceleration, maxRotation, targetRadius, slowRadius);
        playerSO = new DynamicSeek(playerK, enemyK, maxAcceleration).getSteering();

        playerK.Update(playerSO, maxSpeed, Time.deltaTime);
        enemyK.Update(enemySO, maxSpeed, Time.deltaTime);




        //update player
        player.gameObject.GetComponent<Rigidbody>().position = playerK.position;
        player.gameObject.GetComponent<Rigidbody>().rotation = Quaternion.Euler(Vector3.up * (Mathf.Rad2Deg * -playerK.orientation));

        //update target
        target.gameObject.GetComponent<Rigidbody>().position = enemyK.position;
        target.gameObject.GetComponent<Rigidbody>().rotation = Quaternion.Euler(Vector3.up * (Mathf.Rad2Deg* -enemyK.orientation));

    }
}
// The Kinematic class is used to physically manipulate the objects. Gives us greater control over the object's movements.
public struct Kinematic
{
    public Vector3 position;
    public float orientation;
    public Vector3 velocity;
    public float rotation; //angular velocity
    public float maxSpeed;

    public void Update(SteeringOutput steering, float _maxSpeed, float time) {

        position += velocity * time;
        //orientation + angular velocity
        orientation += rotation * time;
        //
        velocity += steering.linear * time;
        orientation += steering.angular * time;

        if(velocity.magnitude > _maxSpeed)
        {
            velocity.Normalize();
            velocity *= _maxSpeed;
        }

    }

    private Vector3 asVector(float _orientation)
    {
        return new Vector3(Mathf.Sin(_orientation), 0f, Mathf.Cos(_orientation));
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Steering Output is used by all classes, as it is a structure that holds a linear vector3 and a float representing the angular
// acceleration of the object.
public struct SteeringOutput {

    public Vector3 linear;
    public float angular;

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Seek
public class DynamicSeek{
    public Kinematic character;
    public Kinematic target;
    public float maxAcceleration;

    public DynamicSeek(Kinematic _character, Kinematic _target, float _maxAcceleration) {
        character = _character;
        target = _target;
        maxAcceleration = _maxAcceleration;
    }
    public SteeringOutput getSteering() {
        SteeringOutput steering = new SteeringOutput();
        steering.linear = target.position - character.position;

        steering.linear.Normalize();
        steering.linear *= maxAcceleration;
        steering.angular = 0;
        return steering;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Flee
public class DynamicFlee
{
    public Kinematic character;
    public Kinematic target;
    public float maxAcceleration;

    public DynamicFlee(Kinematic _character, Kinematic _target, float _maxAcceleration)
    {
        character = _character;
        target = _target;
        maxAcceleration = _maxAcceleration;
    }
    public SteeringOutput getSteering()
    {
        SteeringOutput steering = new SteeringOutput();
        steering.linear = character.position - target.position;
        steering.linear.Normalize();
        steering.linear *= maxAcceleration;
        steering.angular = 0;
        return steering;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Pursue with Arrive
public class DynamicArrive {
    Kinematic character;
    Kinematic target;
    float maxAcceleration;
    float maxSpeed;
    float targetRadius;
    float slowRadius;
    float timeToTarget = 0.1f;
    public DynamicArrive(Kinematic _character, Kinematic _target, float _maxAcceleration, float _maxSpeed,
            float _targetRadius, float _slowRadius)
    {
        character = _character;
        target = _target;
        maxAcceleration = _maxAcceleration;
        maxSpeed = _maxSpeed;
        targetRadius = _targetRadius;
        slowRadius = _slowRadius;
    }

    public SteeringOutput getSteering() {
        SteeringOutput steering = new SteeringOutput();
        Vector3 direction = target.position - character.position;
        float distance = direction.magnitude;

        if (distance < slowRadius)
        {
            //Debug.Log("INSIDE!!!");
            steering.linear = -2*character.velocity;
            //steering.linear = Vector3.zero;
            steering.angular = 0;
            return steering;
        }

        float targetSpeed;
        if(distance > slowRadius) {
            targetSpeed = maxSpeed;
        }
        else {
            //Debug.Log("inside slow radius!!! :" + distance / slowRadius);
            targetSpeed = maxSpeed * (distance / slowRadius);
        }

        Vector3 targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        steering.linear = targetVelocity - character.velocity;
        steering.linear /= timeToTarget;

        if(steering.linear.magnitude > maxAcceleration) {
            steering.linear.Normalize();
            steering.linear *= maxAcceleration;
        }

        steering.angular = 0;

        return steering;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Align
public class DynamicAlign {

    public Kinematic character;
    public Kinematic target;
    public float maxAngularAcceleration;
    public float maxRotation;
    public float targetRadius;
    public float slowRadius;
    public float timeToTarget = 0.1f;

    public DynamicAlign(Kinematic _character, Kinematic _target, float _maxAngularAcceleration, float _maxRotation,
            float _targetRadius, float _slowRadius)
    {
        character = _character;
        target = _target;
        maxAngularAcceleration = _maxAngularAcceleration;
        maxRotation = _maxRotation;
        targetRadius = _targetRadius;
        slowRadius = _slowRadius;
    }
    public SteeringOutput getSteering() {
        SteeringOutput steering = new SteeringOutput();

        float rotation = target.orientation - character.orientation;
        rotation = Mathf.Clamp(rotation, -Mathf.PI, Mathf.PI);
        float rotationSize = Mathf.Abs(rotation);



        if (rotationSize < targetRadius) {
            steering.linear = Vector3.zero;
            steering.angular = 0;
            return steering;
        }

        float targetRotation;
        if (rotationSize > slowRadius) {
            targetRotation = maxRotation;
        }
        else {
            targetRotation = maxRotation * rotationSize / slowRadius;
        }

        if(rotationSize < 0.01f) {
            targetRotation *= 0f;
        }
        else
        {
            targetRotation *= rotation / rotationSize;
        }



        steering.angular = targetRotation - character.rotation;
        steering.angular /= timeToTarget;

        float angularAcceleration = Mathf.Abs(steering.angular);
        if (angularAcceleration > maxAngularAcceleration) {
            steering.angular /= angularAcceleration;
            steering.angular *= maxAngularAcceleration;
        }

        steering.linear = Vector3.zero;

        return steering;
    }

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Pursue without Arrive
class DynamicPursue {
    Kinematic character;
    public Kinematic target;
    float maxAcceleration;
    float maxPrediction;
    DynamicSeek ds;
    public Vector3 predictPos;
    public DynamicPursue(Kinematic _character, Kinematic _target, float _maxAcceleration, float _maxPrediction) {
        character = _character;
        target = _target;
        maxAcceleration = _maxAcceleration;
        maxPrediction = _maxPrediction;

        ds = new DynamicSeek(_character, _target, _maxAcceleration);
    }

    public SteeringOutput getSteering() {

        Vector3 explicitTarget = target.position;
        Vector3 direction = target.position - character.position;
        float distance = direction.magnitude;
        float speed = character.velocity.magnitude;
        float prediction;
        if (speed <= (distance / maxPrediction)) {
            prediction = maxPrediction;
        }
        else {
            prediction = distance / speed;
        }


        ds.target.position += target.velocity * prediction;
        predictPos = ds.target.position;
        return ds.getSteering();

    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Evade
class DynamicEvade
{
    Kinematic character;
    Kinematic target;
    float maxAcceleration;
    float maxPrediction;
    DynamicFlee df;
    public Vector3 predictPos;
    public DynamicEvade(Kinematic _character, Kinematic _target, float _maxAcceleration, float _maxPrediction)
    {
        character = _character;
        target = _target;
        maxAcceleration = _maxAcceleration;
        maxPrediction = _maxPrediction;

        df = new DynamicFlee(_character, _target, _maxAcceleration);
    }

    public SteeringOutput getSteering()
    {

        Vector3 explicitTarget = target.position;
        Vector3 direction = target.position - character.position;
        float distance = direction.magnitude;
        float speed = character.velocity.magnitude;
        float prediction;
        if (speed <= (distance / maxPrediction))
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }


        df.target.position += target.velocity * prediction;
        predictPos = df.target.position;
        return df.getSteering();

    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Face
class DynamicFace {
    public Kinematic target;
    public DynamicAlign a;
    public DynamicFace(Kinematic _target, DynamicAlign _a) {
        target = _target;
        a = _a;
    }

    public SteeringOutput getSteering() {
        Vector3 direction = target.position - a.character.position;
        if(direction.magnitude <= 0.001f) {
            SteeringOutput zero;
            zero.linear = Vector3.zero;
            zero.angular = 0;
            return zero;
        }
        a.target = target;
        a.target.orientation = Mathf.Atan2(-direction.x, direction.z);


        return a.getSteering();

    }

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Wander
class DynamicWander {
    float wanderOffset;
    float wanderRadius;
    float wanderRate;
    float wanderOrientation;
    float maxAcceleration;
    public DynamicFace f;
    public Vector3 targetPos;
    public DynamicWander(float _wanderOffset, float _wanderRadius, float _wanderRate,
                                 float _maxAcceleration, float _wanderOrientation, DynamicFace _f)
    {
        wanderOffset = _wanderOffset;
        wanderRadius = _wanderRadius;
        wanderRate = _wanderRate;
        maxAcceleration = _maxAcceleration;
        wanderOrientation = _wanderOrientation;
        f = _f;
        //wanderOrientation = f.a.character.orientation;
    }
    private float randomBinomial() {
        return Random.value - Random.value;
    }

    private Vector3 asVector(float _orientation) {
        return new Vector3(Mathf.Sin(_orientation), 0f, Mathf.Cos(_orientation));
    }

    public SteeringOutput getSteering() {


        //using face, which is using align which has the characyer;
        float targetOrientation = wanderOrientation + f.a.character.orientation;

        f.target.position = f.a.character.position + wanderOffset * asVector(f.a.character.orientation);

        f.target.position += wanderRadius * asVector(targetOrientation);

        targetPos = f.target.position;

        SteeringOutput steering = f.getSteering();

        steering.linear = maxAcceleration * asVector(f.a.character.orientation);
        //Debug.DrawRay(f.a.character.position, asVector(f.a.character.orientation)* maxAcceleration, Color.blue);
        //Debug.Log("wander linear = " + steering.linear);

        return steering;
    }

}
// For this assignement we need:
// collision prediction
// more intelligent wander
// more intelligent behavior overall
// chase the Player Character(PC)
// obstacle avoidance


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Pursue with Obstacle Avoidance and Arrival
class DynamicObstacleAvoidance
{

    public RaycastHit collisionDetector;
    public float avoidDistance;
    public float lookahead;
    public Vector3 targetPos;
    public DynamicSeek s;

    public DynamicObstacleAvoidance(float _avoidDistance, float _lookahead, DynamicSeek _s) {
        avoidDistance = _avoidDistance;
        lookahead = _lookahead;
        s = _s;
    }

    public SteeringOutput getSteering() {

        Vector3 rayVector = s.character.velocity;
        rayVector.Normalize();
        rayVector *= lookahead;
        Debug.DrawRay(s.character.position, rayVector, Color.cyan);
        //Debug.Log(lookahead);
        // Does the ray intersect any objects excluding the player layer
        collisionDetector = new RaycastHit();
        if (Physics.Raycast(s.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead)) {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }
        rayVector = Quaternion.AngleAxis(-30, Vector3.up) * rayVector;
        Debug.DrawRay(s.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(s.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }

        rayVector = Quaternion.AngleAxis(60, Vector3.up) * rayVector;
        Debug.DrawRay(s.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(s.character.position - Vector3.up *0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }

        //Debug.Log("Good");
        Debug.DrawRay(s.character.position - Vector3.up * 0.5f, s.character.velocity, Color.green);
        targetPos = s.target.position;
        return s.getSteering();



    }
}


    //public SteeringOutput getSteering() { 
class DynamicCollisionAvoidance {
    Kinematic character;
    float radius;
    List<NPCController> targets;
    float maxAcceleration;

    public DynamicCollisionAvoidance(Kinematic _character, float _radius, List<NPCController> _targets, float _maxAcceleration) {
        character = _character;
        radius = _radius;
        targets = _targets;
        maxAcceleration = _maxAcceleration;
    }

    public SteeringOutput getSteering() {
        SteeringOutput steering = new SteeringOutput();
        float shortestTime = Mathf.Infinity;

        Kinematic firstTarget = new Kinematic();
        bool setFirstTarget = false;
        float firstMinSeparation = 0;
        float firstDistance = 0;
        Vector3 firstRelativePos= Vector3.zero;
        Vector3 firstRelativeVel = Vector3.zero;
        float distance = 0;

        Vector3 relativePos = Vector3.zero;
        foreach(NPCController target in targets) {
            relativePos = target.position - character.position;
            Vector3 relativeVel = target.k.velocity - character.velocity;
            float relativeSpeed = relativeVel.magnitude;
            float timeToCollision = (Vector3.Dot(relativePos, relativeVel)) / (relativeSpeed * relativeSpeed);
            distance = relativePos.magnitude;
            float minSeperation = distance - (relativeSpeed * shortestTime);
            if(minSeperation > radius) {
                continue;
            }
            if(timeToCollision>0 && timeToCollision < shortestTime) {
                shortestTime = timeToCollision;
                firstTarget = target.k;
                setFirstTarget = true;
                firstMinSeparation = minSeperation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
            }



        }

        if (!setFirstTarget)
        {
            return new SteeringOutput();
        }
        if (firstMinSeparation <= 0 || distance < 2 * radius)
        {
            relativePos = firstTarget.position - character.position;
        }
        else
        {
            relativePos = firstRelativePos + firstRelativeVel * shortestTime;
        }

        relativePos.Normalize();
        steering.linear = relativePos * maxAcceleration;
        return steering;

    }

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Evade with Obstacle Avoidance and Arrival
class DynamicObstacleFleeAvoidance
{

    public RaycastHit collisionDetector;
    public float avoidDistance;
    public float lookahead;
    public Vector3 targetPos;
    public DynamicFlee f;
    public DynamicSeek s;

    public DynamicObstacleFleeAvoidance(float _avoidDistance, float _lookahead, DynamicFlee _f, DynamicSeek _s)
    {
        avoidDistance = _avoidDistance;
        lookahead = _lookahead;
        f = _f;
        s = _s;
    }

    public SteeringOutput getSteering()
    {

        Vector3 rayVector = f.character.velocity;
        rayVector.Normalize();
        rayVector *= lookahead;
        Debug.DrawRay(f.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);
        //Debug.Log(lookahead);
        // Does the ray intersect any objects excluding the player layer
        collisionDetector = new RaycastHit();
        if (Physics.Raycast(f.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            Debug.Log("setting new position for flee! " + s.target.position);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }
        rayVector = Quaternion.AngleAxis(-30, Vector3.up) * rayVector;
        Debug.DrawRay(f.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(f.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }

        rayVector = Quaternion.AngleAxis(60, Vector3.up) * rayVector;
        Debug.DrawRay(f.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(f.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            s.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = s.target.position;
            Debug.DrawRay(s.character.position, s.character.velocity, Color.green);
            return s.getSteering();
        }

        //Debug.Log("Good");
        Debug.DrawRay(f.character.position - Vector3.up * 0.5f, f.character.velocity, Color.green);
        targetPos = f.target.position;
        return f.getSteering();
    }



}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Evade with Obstacle Avoidance and Arrival
class DynamicObstacleWanderAvoidance
{

    public RaycastHit collisionDetector;
    public float avoidDistance;
    public float lookahead;
    public Vector3 targetPos;
    public DynamicWander f;

    public DynamicObstacleWanderAvoidance(float _avoidDistance, float _lookahead, DynamicWander _f)
    {
        avoidDistance = _avoidDistance;
        lookahead = _lookahead;
        f = _f;
    }

    public SteeringOutput getSteering()
    {

        Vector3 rayVector = f.f.a.character.velocity;
        rayVector.Normalize();
        rayVector *= lookahead;
        Debug.DrawRay(f.f.a.character.position, rayVector, Color.cyan);
        //Debug.Log(lookahead);
        // Does the ray intersect any objects excluding the player layer
        collisionDetector = new RaycastHit();
        if (Physics.Raycast(f.f.a.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            f.f.a.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = f.f.a.target.position;
            Debug.DrawRay(f.f.a.character.position, f.f.a.character.velocity, Color.green);
            return f.getSteering();
        }
        rayVector = Quaternion.AngleAxis(-30, Vector3.up) * rayVector;
        Debug.DrawRay(f.f.a.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(f.f.a.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            f.f.a.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = f.f.a.target.position;
            Debug.DrawRay(f.f.a.character.position, f.f.a.character.velocity, Color.green);
            return f.getSteering();
        }

        rayVector = Quaternion.AngleAxis(60, Vector3.up) * rayVector;
        Debug.DrawRay(f.f.a.character.position - Vector3.up * 0.5f, rayVector, Color.cyan);

        if (Physics.Raycast(f.f.a.character.position - Vector3.up * 0.5f, rayVector, out collisionDetector, lookahead))
        {
            //Debug.Log("HIT!");

            Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
            f.f.a.target.position = collisionDetector.point + (collisionDetector.normal * avoidDistance);
            targetPos = f.f.a.target.position;
            Debug.DrawRay(f.f.a.character.position, f.f.a.character.velocity, Color.green);
            return f.getSteering();
        }

        //Debug.Log("Good");
        Debug.DrawRay(f.f.a.character.position - Vector3.up * 0.5f, f.f.a.character.velocity, Color.green);
        targetPos = f.f.a.target.position;
        return f.getSteering();
    }



}