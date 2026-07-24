extends EnemyBase

class_name EnemyBossCynthia

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

var arrow_timer: float = 0.0
var arrow_cooldown: float = 1.4

func _ready() -> void:
	super._ready()
	max_hp = 350.0
	hp = max_hp
	speed = 100.0 # Fast mobility archer
	gold_reward = 25
	sprite.color = Color(0.2, 0.9, 0.5) # Forest green/teal archer boss aura
	
	# Scale boss size larger
	scale = Vector2(1.5, 1.5)

func _physics_process(delta: float) -> void:
	if not target_player:
		return

	arrow_timer += delta
	var dist = global_position.distance_to(target_player.global_position)
	
	# Maintain tactical distance
	if dist < 200.0:
		velocity.x = -sign(target_player.global_position.x - global_position.x) * speed
	elif dist > 350.0:
		velocity.x = sign(target_player.global_position.x - global_position.x) * speed
	else:
		velocity.x = 0

	if arrow_timer >= arrow_cooldown:
		arrow_timer = 0.0
		_fire_windstorm_arrows()

	if not is_on_floor():
		velocity.y += 980 * delta

	move_and_slide()

func _fire_windstorm_arrows() -> void:
	if not target_player: return
	var dir = (target_player.global_position - global_position).normalized()
	
	# High piercing arrow and low arrow combo
	var proj_high = ProjectileScene.instantiate() as Projectile
	get_parent().add_child(proj_high)
	proj_high.global_position = global_position + Vector2(0, -70)
	proj_high.setup(10.0, Vector2(dir.x, 0), 550.0, false)

	var proj_low = ProjectileScene.instantiate() as Projectile
	get_parent().add_child(proj_low)
	proj_low.global_position = global_position + Vector2(0, -25)
	proj_low.setup(10.0, Vector2(dir.x, 0), 550.0, false)

func _die() -> void:
	# Unlock Elven Archer Cynthia Vessel upon defeat!
	GameManager.UnlockFemale("girl_cynthia")
	print("[BOSS DEFEATED] Elven Archer Cynthia defeated! Vessel [girl_cynthia] Unlocked!")
	super._die()
