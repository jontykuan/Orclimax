extends CharacterBody2D

class_name Player

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

@export var gravity: float = 980.0
@export var jump_velocity: float = -400.0

@onready var orc_sprite: ColorRect = $CompositeVisuals/Orc
@onready var female_sprite: ColorRect = $CompositeVisuals/Female
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
const DOUBLE_TAP_DELAY: float = 0.25

func _ready() -> void:
	# Add to group so enemies can identify player
	add_to_group("player")
	
	# Connect to C# CombatManager signals
	var cm = CombatManager
	cm.connect("WeaponFired", _on_weapon_fired)
	cm.connect("ClimaxTriggered", _on_climax_triggered)
	
	# Update local stats initially
	move_speed = cm.MoveSpeed

func _physics_process(delta: float) -> void:
	# Add gravity
	if not is_on_floor():
		velocity.y += gravity * delta

	# Synchronize stats dynamically from CombatManager
	move_speed = CombatManager.MoveSpeed

	# Handle Dash Cooldown
	if dash_cooldown_timer > 0:
		dash_cooldown_timer -= delta

	# Handle Dash active time
	if is_dashing:
		dash_timer -= delta
		if dash_timer <= 0:
			is_dashing = false
	else:
		_handle_normal_movement(delta)

	move_and_slide()
	_update_visuals_and_animations()

func _handle_normal_movement(_delta: float) -> void:
	# Handle Jump
	if Input.is_action_just_pressed("ui_up") and is_on_floor():
		velocity.y = jump_velocity

	# Handle Horizontal Input
	var dir := Input.get_axis("ui_left", "ui_right")
	
	# Crouch logic (reduce speed and scale down sprite height)
	var is_crouching := Input.is_action_pressed("ui_down")
	var speed_multiplier := 1.0
	
	if is_crouching:
		speed_multiplier = 0.4
		orc_sprite.scale.y = 0.6
		female_sprite.scale.y = 0.6
	else:
		orc_sprite.scale.y = 1.0
		female_sprite.scale.y = 1.0

	# Apply horizontal velocity
	velocity.x = dir * move_speed * speed_multiplier

	# Track double-tap for dash
	if Input.is_action_just_pressed("ui_left"):
		var curr_time := Time.get_ticks_msec() / 1000.0
		if curr_time - last_press_left_time < DOUBLE_TAP_DELAY and dash_cooldown_timer <= 0:
			_start_dash(Vector2.LEFT)
		last_press_left_time = curr_time

	if Input.is_action_just_pressed("ui_right"):
		var curr_time := Time.get_ticks_msec() / 1000.0
		if curr_time - last_press_right_time < DOUBLE_TAP_DELAY and dash_cooldown_timer <= 0:
			_start_dash(Vector2.RIGHT)
		last_press_right_time = curr_time

	# Update direction facing
	if dir > 0:
		facing_direction = Vector2.RIGHT
		$CompositeVisuals.scale.x = 1
	elif dir < 0:
		facing_direction = Vector2.LEFT
		$CompositeVisuals.scale.x = -1

func _start_dash(dir: Vector2) -> void:
	is_dashing = true
	dash_timer = dash_duration
	dash_cooldown_timer = dash_cooldown
	velocity.x = dir.x * move_speed * 2.8
	velocity.y = 0 # Hover during dash

func _update_visuals_and_animations() -> void:
	var cm = CombatManager
	var pleasure = cm.CurrentPleasure
	var max_pleasure = cm.MaxPleasure
	var ratio: float = clamp(pleasure / max_pleasure, 0.0, 1.0)

	# 1. Update sexual thrusting animation speed based on pleasure
	# Calculate speed: standard bounce speed up to very high thrust speed
	var speed_freq: float = 0.015 * (1.0 + ratio * 5.0)
	var time_tick := Time.get_ticks_msec()
	
	# Horizontal relative movement between female and orc (sex action)
	# The female slides back and forth on the Orc's genital height
	female_sprite.position.x = 12.0 + sin(time_tick * speed_freq) * (5.0 + ratio * 10.0)
	
	# Small vertical bounce for both (running/thrusting sync)
	$CompositeVisuals.position.y = -32.0 + sin(time_tick * speed_freq * 2.0) * (2.0 + ratio * 4.0)

	# 2. Particles intensity
	# Make pink hearts particles spawn more frequently and move faster as pleasure rises!
	if particles:
		particles.speed_scale = 0.5 + ratio * 2.0
		if ratio > 0.1:
			particles.emitting = true
		else:
			particles.emitting = false

func _on_weapon_fired(item_id: String, damage: float) -> void:
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
