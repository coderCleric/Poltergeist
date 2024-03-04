# Poltergeist
Adds things for dead players to do by letting them fly around in a freecam and interact with certain objects.

**Features**:
- Instead of spectating normally, fly around in a freecam to watch what your friends are doing, or just to explore the map.
- Teleport to players (or what's left of them) with the press of a button.
- Toggle a global light to help you see in dark areas when dead.
- Interact with certain types of objects. Mess with your friends, or try to communicate with them from *beyond the veil!*

# Controls
- **Movement**: Same as how you move your living character, except that moving forwards and backwards can also change your elevation.
- **Sprint**: Pressing the sprint key will multiply the camera movement speed by 5.
- **Speed**: Accelerate and decelerate by scrolling the mouse wheel. Rebindable in the game settings.
- **Toggle Light**: Toggle a global light that helps you see in dark areas while dead with the left mouse button. Rebindable in the game settings.
- **Teleportation**: Pressing a number key will teleport you to the corresponding player, based on join order. This means that the host will be 1, the first player to join will be 2, and so on.
- **Interact**: Pressing interact (default "e") on the objects listed below will let you interact with them as if you were still alive. Troll your friends, or try to help them out!
- **Mode Switching**: Switch between vanilla and modded spectate modes using the 'Q' key. Rebindable in the game settings.

# Interactible Objects
- **Doors**: Can be opened and closed.
- **Ship Light Switch**: Can be flipped on and off.
- **Noisemaker Props**: Items that make noise, such as the clownhorn, airhorn, and cash register.
- **Boombox**: Can be turned on and off on the ground.
- **Ship Decorations**: All vanilla purchasable ship decorations that have an interact feature can be used by ghosts.
- **Facility Lockers**: The doors of the storage lockers occasionally found in the facility.
- **Pneumatic Doors**: Pneumatic Doors can be opened and closed, assuming they have power.

# Power
Interacting with the mortal plane isn't easy, and requires a certain amount of effort on the part of the ghost. This is represented by the power system.
- Interacting with different objects consumes power, a resource that regenerates slowly over time.
- As more and more players die, the maximum power available to ghosts will increase.
- Some actions require so much power that they may not be possible unless a certain number of players are dead. This prevents ghosts from doing anything grand at the start, but they will become more and more capable as the day goes on!

# Config Options
- **DefaultToVanilla**: If true, dying will start you in the vanilla spectate mode.
- **GhostLightIntensity**: Modifies the intensity of the light available to ghosts. (WARNING; this game has a lot of fog, so really higher light intensities may actually *decrease* visibility.)

# Installing
Just the same as any of the other BepInEx-based mod, put the folder for the mod in BepInEx/plugins and it should work. This mod is not quite client-side, so certain bugs may occur if you play with it when other players don't have it installed.
(Specifically, noisy props and boomboxes won't work properly for the last person who picked them up, if that person doesn't have the mod installed.)

# Known Bugs/Misc Notes
- Haven't quite figured out how to get weather effects to work properly, so for now it's just based on the last player you teleported to (or where you died if you haven't teleported to anyone yet). This means that, if you died in the rain and fly down to the dungeon, you'll see rain inside.
- The dungeon is located way below the surface, so if you want to explore it you need to either teleport to someone who's already there, or just fly through the ground.
- If you find any other bugs feel free to post an issue on the [Github repo](https://github.com/coderCleric/Poltergeist)