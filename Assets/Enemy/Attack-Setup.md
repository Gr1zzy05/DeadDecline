# Attack hitboxes

Zombie:
- Zombie Personality > Attack Cooldown: 2 seconds by default for all provided personalities.
- ZombieAI > Attack Windup: 0.5 seconds before the hit. Leaving range or sight cancels the windup.
- ZombieAI > Attack Point: optional hand/strike transform.
- Attack Offset: local offset from Attack Point, or from the zombie transform if no point is assigned.
- Attack Radius and Attack Mask define the actual sphere used to check damage.
- Show Hitbox On Attack, Hitbox Display Duration and Hitbox Color control the Game-view visualization.
- Attack Range in the personality controls when the zombie starts trying to attack; it is separate from the damage sphere.
- Only the current visible target inside the damage sphere is damaged, once per attack.

Bat / melee weapon:
- Meleeweaphon > Strike Point, Hitbox Offset, Hit Radius and Hit Mask define the damage sphere.
- Owner colliders are ignored, and each Health component is damaged only once per swing.
- Show Hitbox On Attack, Hitbox Display Duration and Hitbox Color control the visible sphere.
- Attack Cooldown controls swing rate.

Guns:
- RangedWeaphon > Range and Hit Mask control the shot ray.
- Show Hitbox On Attack displays the ray and a small endpoint marker.
- The shot ignores its owner's colliders.

Debug spheres and rays appear in Game view on attacks without requiring Gizmos.
Selecting the bat or zombie also shows a persistent sphere preview in Scene view with Gizmos enabled.
Displayed hitboxes remain at the sampled attack position; they do not move with the attacker after the hit.
The display duration is only visual and does not cause repeated damage.

Validation:
Scripts compile against Unity 6000.3.9f1 assemblies, including editor code.
Actual animation alignment, red hitbox rendering and combat behavior still require Play Mode verification.