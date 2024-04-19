# Poltergeist
Adds things for dead players to do by letting them fly around in a freecam and perform various interactions.

**Features**:
- Instead of spectating normally, fly around in a freecam to watch what your friends are doing, or just to explore the map.
- Teleport to players (or what's left of them) with the press of a button.
- Toggle a global light to help you see in dark areas when dead.
- Interact with certain types of objects. Mess with your friends, or try to communicate with them from *beyond the veil!*.

# Controls
- **Movement**: Same as how you move your living character, except that moving forwards and backwards can also change your elevation.
- **Vertical Movement**: Dedicated controls to move up and down, defaulting to r and f. Rebindable in the game settings.
- **Altitude Lock**: Toggleable altitude lock, preventing the camera from moving up and down except via the dedicated vertical controls. Rebindable in the game settings.
- **Sprint**: Pressing the sprint key will multiply the camera movement speed by 5.
- **Speed**: Accelerate and decelerate by scrolling the mouse wheel. Rebindable in the game settings.
- **Toggle Light**: Toggle a global light that helps you see in dark areas while dead with the left mouse button. Rebindable in the game settings.
- **Teleportation**: Pressing a number key will teleport you to the corresponding player, based on join order. This means that the host will be 1, the first player to join will be 2, and so on.
- **Interact**: Pressing interact (default "e") on the objects listed below will let you interact with them as if you were still alive. Troll your friends, or try to help them out! Rebindable in the game settings.
- **Mode Switching**: Switch between vanilla and modded spectate modes using the 'Q' key. Rebindable in the game settings.

# Interactable Objects
- **Doors**: Can be opened and closed.
- **Pneumatic Doors**: Pneumatic Doors can be opened and closed, assuming they have power.
- **Hangar Doors**: Flip the levers on Artifice to open and close the big hangar doors.
- **Valves**: Can stop them from gushing steam.
- **Ship Light Switch**: Can be flipped on and off.
- **Ship Door Buttons**: Allows the ship doors to be opened and closed.
- **Company Bell**: Ring the bell at the company building!
- **Noisemaker Props**: Items that make noise, such as the clownhorn, airhorn, and cash register.
- **Boombox**: Can be turned on and off on the ground.
- **Ship Decorations**: All vanilla purchasable ship decorations that have an interact feature can be used by ghosts.
- **Facility Lockers**: The doors of the storage lockers occasionally found in the facility.
- **Pestering Enemies**: Pester enemies to disrupt their current actions. Be careful though, do it too much and they might get mad at the closest player that they can see!
  - *Note: This just has you hit the enemy for 0 damage. Some enemies don't do anything interesting when hit.*

# Power
Interacting with the mortal plane isn't easy, and requires a certain amount of effort on the part of the ghost. This is represented by the power system.
- Interacting with different objects consumes power, a resource that regenerates slowly over time.
- As more and more players die, the maximum power available to ghosts will increase.
- Some actions require so much power that they may not be possible unless a certain number of players are dead. This prevents ghosts from doing anything grand at the start, but they will become more and more capable as the day goes on!

# Config Options
This mod uses Sigurd's CSync to enforce most config settings from the host onto joining players. This also means that everyone you want to play with has to have the mod installed in order to join the same lobby.

**Client-Side Configs**<br />
These settings aren't synced so that you can tailor them to your personal preferences.
- **DefaultToVanilla**: If true, dying will start you in the vanilla spectate mode.
- **GhostLightIntensity**: Modifies the intensity of the light available to ghosts. (WARNING; this game has a lot of fog, so really high light intensities may actually *decrease* visibility.)

**Synced Configs**<br />
These settings are synced, so whatever settings the host has will be used by everyone
- **Max Power**: The maximum power that can ever be available to ghosts.
- **Power Regen**: How much power ghosts regenerate every second.
- **Alive for Max Power**: The maximum number of players that can still be alive for the ghosts to achieve max power.
  - (Example: if this is set to 3, then ghosts will be at maximum power as long as no more than 3 players are left alive).
- **Pester Aggro Timespan**: How many seconds have to pass before enemies forget that they've been pestered by any ghosts.
- **Aggro Hit Requirement**: How many times do ghosts have to pester the same enemy before that enemy gets mad at the nearest player with line-of-sight.
- **Costs**: Many different interaction costs are configurable, with the categories being:
  - **Regular Doors**
  - **Big Doors**: Both the pressurized facility doors as well as the Artifice hangar doors fall under this.
  - **Noisy Items**
  - **Steam Valves**
  - **Ship Doors**
  - **The Company Bell**
  - **Enemy Pestering**
  - **Miscellaneous**: This is just any interaction not covered by the other categories, such as ship decorations.

# Installing
Just the same as any of the other BepInEx-based mod, put the folder for the mod in BepInEx/plugins and it should work, or just install it via a mod manager (such as r2modman). Make sure to install any dependencies listed on Thunderstore as well. This mod has features that require all players to have the mod installed. If you have the mod active, players without it will be unable to join you, and you will be unable to join them.

# Known Bugs/Misc Notes
- Haven't quite figured out how to get weather effects to work properly, so for now it's just based on the last player you teleported to (or where you died if you haven't teleported to anyone yet). This means that, if you died in the rain and fly down to the dungeon, you'll see rain inside.
- The dungeon is located way below the surface, so if you want to explore it you need to either teleport to someone who's already there, or just fly through the ground.
- If you find any other bugs feel free to post an issue on the [Github repo](https://github.com/coderCleric/Poltergeist)
