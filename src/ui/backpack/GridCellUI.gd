extends ColorRect

class_name GridCellUI

var coords: Vector2i
var zone: int = 0
var is_locked: bool = false
var cell_size: float = 64.0

func setup(p_coords: Vector2i, p_zone: int, p_locked: bool, p_cell_size: float) -> void:
	coords = p_coords
	zone = p_zone
	is_locked = p_locked
	custom_minimum_size = Vector2(p_cell_size, p_cell_size)
	size = custom_minimum_size
	
	update_visuals()

func update_visuals() -> void:
	# Styling colors based on zone (with alpha for glassmorphism style)
	var color_val := Color(0.2, 0.25, 0.3, 0.4) # Default General (dark grey-blue)
	
	if is_locked:
		color_val = Color(0.1, 0.1, 0.1, 0.8) # Locked (dark)
	else:
		match zone:
			1: # Head: soft blue
				color_val = Color(0.2, 0.6, 1.0, 0.5)
			2: # Chest: soft green
				color_val = Color(0.2, 0.8, 0.4, 0.5)
			3: # Groin: soft pink/magenta (groin area)
				color_val = Color(1.0, 0.2, 0.6, 0.5)
			4: # Limbs: soft orange
				color_val = Color(1.0, 0.6, 0.2, 0.5)
			0: # General: neutral
				color_val = Color(0.3, 0.35, 0.4, 0.4)
			5: # Inactive: faint grey
				color_val = Color(0.12, 0.12, 0.16, 0.3)
				
	color = color_val

func highlight(color_highlight: Color) -> void:
	color = color_highlight

func reset_highlight() -> void:
	update_visuals()
