extends CanvasLayer

class_name HUD

@onready var hp_bar: ProgressBar = $BottomLeft/VBox/HPBar
@onready var hp_label: Label = $BottomLeft/VBox/HPLabel
@onready var pleasure_bar: ProgressBar = $BottomLeft/VBox/PleasureBar
@onready var pleasure_label: Label = $BottomLeft/VBox/PleasureLabel

@onready var pip_panel: PanelContainer = $BottomRight/PiPPanel
@onready var expression_label: Label = $BottomRight/PiPPanel/VBox/ExpressionLabel
@onready var status_lbl: Label = $BottomRight/PiPPanel/VBox/StatusLabel

@onready var climax_overlay: ColorRect = $ClimaxOverlay
@onready var climax_label: Label = $ClimaxOverlay/ClimaxLabel
@onready var return_button: Button = $VictoryPanel/VBox/ReturnButton
@onready var victory_panel: PanelContainer = $VictoryPanel

func _ready() -> void:
	# Hide climax and victory overlays initially
	climax_overlay.visible = false
	victory_panel.visible = false
	
	# Connect signals from C#
	var cm = CombatManager
	cm.connect("HpChanged", _on_hp_changed)
	cm.connect("PleasureChanged", _on_pleasure_changed)
	cm.connect("ClimaxTriggered", _on_climax_triggered)
	
	# Connect Game Manager signal to show game over / victory
	GameManager.connect("StateChanged", _on_game_state_changed)

	# Initial values
	_on_hp_changed(cm.CurrentHp, cm.MaxHp)
	_on_pleasure_changed(cm.CurrentPleasure, cm.MaxPleasure)

func _on_hp_changed(current: float, max_val: float) -> void:
	hp_bar.max_value = max_val
	hp_bar.value = current
	hp_label.text = "HP: %d / %d" % [current, max_val]

func _on_pleasure_changed(current: float, max_val: float) -> void:
	pleasure_bar.max_value = max_val
	pleasure_bar.value = current
	pleasure_label.text = "Pleasure: %d%%" % [round((current / max_val) * 100)]
	
	_update_pip_expression(current / max_val)

func _update_pip_expression(ratio: float) -> void:
	var name_str = "None"
	if InventoryManager.CurrentFemale != null:
		name_str = InventoryManager.CurrentFemale.CharacterName
	
	status_lbl.text = "Mounted: " + name_str
	
	if ratio < 0.3:
		expression_label.text = "Expression: Mild / Blushing"
	elif ratio < 0.7:
		expression_label.text = "Expression: Panting / Heavy Breathing"
	else:
		expression_label.text = "Expression: Intense Ecstasy / Blurry Eyes"

func _on_climax_triggered(_female_id: String, skill_name: String) -> void:
	# Show Street Fighter style background H-CG / Climax Cut-in
	climax_label.text = "CLIMAX!\nMAGIC OVERFLOW: " + skill_name.to_upper()
	climax_overlay.visible = true
	climax_overlay.color = Color(1.0, 0.4, 0.7, 0.5) # Glowing semi-transparent pink
	
	# Screen flash animation
	var tween = create_tween()
	tween.tween_property(climax_overlay, "color", Color(1.0, 1.0, 1.0, 0.9), 0.1) # Flash white
	tween.tween_property(climax_overlay, "color", Color(1.0, 0.4, 0.7, 0.6), 0.15) # Flashing pink
	tween.tween_interval(0.8) # Show H-climax screen brief moment
	tween.tween_property(climax_overlay, "visible", false, 0.25)
	
	# Deal massive climax magic damage to all enemies currently on screen!
	var enemies = get_tree().get_nodes_in_group("enemies")
	for enemy in enemies:
		if enemy.has_method("take_damage"):
			enemy.take_damage(50.0) # Massive blast damage

func _on_game_state_changed(new_state: int) -> void:
	if new_state == 2: # GameState.GameOver
		climax_label.text = "ORC DEFEATED"
		climax_overlay.color = Color(0.3, 0.1, 0.1, 0.8)
		climax_overlay.visible = true
		
		# Return to shop button
		var timer = get_tree().create_timer(1.5)
		timer.timeout.connect(func():
			GameManager.CurrentState = 0 # Shop
			get_tree().change_scene_to_file("res://src/ui/backpack/BackpackUI.tscn")
		)

func show_victory() -> void:
	victory_panel.visible = true

func _on_return_button_pressed() -> void:
	# Advance stage and return to shop
	GameManager.AdvanceStage()
	get_tree().change_scene_to_file("res://src/ui/backpack/BackpackUI.tscn")
