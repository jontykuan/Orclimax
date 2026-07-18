extends Area2D

class_name Projectile

var speed: float = 500.0
var damage: float = 10.0
var direction: Vector2 = Vector2.RIGHT
var is_slash: bool = false
var lifetime: float = 0.5

func setup(p_damage: float, p_direction: Vector2, p_speed: float = 500.0, p_is_slash: bool = false) -> void:
	damage = p_damage
	direction = p_direction.normalized()
	speed = p_speed
	is_slash = p_is_slash
	
	var col_shape = $CollisionShape2D
	if col_shape and col_shape.shape is RectangleShape2D:
		if is_slash:
			col_shape.shape.size = Vector2(80, 40)
			col_shape.position = Vector2(40 * direction.x, 0) # Extend outward in front of player
		else:
			col_shape.shape.size = Vector2(16, 16)
			col_shape.position = Vector2.ZERO
	
	if is_slash:
		lifetime = 0.15
		speed = 0.0 # Slashes stay attached to player
	else:
		lifetime = 1.5

func _ready() -> void:
	# Add visual representation
	var color_rect = ColorRect.new()
	if is_slash:
		color_rect.size = Vector2(80, 40)
		color_rect.position = Vector2(0, -20)
		color_rect.color = Color(1.0, 0.4, 0.4, 0.6) # Red slash
	else:
		color_rect.size = Vector2(16, 16)
		color_rect.position = Vector2(-8, -8)
		color_rect.color = Color(1.0, 0.8, 0.2, 1.0) # Yellow magical bolt
	add_child(color_rect)
	
	# Simple timer to free the projectile
	var timer = get_tree().create_timer(lifetime)
	timer.timeout.connect(queue_free)

func _physics_process(delta: float) -> void:
	if not is_slash:
		position += direction * speed * delta

func _on_body_entered(body: Node2D) -> void:
	# Check if the body is an enemy
	if body.is_in_group("enemies") and body.has_method("take_damage"):
		body.take_damage(damage)
		if not is_slash:
			queue_free() # Destroy projectile on hit
