# Bug List - Final Project

**Name:** Glenn Magnell
**Game title:** CUEPOINT

Put this file in `Assets/Documentation/`. Required at Beta (Wednesday Nov 11).
Write a bug down the moment you see it. Fix the worst ones first.

## How to Write a Bug
What I did -> What happened -> What I expected.
> Example: "Pressed Esc during the last 10 seconds -> the timer kept counting -> the timer should stop when I pause."

## Fix Order
1. Stops the game (crash, can't start, can't win or lose, stuck)
2. Breaks a G requirement
3. Confuses a tester
4. Polish

Happens every time, or in the first 30 seconds? Move it up one level.
Test every fix in the Windows build, not just in the editor.

## My Bugs

| #   | What I did -> What happened -> What I expected                                   | How often  | Found by | Order (1-4) | Status                                                                      |
| --- | -------------------------------------------------------------------------------- | ---------- | -------- | ----------- | --------------------------------------------------------------------------- |
| 1   | In a early build where I tested limits, My character ran straight trought ramps. | Every time | Me       | 1           | Fixed<br>(No ramps added to final build but added physics to materials)<br> |
| 2   | Sync with targets                                                                | Often      | Me       | 2           | Fixed                                                                       |
| 3   | Stopped running after intro cinematic on player                                  | Every time | Me       | 3           | Fixed                                                                       |

How often: every time / sometimes / once. Found by: me / a tester.
Status: Fixed / Will fix / Won't fix (because ___).
"Won't fix" is a real answer. Studios ship with known bugs every time. Just write why.

## My Bugs (Own comments)

1.  Might have jump pads and ramps in future levels.
2.  Might change the view of the targets to ephesis the targets perfekt time better.
