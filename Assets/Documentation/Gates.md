# Gates - Final Project

**Name:** Glenn Magnell
**Game title:** CUEPOINT

Put this file in `Assets/Documentation/`. Commit it every time you tick a box. To tick a box, change `[ ]` to `[x]`. Each document: write the file name, or the link if it lives in Google Docs or Miro.

---

## 1. Concept Approval - Monday Nov 2

Documents

- [x] One-pager (`Concept.md`): player fantasy, 3 pillars, core loop, win and/or lose [cite: 1]
- [x] At least one risk and one opportunity (in `Concept.md`) [cite: 1]

In Unity

- [x] Quick greybox test of my biggest risk (Completed: Input System and Camera setups tested)

Peer review - reviewed by: ________   Go / Not yet: ________ What they said, in one line:

Feedback from Brian (write it down while you talk):

- Fun: what's fun about it, in one sentence:  
- Scope: the one thing to cut or simplify:  
- By Wednesday I will prove QUEPOINT is playable.

---

## 2. First Playable - Wednesday Nov 4

Required

- [x] The core loop is playable in a greybox (plain shapes, no art, no menus yet)
- [x] Must feature, with an hour estimate: Everything synchronized decently and working well (Physics, inputs, shooting alignment). Estimate: 8 hours.
- [x] Should feature, with an hour estimate: At least have some targets to get a feel of the connecting bullets. Estimate: 4 hours.
- [x] Could feature, with an hour estimate: Refined audio tracking & buffer polishes. Estimate: 3 hours.

Quick ones (still needed, rough is fine, a few minutes each)

- [x] Content scope: a short list of counts (Completed)
- [ ] Roadmap: 4 lines, one per stage, each with a date  
- [ ] Screen flow: a phone photo of a paper sketch of your screens (start, HUD, pause, win and/or lose)

### Content Scope
* **World & Gameplay:** 1 Level (Futuristic 3-lane corridor), 50+ Rhythm targets minimum, 1 Hazard type (Missing floor).
* **Audio & Visuals:** 5 Sound effects (SFX: Fire a shot, Intro, jump/land), 1 Music track, 1 menu mouse hover.
* **UI & Screens:** 1 Intro/Main Menu, 1 Settings Menu (Volume, Customizable Crosshair), 1 Unique In-game Pause Menu (with Reset Button), 1 Finish Screen (Score, accuracy, timing feedback).

In Unity, recommended

- [ ] A classmate played it while I watched without saying anything, and I wrote down what they did

Peer review - reviewed by: ________   Go / Not yet: ________ What they said, in one line:

---

## 3. Alpha - Monday Nov 9

Documents

- [x] DesignLog.md updated  
- [x] Cut list: what I dropped, and why

In Unity

- [ ] Every Must feature is in (rough is fine)  
- [ ] No new features after today

Peer review - reviewed by: ________   Go / Not yet: ________ What they said, in one line:

---

## 4. Beta - Wednesday Nov 11

Documents

- [ ] DesignLog.md updated  
- [ ] Bug list: what's broken, and which ones I'll fix

In Unity

- [ ] Playable from start to finish  
- [ ] Start screen, HUD, pause, and win and/or lose screen all work  
- [ ] A Windows build runs

Peer review - reviewed by: ________   Go / Not yet: ________ What they said, in one line:

---

## 5. Closing - Thursday Nov 12 and Sunday Nov 15

- [ ] Safety build: link sent to Brian on Teams, Thursday morning  
- [ ] Playtest record (`PlaytestRecord.md`): at least 3 testers  
- [ ] Post mortem (`PostMortem.md`), handed in at the playtest  
- [ ] Final build (optional improvements): link sent by Sunday  
- [ ] Going for VG? `VGClaim.md` committed, and a Teams message to Brian, by Sunday

---

## Time Log

| Date | Activity | Description | Time |
| :--- | :--- | :--- | :--- |
| 2026-10-23 | **Intro & Music** | Music creation in Suno.com (lyrics/prompts), video creation of my main intro to the main menu. | 8 hours |
| 2026-10-25 | **Shooting, Hazards & Torus Split** | Built the camera raycast shooting script, set up the death zone trigger under the map, and created the tool to split the torus into 7 pieces. | 6 hours |
| 2026-10-27 | **Player Movement & Intro Intro** | Programmed basic movement, jumping, and sliding controls. Started working on the first phase of the player intro animation. | 6 hours |
| 2026-10-28 | **Physics Polish & DeltaTime** | Adjusted player gravity and ground-snapping slides. Updated movement frames to use Time.deltaTime to ensure consistent game speed across different computer hardware. | 4 hours |
| 2026-10-31 | **Gun & Animations Complete** | Finished the final phase of the player intro animation. Added the weapon mesh along with hand animations, a muzzle flash effect, and implemented a weapon recoil and camera bopping script. | 8 hours |
| | **Total** | | **32 hours** |
