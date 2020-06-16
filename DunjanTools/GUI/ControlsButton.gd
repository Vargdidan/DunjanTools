extends Button

onready var controls_container = get_parent().get_node("ControlsContainer")

func _ready():
	controls_container.set_visible(false)


func _on_TokenButton_pressed():
	var visible = controls_container.is_visible()
	visible = !visible
	controls_container.set_visible(visible)
