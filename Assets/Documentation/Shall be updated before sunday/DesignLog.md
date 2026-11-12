# Design Log - CUEPOINT

## Intro & Main Menu

### Creating the Intro Video and Music
- **Music Production:** Generated custom music (Both intro song and song for the first level in the game using suno.com with tailored prompts and unique lyrics.
- **Video Editing:** Manually cut and edited the intro sequence to set the right tone immediately upon startup.

## Game Mechanics & Systems

### Player Movement (CapsuleMovement.cs, PlayerJumpAndCrouch.cs, PlayerRunController.cs)
- Set up basic movement, sprinting, jumping, and sliding. Controlled movement between 3 lanes.
- Added custom gravity so the player falls faster than in the intro and does not feel floaty in the air.
- Fixed the slide mechanic so the player shrinks down perfectly along the ground.
- Built a respawn system that saves the starting position and teleports the player back instantly if they fall off the map.

### Shooting System & Crosshair (MouseLook.cs, PlayerShooting.cs, WeaponController.cs) Visual control of aim, Raycasting, Controlled bullet/muzzle flash visuals. 
- Made the player shoot exactly where the camera is looking.
- Script with a green crosshair UI directly in the center of the screen and added new script to test the mouselook (mouseDelta).
- Clamp added to limit crosshair going inverted after 90 degree upwards look.


### Target Core & Ring Slicing (SpiralTarget3DController.cs & TorusSplitter.cs)
- Set up an interactive target that plays a 26-frame 2D animation to identify when it should be hit.
- Created a script that automatically cuts a standard 3D Torus ring into 7 unique pieces directly inside Unity.
- When shot, the ring segments explode outward using physics.

### Hazard Zone (DeathZone.cs)
- Created a death zone script for the floor under the level that detects when the player falls off and instantly triggers the player's respawn function.

### Audio Export Configurations
- Researched the best export settings for tracks with dense frequencies and heavy sub-bass to prevent quality loss. Settled on maximum quality masters and 320 kbps MP3 for the files.

### Countdown & SecuenceManager (MatchStartSequence.cs)
- Intro sequence start -> (Fade in -> Countdown timer overlaps -> Starting song -> (Scrapt cinematicintromovement)
- Scrapt cinematicintromovement, This got replaced with an animation within unity.
- Countdown audio and animation created and retouched in aftereffects.


  

## Cut list
Mini tutorial on timing, Leaderboard   


## Time Log

| Date | Activity | Description | Time |
| :--- | :--- | :--- | :--- |
| 2026-10-23 | **Intro & Music** | Music creation in Suno.com (lyrics/prompts), video creation of my main intro to the main menu. | 8 hours |
| 2026-10-25 | **Shooting, Hazards & Torus Split** | Built the camera raycast shooting script, set up the death zone trigger under the map, and created the tool to split the torus into 7 pieces. | 6 hours |
| 2026-10-27 | **Player Movement & Intro Intro** | Programmed basic movement, jumping, and sliding controls. Started working on the first phase of the player intro animation. | 6 hours |
| 2026-10-28 | **Physics Polish & DeltaTime** | Adjusted player gravity and ground-snapping slides. Updated movement frames to use Time.deltaTime to ensure consistent game speed across different computer hardware. | 4 hours |
| 2026-10-31 | **Gun & Animations Complete** | Finished the final phase of the player intro animation. Added the weapon mesh along with hand animations, a muzzle flash effect, and implemented a weapon recoil and camera bopping script. | 8 hours |
| 2026-11-01 | **Update Raycasting & Better muzzleflash** | Polished the muzzle flash visual. Improved my script by changing to raycasting | 3 hours |
| 2026-11-02 | **Added documentaion** | Added Concept, Designlog and gates | 1 hours |
| 2026-11-02 | **Added Greybox** | Late add but added after lecture | 1 hours |
| 2026-11-05 | **Small fixes** | Removed useless scripts and added intro scene | 2 hours |
| 2026-11-08 | **Added HUD** | ScoreHUD, HitJudgementText, TimerHUD, TimingHUD | 5 hours |

| | **Total** | | **44 hours** |
