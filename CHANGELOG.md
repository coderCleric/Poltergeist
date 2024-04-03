# 0.3.1
- Added controls for vertical movement.
- Adds a button to stop regular camera movement from changing altitude.
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