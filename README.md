Magnet Collision
================

A two-player online strategy game inspired by the physical game *Kollide*. Players take turns placing magnets into a shared play area, attempting to avoid collisions caused by magnetic attraction. If magnets collide during a player's turn, those magnets are returned to that player's stack.

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

### Other Controls

- **esc**
  - Pause gameplay

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

### Audio

- Audio Manager control sound
- Background music plays during game scenes
- Players can control their individual sound settings

### Menu Screens

- Main Menu
  - Start game as Host
  - Start game as P2 (Client)
  - Button that links to a "how to play" page
 
- Pause Menu
  - Toggle music on/off
  - Change volume
  - Resume Play
  - Return to home


Architecture / Technical Requirements
----------------------------------------

### Singleton Pattern

-   `GameManager`
    -   Controls global game state and flow

### Observer Pattern (Delegates)

-   `OnMagnetPickedUp`
-   `OnMagnetDropped`

Used for decoupled event handling between gameplay, UI, and networking systems.

### Audio
**Audio:**
- `AudioManager` (singleton) manages all in-game sounds
- Background music loops throughout gameplay scenes
- Volume is adjustable via the pause menu and persists via `PlayerPrefs`
- Players can individual edit whether music is playing and at what volume

### Multiplayer
- Built with **Unity Netcode for GameObjects**
- Supports 2 players: one hosts, one joins as client
- All magnet state (position, ownership, collision) is networked and authoritative on the host
- Turn-based lock-out prevents the non-active player from interacting on the other's turn

### Gameplay Scenes
The game includes the following scenes:
1. **Main Menu** — Host/join screen
2. **Arena 1 — Standard Board** 
3. **Arena 2 — Additional Board**
4. **Game Over - Displays who one and give the option to switch to a different board**
5. **How to Play - Gives instructions on controls and turn structure**

Scene transitions are managed by `GameManager` and synchronized across host and client.

### HUD 
The in-game HUD displays:
- **Current turn indicator** — States whose turn it is (Player 1 / Player 2)
- **Magnet stack count** — live count of remaining magnets per player
- HUD updates are driven by the delegate events (`OnMagnetDropped`, `OnMagnetCollision`) and reflect networked state on both host and client

### Pause Menu
Accessible at any time during gameplay via the **Escape** key:
- **Music Toggle** — turns music on or off
- **Volume slider** — adjusts master audio volume; saved via `PlayerPrefs`
- **Return to Menu** — returns to the main menu 
- **Resume Game** — exits the pause menu / resumes play


### Core Game Loop Summary
| Phase | Description |
|---|---|
| Session Start | Host creates session by clicking "Start as Host"; client joins "Join as P2" |
| Setup | Magnet stacks instantiated (10 per player); players spawn on opposite sides |
| Turn Phase | Active player picks up and places a magnet |
| Resolution | Physics resolves; collided magnets return to player's stack |
| Turn Switch | Turn passes to the other player |
| Win Condition | First player to reach 0 magnets in their stack wins |
| End Screen | Win/loss displayed; option to move to a different game board |


### Database Integration & Save/Load System
SQLite integration was begun via `DatabaseManager.cs`, including initial setup and dependency installation (currently commented out).
Full read/write operations and persistent save/load functionality were scoped for this project but not
completed within the development timeline. 

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
3.  Open up a clone of the project
4.  Play the clone
5.  Click the **Start as Host** button on the host instance
6.  Click the **Join as P2** button on the client instance
