# 1.2.3
- Added a more robust fix for the NRE in DoSetup. Should give more info if it fails and try to recover more meaningfully.
# 1.2.2
- Added a bunch of debug messages to help troubleshoot some audio issues.
- june
# 1.2.1
- Changed the static audio played by manifesting to not suck.
# 1.2.0
- Made it so that ghosts can now play sounds with the 'V' key
  - The sounds can be changed by changing the contents of the 'sounds' folder. See the readme for more info.
- Manifesting now makes a noise, to better get living player's attention.
- If a ghost is inside the ship, they will move with the ship instead of having to chase it.
- The mine elevator can now be sent up and down by ghosts.
- Removed interactions from Manticoils since they were super inconsistent.
- Added a config option to enable/disable debug messages.
- Bug Fixes:
  - Edited control display to include the number key teleportation.
  - Attempted to fix a random error that would sometimes occur with enemies and props (hopefully it worked!)
  - Fixed a softlock that would occur when certain controls were completely unbound.
# 1.1.1
- Made it so that the mod actually loads on v61.
- Fixed some issues that were making it so that the whoopie cushion interaction wasn't working.
- I haven't had the time to do full testing in v61, but this should at least make the mod work. Please let me know on Github if you notice any issues!
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
