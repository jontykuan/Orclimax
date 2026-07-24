extends EnemyBase

class_name EnemyFlying

@export var hover_altitude: float = -180.0
@export var sine_frequency: float = 3.0
@export var sine_amplitude: float = 40.0

var time_passed: float = 0.0

func _ready() -> void:
	super._ready()
	max_hp = 25.0
	hp = max_hp
	speed = 110.0 # Fast flying speed
	sprite.color = Color(0.7, 0.2, 0.8) # Purple flyer tint

func _physics_process(delta: float) -> void:
	if not target_player:
		return

	time_passed += delta
	var target_pos = target_player.global_position + Vector2(0, hover_altitude)
	var dir_x = sign(target_player.global_position.x - global_position.x)
	
	# Horizontal movement towards player
	velocity.x = dir_x * speed
	
	# Vertical sine wave flying motion
	var sine_offset = sin(time_passed * sine_frequency) * sine_amplitude
	velocity.y = (target_pos.y + sine_offset - global_position.y) * 2.0

	move_and_slide()
