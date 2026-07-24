extends EnemyBase

class_name EnemyRanged

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

@export var shoot_cooldown: float = 2.5
var shoot_timer: float = 0.0

func _ready() -> void:
	super._ready()
	speed = 60.0 # Ranged enemy moves slower
	sprite.color = Color(0.9, 0.4, 0.1) # Orange tint

func _physics_process(delta: float) -> void:
	if not target_player:
		return

	shoot_timer += delta
	var dist = global_position.distance_to(target_player.global_position)
	
	# Stay at distance (150-300px away)
	if dist < 180.0:
		velocity.x = -sign(target_player.global_position.x - global_position.x) * speed
	elif dist > 320.0:
		velocity.x = sign(target_player.global_position.x - global_position.x) * speed
	else:
		velocity.x = 0

	if shoot_timer >= shoot_cooldown:
		shoot_timer = 0.0
		_fire_ranged_attack()

	if not is_on_floor():
		velocity.y += 980 * delta

	move_and_slide()

func _fire_ranged_attack() -> void:
	# Randomly choose attack type: Parabolic (0), High Flat (1), or Low Flat (2)
	var type = randi() % 3
	var dir = (target_player.global_position - global_position).normalized()
	
	match type:
		0: # Parabolic lob shot (dodged by moving left/right)
			var proj = ProjectileScene.instantiate() as Projectile
			get_parent().add_child(proj)
			proj.global_position = global_position + Vector2(0, -30)
			# Setup parabolic trajectory velocity
			var arc_vel = Vector2(dir.x * 350.0, -380.0)
			proj.setup(6.0, arc_vel, 0.0, false)
			
		1: # High Flat shot (must be dodged by crouching)
			var proj = ProjectileScene.instantiate() as Projectile
			get_parent().add_child(proj)
			# Spawn at high height (head level)
			proj.global_position = global_position + Vector2(0, -90)
			proj.setup(5.0, Vector2(dir.x, 0), 450.0, false)
			
		2: # Low Flat shot (must be dodged by jumping)
			var proj = ProjectileScene.instantiate() as Projectile
			get_parent().add_child(proj)
			# Spawn at low height (foot level)
			proj.global_position = global_position + Vector2(0, -20)
			proj.setup(5.0, Vector2(dir.x, 0), 450.0, false)
