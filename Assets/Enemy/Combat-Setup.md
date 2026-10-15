# Combat and health settings

- Default Health is 100.
- In the Health Inspector, toggle Infinite Health on/off before or during Play Mode.
- Infinite Health prevents health loss and death, while still showing red hit feedback.
- Restore Full Health is available from the Health component context menu during Play Mode.
- Flash On Hit, Hit Color, Flash Duration, and Hit Renderers control the character's red flash.
- The player also gets a red screen flash. Toggle Show Player Screen Flash to disable it.
- Renderers are found automatically when Hit Renderers is empty. Materials using _BaseColor or _Color are supported.
- Ranged weapons Gun and FPS_Derringer deal 34 damage. With 100 health, remaining HP is 66, 32, then 0.
- Changing weapon damage, zombie health, or enabling Infinite Health changes the three-shot result.
- Normal Zombie Personality uses Player Speed Multiplier 0.6: player speed 5 means zombie speed 3.
- Sensitive uses 0.75, Deaf 0.5, Timid 0.7.
- Turn off Use Player Relative Speed in a personality to use its fixed Movement Speed.
- Nonessential comments were removed from AI, Health, sight/noise scripts, FPSController and RangedWeaphon.
- Compilation checked against Unity 6000.3.9f1 assemblies. Play Mode visual verification is still pending.