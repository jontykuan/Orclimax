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

	back_vessel_btn.pressed.connect(func(): GameManager.GoToVesselSelect())
	prep_btn.pressed.connect(func(): GameManager.GoToBackpack())
	combat_btn.pressed.connect(func(): GameManager.StartCombatNode())

	btn_stage1.pressed.connect(func(): _on_node_selected("node_1", "Stage 1-1: Orc Borderlands", "Normal Combat - Basic enemies ahead"))
	btn_stage2a.pressed.connect(func(): _on_node_selected("node_2a", "Stage 1-2A: Black Market Outpost", "Shop Node - Purchase equipment and toys"))
	btn_stage2b.pressed.connect(func(): _on_node_selected("node_2b", "Stage 1-2B: Elite Vanguard", "Elite Combat - Dangerous enemy with higher loot drop rate"))
	btn_stage3.pressed.connect(func(): _on_node_selected("node_3", "Stage 1-3: Sanctuary Keep", "Boss Combat - Stage Guardian"))

	_update_node_visuals()

func _on_gold_changed(gold: int) -> void:
	gold_label.text = "GOLD: %d" % gold

func _on_stage_changed(stage: int) -> void:
	stage_label.text = "WORLD MAP — STAGE %d" % stage

func _on_node_selected(id: String, title: String, desc: String) -> void:
	selected_node_id = id
	node_title_lbl.text = title
	node_desc_lbl.text = desc
	_update_node_visuals()

func _update_node_visuals() -> void:
	# Reset modulations
	btn_stage1.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "node_1" else Color(1, 1, 1)
	btn_stage2a.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "node_2a" else Color(1, 1, 1)
	btn_stage2b.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "node_2b" else Color(1, 1, 1)
	btn_stage3.self_modulate = Color(0.4, 0.9, 0.4) if selected_node_id == "node_3" else Color(1, 1, 1)
