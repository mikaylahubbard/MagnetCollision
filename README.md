Magnet Collision
================

A two-player online strategy game inspired by the physical game *Kollide*. Players take turns placing magnets into a shared play area, attempting to avoid collisions caused by magnetic attraction and repulsion. If magnets collide during a player's turn, those magnets are returned to that player's stack.

Overview
--------------------

-   Two players compete in a shared arena
-   Players take turns placing magnets onto a central board
-   Magnets have attraction-based interaction
-   If magnets collide during placement, they are returned to the player's stack
-   First player to successfully place all magnets wins


Win Condition
----------------

A player wins when:

-   They successfully place their **final magnet**
-   Their stack reaches **0 remaining magnets**
  

Core Game Loop
-----------------

1.  Start game session (host or client join)
2.  Players spawn on opposite sides of the board
3.  Each player begins with a stack of magnets (default: 10)
4.  Turn-based play begins:
    -   Move your player "hand" (triangle) via the arrow keys
    -   Hover over a magnet in your stack and press **Space** to pick it up
    -   Move your player to a spot on the board
    -   Press **Space** to place/drop the magnet
    -   Magnet resolves physics interactions
    -   Any collided magnets are returned to the player's stack
5.  Turn switches to the other player
6.  Repeat until a player wins

 Controls
-----------

### Movement

-   **Arrow Keys** → Move player hand

### Magnet Interaction

-   **Space**
    -   Pick up magnet (if not holding one)
    -   Drop magnet (if currently holding one)
 -   **Q/E**
    -   Rotate the magnet (if currently holding one)

Current Implementation
-------------------------

### Networking (Unity Netcode)

-   Unity Netcode for GameObjects
-   Host-authoritative magnet spawning
-   Synchronized player + magnet movement
-   All clients see consistent magnet states


### Player System

-   Players spawn as triangle avatars
-   Player 1 → Red
-   Player 2 → Blue
-   Real-time networked movement
-   Player turns


### Magnet System

-   Magnets spawn in two stacks (per player)
-   Only host/server spawns magnets
-   Magnets are fully networked objects
-   Supports pickup, carry, and drop mechanics
-   Players only own the magnets in their stack
-   Magnet count per player is tracked and displayed


Architecture / Technical Requirements
----------------------------------------

### Singleton Pattern

-   `GameManager`
    -   Controls global game state and flow

### Observer Pattern (Delegates)

-   `OnMagnetPickedUp`
-   `OnMagnetDropped`

Used for decoupled event handling between gameplay, UI, and networking systems.


Known Issues / Work In Progress
----------------------------------

-   MainMenu Screen
-   Instructions/rules Screen
-   Second Game round, randomized board shapes?
-   Additional Design Patterns



* * * * *

Setup Instructions
----------------------

### Clone the Repository

`git clone https://github.com/mikaylahubbard/MagnetCollision.git`

* * * * *

### Open in Unity

1.  Open **Unity Hub**
2.  Click **"Open Project"**
3.  Select the cloned repository folder
4.  Open the project in Unity


### Install Dependencies

If needed:

-   Open **Window → Package Manager**
-   Install:
    -   Netcode for GameObjects
    -   Unity Transport
    -   Input System (if used)

 
How to Test Multiplayer
--------------------------

### Host + Client

1.  Press **Play** in Unity (acts as Host)
2.  Press **Start Host** in the Network Manager
3.  Open up a clone of the project
4.  Play the clone
5.  Press **Start Client** in the clone's Network Manager
