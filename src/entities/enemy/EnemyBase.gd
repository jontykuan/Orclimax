extends CharacterBody2D

class_name EnemyBase

@export var max_hp: float = 30.0
@export var speed: float = 80.0
@export var damage: float = 5.0
@export var gold_reward: int = 2
@export var drop_chance: float = 0.25 # 25% chance to drop item on death

var hp: float = 30.0
var target_player: CharacterBody2D = null

@onready var sprite: ColorRect = $ColorRect

func _ready() -> void:
	hp = max_hp
	add_to_group("enemies")
	
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

	# Simple AI: Walk towards player
	var dir = (target_player.global_position - global_position).normalized()
	velocity.x = dir.x * speed
	
	# Simple gravity
	if not is_on_floor():
		velocity.y += 980 * delta
	else:
		velocity.y = 0

	move_and_slide()

	# Damage player on overlap (AABB check since physical collision mask is 4)
	_check_player_overlap(delta)

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
	# In our prototype, BackpackUI test_items contains all item templates.
	# We can pick a random item and add it directly to the stash!
	# To make this clean, we emit a floating text or print message.
	var backpack_ui = get_tree().get_first_node_in_group("backpack_ui")
	var item_pool = []
	if backpack_ui and backpack_ui.has_method("_setup_test_items"):
		item_pool = backpack_ui.test_items
	
	# Fallback if BackpackUI not active (e.g. direct scene run)
	if item_pool.size() == 0:
		# Just create a mock sword
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

	# Print floating alert
	print("Loot Dropped: ", dropped_item.ItemName, " (Sent to Stash!)")

func _check_player_overlap(delta: float) -> void:
	if not target_player:
		return
	
	# Horizontal check: origins are at bottom center, player half-width 64, enemy half-width 48
	var dx: float = abs(target_player.global_position.x - global_position.x)
	var overlap_x: bool = dx < 96.0 # 48 (enemy half-width) + 64 (player half-width) - 16 leeway
	
	# Vertical check: origins are at feet (bottom center)
	# Player height is 256.0 (or 153.6 when crouching), Enemy height is 192.0
	var dy: float = target_player.global_position.y - global_position.y
	var player_height: float = 153.6 if target_player.is_crouching else 256.0
	
	# Check if vertical intervals [y-height, y] and [y-192, y] overlap
	var overlap_y: bool = (dy > -player_height and dy < 192.0)
	
	if overlap_x and overlap_y:
		CombatManager.TakeDamage(damage * delta)
