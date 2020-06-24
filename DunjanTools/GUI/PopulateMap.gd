extends VBoxContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	dir_contents(ClientVariables.map_path)
func dir_contents(path):
	for child in get_children():
		child.queue_free()
	
	var dir = Directory.new()
	if dir.open(path) == OK:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			if !dir.current_is_dir():
				if !file_name.match("*.import"):
					var name = file_name.split(".")
					create_map(name[0], file_name)
			file_name = dir.get_next()
	else:
		print("An error occurred when trying to access the path.")

func create_map(name, file_name):
	var token = Button.new()
	token.text = name
	token.set_text_align(Button.ALIGN_RIGHT)
	token.set_enabled_focus_mode(FOCUS_NONE)
	token.add_font_override("font", load("res://Assets/Fonts/DefaultFont.tres"))
	token.connect("pressed", self, "_selected_map", [file_name])
	add_child(token)

func _selected_map(name):
	ClientVariables.current_map = name
	Global.change_map()
