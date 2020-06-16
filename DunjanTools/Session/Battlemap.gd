extends Node2D


var token_scene = preload("res://Session/Token.tscn")

func _ready():
	Global.connect("changed_map", self, "_recieved_change_map")
	get_tree().connect("files_dropped", self, "on_dropped")

func _process(delta):
	if (Input.is_action_pressed("insert")):
		if (ClientVariables.selected_insert_token != null):
			get_node("UI/InsertToken").set_visible(true)
			get_node("UI/InsertToken").set_text("Insert: " + ClientVariables.selected_insert_token)
			if (Input.is_action_just_pressed("ui_mouse_click")):
				var position = get_global_mouse_position()
				if (get_tree().get_network_peer() != null):
					rpc("create_token", ClientVariables.selected_insert_token, position)
				create_token( ClientVariables.selected_insert_token, position)
	else:
		get_node("UI/InsertToken").set_visible(false)
	
	if (Input.is_action_just_pressed("delete")):
		if (get_tree().get_network_peer() != null):
			rpc("remove_token", ClientVariables.selected_token.name)
		remove_token( ClientVariables.selected_token.name)
	
	if ((get_tree().get_network_peer() != null) && !get_tree().is_network_server()):
		get_node("UI/MapList").set_visible(false)

remote func create_token(token_name, position):
	var token = token_scene.instance()
	get_node("Tokens").add_child(token)
	token.get_node("Sprite").initialize(token_name, position)
	ClientVariables.inserted_tokens[token.name] = [token_name, position]
	

remote func remove_token(token_name):
	var temp = get_node("Tokens").get_node(token_name)
	ClientVariables.inserted_tokens.erase(token_name)
	temp.queue_free()

func _recieved_change_map():
	if (get_tree().get_network_peer() != null):
		rpc("change_map", ClientVariables.current_map)
	change_map(ClientVariables.current_map)

remote func change_map(map):
	get_node("Map").initialize(map)
	

func on_dropped(files, droppedFrom):
	var file_path = files[0]
	var pos = file_path.rfindn("Tokens\\")
	var file = file_path.right(pos+7)
	var position = get_global_mouse_position()
	if (get_tree().get_network_peer() != null):
		rpc("create_token", file, position)
	create_token( file, position)
	
