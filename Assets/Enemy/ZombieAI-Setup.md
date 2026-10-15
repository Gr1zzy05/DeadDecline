# Zombie AI — DeadDecline / TestField

ระบบที่เชื่อมแล้ว:
- ZombieAI มองเห็นในกรวยด้านหน้าและต้องไม่มีสิ่งกีดขวางใน Obstacle Mask
- เสียงเดิน/ยิงตรวจจับได้ 360 องศา เมื่อความดังที่ได้รับถึง Hearing Threshold
- เสียงพาไปตำแหน่งที่เกิดเสียง ไม่รู้ตำแหน่งผู้เล่นที่กำลังหลบโดยอัตโนมัติ
- เห็นผู้เล่นแล้วไล่ เข้า Attack Range แล้วทำ damage ผ่าน Health และเรียก On Attack
- มองไม่เห็นแล้วตามตำแหน่งสุดท้ายตาม Memory Duration ก่อนกลับ Idle
- เพิ่ม Health (100) ให้ Player prefab โดยใช้ Health เดิมของโปรเจกต์ เมื่อหมดจะ Destroy ผู้เล่น
- TestField ใช้ Normal และ Rigidbody ของ Zombie เป็น Kinematic เพื่อไม่ให้ขัดกับ NavMeshAgent

## เปลี่ยนนิสัยผ่าน Inspector
เลือก Zombie > ZombieAI > Personality แล้วลาก asset จาก Assets/Enemy:
Normal: hearing 30, threshold 0.6, sensitivity 1
Sensitive: hearing 60, threshold 0.25, sensitivity 1.5
Deaf: hearing 0, sensitivity 0
Timid: ได้ยินแล้วหนีออกจากแหล่งเสียง แต่เมื่อเห็นผู้เล่นยังไล่/โจมตีตามปกติ

สร้างชุดใหม่ Assets > Create > Dead Decline > Zombie Personality
ปรับ Sight Range / Field Of View / Hearing Range / Hearing Threshold / Hearing Sensitivity
Use Player Relative Speed / Player Speed Multiplier / Movement Speed / Memory Duration / Investigation Duration / Attack Range / Damage / Cooldown
Noise Reaction เลือก Ignore, Investigate หรือ Flee

หากต้องการเขียนนิสัยเพิ่ม ให้ subclass ZombiePersonality, ใส่ CreateAssetMenu,
override ReactToNoise(NoiseSystem.Noise noise, float perceivedLoudness), แล้วสร้าง asset ลากเข้า Personality
อย่าเก็บสถานะเฉพาะ Zombie ใน asset เพราะหลายตัวสามารถแชร์ asset เดียวกันได้

## เสียงเดินและปืน
FPSController: Footstep Loudness = 1, Full Volume Radius = 2, Max Radius = 10, Step Distance = 1.5
เสียงขึ้นกับระยะที่เคลื่อนที่จริงบนพื้น จึงไม่ส่งเสียงเมื่อยืนเฉยหรือดันกำแพง
RangedWeaphon: Gunshot Loudness = 5, Full Volume Radius = 5, Max Radius = 50
แต่ละปืนปรับค่าเสียงของตัวเองได้ เสียงส่งเฉพาะเมื่อยิงสำเร็จ

ความดังรับ = Loudness × falloff × Hearing Sensitivity
falloff = 1 ภายใน Full Volume Radius และลดเป็นเส้นตรงจนเป็น 0 ที่ Max Radius
เสียงเดิน 1: ระยะ 2m = 1, ระยะ 6m = 0.5, ระยะ 10m = 0
Normal threshold 0.6 จึงไม่ได้ยินเสียงเดินที่ 6m แต่ Sensitive ได้ยิน
เสียงต้องอยู่ใน Hearing Range ของ Zombie ด้วย
ระบบนี้เป็น gameplay event แยกจาก AudioSource และยังไม่มีการลดเสียงผ่านกำแพง

ส่งเสียงจากประตู/สิ่งของเพิ่มได้:
NoiseSystem.Emit(transform.position, 2f, 2f, 20f, transform, NoiseSystem.NoiseKind.Other);
