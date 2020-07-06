extends Button

onready var controls_info = get_parent().get_node("Panel")

func _ready():
	controls_info.set_visible(false)


func _on_TokenButton_pressed():
	var visible = controls_info.is_visible()
	visible = !visible
	controls_info.set_visible(visible)
