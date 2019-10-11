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
public interface SteeringBehaviour
{
    Kinematic getCharacter();
    Kinematic getTarget();
    void setTargetPosition(Vector3 newTargetPos);
    SteeringOutput getSteering();
    bool isStuck();

}

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
public class DynamicSeek : SteeringBehaviour
{
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
    public Kinematic getCharacter() {
        return character;
    }

    public Kinematic getTarget() {
        return target;
    }

    public void setTargetPosition(Vector3 newTargetPos) {
        target.position = newTargetPos;
    }

    public bool isStuck() {
        if (Vector3.Dot(character.velocity.normalized, (target.position - character.position).normalized) < 0.8f) {
            return true;
        }
        return false;
    }
}   
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Flee
public class DynamicFlee : SteeringBehaviour
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
    public Kinematic getCharacter()
    {
        return character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        target.position = newTargetPos;
    }

    public bool isStuck()
    {
        if (Vector3.Dot(character.velocity.normalized, (character.position - target.position).normalized) < 0.8f)
        {
            return true;
        }
        return false;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Pursue with Arrive
public class DynamicArrive : SteeringBehaviour
{
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
    public Kinematic getCharacter()
    {
        return character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        target.position = newTargetPos;
    }

    public bool isStuck() {
        return false;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Align
public class DynamicAlign : SteeringBehaviour
{

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
    public Kinematic getCharacter()
    {
        return character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        target.position = newTargetPos;
    }
    public bool isStuck()
    {
        return false;
    }

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Pursue without Arrive
class DynamicPursue : SteeringBehaviour
{
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
    public Kinematic getCharacter()
    {
        return character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        ds.setTargetPosition(newTargetPos);
    }

    public bool isStuck()
    {
        return ds.isStuck();
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Evade
class DynamicEvade: SteeringBehaviour
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
    public Kinematic getCharacter()
    {
        return character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        target.position = newTargetPos;
    }

    public bool isStuck()
    {
        return df.isStuck();
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Face
class DynamicFace : SteeringBehaviour
{
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
    public Kinematic getCharacter()
    {
        return a.character;
    }
    public Kinematic getTarget()
    {
        return target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        target.position = newTargetPos;
    }
    public bool isStuck()
    {
        return a.isStuck();
    }

}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Dynamic Wander
class DynamicWander : SteeringBehaviour
{
    float wanderOffset;
    float wanderRadius;
    float wanderRate;
    float wanderOrientation;
    float maxAcceleration;
    public Vector3 targetPos;
    DynamicSeek ds;
    public DynamicWander(float _wanderOffset, float _wanderRadius, float _wanderRate,
                                 float _maxAcceleration, float _wanderOrientation, DynamicSeek _ds)
    {
        wanderOffset = _wanderOffset;
        wanderRadius = _wanderRadius;
        wanderRate = _wanderRate;
        maxAcceleration = _maxAcceleration;
        wanderOrientation = _wanderOrientation;
        ds = _ds;
        //wanderOrientation = f.a.character.orientation;
    }
    private float randomBinomial() {
        return Random.value - Random.value;
    }

    private Vector3 asVector(float _orientation) {
        return new Vector3(Mathf.Sin(_orientation), 0f, Mathf.Cos(_orientation));
    }

    public SteeringOutput getSteering() {


        Vector3 centerOfCircle = getCharacter().position + getCharacter().velocity.normalized * wanderOffset;
        Vector2 randomPoint = Random.insideUnitCircle.normalized;
        Vector3 target = centerOfCircle + new Vector3(randomPoint.x, 0f, randomPoint.y)*wanderRadius;
        Debug.DrawLine(centerOfCircle, target, Color.blue);
        Kinematic targetK = new Kinematic
        {
            position = target
        };

        ds.target = targetK;
        return ds.getSteering();
    }
    public Kinematic getCharacter()
    {
        return ds.character;
    }
    public Kinematic getTarget()
    {
        return ds.target;
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        ds.target.position = newTargetPos;
    }

    public bool isStuck()
    {
        return ds.character.velocity.magnitude<0.8f;
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
class DynamicObstacleAvoidance: SteeringBehaviour
{

    public RaycastHit collisionDetector;
    public float avoidDistance;
    public float lookahead;
    public Vector3 targetPos;
    public float maxAcceleration;
    public SteeringBehaviour s;

    public DynamicObstacleAvoidance(float _avoidDistance, float _lookahead, SteeringBehaviour _s, float _maxAcceleration) {
        avoidDistance = _avoidDistance;
        lookahead = _lookahead;
        s = _s;
        maxAcceleration = _maxAcceleration;

    }

    public SteeringOutput getSteering() {
        Vector3 rayVector = s.getCharacter().velocity;
        rayVector.Normalize();
        rayVector *= lookahead;
        //Debug.Log(lookahead);
        // Does the ray intersect any objects excluding the player layer
        collisionDetector = new RaycastHit();
        float angleInc = 10f;
        //Debug.DrawRay(s.getCharacter().position, s.getCharacter().velocity, Color.green);
        for (int i = 0; i < 6; i++) {
            Vector3 rotRayVec;
            if (i%2 == 0) {
                rotRayVec = Quaternion.AngleAxis(angleInc * -(i/2), Vector3.up) * rayVector;
            }
            else {
                rotRayVec = Quaternion.AngleAxis(angleInc * Mathf.Ceil(i/2), Vector3.up) * rayVector;
            }


            rotRayVec.Scale(new Vector3(1f, 0f, 1f));
            //Debug.DrawRay(s.getCharacter().position, rotRayVec, Color.cyan);
            if (Physics.Raycast(s.getCharacter().position, rotRayVec, out collisionDetector, lookahead))
            {


                //Debug.DrawRay(collisionDetector.point, collisionDetector.normal * avoidDistance, Color.red);
                s.setTargetPosition(collisionDetector.point + (collisionDetector.normal * avoidDistance));
                targetPos = s.getTarget().position;
                DynamicSeek seekAvoidPoint = new DynamicSeek(s.getCharacter(), s.getTarget(), maxAcceleration);
                return seekAvoidPoint.getSteering();
            }
            

        }


        targetPos = s.getTarget().position;
        return s.getSteering();



    }
    public Kinematic getCharacter()
    {
        return s.getCharacter();
    }
    public Kinematic getTarget() {
        return s.getTarget();
    }
    public void setTargetPosition(Vector3 newTargetPos)
    {
        s.setTargetPosition(newTargetPos);
    }

    public bool isStuck()
    {
        return s.isStuck();
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


class DynamicPathFollowing : SteeringBehaviour
{

    public Path path;

    float pathOffset;
    Transform currentParam;

    public SteeringBehaviour s;

    public DynamicPathFollowing(Path _path, float _pathOffset, Transform _currentParam, SteeringBehaviour _s) {
        path = _path;
        pathOffset = _pathOffset;
        currentParam = _currentParam;
        s = _s;

    }

    public SteeringOutput getSteering() {


        path.getClosestPointOnPath(path.nodeList[0].transform, path.nodeList[1].transform, s.getCharacter().position);
        return new SteeringOutput();
    }


    public Kinematic getCharacter()
    {
        throw new System.NotImplementedException();
    }

    public Kinematic getTarget()
    {
        throw new System.NotImplementedException();
    }

    public void setTargetPosition(Vector3 newTargetPos)
    {
        throw new System.NotImplementedException();
    }

    public bool isStuck()
    {
        return s.isStuck();
    }
}

class Path {

    public GameObject[] nodeList;

    public Path(GameObject[] _nodeList) {
        nodeList = _nodeList;
    }
    public Vector3 getClosestPointOnPath(Transform pointA, Transform pointB, Vector3 pos) {
        Vector3 pointAToTar = pointA.position - pos;
        Vector3 pointBToTar = pointB.position - pos;


        //base case if it isn't between the two nodes
        if (Vector3.Dot(pointAToTar, pointBToTar) > 0f) {
            if (pointAToTar.magnitude > pointBToTar.magnitude) {
                return pointA.position;
            }
            return pointB.position;
        }

        Vector3 pointAToPointB = pointA.position - pointB.position;
        Vector3 orth = Quaternion.Euler(0f, 90f, 0f) * pointAToPointB;
        Debug.DrawRay(pos, orth, Color.green);
        if (Input.GetKeyDown(KeyCode.D)) {
            Debug.Log("Dot prod: " + Vector3.Dot(pointAToTar, pointBToTar));
        }
        return Vector3.zero;
    }

}