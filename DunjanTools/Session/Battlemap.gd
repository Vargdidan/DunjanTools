extends Node2D

var token_scene = preload("res://Session/Token.tscn")
onready var token_counter = 0
onready var tokens = get_node("Tokens")
onready var map = get_node("Map")

func _ready():
	Global.connect("changed_map", self, "_recieved_change_map")
	get_tree().connect("files_dropped", self, "on_dropped")
	rset_config("token_counter", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	
	if (get_tree().get_network_peer() != null):
		get_node("UI/Players").set_visible(true)
		var id = get_tree().get_network_unique_id()
		rpc("add_player", id, ClientVariables.username)

func _process(delta):
	if (Input.is_action_just_pressed("delete")):
		remove_token( ClientVariables.selected_token.name)
	
	if ((get_tree().get_network_peer() != null) && !get_tree().is_network_server()):
		get_node("UI/MapList").set_visible(false)
	
	if ((get_tree().get_network_peer() != null) && get_tree().is_network_server()):
			rset("token_counter", token_counter)

remotesync func create_token(token_name, token_file, position):
	var token = token_scene.instance()
	token.set_name(token_name)
	tokens.add_child(token)
	token.get_node("Sprite").initialize(token_file, position)
	
	ClientVariables.inserted_tokens[token_counter] = [token_name, token_file]
	token_counter += 1
	

remotesync func remove_token(token_name):
	var temp = tokens.get_node(token_name)
	temp.queue_free()

func _recieved_change_map():
	if (get_tree().get_network_peer() != null):
		rpc("change_map", ClientVariables.current_map)
	else:
		change_map(ClientVariables.current_map)

remotesync func change_map(map):
	get_node("Map").initialize(map)
	

func on_dropped(files, droppedFrom):
	var file_path = files[0]
	var pos = file_path.rfindn("Tokens\\")
	var file = file_path.right(pos+7)
	var position = get_global_mouse_position()
	
	var name = file.split(".")[0]
	var index = name.rfindn("\\")
	if (index != -1):
		name = name.right(index+1)
		
	name = name + "_" + String(token_counter)
	if (get_tree().get_network_peer() != null):
		rpc("create_token", name, file, position)
	else:
		create_token(name, file, position)
	

remotesync func add_player(id, name):
	var player = Label.new()
	player.set_name(String(id))
	player.set_text(name)
	player.add_font_override("font", load("res://Assets/Fonts/DefaultFont.tres"))
	get_node("UI/Players").add_child(player)
	ClientVariables.connected_players[id] = [id, name]
	

remotesync func remove_player(id):
	ClientVariables.connected_players.erase(id)
	get_node("UI/Players").get_node(String(id)).queue_free()
