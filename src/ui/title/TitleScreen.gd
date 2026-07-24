extends Control

@onready var start_btn: Button = $MainContainer/VBox/MenuButtons/StartButton
@onready var save_load_btn: Button = $MainContainer/VBox/MenuButtons/SaveLoadButton
@onready var gallery_btn: Button = $MainContainer/VBox/MenuButtons/GalleryButton
@onready var settings_btn: Button = $MainContainer/VBox/MenuButtons/SettingsButton
@onready var quit_btn: Button = $MainContainer/VBox/MenuButtons/QuitButton

@onready var settings_modal: PanelContainer = $SettingsModal
@onready var save_load_modal: PanelContainer = $SaveLoadModal
@onready var gallery_modal: PanelContainer = $GalleryModal

@onready var close_settings_btn: Button = $SettingsModal/Margin/VBox/CloseButton
@onready var close_saveload_btn: Button = $SaveLoadModal/Margin/VBox/CloseButton
@onready var close_gallery_btn: Button = $GalleryModal/Margin/VBox/CloseButton

func _ready() -> void:
	settings_modal.visible = false
	save_load_modal.visible = false
	gallery_modal.visible = false

	start_btn.pressed.connect(_on_start_pressed)
	save_load_btn.pressed.connect(func(): save_load_modal.visible = true)
	gallery_btn.pressed.connect(func(): gallery_modal.visible = true)
	settings_btn.pressed.connect(func(): settings_modal.visible = true)
	quit_btn.pressed.connect(func(): get_tree().quit())

	close_settings_btn.pressed.connect(func(): settings_modal.visible = false)
	close_saveload_btn.pressed.connect(func(): save_load_modal.visible = false)
	close_gallery_btn.pressed.connect(func(): gallery_modal.visible = false)

func _on_start_pressed() -> void:
	GameManager.StartNewGame()
