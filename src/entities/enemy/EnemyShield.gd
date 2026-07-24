extends EnemyBase

class_name EnemyShield

func _ready() -> void:
	super._ready()
	max_hp = 60.0 # High health tank
	hp = max_hp
	speed = 45.0 # Slow movement
	sprite.color = Color(0.3, 0.4, 0.6) # Steel blue shield color

func take_damage_type(amount: float, is_magic: bool) -> void:
	var final_damage = amount
	if is_magic:
		# Low magic defense: takes bonus magic damage
		var mult: float = GameConfig.ShieldEnemyMagicDamageMultiplier if GameConfig else 2.0
		final_damage *= mult
	else:
		# High physical armor: takes reduced physical damage
		var ratio: float = GameConfig.ShieldEnemyPhysArmorRatio if GameConfig else 0.75
		final_damage *= (1.0 - ratio)

	take_damage(final_damage)
