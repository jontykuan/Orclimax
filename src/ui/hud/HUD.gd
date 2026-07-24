extends CanvasLayer

class_name HUD

const ProjectileScene = preload("res://src/entities/weapons/Projectile.tscn")

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

@onready var dodge_bar: ProgressBar = $TopLeft/CooldownHBox/DodgeCD/Bar
@onready var parry_bar: ProgressBar = $TopLeft/CooldownHBox/ParryCD/Bar
@onready var thrust_bar: ProgressBar = $TopLeft/CooldownHBox/ThrustCD/Bar
@onready var buff_list: VBoxContainer = $TopRight/BuffVBox/BuffList

@onready var timer_label: Label = $TopCenter/TimerVBox/TimerLabel
@onready var warning_label: Label = $TopCenter/TimerVBox/WarningLabel

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

func update_stage_timer(remaining_seconds: float) -> void:
	if not timer_label: return
	var total_sec = max(0, int(ceil(remaining_seconds)))
	var mins = total_sec / 60
	var secs = total_sec % 60
	timer_label.text = "TIME LEFT: %02d:%02d" % [mins, secs]

func update_wave_warning(seconds_until_wave: float) -> void:
	if not warning_label: return
	if seconds_until_wave <= 5.0 and seconds_until_wave > 0.0:
		var sec_display = int(ceil(seconds_until_wave))
		warning_label.text = "[WARNING] ENEMY WAVE APPROACHING IN %ds!" % sec_display
		warning_label.visible = true
	else:
		warning_label.visible = false

func _process(_delta: float) -> void:
	var player = get_tree().get_first_node_in_group("player")
	if not player: return

	# Update Cooldown Progress Bars (0 = ready/full bar, >0 = on cooldown)
	var dash_max = player.dash_cooldown if player.dash_cooldown > 0 else 1.0
	dodge_bar.value = (1.0 - (player.dash_cooldown_timer / dash_max)) * 100.0

	var parry_max = GameConfig.ParryCooldown if GameConfig and GameConfig.ParryCooldown > 0 else 4.0
	parry_bar.value = (1.0 - (player.parry_cooldown_timer / parry_max)) * 100.0

	var thrust_max = GameConfig.ThrustCooldown if GameConfig and GameConfig.ThrustCooldown > 0 else 5.0
	thrust_bar.value = (1.0 - (player.thrust_cooldown_timer / thrust_max)) * 100.0

	# Update Active Passive Buffs
	_update_buff_list(player)

func _update_buff_list(player: Node2D) -> void:
	if not buff_list: return
	for child in buff_list.get_children():
		if is_instance_valid(child):
			child.queue_free()

	# 1. Maye Climax Item Speed Buff
	if CombatManager.TempAttackSpeedMultiplierTimer > 0:
		var lbl = Label.new()
		lbl.text = "[BUFF] Temporal Surge (+150%% Speed): %.1fs" % CombatManager.TempAttackSpeedMultiplierTimer
		lbl.add_theme_color_override("font_color", Color(0.96, 0.8, 0.2))
		lbl.add_theme_font_size_override("font_size", 12)
		buff_list.add_child(lbl)

	# 2. Stackable Burn Debuff Status
	if CombatManager.BurnTimer > 0:
		var lbl = Label.new()
		lbl.text = "[BURN] Fire Damage x%d Stacks (%.1fs)" % [CombatManager.BurnStacks, CombatManager.BurnTimer]
		lbl.add_theme_color_override("font_color", Color(1.0, 0.35, 0.1))
		lbl.add_theme_font_size_override("font_size", 12)
		buff_list.add_child(lbl)

	# 3. Player Dash i-Frames
	if player.get("is_dashing"):
		var lbl = Label.new()
		lbl.text = "[STATE] Evasion i-Frames Active"
		lbl.add_theme_color_override("font_color", Color(0.3, 0.9, 0.9))
		lbl.add_theme_font_size_override("font_size", 12)
		buff_list.add_child(lbl)

	# 4. Player Parry Guard Stance
	if player.get("is_parrying"):
		var lbl = Label.new()
		lbl.text = "[STATE] Precise Counter Stance Active"
		lbl.add_theme_color_override("font_color", Color(0.9, 0.3, 0.3))
		lbl.add_theme_font_size_override("font_size", 12)
		buff_list.add_child(lbl)

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
	if InventoryManager.CurrentVessel != null:
		name_str = InventoryManager.CurrentVessel.CharacterName
	
	status_lbl.text = "Mounted: " + name_str
	
	if ratio < 0.3:
		expression_label.text = "Expression: Mild / Blushing"
	elif ratio < 0.7:
		expression_label.text = "Expression: Panting / Heavy Breathing"
	else:
		expression_label.text = "Expression: Intense Ecstasy / Blurry Eyes"

func _on_climax_triggered(_female_id: String, skill_name: String) -> void:
	# Show Street Fighter style background H-CG / Climax Cut-in
	var vessel_name = "Vessel"
	if InventoryManager.CurrentVessel != null:
		vessel_name = InventoryManager.CurrentVessel.CharacterName
	climax_label.text = "CLIMAX!\n%s\nSKILL: %s" % [vessel_name.to_upper(), skill_name.to_upper()]
	climax_overlay.visible = true
	climax_overlay.color = Color(1.0, 0.4, 0.7, 0.5) # Glowing semi-transparent pink
	
	# Screen flash animation
	var tween = create_tween()
	tween.tween_property(climax_overlay, "color", Color(1.0, 1.0, 1.0, 0.9), 0.1) # Flash white
	tween.tween_property(climax_overlay, "color", Color(1.0, 0.4, 0.7, 0.6), 0.15) # Flashing pink
	tween.tween_interval(0.8) # Show H-climax screen brief moment
	tween.tween_property(climax_overlay, "visible", false, 0.25)
	
	# Trigger specific climax visual/combat effects based on active female_id!
	if female_id == "girl_lydia":
		# Lydia: Ring of fire pillars around Orc dealing damage & inflicting Burn debuff
		var player = get_tree().get_first_node_in_group("player")
		if player:
			var center_pos = player.global_position
			var num_pillars = 10
			var radius = 180.0
			for i in range(num_pillars):
				var angle = i * (TAU / num_pillars)
				var offset = Vector2(cos(angle), sin(angle)) * radius
				var proj = ProjectileScene.instantiate() as Projectile
				get_tree().current_scene.add_child(proj)
				proj.global_position = center_pos + offset
				proj.setup(15.0, Vector2.ZERO, 0.0, false, true) # 15 dmg, fire pillar, applies Burn!

			var enemies = get_tree().get_nodes_in_group("enemies")
			for enemy in enemies:
				if is_instance_valid(enemy):
					var dist = center_pos.distance_to(enemy.global_position)
					if dist <= 240.0 and enemy.has_method("take_damage"):
						enemy.take_damage(25.0)

	elif female_id == "girl_cynthia":
		# Cynthia: Rapidly shoot 5 arrows forward in direction facing
		var player = get_tree().get_first_node_in_group("player")
		if player:
			var center_pos = player.global_position + Vector2(0, -30)
			var base_dir = player.facing_direction if player.get("facing_direction") else Vector2.RIGHT
			var angles = [-0.26, -0.13, 0.0, 0.13, 0.26]
			for a in angles:
				var shoot_dir = base_dir.rotated(a)
				var proj = ProjectileScene.instantiate() as Projectile
				get_tree().current_scene.add_child(proj)
				proj.global_position = center_pos
				proj.setup(22.0, shoot_dir, 650.0, false, false)
	# Maye ("girl_maye") grants 120% item speed boost for 0.8s via CombatManager, NO screen wipe!

func _on_game_state_changed(new_state: int) -> void:
	if new_state == 5: # GameState.GameOver
		climax_label.text = "ORC DEFEATED"
		climax_overlay.color = Color(0.3, 0.1, 0.1, 0.8)
		climax_overlay.visible = true
		
		var timer = get_tree().create_timer(1.5)
		timer.timeout.connect(func():
			GameManager.GoToTitle()
		)
	elif new_state == 6: # GameState.Victory
		show_victory()

func show_victory() -> void:
	victory_panel.visible = true

func _on_return_button_pressed() -> void:
	# Advance stage and return to map
	GameManager.AdvanceStage()
