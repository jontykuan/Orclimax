extends Node2D

const EnemyScene = preload("res://src/entities/enemy/EnemyBase.tscn")
const PlayerScene = preload("res://src/entities/player/Player.tscn")
const HUDScene = preload("res://src/ui/hud/HUD.tscn")

@onready var spawn_timer: Timer = $SpawnTimer

var player_node: Player = null
var hud_node: HUD = null

var wave_duration: float = 20.0 # 20 second wave for quick testing
var time_remaining: float = 20.0
var wave_active: bool = true

func _ready() -> void:
	# 1. Instantiate Player
	player_node = PlayerScene.instantiate() as Player
	add_child(player_node)
	player_node.position = Vector2(576, 1050) # Center bottom

	# 2. Instantiate HUD
	hud_node = HUDScene.instantiate() as HUD
	add_child(hud_node)

	# 3. Setup wave timer
	time_remaining = wave_duration
	wave_active = true
	spawn_timer.start()

func _process(delta: float) -> void:
	if not wave_active: return

	# Tick wave time
	time_remaining -= delta
	if time_remaining <= 0:
		_complete_wave()

func _on_spawn_timer_timeout() -> void:
	if not wave_active or not player_node: return

	# Spawn enemy on left or right of the player
	var spawn_left = randf() < 0.5
	var spawn_x = player_node.global_position.x + (randf_range(-700, -500) if spawn_left else randf_range(500, 700))
	
	var enemy = EnemyScene.instantiate() as EnemyBase
	add_child(enemy)
	enemy.position = Vector2(spawn_x, 1050) # same level

func _complete_wave() -> void:
	wave_active = false
	spawn_timer.stop()
	
	# Transition GameState back to Shop/Camp
	# Note: In a complete game, we might wait for the player to click a button.
	# We show the victory HUD panel
	if hud_node:
		hud_node.show_victory()

	# Clear all remaining enemies on screen
	var enemies = get_tree().get_nodes_in_group("enemies")
	for enemy in enemies:
		enemy.queue_free()
