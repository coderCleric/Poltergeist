# Poltergeist
Adds things for dead players to do by letting them fly around in a freecam and interact with certain objects.

Features:
- Instead of spectating normally, fly around in a freecam to watch what your friends are doing, or just to explore the map.
- Teleport to players with the press of a button.
- Toggle a global light to help you see in dark areas when dead.
- Interact with certain types of objects. Mess with your friends, or try to communicate with them from *beyond the veil!*

# Controls
- **Movement**: Same as how you move your living character, except that moving forwards and backwards can also change your elevation.
- **Sprint**: Pressing the sprint key will multiply the camera movement speed by 5.
- **Speed**: You can hold the "+" key to increase your speed, and the "-" key to decrease it.
- **Toggle Light**: Pressing "use item" (default LMB) will toggle a global light that helps you see in dark areas while dead.
- **Teleportation**: Pressing a number key will teleport you to the corresponding player, based on join order. This means that the host will be 1, the first player to join will be 2, and so on.
- **Interact**: Pressing interact (default "e") on the objects listed below will let you interact with them as if you were still alive. Troll your friends, or try to help them out!

# Interactible Objects
- **Doors**: Can be opened and closed.
- **Ship Light Switch**: Can be flipped on and off.
- **Airhorn/Clownhorn**: Can be honked on the ground.
- **Boombox**: Can be turned on and off on the ground.

# Installing
Just the same as any of the other BepInEx-based mod, put the folder for the mod in BepInEx/plugins and it should work. This mod is client-side, so having it installed won't cause the game to break if somebody else doesn't have it.

# Known Bugs/Misc Notes
- Haven't quite figured out how to get weather effects to work properly, so for now it's just based on the last player you teleported to (or where you died if you haven't teleported to anyone yet). This means that, if you died in the rain and fly down to the dungeon, you'll see rain inside.
- The dungeon is located way below the surface, so if you want to explore it you need to either teleport to someone who's already there, or just fly through the ground.