# 1.1.0
- Significant Additions
  - Gave dead players a "ghost head" as an avatar. This is visible to other dead players.
  - Added the ability to manifest when dead, allowing ghosts to be temporarily visible to living players.
  - Added the ability to interact with a few more things:
    - Loudhorn
    - Remotes
    - Radar Boosters
    - Whoopie Cushions (activated when ghosts fly through them)
- Fixes/Improvements
  - Ghosts can now see nametags over living players.
  - Made the ghost light come in from more angles and no longer affect fog (This allows the light to be made more intense).
  - Fixed a formatting error on interact prompts.
  - Added an element to the death HUD to tell the player the controls.
# 1.0.2
- Fixed an issue where looking straight up/down with the altitude locked would prevent movement forward.
- Made it so duplicate camera controllers will destroy themselves (fixed a conflict with suitsTerminal).
- Made the radar screen more consistent as a ghost.
# 1.0.1
- Resolved an issue where CullFactory would cause the ghost light to not work inside.
# 1.0.0
- Started using CSync to allow creation of many more options, such as:
  - Max power.
  - Power regen.
  - Alive for max power.
  - Pester aggro timespan.
  - Aggro hit requirement.
  - A bunch of cost settings.
- Add a bunch more interactions, such as:
  - Stopping steam valves.
  - Opening/closing ship doors.
  - Ringing the company bell.
  - Opening/closing Artifice hangar doors.
  - Pestering enemies.
- Made the ghost girl visible to dead players.
- Made the interact key rebindable.
# 0.3.1
- Added controls for vertical movement.
- Added a button to stop regular camera movement from changing altitude.
- Updated development environment, resulted in config name changing from "Poltergeist" to "coderCleric.Poltergeist". Sorry about that :(
- Tested for v50 compatability, everything seems to work properly!
# 0.3.0
- Started using InputUtils, allowing the remapping of the ghost light, accelerate, decelerate, and spectate mode switching actions.
- Added a power system that aims to prevent spam and provide an interesting scaling as the game goes on.
	- Interacting with objects now costs a certain amount of power, a resource which will regenerate over time.
	- The maximum power available to ghosts scales with the percentage of players that are dead, making them more powerful as the day goes on.
- Added support for additional interactables.
	- Generic support for noise maker props (thanks TheBlackEntity!).
	- Purchasable ship decorations.
	- Storage lockers in the facility.
	- Pneumatic doors.
# 0.2.0
- Added the ability to switch between the modded and vanilla spectate modes using the "Item Secondary Use" key (default Q).
	- Also added a "DefaultToVanilla" config option to determine what spectate mode you start in on death.
- Allow teleporting to player corpses and masked players.
- Removed the "RunBarebones" option.
# 0.1.1
- Added two config options to make things more customizeable.
	- RunBarebones: Tells the mod to not do most of the main functionality (good for people who like vanilla spectating, but want to play with others with the mod).
	- GhostLightIntensity: Determines the intensity of the global light for ghosts.
# 0.1.0
- First release with basic functionality.
