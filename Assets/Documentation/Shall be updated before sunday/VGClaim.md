# VG Claim - Final Project

**Name: Glenn magnell
Game title: CUEPOINT
GitHub repo: https://github.com/magnelledits8/CUEPOINT 
Final build link:** 

Commit this file to `Assets/Documentation/` by Sunday Nov 15.
Then send Brian a Teams message: "I'm requesting a VG review."

\---

## The Rules

* At least **6** Toolbox items in total
* At least **4** of the 6 from the Code list (items 1 to 14)
* The **3 required items** are already in the table: 3, 10 and 18
* An item counts when the player experiences it in your game
* Your code is commented in your own words

## Where to Find It

* **Code items:** the script, the function, and the line number. Example: `PlayerHealth.cs`, `TakeDamage()`, line 24
* **Design items:** the scene and the object. Add a screenshot in `Assets/Documentation/` and write its file name
* **Item 18 (difficulty):** the date of your DesignLog.md entry with the before and after numbers

## My Items

| #   | Item                                                                         | Where to find it                                                                                                                                                                                                                                              | Because I built \_\_\_, the player \_\_\_.                                                                                                                                                                                                            |
| --- | ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 3   | Your own function that takes a parameter (REQUIRED)                          | `ScoreHUD.cs`, `RegisterHit(int points)`, line 43                                                                                                                                                                                                             | Because I built `RegisterHit(int points)`, the player gets different scores depending on their shot timing.                                                                                                                                           |
| 10  | A C# event with at least one listener (REQUIRED)                             | `ScoreHUD.cs` – `OnHitRegistered`, `RegisterHit(int points)` och `HitSoundListener.cs` – `OnEnable()`, `PlayHitSound(int points)`                                                                                                                             | Because I built an event that triggers when a target is hit, the player hears a sound confirming a successful shot.                                                                                                                                   |
| 18  | Difficulty tuned with numbers, with before and after written down (REQUIRED) | DesignLog.md – Perfect Window changed from 0.18 to 0.20 seconds                                                                                                                                                                                               | Because I increased the Perfect Window from 0.18 to 0.20 seconds, the player has a slightly easier time getting Perfect hits. During testing, players got slightly more Perfect scores instead of Early or Late scores, but the difference was small. |
| 1   | Prefabs spawned and removed by code                                          | WeaponController.cs , `SpawnBulletTracer()`, lines 191–216<br><br>Targets also containes removed by code feature.                                                                                                                                             | Because I built a bullet tracer that is spawned and removed by code, the player can see a shot coming from the gun even if it's just a visual detail.                                                                                                 |
| 2   | A loop over many things                                                      | `TargetCueManager.cs`, `Update()`, lines 92–98                                                                                                                                                                                                                | Because I built a loop that updates all target cues, the player sees the targets appear and move at the right time with the music.                                                                                                                    |
| 4   | Your own function that returns an answer                                     | SpiralTarget3DController.cs , `TryShoot()`, around lines 389–483\|                                                                                                                                                                                            | Because I built TryShoot to return whether a shot was accepted, the player only scores when the target can be hit.                                                                                                                                    |
| 5   | Physics movement or forces                                                   | `PlayerJumpAndCrouch.cs`, `NormalJump()`, lines 96–104, and `FixedUpdate()`, lines 88–93                                                                                                                                                                      | Because I built jumping with Rigidbody velocity and gravity, the player can jump over obstacles.                                                                                                                                                      |
| 6   | Respawn and checkpoints                                                      | Respawn exist but I don't want a checkpoint. `PlayerHealth.cs`<br>Why? The game should see the level as a challenge and it should contain a leaderboard in the future. Not for multiple attempts. Although there will be easier maps for more casual players. | Because I built a respawn point, the player doesn't need to boot up the game again.                                                                                                                                                                   |
| 7   | An enemy with readable rules                                                 | Animation of target visually shows this.[[Assets/Target/Frames/frame_01.png]]<br>`TargetCue.cs`, rad 105–119<br>`activationDistance`                                                                                                                          | Because I built an animation with clear visuals, the player can time their shots. All my targets are the same and all my floor hazards are the same red color.                                                                                        |
| 9   |                                                                              |                                                                                                                                                                                                                                                               |                                                                                                                                                                                                                                                       |
|     |                                                                              |                                                                                                                                                                                                                                                               |                                                                                                                                                                                                                                                       |
| 11  | A GameManager that tracks game states                                        | My sceneLoader tracks what scene we are in. It kind of a state but I don't know if this is the type of state you actually mean.                                                                                                                               | Because I built a sceneloader, the game runs more smooth than having everything packed in 1 scene and I'm able to pause.                                                                                                                              |
| 12  |                                                                              |                                                                                                                                                                                                                                                               |                                                                                                                                                                                                                                                       |
| 13  |                                                                              |                                                                                                                                                                                                                                                               |                                                                                                                                                                                                                                                       |
| 14  | More than one scene                                                          | `SceneLoader.cs`, `LoadScene(string sceneName)`, lines 10–13, `PlayGame()`, `OpenMenu()`\|                                                                                                                                                                    | Because I built a scene loader for the menu and the game, the player can start a level and return to the main menu.                                                                                                                                   |
| 15  | Guiding the player using landmarks, light, lines, framing or gates           | End of the tunnel is the end, the timer also showing how much time is left of the song.                                                                                                                                                                       | <br>Because I built multiple things to guid the player, the player can understand when the level is finished.                                                                                                                                         |
| 16  | A juice pass                                                                 | `Level1`, weapon effects, sound and hit judgement HUD. `HitSoundListener`, `HitJudgementHUD.cs`.                                                                                                                                                              | Because I built small shooting effects and stronger feedback when a target is hit, the player can hear the shot and see if the timing was Early, Perfect or Late.\|                                                                                   |
| 17  | An accessibility option                                                      | Wanted to add sensitivity settings but couldn't get it to work. Might ask you at another time.                                                                                                                                                                | Because I didn't get to add a mouse sensitivity slider, the player can't change how fast the camera moves to suit their aiming preference.                                                                                                            |

Add more rows if you used more items. More is fine, but 6 is enough.

## The Sentence

"Because I built \_\_\_, the player \_\_\_."

* **The first blank:** the choice, by name. A script and a function, a component, or a setting.
* **The second blank:** what the player sees, hears or does that would change without it.

> Example: "Because I built `OnShipSaved`, an event the HUD and the foghorn both listen to, the player hears and sees every rescue at the same moment."

## Check Before You Send

* \[x] At least 6 rows filled in
* \[x] At least 4 of them are Code items (1 to 14)
* \[x] Items 3, 10 and 18 are filled in
* \[ ] Every Code item has a file and a line number
* \[x] Brian is added to my GitHub repo
* \[x] Teams message sent

