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


### Difficulty Tuning

I changed the Perfect Window from 0.18 to 0.20 seconds to make the shooting timing slightly easier.

**Before:** 0.18 seconds.

**After:** 0.20 seconds.

**Reason:** I wanted to make it a little easier for the player to get a Perfect hit but not to easy.

**Test results:** After the change, players got slightly more Perfect scores and fewer Early and Late scores. The difference was small, but the timing felt a little more forgiving.

## Cut list
Mini tutorial on timing, Leaderboard, Bounce pad (New mechanic instead of bounce pad after a while. It is a double lane change example: Jump + right + right)


## Time Log

| Date       | Activity                                   | Description                                                                                                                                                                                 | Time    |
| :--------- | :----------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------ |
| 2026-10-23 | **Intro & Music**                          | Music creation in Suno.com (lyrics/prompts), video creation of my main intro to the main menu.                                                                                              | 8 hours |
| 2026-10-25 | **Shooting, Hazards & Torus Split**        | Built the camera raycast shooting script, set up the death zone trigger under the map, and created the tool to split the torus into 7 pieces.                                               | 6 hours |
| 2026-10-27 | **Player Movement & Intro Intro**          | Programmed basic movement, jumping, and sliding controls. Started working on the first phase of the player intro animation.                                                                 | 6 hours |
| 2026-10-28 | **Physics Polish & DeltaTime**             | Adjusted player gravity and ground-snapping slides. Updated movement frames to use Time.deltaTime to ensure consistent game speed across different computer hardware.                       | 4 hours |
| 2026-10-31 | **Gun & Animations Complete**              | Finished the final phase of the player intro animation. Added the weapon mesh along with hand animations, a muzzle flash effect, and implemented a weapon recoil and camera bopping script. | 8 hours |
| 2026-11-01 | **Update Raycasting & Better muzzleflash** | Polished the muzzle flash visual. Improved my script by changing to raycasting                                                                                                              | 3 hours |
| 2026-11-02 | **Added documentaion**                     | Added Concept, Designlog and gates                                                                                                                                                          | 1 hours |
| 2026-11-02 | **Added Greybox**                          | Late add but added after lecture                                                                                                                                                            | 1 hours |
| 2026-11-05 | **Small fixes**                            | Removed useless scripts and added intro scene                                                                                                                                               | 2 hours |
| 2026-11-08 | **Added HUD**                              | ScoreHUD, HitJudgementText, TimerHUD, TimingHUD                                                                                                                                             | 5 hours |
| 2026-11-09 | **Small adds &  documents**                | Added tutorial image to menu and updated documents                                                                                                                                          | 2 hours |
| 2026-11-10 | **Pause Menu & Winscreen**                 | Pause Menu & Winscreen added + Built out the map further                                                                                                                                    | 4 hours |
| 2026-11-11 | **Fixed Screen Sizing**                    | Fixed stretched fit to all screen                                                                                                                                                           | 1 hours |
| 2026-11-12 | **Updated documents**                      | Updated documents                                                                                                                                                                           | 1 hours |
| 2026-11-13 | **Full map & Codecomments**                | Extended targets to the end of the level, own comments to code, Updated documents.                                                                                                          | 4 hours |

| | **Total** | | **56 hours** |



## Playtest

I had some expectations on the Play testing. I could have fixed sensitivity settings and horizontal bar to slide under to make it easier for the testers. But in the end I didn't have time to sort these things out. Still got some really good inputs from viewing the testers playing and discussing their take on the game. 


## Future plans

Fix the missing thing. Like sensitivity setting, keybind settings, Slide obstacle look (ATM just a red horizontal bar)

New environments, timed floor hazards, Intro for new players, Pop-up to revisit through button, Jump pad, Ramps, Targets showing up on 90 degree angles, Considering remove bottom of hud to show combo instead of shooting timer, Adjust perfect hit time to be longer and to shorten the duration of the fly in time. Better animation on Hand + Gun. Visuals on map synced with audio (Lights go black) Gaussian blur with extreme drops.   

Look at timing with targets to make it more automatic.
Sorted out the sync manually by tapping marker to the sound in Premier pro. 

[[Assets/Documentation/Screenshot 2026-11-08 020746.png]]
