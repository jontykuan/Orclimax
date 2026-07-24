extends EnemyBase

class_name EnemyBossLydia

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

var spell_timer: float = 0.0
var spell_cooldown: float = 2.0

func _ready() -> void:
	super._ready()
	max_hp = 300.0
	hp = max_hp
	speed = 50.0
	gold_reward = 10
	sprite.color = Color(0.9, 0.2, 0.9) # Bright magenta mage boss aura
	
	# Scale boss size larger
	scale = Vector2(1.6, 1.6)

func _physics_process(delta: float) -> void:
	if not target_player:
		return

	spell_timer += delta
	var dist = global_position.distance_to(target_player.global_position)
	
	# Keep distance
	if dist > 250.0:
		velocity.x = sign(target_player.global_position.x - global_position.x) * speed
	else:
		velocity.x = 0

	if spell_timer >= spell_cooldown:
		spell_timer = 0.0
		_cast_magic_barrage()

	if not is_on_floor():
		velocity.y += 980 * delta

	move_and_slide()

func _cast_magic_barrage() -> void:
	if not target_player: return
	var dir = (target_player.global_position - global_position).normalized()
	
	# Fire 3 spread projectiles
	for i in range(-1, 2):
		var proj = ProjectileScene.instantiate() as Projectile
		get_parent().add_child(proj)
		proj.global_position = global_position + Vector2(0, -60)
		var spread_dir = dir.rotated(deg_to_rad(i * 15))
		proj.setup(12.0, spread_dir, 400.0, false)

func _die() -> void:
	# Unlock Elven Mage Lydia Vessel upon defeat!
	GameManager.UnlockFemale("girl_lydia")
	print("[BOSS DEFEATED] Elven Mage Lydia defeated! Vessel [girl_lydia] Unlocked!")
	super._die()
