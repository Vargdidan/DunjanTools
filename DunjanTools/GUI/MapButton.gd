extends Button

onready var map_container = get_parent().get_node("MapContainer")

func _ready():
	map_container.set_visible(false)


func _on_TokenButton_pressed():
	var visible = map_container.is_visible()
	visible = !visible
	map_container.set_visible(visible)
