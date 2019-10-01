Stephen Frombach: Worked on More Intelligent Wander, new Map StateManager, new Line Renderer
Simon Hopkins: Worked on More Intelligent pursue, Obstacle/Wall avoidance, Collision avoidance

The important parts of this implementation reside in NPCController.cs, SteeringBehavior.cs, and MillingtonImplementation.cs. MillingtonImplementation is the most important file; in it are all of our code for the various algorithms, written as separate classes. NPCController acts mostly as a switch telling the object which algorithm to run. SteeringBehavior takes the appropriate algorithm, given from NPCController, and determines which classes from MillingtonImplementation it requires.

How Phases Work:
When the game begins, the different phases/scenes will play out based on a timer. This process starts automatically. You may press buttons 0-8 to play out any scene that you wish. However, the scenes will still play out on a timer; that is, they will always only be a certain length.

Scenes (in order):

Scene 0: Intelligent Dynamic Wander (Wolf)
Scene 1: Pursue/arrive and Evade with obstacle/wall avoidance
Scene 2: Pursue/arrive a still object with obstacle/wall avoidance
Scene 3: Pursue/arrive a still object with obstacle/wall avoidance
Scene 4: Pursue/arrive and Pursue/arrive with obstacle/wall avoidance
Scene 5: 2 Pursue/arrive a still object with collision detection/prediction
Scene 6: Pursue/arrive a still object with obstacle/wall avoidance
Scene 7: Pursue/arrive a still object with obstacle/wall avoidance to the player
Scene 8: Nothing but text