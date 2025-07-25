# SCP: Secret Laboratory Legacy
**SCP: Secret Laboratory Legacy** is an open-source revival of the first version of SCP: Secret Laboratory, faithfully ported to modern systems.

This project includes the following major updates:
* **Unity 6 migration:** Upgraded from Unity 2017 versions to Unity 6.
* **Networking overhaul:** Replaced deprecated UNet with Mirror.
* **Voice-Chat overhaul:** Currently disabled. ~~Replaced proprietary Dissonance with UniVoice.~~
* **Graphical Changes:** Replaced proprietary UBER with Standard Materials.

## Implementation of Cut-Content
### Weapons
| Status | Name                              | Description                                                                         |
|--------|-----------------------------------|-------------------------------------------------------------------------------------|
| ✅      | H.I.D. Prototype Handheld Firearm | It was easy to dodge, leading to the modern version to function like TF2’s Minigun. |
| ✅      | Ammometer                         | Designed to limit the number of UI elements on screen.                              |
| ✅      | Disarmer                          | Implemented to prevent players from disarming each other freely.                    |
| ✅      | Coin                              | Intended for use with SCP-294.                                                      |
| ✅      | (Empty) Cup                       | Meant to receive a substance dispensed by SCP-294.                                  |
| ✅      | Fusion Rifle                      | Lore-wise, created by upgrading weapons through SCP-914.                            |
| ❌      | Fusion Sniper Rifle               | Likely never implemented due to the game not being designed to support it.          |
| ✅      | Fusion Pistol                     | Lore-wise, created by upgrading weapons through SCP-914.                            |
| ✅      | Positronic Grenade                | Said to use the same technology found in tesla gates.                               |
| ✅      | Logicer                           | Was just an asset flip.                                                             |
| ✅      | Smoke Grenade                     | Removed due to poor particle effects at the time.                                   |
| ✅      | Scorpion                          | Was just an asset flip.                                                             |
| ✅      | M1911 Pistol                      | Was just an asset flip.                                                             |
| ✅      | Ak-47 Ammo Magazine               | Self explanatory                                                                    |
| ✅      | SFA Fusion Ammo                   | Self explanatory                                                                    |

### Keycards
The Breaking Card, which was eventually reworked into the Chaos Insurgency Keycard.

### SCPs
| Status | SCP Item # | Description                                                                                                                                                                                                                                                                                                                                                            |
|--------|------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ✅      | SCP-457    | Players within 2 meters of SCP-457 will be ignited for 5 seconds, taking 2 HP of damage every 2 seconds. In addition, SCP-457 does have a containment room/cell. It used the Unity standard asset's fire on the old SCP-106 model.                                                                                                                                     |
| ❌      | SCP-106    | An unused ability was SCP-106's ultimate points which could unlock abilities.                                                                                                                                                                                                                                                                                          |
| ❌      | SCP-294    | SCP-294 was located in the now-removed cafeteria room in the Entrance Zone. Interacting with the machine would consume a coin and dispense a random substance in a cup.                                                                                                                                                                                                |
| ❌      | SCP-079    | Before being disabled in version 1.0.0, SCP-079 could hack doors and tesla gates using a menu-based system after a timed hack, with controls mapped to number keys and WASD for camera movement. Earlier alpha builds had him limited to viewing only HCZ cameras via a clickable map, with no hacking, generators, or AP system, and could even be killed by gunfire. |
| ❌      | SCP-096    | SCP-096 was implemented in version 3.0.0. At the time, it could not see targets through walls and had to break through any closed doors in its path. It was one of the shorter SCPs, moving at 4.7 m/s when docile and 12.36 m/s while enraged.                                                                                                                        |
| ❌      | SCP-049-2  | Male variants of MTF, Chaos Insurgency, and SCP-049-2 Class-D zombies were created at one point, but only the male Class-D model is currently used in the game. The game had no way of knowing which class a player last played as, so this feature was not-implemented.                                                                                               |
| ❌      | SCP-939    | Implemented in version 7.0.0. Interestingly, the old SCP-939 room was referred to as the "SCP-096 room" and "test room."                                                                                                                                                                                                                                               |

### Human Classes
The old ragdoll models were created because the game had no way of knowing which class a player last played as.

| Status | Class Variant                                 | Description                                                                                                                                      |
|--------|-----------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| ✅      | Jan Kalous (D-55240) - **Default**            | Male version. Starts with no items and a maximum health of 100 HP. Walk speed is 7 m/s, and sprint speed is 6.5 m/s for up to 7 seconds.         |
| ❌      | Charlotte Richer or Niklas Leonidas (D-34910) | Male version. Starts with a janitor keycard and a maximum health of 80 HP. Walk speed is 5 m/s, and sprint speed is 6.5 m/s for up to 7 seconds. |
| ❌      | Kate Norris (D-29640)                         | Female version. Starts with no items and a maximum health of 110 HP. Walk speed of 5 m/s, and sprint speed is 6.5 m/s for up to 3 seconds.       |
| ❌      | Claudia Meyer (D-31972)                       | Female version. Starts with a medkit and a maximum health of 70 HP. Walk speed of 6 m/s, and sprint speed is 8 m/s for up to 8 seconds.          |
| ❌      | Unknown Name (D-CLASS)                        | Male version. Starts with no items and a maximum health of 170 HP. Walk speed of 3.5 m/s, and sprint speed is 5 m/s for up to 5 seconds.         |
| ✅      | Unknown Name (Scientist) - **Default**        | Male version with glasses.                                                                                                                       |
| ❌      | Pszemek (Scientist)                           | Male version without glasses.                                                                                                                    |
| ❌      | Pani Tomek (Scientist)                        | Female version with Ginger Hair.                                                                                                                 |
| ❌      | Grazynka (Scientist)                          | Female version with Black Hair.                                                                                                                  |
| ✅      | MTF - **Default**                             | Male version wearing a helmet.                                                                                                                   |
| ❌      | MTF Security                                  | Male version wearing hats instead of a helmet.                                                                                                   |
| ❌      | Unknown Name (MTF)                            | Female version with a helmet and wearing a gas mask.                                                                                             |
| ❌      | Unknown Name (MTF)                            | Female version without a helmet, wearing a gas mask.                                                                                             |

### Tutorials / Shooting Range
The old tutorials couldn’t be implemented properly because Unity, at the time, lacked a clean way to sync core gameplay with tutorial content—particularly when using nested prefabs.

The shooting range, it was difficult to keep two versions of the game in sync—one for debugging and one for release—which made maintaining separate tutorial scenes or logic more complicated.

| Status | Tutorial               | Description                                                 |
|--------|------------------------|-------------------------------------------------------------|
| ✅      | The basics             | Introduces the player to keycards.                          |
| ✅      | Using weapons          | Introduces & challenges the player to use the M1911 pistol. |
| ✅      | SCP-914                | Introduces the player to upgrading keycards.                |
| ❌      | General class overview | Never completed/implemented.                                |

### MTF Helicopter Controls & Context
Status: ❌

The MTF helicopter appears to have been intended as a player-controllable vehicle, with the following keybinds:  
* `Q / E` – Turn Left / Right  
* `W / S` – Turn Forward / Back  
* `A / D` – Tilt Left / Right  
* `Shift` – Ascend  
* `Space` – Descend  
* `M` – Toggle Music

Players would have needed to land on a helipad and navigate through checkpoints, with an arrow indicator showing their heading.

The *Ride of the Valkyries* track found in the game files. This is likely a reference to its iconic use in *Apocalypse Now* (1979), where the U.S. 1/9 Air Cavalry plays the piece over helicopter-mounted loudspeakers during a helicopter assault.

*This conclusion is based on in-game assets, so take it with a grain of salt.*

## References
[SCPSL Fandom Wiki Unused/Cut/Legacy Content](https://scp-secret-laboratory.fandom.com/wiki/Unused/Cut/Legacy_Content)

[SCPSL's Original Concept](https://steamcommunity.com/sharedfiles/filedetails/?id=862448114&searchtext=breach)
