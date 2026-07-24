extends CharacterBody2D

class_name EnemyBase

# Helper class for enemy action rotation
class EnemyAction:
	var name: String
	var cooldown: float
	var timer: float
	var damage: float
	var action_range: float
	var description: String

	func _init(p_name: String, p_cooldown: float, p_damage: float, p_range: float, p_desc: String) -> void:
		name = p_name
		cooldown = p_cooldown
		timer = 0.0
		damage = p_damage
		action_range = p_range
		description = p_desc

@export var max_hp: float = 30.0
@export var speed: float = 80.0
@export var gold_reward: int = 2
@export var drop_chance: float = 0.25 # 25% chance to drop item on death

var hp: float = 30.0
var target_player: CharacterBody2D = null
var actions: Array[EnemyAction] = []
var current_action_idx: int = 0

@onready var sprite: ColorRect = $ColorRect

func _ready() -> void:
	if GameConfig:
		max_hp = GameConfig.DefaultEnemyMaxHp
		speed = GameConfig.DefaultEnemySpeed
		gold_reward = GameConfig.DefaultGoldReward
		drop_chance = GameConfig.DefaultDropChance

	hp = max_hp
	add_to_group("enemies")
	
	# Configure multiple actions for rotation mechanism
	var claw_cd = GameConfig.ClawSlashCooldown if GameConfig else 1.5
	var claw_dmg = GameConfig.ClawSlashDamage if GameConfig else 3.0
	var claw_rng = GameConfig.ClawSlashRange if GameConfig else 110.0
	actions.append(EnemyAction.new("Claw Slash", claw_cd, claw_dmg, claw_rng, "Quick front slash"))

	var cleave_cd = GameConfig.HeavyCleaveCooldown if GameConfig else 4.0
	var cleave_dmg = GameConfig.HeavyCleaveDamage if GameConfig else 8.0
	var cleave_rng = GameConfig.HeavyCleaveRange if GameConfig else 130.0
	actions.append(EnemyAction.new("Heavy Cleave", cleave_cd, cleave_dmg, cleave_rng, "Devastating slow cleave"))
	
	# Find player
	var players = get_tree().get_nodes_in_group("player")
	if players.size() > 0:
		target_player = players[0]

func _physics_process(delta: float) -> void:
	if not target_player:
		# Search again in case player spawned late
		var players = get_tree().get_nodes_in_group("player")
		if players.size() > 0:
			target_player = players[0]
		return

	# Tick cooldown timers of all actions
	for action in actions:
		action.timer += delta

	# Check player distance
	var dist = global_position.distance_to(target_player.global_position)
	var dir = (target_player.global_position - global_position).normalized()

	# Check rotation attack status
	var current_action = actions[current_action_idx]
	var is_attacking = false

	if dist <= current_action.action_range:
		is_attacking = true
		if current_action.timer >= current_action.cooldown:
			_execute_enemy_action(current_action)
			current_action.timer = 0.0
			current_action_idx = (current_action_idx + 1) % actions.size()

	if is_attacking:
		# Stop moving horizontally during attack execution
		velocity.x = 0
	else:
		# Walk towards player
		velocity.x = dir.x * speed
	
	# Simple gravity
	if not is_on_floor():
		var grav: float = GameConfig.Gravity if GameConfig else 980.0
		velocity.y += grav * delta
	else:
		velocity.y = 0

	move_and_slide()

func _execute_enemy_action(action: EnemyAction) -> void:
	# Deal damage to player via CombatManager
	CombatManager.TakeDamage(action.damage)
	
	# Visual feedback: attack telegraph flash
	var tween = create_tween()
	sprite.color = Color(1.0, 1.0, 1.0, 1.0) # Flash white
	tween.tween_property(sprite, "color", Color(0.8, 0.1, 0.1, 1.0), 0.15) # Back to crimson
	
	print("Enemy performed [", action.name, "] dealing ", action.damage, " damage!")

func take_damage(amount: float) -> void:
	hp -= amount
	# Visual flash
	var tween = create_tween()
	sprite.color = Color(1.0, 1.0, 1.0, 1.0) # Flash white
	tween.tween_property(sprite, "color", Color(0.8, 0.1, 0.1, 1.0), 0.15) # Back to crimson red

	if hp <= 0:
		_die()

func _die() -> void:
	# 1. Give gold
	GameManager.AddGold(gold_reward)
	
	# 2. Check item drop
	if randf() < drop_chance:
		_spawn_loot()
		
	queue_free()

func _spawn_loot() -> void:
	var backpack_ui = get_tree().get_first_node_in_group("backpack_ui")
	var item_pool = []
	if backpack_ui and backpack_ui.has_method("_setup_test_items"):
		item_pool = backpack_ui.test_items
	
	if item_pool.size() == 0:
		var sword = load("res://src/core/ItemData.cs").new()
		sword.Id = "wpn_sword"
		sword.ItemName = "Dropped Blade"
		sword.Category = 0
		sword.RequiredZone = 0
		sword.ShapeOffsets = [Vector2i(0, 0)]
		sword.Damage = 5.0
		sword.Cooldown = 1.5
		sword.BasePrice = 6
		item_pool.append(sword)

	var dropped_item = item_pool[randi() % item_pool.size()]
	InventoryManager.AddItemToStash(dropped_item)

	print("Loot Dropped: ", dropped_item.ItemName, " (Sent to Stash!)")
