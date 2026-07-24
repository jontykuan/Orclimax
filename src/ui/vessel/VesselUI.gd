extends Control

@onready var vessel_list_container: VBoxContainer = $MainLayout/ContentHBox/VesselSelectionList/VesselScroll/VesselList
@onready var vessel_name_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/VesselNameLabel

@onready var head_sens_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/SensitivitiesGroup/HeadSensLabel
@onready var chest_sens_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/SensitivitiesGroup/ChestSensLabel
@onready var groin_sens_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/SensitivitiesGroup/GroinSensLabel
@onready var limbs_sens_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/SensitivitiesGroup/LimbsSensLabel

@onready var pleasure_stats_label: Label = $MainLayout/ContentHBox/DetailsPanel/LeftDetails/PleasureMultipliersLabel
@onready var skill_title_label: Label = $MainLayout/ContentHBox/DetailsPanel/RightDetails/SkillTitleLabel
@onready var skill_desc_label: Label = $MainLayout/ContentHBox/DetailsPanel/RightDetails/SkillDescLabel
@onready var traits_desc_label: RichTextLabel = $MainLayout/ContentHBox/DetailsPanel/RightDetails/TraitsTextLabel

@onready var confirm_button: Button = $MainLayout/ContentHBox/DetailsPanel/RightDetails/SelectVesselButton
@onready var back_title_button: Button = $MainLayout/TopBar/BackToTitleButton

var available_vessels: Array[Resource] = []
var selected_vessel: Resource = null

func _ready() -> void:
	_create_vessel_data_objects()
	_populate_vessel_list()
	
	back_title_button.pressed.connect(func(): GameManager.GoToTitle())
	confirm_button.pressed.connect(_on_confirm_pressed)

func _create_vessel_data_objects() -> void:
	available_vessels.clear()

	# Vessel 0: Maye (Initial Unlocked Maiden)
	var v0 = load("res://src/core/VesselData.cs").new()
	v0.Id = "girl_maye"
	v0.CharacterName = "Human Maiden Maye"
	v0.ClimaxSkillName = "Temporal Acceleration"
	v0.ClimaxSkillDescription = "Grants Orc 150% item trigger speed for 1.8 seconds upon Climax."
	v0.BaseMaxPleasure = 90.0
	v0.PleasureBuildRateMultiplier = 0.7
	v0.HeadSensitivity = 1.0
	v0.ChestSensitivity = 1.0
	v0.GroinSensitivity = 1.2
	v0.LimbsSensitivity = 0.9
	v0.HeadDevLevel = 1
	v0.ChestDevLevel = 1
	v0.GroinDevLevel = 1
	v0.LimbsDevLevel = 1
	v0.TraitsDescription = "Human maiden with high sensitivity (0.7x). Climax accelerates item triggers by 150% for 1.8s."
	v0.HeadCells = [Vector2i(2, 0)]
	v0.ChestCells = [Vector2i(1, 1), Vector2i(2, 1), Vector2i(3, 1)]
	v0.GroinCells = [Vector2i(2, 2), Vector2i(2, 3)]
	v0.LimbsCells = [Vector2i(0, 1), Vector2i(4, 1), Vector2i(1, 4), Vector2i(3, 4)]
	v0.GeneralCells = [Vector2i(1, 2), Vector2i(3, 2), Vector2i(2, 4)]
	v0.InactiveCells = [Vector2i(0, 2), Vector2i(4, 2), Vector2i(0, 3), Vector2i(1, 3), Vector2i(3, 3), Vector2i(4, 3)]
	v0.InitiallyLockedCells = [Vector2i(1, 4), Vector2i(3, 4)]
	available_vessels.append(v0)

	# Vessel 1: Lydia (Unlocked via Stage 1 Boss: Flame Spire)
	var v1 = load("res://src/core/VesselData.cs").new()
	v1.Id = "girl_lydia"
	v1.CharacterName = "Elven Mage Lydia"
	v1.ClimaxSkillName = "Lightning Cascade"
	v1.ClimaxSkillDescription = "Unleashes a cascade of divine lightning dealing massive area damage."
	v1.BaseMaxPleasure = 100.0
	v1.PleasureBuildRateMultiplier = 0.5
	v1.HeadSensitivity = 1.1
	v1.ChestSensitivity = 1.4
	v1.GroinSensitivity = 1.8
	v1.LimbsSensitivity = 0.9
	v1.HeadDevLevel = 1
	v1.ChestDevLevel = 2
	v1.GroinDevLevel = 3
	v1.LimbsDevLevel = 1
	v1.TraitsDescription = "High groin sensitivity. Rapid pleasure accumulation during thrusting."
	v1.HeadCells = [Vector2i(2, 0), Vector2i(2, 1)]
	v1.ChestCells = [Vector2i(1, 2), Vector2i(2, 2), Vector2i(3, 2)]
	v1.GroinCells = [Vector2i(1, 3), Vector2i(2, 3), Vector2i(3, 3), Vector2i(2, 4)]
	v1.LimbsCells = [Vector2i(0, 2), Vector2i(4, 2), Vector2i(1, 5), Vector2i(3, 5)]
	v1.GeneralCells = [Vector2i(2, 5)]
	v1.InactiveCells = [Vector2i(0, 3), Vector2i(4, 3), Vector2i(0, 4), Vector2i(1, 4), Vector2i(3, 4), Vector2i(4, 4)]
	v1.InitiallyLockedCells = [Vector2i(2, 4), Vector2i(3, 5)]
	available_vessels.append(v1)

	# Vessel 2: Cynthia (Unlocked via Stage 2 Boss: Tale Breeze)
	var v2 = load("res://src/core/VesselData.cs").new()
	v2.Id = "girl_cynthia"
	v2.CharacterName = "Elven Archer Cynthia"
	v2.ClimaxSkillName = "Windstorm Arrow"
	v2.ClimaxSkillDescription = "Summons gale-force arrows sweeping through all enemy ranks."
	v2.BaseMaxPleasure = 120.0
	v2.PleasureBuildRateMultiplier = 0.6
	v2.HeadSensitivity = 1.0
	v2.ChestSensitivity = 1.6
	v2.GroinSensitivity = 1.3
	v2.LimbsSensitivity = 1.2
	v2.HeadDevLevel = 1
	v2.ChestDevLevel = 3
	v2.GroinDevLevel = 2
	v2.LimbsDevLevel = 2
	v2.TraitsDescription = "Sensitive chest region. Increased movement speed bonuses."
	v2.HeadCells = [Vector2i(2, 0)]
	v2.ChestCells = [Vector2i(1, 1), Vector2i(2, 1), Vector2i(3, 1)]
	v2.GroinCells = [Vector2i(2, 2), Vector2i(2, 3)]
	v2.LimbsCells = [Vector2i(0, 1), Vector2i(4, 1), Vector2i(0, 2), Vector2i(4, 2)]
	v2.GeneralCells = [Vector2i(1, 2), Vector2i(3, 2)]
	v2.InactiveCells = [Vector2i(0, 3), Vector2i(1, 3), Vector2i(3, 3), Vector2i(4, 3), Vector2i(2, 4)]
	v2.InitiallyLockedCells = [Vector2i(0, 2), Vector2i(4, 2)]
	available_vessels.append(v2)

func _populate_vessel_list() -> void:
	for child in vessel_list_container.get_children():
		if is_instance_valid(child):
			child.queue_free()

	var unlocked_ids = GameManager.UnlockedFemaleIds
	var first_unlocked_vessel = null

	for v in available_vessels:
		var is_unlocked = unlocked_ids.has(v.Id)
		var btn = Button.new()
		if is_unlocked:
			btn.text = v.CharacterName
			btn.disabled = false
			btn.pressed.connect(func(): _select_vessel(v))
			if first_unlocked_vessel == null:
				first_unlocked_vessel = v
		else:
			btn.text = "[LOCKED] " + v.CharacterName
			btn.disabled = true
		btn.custom_minimum_size = Vector2(240, 44)
		vessel_list_container.add_child(btn)

	# Select first unlocked vessel or match InventoryManager
	var current = InventoryManager.CurrentVessel
	if current != null and unlocked_ids.has(current.Id):
		_select_vessel(current)
	elif first_unlocked_vessel != null:
		_select_vessel(first_unlocked_vessel)

func _select_vessel(vessel: Resource) -> void:
	selected_vessel = vessel
	InventoryManager.SetVessel(vessel, true)

	vessel_name_label.text = vessel.CharacterName
	head_sens_label.text = "Head: Sens x%.1f (Dev Lv.%d)" % [vessel.HeadSensitivity, vessel.HeadDevLevel]
	chest_sens_label.text = "Chest: Sens x%.1f (Dev Lv.%d)" % [vessel.ChestSensitivity, vessel.ChestDevLevel]
	groin_sens_label.text = "Groin: Sens x%.1f (Dev Lv.%d)" % [vessel.GroinSensitivity, vessel.GroinDevLevel]
	limbs_sens_label.text = "Limbs: Sens x%.1f (Dev Lv.%d)" % [vessel.LimbsSensitivity, vessel.LimbsDevLevel]

	pleasure_stats_label.text = "Max Pleasure: %d | Gain Rate: %.1fx" % [vessel.BaseMaxPleasure, vessel.PleasureBuildRateMultiplier]
	skill_title_label.text = "Climax Skill: " + vessel.ClimaxSkillName
	skill_desc_label.text = vessel.ClimaxSkillDescription
	traits_desc_label.text = "[color=#f59e0b]Traits:[/color]\n" + vessel.TraitsDescription

func _on_confirm_pressed() -> void:
	if selected_vessel != null:
		InventoryManager.SetVessel(selected_vessel, true)
	GameManager.GoToMap()
