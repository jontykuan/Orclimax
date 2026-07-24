extends Node2D

const EnemyScene = preload("res://src/entities/enemy/EnemyBase.tscn")
const PlayerScene = preload("res://src/entities/player/Player.tscn")
const HUDScene = preload("res://src/ui/hud/HUD.tscn")

@onready var spawn_timer: Timer = $SpawnTimer

var player_node: Player = null
var hud_node: HUD = null

var stage_duration: float = 180.0 # 3 minutes total
var stage_elapsed_time: float = 0.0
var wave_timer: float = 0.0
var wave_counter: int = 0
var wave_interval: float = 30.0 # Wave every 30 seconds

var stage_active: bool = true
var boss_spawned: bool = false
var current_stage_id: String = "flame_spire"

func _ready() -> void:
	current_stage_id = GameManager.CurrentSelectedStageId
	
	# 1. Instantiate Player
	player_node = PlayerScene.instantiate() as Player
	add_child(player_node)
	player_node.position = Vector2(576, 1050)

	# 2. Instantiate HUD
	hud_node = HUDScene.instantiate() as HUD
	add_child(hud_node)

	# 3. Initial Wave Spawn (Wave 1 at 0s)
	stage_active = true
	stage_elapsed_time = 0.0
	wave_timer = 0.0
	wave_counter = 0
	boss_spawned = false
	
	_spawn_wave()

func _process(delta: float) -> void:
	if not stage_active: return

	stage_elapsed_time += delta
	wave_timer += delta

	# Every 30 seconds (at 30s, 60s, 90s, 120s), spawn a new wave before boss
	if wave_timer >= wave_interval and stage_elapsed_time < 150.0:
		wave_timer = 0.0
		_spawn_wave()

	# At 150 seconds, Boss spawns!
	if stage_elapsed_time >= 150.0 and not boss_spawned:
		boss_spawned = true
		_spawn_stage_boss()

	# Stage ends at 180s (3 minutes)
	if stage_elapsed_time >= stage_duration:
		_complete_stage()

func _spawn_wave() -> void:
	if not stage_active or not player_node: return

	wave_counter += 1
	var num_enemies = 3 + (wave_counter - 1) * 2 # Wave 1: 3, Wave 2: 5, Wave 3: 7, Wave 4: 9, Wave 5: 11
	
	for i in range(num_enemies):
		var spawn_left = randf() < 0.5
		var spawn_x = player_node.global_position.x + (randf_range(-750, -450) if spawn_left else randf_range(450, 750))
		var spawn_y = 1050.0
		
		var enemy = EnemyScene.instantiate() as EnemyBase
		
		if current_stage_id == "flame_spire":
			# Flame Spire enemies: Melee Infantry (0), Heavy Shield (1), Flat-shot Ranged (2)
			var type = randi() % 3
			match type:
				1:
					enemy.set_script(load("res://src/entities/enemy/EnemyShield.gd"))
				2:
					enemy.set_script(load("res://src/entities/enemy/EnemyRanged.gd"))
		else:
			# Tale Breeze enemies: Flat-shot Ranged (0), Parabolic Lob Ranged (1), Vessel Snatcher (2)
			var type = randi() % 3
			match type:
				0, 1:
					enemy.set_script(load("res://src/entities/enemy/EnemyRanged.gd"))
				2:
					enemy.set_script(load("res://src/entities/enemy/EnemySnatcher.gd"))

		add_child(enemy)
		enemy.position = Vector2(spawn_x, spawn_y)

func _spawn_stage_boss() -> void:
	if not player_node: return
	
	var spawn_x = player_node.global_position.x + 600.0
	var boss = EnemyScene.instantiate() as EnemyBase
	
	if current_stage_id == "flame_spire":
		boss.set_script(load("res://src/entities/enemy/EnemyBossLydia.gd"))
		print("[STAGE 150s] Boss Elven Mage Lydia Spawned!")
	else:
		boss.set_script(load("res://src/entities/enemy/EnemyBossCynthia.gd"))
		print("[STAGE 150s] Boss Elven Archer Cynthia Spawned!")
		
	add_child(boss)
	boss.position = Vector2(spawn_x, 1050.0)

func _complete_stage() -> void:
	stage_active = false
	if spawn_timer: spawn_timer.stop()
	
	if hud_node:
		hud_node.show_victory()

	# Clear all remaining enemies on screen
	var enemies = get_tree().get_nodes_in_group("enemies")
	for enemy in enemies:
		if is_instance_valid(enemy):
			enemy.queue_free()
