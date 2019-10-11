Stephen Frombach: Worked on Path Following, MapStateManager, Spawners, and Scene
Simon Hopkins: Worked on Path Following, Better Wander, Better Pursue and Evade

The important parts of this implementation reside in NPCController.cs, SteeringBehavior.cs, and MillingtonImplementation.cs. MillingtonImplementation is the most important file; in it are all of our code for the various algorithms, written as separate classes. NPCController acts mostly as a switch telling the object which algorithm to run. SteeringBehavior takes the appropriate algorithm, given from NPCController, and determines which classes from MillingtonImplementation it requires. SteeringBehavior holds our Path Follow algorithm, while NPCController calls upon the algorithm.

The Path Follower functions by repeatedly calling Seek with Obstacle Avoidance on the different pieces of the Path, choosing the next piece of the Path when it gets close enough (which can
be seen by a small circle flash).

How Phases Work:
When the game begins, the different phases/scenes will play out based on a timer or based on contact. This process starts automatically. You may press buttons 0-9 to play out any scene that you wish. However, the scenes will still play out on a timer/contact; that is, they will always only be a certain length.

The "scenes", or phases, are those assigned to us in the assignment. Note that all behaviors include Obstacle/Collision Avoidance. In order, they are:

Scene 0: Intro (nothing but text)
Scene 1: Hunter appears, Wanders through woods
Scene 2: Wolf appears, Wanders through woods
Scene 3: When Hunter and Wolf get close enough, Hunter pursues the Evading Wolf
Scene 4: Red appears, Hunter and Wolf dissapear, Red Path Follows towards House for a certain time period
Scene 5: Wolf appears, Pursues the Path Following Red
   - Scene 5.5: When Wolf gets close enough to Red, they both stop and talk to one another.
Scene 6: Red continues Path Following, while Wolf Pursues the House.
Scene 7: Hunter appears, Red and Wolf dissapear. Hunter waits.
Scene 8: Hunter Pursues House.
Scene 9: End (nothing but text)