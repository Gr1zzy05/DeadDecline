# Stealth hits and viewmodel timing

Unaware zombies die from one weapon hit when One Hit When Unaware is enabled on Health.
A zombie is alerted while it sees the player or is actively chasing/investigating/fleeing within its memory duration.
When awareness expires it can become vulnerable to a stealth hit again.
Alerted zombies take normal weapon damage (standard guns: 34 damage, three hits at 100 HP).
Infinite Health overrides both normal and stealth damage.

WeaponSystem > Impact Normalized Time determines when damage, hitbox debug and shot effects occur:
0 is the start of the Attack animation; 1 is its end.
Bat uses 0.45, baguette uses 0.15, ranged prefabs use 0.05.
Adjust the value while watching the viewmodel to align it with the visible contact/fire frame.
Attack animation speed changes automatically affect timing because it uses Animator normalized time.
Each swing/shot commits at most once.
A weapon without an assigned animation controller attacks immediately.

Animator state tags:
Equip blocks attacks during pickup/equip.
Reload blocks attacks during reload.
Attack identifies the animation used to synchronize the hit and blocks another attack until recovery.
Existing bat, baguette and Derringer controllers have been configured.
New viewmodels must use these state tags on the relevant layer-0 states.
Attack animations must reach Impact Normalized Time before leaving their Attack state.
Interrupted or unequipped weapons cancel pending hits.
Shots consume ammunition only when the animation reaches its impact frame.

Scripts compile against the installed Unity 6000.3.9f1 assemblies.
Play Mode testing and exact visual impact alignment are still pending.