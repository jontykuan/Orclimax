extends EnemyBase

class_name EnemySnatcher

var has_stolen_vessel: bool = false
var snatch_cooldown: float = 3.0
var snatch_timer: float = 0.0

@onready var stolen_vessel_visual: ColorRect = null

func _ready() -> void:
	super._ready()
	max_hp = 35.0
	hp = max_hp
	speed = 130.0 # Fast runner to snatch and escape
	sprite.color = Color(0.9, 0.2, 0.5) # Hot pink snatcher tint
	
	# Add visual representation of stolen vessel on back
	stolen_vessel_visual = ColorRect.new()
	stolen_vessel_visual.size = Vector2(20, 40)
	stolen_vessel_visual.color = Color(1.0, 0.4, 0.7)
	stolen_vessel_visual.position = Vector2(5, -20)
	stolen_vessel_visual.visible = false
	add_child(stolen_vessel_visual)

func _physics_process(delta: float) -> void:
	if not target_player:
		super._physics_process(delta)
		return

	snatch_timer += delta
	var dist = global_position.distance_to(target_player.global_position)

	if not has_stolen_vessel:
		# Chase Orc aggressively to snatch Vessel
		var dir = (target_player.global_position - global_position).normalized()
		velocity.x = dir.x * speed
		
		if dist <= 90.0 and snatch_timer >= snatch_cooldown:
			_snatch_vessel()
	else:
		# Escape away from Orc after stealing Vessel!
		var dir_away = -sign(target_player.global_position.x - global_position.x)
		velocity.x = dir_away * speed * 1.1
		
		# If Orc catches up and touches the Snatcher, Orc reclaims Vessel!
		if dist <= 60.0:
			_reclaim_vessel()

	if not is_on_floor():
		velocity.y += 980 * delta

	move_and_slide()

func _snatch_vessel() -> void:
	if has_stolen_vessel: return
	has_stolen_vessel = true
	snatch_timer = 0.0
	stolen_vessel_visual.visible = true
	
	if target_player and target_player.has_method("detach_vessel"):
		target_player.detach_vessel()
		
	print("[EnemySnatcher] Vessel stolen from Orc!")

func _reclaim_vessel() -> void:
	if not has_stolen_vessel: return
	has_stolen_vessel = false
	if stolen_vessel_visual: stolen_vessel_visual.visible = false
	
	if target_player and target_player.has_method("attach_vessel"):
		target_player.attach_vessel()
		
	print("[Player] Vessel reclaimed! EnemySnatcher defeated!")
	_die() # Snatcher dies immediately upon Orc reclaiming the Vessel!

func _die() -> void:
	if has_stolen_vessel:
		has_stolen_vessel = false
		if target_player and target_player.has_method("attach_vessel"):
			target_player.attach_vessel()
	super._die()
