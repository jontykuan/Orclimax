extends Control

class_name BackpackUI

const GridCellUIScene = preload("res://src/ui/backpack/GridCellUI.tscn")
const ItemUIScene = preload("res://src/ui/backpack/ItemUI.tscn")

@onready var grid_container: Control = $MainLayout/HBox/GridArea/Panel/GridContainer
@onready var stash_container: HFlowContainer = $MainLayout/HBox/StashArea/ScrollContainer/StashContainer
@onready var shop_container: HBoxContainer = $MainLayout/ShopArea/ShopItems
@onready var gold_label: Label = $MainLayout/Header/GoldLabel
@onready var stage_label: Label = $MainLayout/Header/StageLabel
@onready var female_title_label: Label = $MainLayout/HBox/GridArea/FemaleTitle
@onready var reroll_button: Button = $MainLayout/ShopArea/ShopHeader/RerollButton

var cell_size: float = 64.0
var cell_nodes: Dictionary = {} # Vector2i -> GridCellUI
var item_nodes: Dictionary = {} # String (instance_id) -> ItemUI

# Drag and drop state
var dragging_item: ItemUI = null
var drag_start_from_grid: bool = false
var drag_original_anchor: Vector2i
var drag_original_rotation: int
var drag_original_stash_index: int = -1

# Pool of items for the shop (we can define some test items)
var test_items: Array[Resource] = []
var current_shop_items: Array:
	get:
		return InventoryManager.CurrentShopItems
	set(val):
		InventoryManager.CurrentShopItems = val

var shop_slots_locked: Array:
	get:
		return InventoryManager.ShopSlotsLocked
	set(val):
		InventoryManager.ShopSlotsLocked = val

func _ready() -> void:
	# 1. Load C# Singletons and connect signals
	var gm = GameManager
	var im = InventoryManager
	
	gm.connect("GoldChanged", _on_gold_changed)
	gm.connect("StageChanged", _on_stage_changed)
	
	im.connect("FemaleChanged", _on_female_changed)
	im.connect("GridUpdated", _on_grid_updated)
	im.connect("StashUpdated", _on_stash_updated)

	# 2. Setup mock test items
	_setup_test_items()

	# 3. Initialize default female (if none is set, create a mockup girl)
	_setup_mock_female_if_needed()

	# 4. Initial UI draws
	# Connect reroll button
	if reroll_button:
		reroll_button.pressed.connect(_on_reroll_pressed)
	
	# Generate initial shop items (at the very beginning / start of scene load)
	_generate_new_shop_items()

	# Trigger automatic fusions (after battle, when reloading the camp/shop UI)
	im.TriggerFusions()

	_on_gold_changed(gm.Gold)
	_on_stage_changed(gm.CurrentStage)
	_on_female_changed(im.CurrentFemale)
	_on_stash_updated() # Force initial stash draw so dropped items show up!

func _setup_test_items() -> void:
	# Create test items in code for convenience
	# Item 1: Dildo (Toy, Groin zone)
	var dildo = load("res://src/core/ItemData.cs").new()
	dildo.Id = "toy_dildo"
	dildo.ItemName = "Vibrator"
	dildo.Category = 1 # Toy
	dildo.RequiredZone = 3 # Groin
	dildo.ShapeOffsets = [Vector2i(0, 0), Vector2i(0, 1)] # 1x2 vertical
	dildo.PleasureGain = 2.5
	dildo.BasePrice = 6
	test_items.append(dildo)

	# Item 2: Short Sword (Weapon, General zone)
	var sword = load("res://src/core/ItemData.cs").new()
	sword.Id = "wpn_sword"
	sword.ItemName = "Rusty Sword"
	sword.Category = 0 # Weapon
	sword.RequiredZone = 0 # General
	sword.ShapeOffsets = [Vector2i(0, 0), Vector2i(1, 0)] # 2x1 horizontal
	sword.Damage = 8.0
	sword.Cooldown = 1.8
	sword.BasePrice = 8
	test_items.append(sword)

	# Item 3: Mouth Gag (Toy, Head zone)
	var gag = load("res://src/core/ItemData.cs").new()
	gag.Id = "toy_gag"
	gag.ItemName = "Mouth Gag"
	gag.Category = 1 # Toy
	gag.RequiredZone = 1 # Head
	gag.ShapeOffsets = [Vector2i(0, 0)] # 1x1
	gag.PleasureGain = 1.0
	gag.BasePrice = 4
	test_items.append(gag)

	# Item 4: Leather Harness (Clothing, Chest zone)
	var harness = load("res://src/core/ItemData.cs").new()
	harness.Id = "cloth_harness"
	harness.ItemName = "Harness"
	harness.Category = 2 # Clothing
	harness.RequiredZone = 2 # Chest
	harness.ShapeOffsets = [Vector2i(0, 0), Vector2i(0, 1), Vector2i(1, 0)] # L-shape
	harness.ArmorBonus = 3.0
	harness.BasePrice = 10
	test_items.append(harness)

	# Item 5: Poison Potion (Consumable)
	var poison = load("res://src/core/ItemData.cs").new()
	poison.Id = "item_poison"
	poison.ItemName = "Poison Potion"
	poison.Category = 4 # Consumable
	poison.RequiredZone = 0 # General
	poison.ShapeOffsets = [Vector2i(0, 0)]
	poison.BasePrice = 3
	test_items.append(poison)

	# Register a fusion recipe: Rusty Sword + Poison Potion = Poison Sword
	var poison_sword = load("res://src/core/ItemData.cs").new()
	poison_sword.Id = "wpn_poison_sword"
	poison_sword.ItemName = "Poison Sword"
	poison_sword.Category = 0 # Weapon
	poison_sword.RequiredZone = 0 # General
	poison_sword.ShapeOffsets = [Vector2i(0, 0), Vector2i(1, 0)]
	poison_sword.Damage = 14.0
	poison_sword.Cooldown = 1.5
	poison_sword.BasePrice = 15
	poison_sword.SynergyDescription = "Deals poison damage."

	var recipe = load("res://src/core/FusionRecipe.cs").new()
	recipe.ComponentA = sword
	recipe.ComponentB = poison
	recipe.Result = poison_sword
	
	# Register a fusion recipe: Rusty Sword + Vibrator = Vibrating Sword
	var vibrating_sword = load("res://src/core/ItemData.cs").new()
	vibrating_sword.Id = "wpn_vibrating_sword"
	vibrating_sword.ItemName = "Vibrating Sword"
	vibrating_sword.Category = 0 # Weapon
	vibrating_sword.RequiredZone = 0 # General
	vibrating_sword.ShapeOffsets = [Vector2i(0, 0), Vector2i(0, 1)] # 1x2 vertical
	vibrating_sword.Damage = 12.0
	vibrating_sword.PleasureGain = 1.5
	vibrating_sword.Cooldown = 1.4
	vibrating_sword.BasePrice = 18
	vibrating_sword.SynergyDescription = "Deals damage and increases pleasure simultaneously."

	var recipe2 = load("res://src/core/FusionRecipe.cs").new()
	recipe2.ComponentA = sword
	recipe2.ComponentB = dildo
	recipe2.Result = vibrating_sword

	if InventoryManager.Recipes.size() == 0:
		InventoryManager.Recipes.append(recipe)
		InventoryManager.Recipes.append(recipe2)

func _setup_mock_female_if_needed() -> void:
	var im = InventoryManager
	if im.CurrentFemale == null:
		var girl = load("res://src/core/FemaleData.cs").new()
		girl.Id = "girl_lydia"
		girl.CharacterName = "Elven Mage Lydia"
		
		# Define Lydia's grid (shaped like a body profile)
		# Head cells at top
		girl.HeadCells = [Vector2i(2, 0), Vector2i(2, 1)]
		# Chest cells in middle
		girl.ChestCells = [Vector2i(1, 2), Vector2i(2, 2), Vector2i(3, 2)]
		# Groin cells at lower middle
		girl.GroinCells = [Vector2i(2, 3), Vector2i(2, 4)]
		# Limbs at sides
		girl.LimbsCells = [Vector2i(0, 2), Vector2i(4, 2), Vector2i(1, 5), Vector2i(3, 5)]
		# General cells connecting them
		girl.GeneralCells = [Vector2i(1, 3), Vector2i(3, 3), Vector2i(2, 5)]
		
		# (2, 4) groin cell and (3, 5) limb cell start out locked!
		girl.InitiallyLockedCells = [Vector2i(2, 4), Vector2i(3, 5)]

		im.SetFemale(girl)

func _on_gold_changed(new_gold: int) -> void:
	gold_label.text = "GOLD: %d" % new_gold
	if current_shop_items.size() > 0:
		_refresh_shop()

func _on_stage_changed(new_stage: int) -> void:
	stage_label.text = "STAGE: %d" % new_stage

func _on_female_changed(female: Resource) -> void:
	if female == null: return
	female_title_label.text = "Carrier: " + female.CharacterName
	_rebuild_grid()

func _rebuild_grid() -> void:
	# Clear old nodes
	for cell_node in cell_nodes.values():
		cell_node.queue_free()
	cell_nodes.clear()

	var grid = InventoryManager.BackpackGrid
	if grid == null: return

	# Determine bounding box
	var min_x = 999
	var min_y = 999
	for coord in grid.GetCellCoords():
		min_x = min(min_x, coord.x)
		min_y = min(min_y, coord.y)

	# Build UI cells
	for coord in grid.GetCellCoords():
		var cell = grid.Cells[coord]
		var cell_ui = GridCellUIScene.instantiate() as GridCellUI
		grid_container.add_child(cell_ui)
		
		# Offset positions so they center within grid area
		# We multiply coordinate by cell_size
		cell_ui.setup(
			coord,
			int(cell.Zone),
			cell.IsLocked,
			cell_size
		)
		# Position relative to container
		cell_ui.position = Vector2((coord.x - min_x) * cell_size, (coord.y - min_y) * cell_size)
		cell_nodes[coord] = cell_ui

	# Adjust GridContainer custom minimum size
	# So scrolling / alignment works
	var max_w = 0
	var max_h = 0
	for cell_ui in cell_nodes.values():
		max_w = max(max_w, cell_ui.position.x + cell_size)
		max_h = max(max_h, cell_ui.position.y + cell_size)
	grid_container.custom_minimum_size = Vector2(max_w, max_h)

	# Re-draw placed items
	_rebuild_placed_items()

func _rebuild_placed_items() -> void:
	# Clear old items
	for item_node in item_nodes.values():
		item_node.queue_free()
	item_nodes.clear()

	var grid = InventoryManager.BackpackGrid
	if grid == null: return

	var min_x = 999
	var min_y = 999
	for coord in grid.GetCellCoords():
		min_x = min(min_x, coord.x)
		min_y = min(min_y, coord.y)

	for inst_id in grid.GetPlacedItemIds():
		var inst = grid.PlacedItems[inst_id]
		var item_ui = ItemUIScene.instantiate() as ItemUI
		grid_container.add_child(item_ui)
		
		item_ui.setup(inst.Item, inst.InstanceId, -1, inst.RotationSteps, cell_size)
		
		# Position matches anchor cell
		item_ui.position = Vector2((inst.AnchorCoords.x - min_x) * cell_size, (inst.AnchorCoords.y - min_y) * cell_size)
		item_ui.drag_started.connect(_on_item_drag_started)
		item_nodes[inst_id] = item_ui

func _on_grid_updated() -> void:
	_rebuild_placed_items()

func _on_stash_updated() -> void:
	# Rebuild stash items list
	for child in stash_container.get_children():
		child.queue_free()
		
	var stash = InventoryManager.Stash
	for i in range(stash.size()):
		var item = stash[i]
		var item_ui = ItemUIScene.instantiate() as ItemUI
		stash_container.add_child(item_ui)
		item_ui.setup(item, "", i, 0, cell_size)
		item_ui.drag_started.connect(_on_item_drag_started)

func _generate_new_shop_items() -> void:
	if current_shop_items.size() < 3:
		current_shop_items.resize(3)
		current_shop_items.fill(null)

	var temp_pool = test_items.duplicate()
	# Remove already locked items from temp_pool to prevent duplicates in other slots
	for i in range(3):
		if shop_slots_locked[i] and current_shop_items[i] != null:
			for j in range(temp_pool.size() - 1, -1, -1):
				if temp_pool[j].Id == current_shop_items[i].Id:
					temp_pool.remove_at(j)

	for i in range(3):
		if shop_slots_locked[i]:
			continue
		if temp_pool.size() > 0:
			var idx = randi() % temp_pool.size()
			current_shop_items[i] = temp_pool[idx]
			temp_pool.remove_at(idx)
		else:
			current_shop_items[i] = null

func _on_reroll_pressed() -> void:
	if GameManager.Gold >= 2:
		if GameManager.SpendGold(2):
			_generate_new_shop_items()
			_refresh_shop()

func _refresh_shop() -> void:
	for child in shop_container.get_children():
		child.queue_free()

	# Update Reroll button disabled state
	if reroll_button:
		reroll_button.disabled = GameManager.Gold < 2

	# Draw items in current_shop_items
	for i in range(current_shop_items.size()):
		var item = current_shop_items[i]
		
		var shop_item_panel = Button.new()
		shop_item_panel.custom_minimum_size = Vector2(100, 120)
		shop_container.add_child(shop_item_panel)

		# Capture index for right-click toggles and signal connections
		var item_idx = i

		# Listen to right clicks on the shop slot panel to toggle lock
		shop_item_panel.gui_input.connect(func(event):
			if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_RIGHT and event.pressed:
				if current_shop_items[item_idx] != null:
					shop_slots_locked[item_idx] = not shop_slots_locked[item_idx]
					_refresh_shop()
		)

		if item == null:
			var sold_lbl = Label.new()
			sold_lbl.text = "SOLD"
			sold_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
			sold_lbl.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
			shop_item_panel.add_child(sold_lbl)
			continue

		# Color tint locked vs unlocked
		if shop_slots_locked[i]:
			shop_item_panel.self_modulate = Color(1.0, 0.85, 0.3) # Gold tint for locked slot
		else:
			shop_item_panel.self_modulate = Color(1.0, 1.0, 1.0) # Normal tint

		var vbox = VBoxContainer.new()
		vbox.mouse_filter = Control.MOUSE_FILTER_PASS
		shop_item_panel.add_child(vbox)

		var name_lbl = Label.new()
		if shop_slots_locked[i]:
			name_lbl.text = "🔒 " + item.ItemName
		else:
			name_lbl.text = item.ItemName
		name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		name_lbl.mouse_filter = Control.MOUSE_FILTER_PASS
		vbox.add_child(name_lbl)

		var price_lbl = Label.new()
		price_lbl.text = "%d G" % item.BasePrice
		price_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		price_lbl.mouse_filter = Control.MOUSE_FILTER_PASS
		vbox.add_child(price_lbl)

		var buy_btn = Button.new()
		buy_btn.text = "BUY"
		buy_btn.disabled = GameManager.Gold < item.BasePrice
		buy_btn.pressed.connect(func():
			if InventoryManager.BuyItem(item):
				current_shop_items[item_idx] = null
				shop_slots_locked[item_idx] = false # Unlock on purchase
				_refresh_shop()
		)
		vbox.add_child(buy_btn)


func _on_item_drag_started(item_ui: ItemUI) -> void:
	if dragging_item != null: return

	dragging_item = item_ui
	dragging_item.is_dragging = true

	# Save original placement in case we need to roll back
	if item_ui.instance_id != "":
		# Dragging from grid
		drag_start_from_grid = true
		var inst = InventoryManager.BackpackGrid.PlacedItems[item_ui.instance_id]
		drag_original_anchor = Vector2i(inst.AnchorCoords.x, inst.AnchorCoords.y)
		drag_original_rotation = inst.RotationSteps
		
		# Temporarily remove from C# backend so cells are freed
		InventoryManager.BackpackGrid.RemoveItem(item_ui.instance_id)
		# Reparent node to BackpackUI root so it draws on top
		dragging_item.reparent(self)
	else:
		# Dragging from stash
		drag_start_from_grid = false
		drag_original_stash_index = item_ui.stash_index
		dragging_item.reparent(self)

func _input(event: InputEvent) -> void:
	if dragging_item == null: return

	# 1. Update position
	if event is InputEventMouseMotion:
		dragging_item.global_position = event.global_position - dragging_item.drag_offset
		_highlight_hovered_cells()
		
		# Hover shop to sell visual highlight
		var shop_rect: Rect2 = $MainLayout/ShopArea.get_global_rect()
		var is_over_shop: bool = shop_rect.has_point(event.global_position)
		var shop_lbl: Label = $MainLayout/ShopArea/ShopHeader/Label
		if is_over_shop:
			shop_lbl.text = "Black Market Shop (DROP TO SELL - 50% G)"
			shop_lbl.add_theme_color_override("font_color", Color(0.98, 0.81, 0.32)) # gold
		else:
			shop_lbl.text = "Black Market Shop"
			shop_lbl.remove_theme_color_override("font_color")

	# 2. Right click / R key to rotate
	if (event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_RIGHT and event.pressed) or \
	   (event is InputEventKey and event.keycode == KEY_R and event.pressed):
		dragging_item.rotation_steps = (dragging_item.rotation_steps + 1) % 4
		dragging_item.update_visuals()
		_highlight_hovered_cells()
		get_viewport().set_input_as_handled()

	# 3. Release left click to drop
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and not event.pressed:
		_handle_drop()
		get_viewport().set_input_as_handled()

func _highlight_hovered_cells() -> void:
	_clear_cell_highlights()

	var grid = InventoryManager.BackpackGrid
	if grid == null or dragging_item == null: return

	var anchor = _calculate_hover_anchor()
	var can_place = grid.CanPlaceItem(dragging_item.item_ref, anchor, dragging_item.rotation_steps)
	
	# Fetch rotated offsets
	var offsets: Array = dragging_item.item_ref.GetRotatedOffsets(dragging_item.rotation_steps)
	
	# Highlight cells
	var highlight_color = Color(0.2, 0.8, 0.2, 0.6) if can_place else Color(0.8, 0.2, 0.2, 0.6)
	
	for offset in offsets:
		var cell_coord = anchor + offset
		if cell_nodes.has(cell_coord):
			cell_nodes[cell_coord].highlight(highlight_color)

func _clear_cell_highlights() -> void:
	for cell in cell_nodes.values():
		cell.reset_highlight()

func _calculate_hover_anchor() -> Vector2i:
	# Calculate coordinate offset based on grid top-left min coordinates
	var grid = InventoryManager.BackpackGrid
	var min_x = 999
	var min_y = 999
	for coord in grid.GetCellCoords():
		min_x = min(min_x, coord.x)
		min_y = min(min_y, coord.y)

	# Local position relative to grid container
	var local_pos = grid_container.get_global_transform().affine_inverse() * dragging_item.global_position
	
	# Determine snapping cell
	var snap_x = round(local_pos.x / cell_size) + min_x
	var snap_y = round(local_pos.y / cell_size) + min_y
	return Vector2i(snap_x, snap_y)

func _handle_drop() -> void:
	_clear_cell_highlights()
	
	# Reset shop label visual overrides
	var shop_lbl: Label = $MainLayout/ShopArea/ShopHeader/Label
	shop_lbl.text = "Black Market Shop"
	shop_lbl.remove_theme_color_override("font_color")

	var grid = InventoryManager.BackpackGrid
	var im = InventoryManager
	
	# 1. Check if dropped on ShopArea to sell
	var shop_rect: Rect2 = $MainLayout/ShopArea.get_global_rect()
	var is_over_shop: bool = shop_rect.has_point(get_global_mouse_position())
	if is_over_shop:
		if drag_start_from_grid:
			im.SellPlacedItem(dragging_item.instance_id)
		else:
			im.SellStashedItem(drag_original_stash_index)
		if is_instance_valid(dragging_item):
			dragging_item.queue_free()
		dragging_item = null
		_refresh_shop()
		return

	# 2. Check if dropped on StashArea to unequip
	var stash_rect: Rect2 = $MainLayout/HBox/StashArea.get_global_rect()
	var is_over_stash: bool = stash_rect.has_point(get_global_mouse_position())
	if is_over_stash and drag_start_from_grid:
		im.TryTakeItemToStash(dragging_item.instance_id)
		if is_instance_valid(dragging_item):
			dragging_item.queue_free()
		dragging_item = null
		_refresh_shop()
		return

	var anchor = _calculate_hover_anchor()
	var placed_successfully = false

	if grid.CanPlaceItem(dragging_item.item_ref, anchor, dragging_item.rotation_steps):
		if drag_start_from_grid:
			# Re-place on grid in C#
			grid.PlaceItem(dragging_item.item_ref, anchor, dragging_item.rotation_steps, dragging_item.instance_id)
			placed_successfully = true
			im.emit_signal("GridUpdated")
		else:
			# Place from stash
			placed_successfully = im.TryPlaceItemFromStash(drag_original_stash_index, anchor, dragging_item.rotation_steps)

	if not placed_successfully:
		# Rollback
		if drag_start_from_grid:
			grid.PlaceItem(dragging_item.item_ref, drag_original_anchor, drag_original_rotation, dragging_item.instance_id)
			im.emit_signal("GridUpdated")
		else:
			# Just put back in stash
			im.emit_signal("StashUpdated")
			
	if is_instance_valid(dragging_item):
		dragging_item.queue_free()
	dragging_item = null
	_refresh_shop()


func _on_start_combat_pressed() -> void:
	# Transition to Combat scene
	GameManager.CurrentState = 1 # GameState.Combat
	CombatManager.StartCombat()
	
	# For prototype, we will load the combat scene
	get_tree().change_scene_to_file("res://src/entities/player/Level.tscn")
