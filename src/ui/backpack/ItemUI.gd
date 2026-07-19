extends Control

class_name ItemUI

signal drag_started(item_ui)
signal hovered(item_data)
signal unhovered()

func _ready() -> void:
	mouse_entered.connect(_on_mouse_entered)
	mouse_exited.connect(_on_mouse_exited)

func _on_mouse_entered() -> void:
	if item_ref:
		hovered.emit(item_ref)

func _on_mouse_exited() -> void:
	unhovered.emit()

var item_ref: Resource # C# ItemData
var instance_id: String = "" # Empty if in stash
var stash_index: int = -1 # -1 if placed in grid
var rotation_steps: int = 0
var cell_size: float = 64.0

var blocks: Array[ColorRect] = []

# Mouse drag state
var is_dragging: bool = false
var drag_offset: Vector2 = Vector2.ZERO
var press_pos: Vector2 = Vector2.ZERO
var drag_threshold: float = 6.0
var has_moved_for_drag: bool = false
var is_pressed: bool = false

func setup(p_item_ref: Resource, p_instance_id: String, p_stash_index: int, p_rotation_steps: int, p_cell_size: float = 64.0) -> void:
	item_ref = p_item_ref
	instance_id = p_instance_id
	stash_index = p_stash_index
	rotation_steps = p_rotation_steps
	cell_size = p_cell_size
	
	mouse_filter = Control.MOUSE_FILTER_STOP
	update_visuals()

func update_visuals() -> void:
	# Clear old blocks
	for b in blocks:
		b.queue_free()
	blocks.clear()

	if not item_ref: return

	# 1. Get rotated offsets from C#
	var rotated_offsets: Array = item_ref.GetRotatedOffsets(rotation_steps)
	
	# Determine item theme color based on category
	# ItemCategory enum values: 0=Weapon, 1=Toy, 2=Clothing, 3=Accessory, 4=Consumable
	var cat: int = item_ref.Category
	var border_color := Color(0.9, 0.9, 0.9, 0.8) # Default white
	var fill_color := Color(0.2, 0.2, 0.2, 0.7) # Glassy grey
	
	match cat:
		0: # Weapon
			border_color = Color(1.0, 0.3, 0.3, 0.9) # Red-orange
			fill_color = Color(0.35, 0.15, 0.15, 0.8)
		1: # Toy
			border_color = Color(0.9, 0.2, 0.8, 0.9) # Magenta-pink
			fill_color = Color(0.3, 0.1, 0.25, 0.8)
		2: # Clothing
			border_color = Color(0.2, 0.9, 0.5, 0.9) # Light green
			fill_color = Color(0.1, 0.3, 0.2, 0.8)
		3: # Accessory
			border_color = Color(1.0, 0.85, 0.2, 0.9) # Gold
			fill_color = Color(0.3, 0.25, 0.1, 0.8)
		4: # Consumable
			border_color = Color(0.2, 0.7, 1.0, 0.9) # Cyan
			fill_color = Color(0.1, 0.2, 0.3, 0.8)

	if item_ref and item_ref.IsLocked:
		border_color = Color(1.0, 0.1, 0.1, 1.0) # Bright red lock border


	# 2. Draw blocks for each shape offset
	var min_x = 9999
	var max_x = -9999
	var min_y = 9999
	var max_y = -9999

	for offset in rotated_offsets:
		min_x = min(min_x, offset.x)
		max_x = max(max_x, offset.x)
		min_y = min(min_y, offset.y)
		max_y = max(max_y, offset.y)

		# Create cell block Rect
		var block = ColorRect.new()
		block.size = Vector2(cell_size - 4, cell_size - 4) # leave 2px gap
		block.position = Vector2(offset.x * cell_size + 2, offset.y * cell_size + 2)
		block.color = fill_color
		block.mouse_filter = Control.MOUSE_FILTER_PASS
		
		# Draw thin border
		var border = ReferenceRect.new()
		border.size = block.size
		border.border_color = border_color
		border.border_width = 2
		border.editor_only = false
		border.mouse_filter = Control.MOUSE_FILTER_PASS
		block.add_child(border)

		add_child(block)
		blocks.append(block)

	# 3. Adjust the parent control size to fit the bounding box
	var w = (max_x - min_x + 1) * cell_size
	var h = (max_y - min_y + 1) * cell_size
	custom_minimum_size = Vector2(w, h)
	size = custom_minimum_size

	# 4. Add a label on the anchor cell (0, 0)
	var label = Label.new()
	if item_ref.IsLocked:
		label.text = "🔒 " + item_ref.ItemName
	else:
		label.text = item_ref.ItemName
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.size = Vector2(cell_size - 4, cell_size - 4)
	label.position = Vector2(2, 2)
	label.add_theme_font_size_override("font_size", 10)
	label.mouse_filter = Control.MOUSE_FILTER_PASS
	add_child(label)

func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				is_pressed = true
				press_pos = event.position
				has_moved_for_drag = false
				accept_event()
			else:
				if is_pressed:
					is_pressed = false
					if not has_moved_for_drag:
						_toggle_lock()
					accept_event()
					
	elif event is InputEventMouseMotion:
		if is_pressed and not has_moved_for_drag:
			if event.position.distance_to(press_pos) > drag_threshold:
				has_moved_for_drag = true
				drag_offset = press_pos
				drag_started.emit(self)
				accept_event()

func _toggle_lock() -> void:
	if item_ref:
		item_ref.IsLocked = not item_ref.IsLocked
		update_visuals()
