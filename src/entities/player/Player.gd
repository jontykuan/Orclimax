extends CharacterBody2D

class_name Player

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

@export var gravity: float = 980.0
@export var jump_velocity: float = -550.0

@onready var orc_sprite: ColorRect = $CompositeVisuals/Orc
@onready var vessel_sprite: ColorRect = $CompositeVisuals/Vessel
@onready var particles: CPUParticles2D = $CompositeVisuals/HeartParticles
@onready var camera: Camera2D = $Camera2D

# Movement & Dash variables
var move_speed: float = 250.0
var facing_direction: Vector2 = Vector2.RIGHT

var is_dashing: bool = false
var dash_timer: float = 0.0
var dash_duration: float = 0.15
var dash_cooldown_timer: float = 0.0
var dash_cooldown: float = 0.6

# Double tap tracking
var last_press_left_time: float = -999.0
var last_press_right_time: float = -999.0
var last_press_up_time: float = -999.0
var last_press_down_time: float = -999.0
var DOUBLE_TAP_DELAY: float = 0.25

# Action states & Cooldowns
var parry_timer: float = 0.0
var parry_cooldown_timer: float = 0.0
var is_parrying: bool = false

var thrust_cooldown_timer: float = 0.0
var is_vessel_attached: bool = true

# Crouching collision variables
@export var crouch_speed_multiplier: float = 0.4
@export var crouch_height_ratio: float = 0.6
@export var visual_base_scale: float = 4.0

@onready var collision_shape: CollisionShape2D = $CollisionShape2D
var original_collision_height: float = 256.0
var original_collision_pos_y: float = -128.0
var is_crouching: bool = false

func _ready() -> void:
	# Add to group so enemies can identify player
	add_to_group("player")
	
	if GameConfig:
		gravity = GameConfig.BaseGravity
		jump_velocity = GameConfig.BaseJumpVelocity
		move_speed = GameConfig.BaseMoveSpeed
		dash_duration = GameConfig.DashDuration
		dash_cooldown = GameConfig.DashCooldown
		DOUBLE_TAP_DELAY = GameConfig.DoubleTapDelay
		crouch_speed_multiplier = GameConfig.CrouchSpeedMultiplier
		crouch_height_ratio = GameConfig.CrouchHeightRatio
		visual_base_scale = GameConfig.VisualBaseScale

	# Connect to C# CombatManager signals
	var cm = CombatManager
	cm.connect("WeaponFired", _on_weapon_fired)
	cm.connect("ClimaxTriggered", _on_climax_triggered)
	
	# Update local stats initially
	move_speed = cm.MoveSpeed

	$CompositeVisuals.scale = Vector2(visual_base_scale, visual_base_scale)

	# Store original collision shape metrics
	if collision_shape and collision_shape.shape is RectangleShape2D:
		original_collision_height = collision_shape.shape.size.y
		original_collision_pos_y = collision_shape.position.y

func _physics_process(delta: float) -> void:
	# Add gravity
	if not is_on_floor():
		velocity.y += gravity * delta

	# Synchronize stats dynamically from CombatManager
	move_speed = CombatManager.MoveSpeed

	# Handle Cooldown Timers
	if dash_cooldown_timer > 0: dash_cooldown_timer -= delta
	if parry_cooldown_timer > 0: parry_cooldown_timer -= delta
	if thrust_cooldown_timer > 0: thrust_cooldown_timer -= delta

	# Handle Active Action Timers
	if is_dashing:
		dash_timer -= delta
		if dash_timer <= 0: is_dashing = false
		
	if is_parrying:
		parry_timer -= delta
		if parry_timer <= 0: is_parrying = false

	if not is_dashing:
		_handle_normal_movement(delta)

	move_and_slide()
	_update_visuals_and_animations()

func _handle_normal_movement(_delta: float) -> void:
	var curr_time := Time.get_ticks_msec() / 1000.0

	# 1. Double-Tap UP: Precise Guard / Parry
	if Input.is_action_just_pressed("ui_up"):
		if curr_time - last_press_up_time < DOUBLE_TAP_DELAY and parry_cooldown_timer <= 0:
			_start_parry()
		elif is_on_floor():
			velocity.y = jump_velocity
		last_press_up_time = curr_time

	# 2. Double-Tap DOWN: Heavy Thrust / AOE Knockback
	if Input.is_action_just_pressed("ui_down"):
		if curr_time - last_press_down_time < DOUBLE_TAP_DELAY and thrust_cooldown_timer <= 0:
			_execute_heavy_thrust()
		last_press_down_time = curr_time

	# Handle Horizontal Input
	var dir := Input.get_axis("ui_left", "ui_right")
	
	# Crouch logic (reduce speed and scale down parent visual node)
	is_crouching = Input.is_action_pressed("ui_down")
	var speed_multiplier := 1.0
	
	if is_crouching:
		speed_multiplier = crouch_speed_multiplier
		$CompositeVisuals.scale.y = visual_base_scale * crouch_height_ratio
		
		# Shrink collision shape downwards (bottom remains at y=0)
		if collision_shape and collision_shape.shape is RectangleShape2D:
			collision_shape.shape.size.y = original_collision_height * crouch_height_ratio
			collision_shape.position.y = -original_collision_height * crouch_height_ratio / 2.0
	else:
		$CompositeVisuals.scale.y = visual_base_scale
		
		# Restore original collision shape size
		if collision_shape and collision_shape.shape is RectangleShape2D:
			collision_shape.shape.size.y = original_collision_height
			collision_shape.position.y = original_collision_pos_y

	# Apply horizontal velocity
	velocity.x = dir * move_speed * speed_multiplier

	# 3. Track double-tap Left/Right for Dash
	if Input.is_action_just_pressed("ui_left"):
		if curr_time - last_press_left_time < DOUBLE_TAP_DELAY and dash_cooldown_timer <= 0:
			_start_dash(Vector2.LEFT)
		last_press_left_time = curr_time

	if Input.is_action_just_pressed("ui_right"):
		if curr_time - last_press_right_time < DOUBLE_TAP_DELAY and dash_cooldown_timer <= 0:
			_start_dash(Vector2.RIGHT)
		last_press_right_time = curr_time

	# Update direction facing
	if dir > 0:
		facing_direction = Vector2.RIGHT
		$CompositeVisuals.scale.x = visual_base_scale
	elif dir < 0:
		facing_direction = Vector2.LEFT
		$CompositeVisuals.scale.x = -visual_base_scale

func _start_dash(dir: Vector2) -> void:
	is_dashing = true
	dash_timer = dash_duration
	dash_cooldown_timer = dash_cooldown
	var dash_mult: float = GameConfig.DashSpeedMultiplier if GameConfig else 2.8
	velocity.x = dir.x * move_speed * dash_mult
	velocity.y = 0 # Hover during dash

func _start_parry() -> void:
	is_parrying = true
	parry_timer = GameConfig.ParryWindowDuration if GameConfig else 0.22
	parry_cooldown_timer = GameConfig.ParryCooldown if GameConfig else 1.0
	
	# Visual feedback: flash blue guard tint
	var tween = create_tween()
	orc_sprite.color = Color(0.2, 0.8, 1.0, 1.0)
	tween.tween_property(orc_sprite, "color", Color(0.12, 0.4, 0.16, 1.0), 0.2)

func _execute_heavy_thrust() -> void:
	thrust_cooldown_timer = GameConfig.ThrustCooldown if GameConfig else 1.2
	
	# 1. Forceful thrust adds immediate pleasure boost
	var pleasure_boost: float = GameConfig.ThrustPleasureBonus if GameConfig else 15.0
	CombatManager.AddPleasure(pleasure_boost)

	# 2. Short-range AOE knockback to surrounding enemies
	var knock_radius: float = GameConfig.ThrustKnockbackRadius if GameConfig else 140.0
	var knock_force: float = GameConfig.ThrustKnockbackForce if GameConfig else 450.0
	
	var enemies = get_tree().get_nodes_in_group("enemies")
	for enemy in enemies:
		if is_instance_valid(enemy):
			var dist = global_position.distance_to(enemy.global_position)
			if dist <= knock_radius:
				var push_dir = (enemy.global_position - global_position).normalized()
				if push_dir.x == 0: push_dir.x = facing_direction.x
				if enemy is CharacterBody2D:
					enemy.velocity += push_dir * knock_force

	# Visual feedback: violent thrust recoil & screen pulse
	var tween = create_tween()
	vessel_sprite.position.x = 25.0
	tween.tween_property(vessel_sprite, "position:x", 12.0, 0.15)

func detach_vessel() -> void:
	if not is_vessel_attached: return
	is_vessel_attached = false
	vessel_sprite.visible = false

func attach_vessel() -> void:
	if is_vessel_attached: return
	is_vessel_attached = true
	vessel_sprite.visible = true

func _update_visuals_and_animations() -> void:
	if not is_vessel_attached:
		vessel_sprite.visible = false
		return
	else:
		vessel_sprite.visible = true

	var cm = CombatManager
	var pleasure = cm.CurrentPleasure
	var max_pleasure = cm.MaxPleasure
	var ratio: float = clamp(pleasure / max_pleasure, 0.0, 1.0)

	# 1. Update sexual thrusting animation speed based on pleasure
	var speed_freq: float = 0.015 * (1.0 + ratio * 5.0)
	var time_tick := Time.get_ticks_msec()
	
	vessel_sprite.position.x = 12.0 + sin(time_tick * speed_freq) * (5.0 + ratio * 10.0)
	
	var bounce_amplitude: float = (2.0 + ratio * 4.0) * (crouch_height_ratio if is_crouching else 1.0) * visual_base_scale
	$CompositeVisuals.position.y = sin(time_tick * speed_freq * 2.0) * bounce_amplitude

	# 2. Particles intensity
	if particles:
		particles.speed_scale = 0.5 + ratio * 2.0
		if ratio > 0.1:
			particles.emitting = true
		else:
			particles.emitting = false

func _on_weapon_fired(item_id: String, damage: float) -> void:
	# While Vessel is detached by a Snatcher enemy, item attacks are disabled!
	if not is_vessel_attached: return

	# Spawn visual projectile in direction of facing
	var proj = ProjectileScene.instantiate() as Projectile
	get_parent().add_child(proj)
	proj.global_position = global_position
	
	# If the weapon is a sword, it's a melee slash
	var is_slash = item_id.contains("sword")
	proj.setup(damage, facing_direction, 500.0, is_slash)

func _on_climax_triggered(_female_id: String, _skill_name: String) -> void:
	# Play screenshake
	var tween = create_tween()
	tween.tween_property(camera, "offset", Vector2(randf_range(-15, 15), randf_range(-15, 15)), 0.05)
	tween.tween_property(camera, "offset", Vector2(randf_range(-15, 15), randf_range(-15, 15)), 0.05)
	tween.tween_property(camera, "offset", Vector2(randf_range(-15, 15), randf_range(-15, 15)), 0.05)
	tween.tween_property(camera, "offset", Vector2.ZERO, 0.1)
