extends Control

@onready var stage_label: Label = $MainLayout/Header/StageTitle
@onready var gold_label: Label = $MainLayout/Header/GoldLabel
@onready var back_vessel_btn: Button = $MainLayout/Header/BackToVesselButton
@onready var prep_btn: Button = $MainLayout/DetailPanel/Margin/DetailHBox/EnterPrepButton
@onready var combat_btn: Button = $MainLayout/DetailPanel/Margin/DetailHBox/EnterCombatButton

@onready var node_title_lbl: Label = $MainLayout/DetailPanel/Margin/DetailHBox/NodeInfo/NodeTitle
@onready var node_desc_lbl: Label = $MainLayout/DetailPanel/Margin/DetailHBox/NodeInfo/NodeDesc

# Node buttons
@onready var btn_stage1: Button = $MainLayout/MapViewport/NodesContainer/Node1
@onready var btn_stage2a: Button = $MainLayout/MapViewport/NodesContainer/Node2A
@onready var btn_stage2b: Button = $MainLayout/MapViewport/NodesContainer/Node2B
@onready var btn_stage3: Button = $MainLayout/MapViewport/NodesContainer/Node3

var selected_node_id: String = "node_1"

func _ready() -> void:
	var gm = GameManager
	gm.connect("GoldChanged", _on_gold_changed)
	gm.connect("StageChanged", _on_stage_changed)

	_on_gold_changed(gm.Gold)
	_on_stage_changed(gm.CurrentStage)

	# Back button returns to Preparation (Backpack) phase!
	back_vessel_btn.pressed.connect(func(): GameManager.GoToBackpack())
	prep_btn.pressed.connect(func(): GameManager.GoToBackpack())
	combat_btn.pressed.connect(func():
		if not GameManager.ClearedStageIds.has(selected_node_id):
			GameManager.CurrentSelectedStageId = selected_node_id
			GameManager.StartCombatNode()
	)

	btn_stage1.pressed.connect(func(): _on_node_selected("flame_spire", "Stage 1: Flame Spire", "Enemies: Melee Infantry, Heavy Shield, Flat-shot Ranged.\nBoss at 150s: Elven Mage Lydia (Unlocks Lydia)"))
	btn_stage2a.pressed.connect(func(): _on_node_selected("tale_breeze", "Stage 2: Tale Breeze", "Enemies: Flat-shot Ranged, Parabolic Lob Ranged, Vessel Snatcher.\nBoss at 150s: Elven Archer Cynthia (Unlocks Cynthia)"))
	btn_stage2b.pressed.connect(func(): _on_node_selected("flame_spire", "Stage 1: Flame Spire", "Enemies: Melee Infantry, Heavy Shield, Flat-shot Ranged.\nBoss at 150s: Elven Mage Lydia"))
	btn_stage3.pressed.connect(func(): _on_node_selected("tale_breeze", "Stage 2: Tale Breeze", "Enemies: Flat-shot Ranged, Parabolic Lob Ranged, Vessel Snatcher.\nBoss at 150s: Elven Archer Cynthia"))

	_on_node_selected("flame_spire", "Stage 1: Flame Spire", "Enemies: Melee Infantry, Heavy Shield, Flat-shot Ranged.\nBoss at 150s: Elven Mage Lydia (Unlocks Lydia)")

func _on_gold_changed(gold: int) -> void:
	gold_label.text = "GOLD: %d" % gold

func _on_stage_changed(stage: int) -> void:
	stage_label.text = "WORLD MAP — SELECT COMBAT STAGE"

func _on_node_selected(id: String, title: String, desc: String) -> void:
	selected_node_id = id
	var is_cleared = GameManager.ClearedStageIds.has(id)
	node_title_lbl.text = title + (" [CLEARED / 已攻略]" if is_cleared else " [AVAILABLE / 可攻略]")
	node_desc_lbl.text = desc
	_update_node_visuals()

func _update_node_visuals() -> void:
	var cleared_ids = GameManager.ClearedStageIds
	
	# Update Flame Spire Node button text & status
	var flame_cleared = cleared_ids.has("flame_spire")
	btn_stage1.text = (" [CLEARED]\nStage 1: Flame Spire" if flame_cleared else "[AVAILABLE]\nStage 1: Flame Spire")
	btn_stage1.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "flame_spire" else Color(1, 1, 1)

	# Update Tale Breeze Node button text & status
	var breeze_cleared = cleared_ids.has("tale_breeze")
	btn_stage2a.text = ("[CLEARED]\nStage 2: Tale Breeze" if breeze_cleared else "[AVAILABLE]\nStage 2: Tale Breeze")
	btn_stage2a.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "tale_breeze" else Color(1, 1, 1)

	btn_stage2b.visible = false
	btn_stage3.visible = false

	# Cleared stages can be viewed, but cannot be re-entered!
	var selected_is_cleared = cleared_ids.has(selected_node_id)
	if selected_is_cleared:
		combat_btn.disabled = true
		combat_btn.text = "CLEARED (已攻略)"
	else:
		combat_btn.disabled = false
		combat_btn.text = "ENTER COMBAT (進入戰鬥)"
