extends Node2D

var token_scene = preload("res://Session/Token.tscn")
onready var token_counter = 0
onready var tokens = $Tokens
onready var map = $Map
onready var ping = $Ping

func _ready():
	Global.connect("changed_map", self, "_recieved_change_map")
	get_tree().connect("files_dropped", self, "on_dropped")
	rset_config("token_counter", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	
	if (get_tree().get_network_peer() != null):
		get_node("UI/Players").set_visible(true)
		var id = get_tree().get_network_unique_id()
		rpc("request_to_add_player", id, ClientVariables.username)
		
		if (!ClientVariables.dm):
			get_node("UI/MapList").set_visible(false)

func _process(_delta):
	if (get_tree().get_network_peer() != null):
		if (Input.is_action_just_pressed("delete")):
			rpc("remove_token", ClientVariables.selected_token.name)
		
		if (get_tree().is_network_server()):
				rset("token_counter", token_counter)
		
		if (Input.is_action_just_pressed("ping")):
			if (!ping.is_emitting()):
				rpc("ping_map", get_global_mouse_position())
	else:
		if (Input.is_action_just_pressed("delete")):
			remove_token( ClientVariables.selected_token.name)
		
		if (Input.is_action_just_pressed("ping")):
			if (!ping.is_emitting()):
				ping.set_position(get_global_mouse_position())
				ping.set_emitting(true)

remotesync func create_token(token_name, token_file, position, _scale):
	var token = token_scene.instance()
	token.set_name(token_name)
	tokens.add_child(token)
	token.get_node("Sprite").initialize(token_file, position, _scale)
	
	ClientVariables.inserted_tokens[token_counter] = [token_name, token_file]
	token_counter += 1
	

remotesync func remove_token(token_name):
	var token_key = token_name.rsplit("_",true,1)[1]
	var temp_dict = {}
	for token in ClientVariables.inserted_tokens :
		if (token != int(token_key)) :
			temp_dict[token] = ClientVariables.inserted_tokens.get(token)
	ClientVariables.inserted_tokens.clear()
	ClientVariables.inserted_tokens = temp_dict
	var temp = tokens.get_node(token_name)
	temp.queue_free()

func _recieved_change_map():
	if (get_tree().get_network_peer() != null):
		if ClientVariables.dm:
			save_battlemap()
			rpc("change_map", ClientVariables.selected_map)
	else:
		save_battlemap()
		change_map(ClientVariables.selected_map)

remotesync func change_map(_map):
	ClientVariables.reset_tokens()
	token_counter = 0
	for token in tokens.get_children():
		token.queue_free()
	map.initialize(_map)
	
	if (get_tree().get_network_peer() != null):
		if ClientVariables.dm:
			load_battlemap()
	else:
		load_battlemap()
	

func on_dropped(files, _droppedFrom):
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
		rpc("create_token", name, file, position, null)
	else:
		create_token(name, file, position, null)
	

remotesync func request_to_add_player(id, name):
	if get_tree().is_network_server():
		rpc("add_player", id, name)

remotesync func add_player(id, name):
	var player = Label.new()
	player.set_name(String(id))
	player.set_text(name)
	player.add_font_override("font", load("res://Assets/Fonts/Default.tres"))
	get_node("UI/Players").add_child(player)
	ClientVariables.connected_players[id] = [id, name]
	

remotesync func remove_player(id):
	ClientVariables.connected_players.erase(id)
	get_node("UI/Players").get_node(String(id)).queue_free()

remotesync func ping_map(positon):
	ping.set_position(positon)
	ping.set_emitting(true)

func _on_BackButton_pressed():
	if (get_tree().network_peer != null):
		if ClientVariables.dm:
			save_battlemap()
		var peer = NetworkedMultiplayerENet.new()
		peer.close_connection()
		get_tree().network_peer = null
	else:
		save_battlemap()
	Global.goto_scene("res://GUI/MainMenu.tscn")

func save_battlemap():
	if map.current_map == null:
		return
	
	var battlemap = File.new()
	battlemap.open(ClientVariables.data_path + map.current_map.split(".")[0] + ".dat", File.WRITE)
	var json_tokens = []
	for token in tokens.get_children():
		json_tokens.append(token.get_node("Sprite").save_token())
	
	var save_dict = {
		"map" : map.save_map(),
		"tokens" : json_tokens
	}
	battlemap.store_line(to_json(save_dict))
	battlemap.close()

func load_battlemap():
	var battlemap = File.new()
	if not battlemap.file_exists(ClientVariables.data_path + ClientVariables.selected_map.split(".")[0] + ".dat"):
		return
	
	battlemap.open(ClientVariables.data_path + ClientVariables.selected_map.split(".")[0] + ".dat", File.READ)
	while(battlemap.get_position() < battlemap.get_len()):
		var data = parse_json(battlemap.get_line())
		
		var map_data = data["map"]
		map.set_scale(Vector2(map_data["map_scale_x"], map_data["map_scale_y"]))
		
		if (!data["tokens"].empty()):
			for token in data["tokens"]:
				if (get_tree().get_network_peer() != null):
					rpc("create_token", token["name"], token["texture_name"], Vector2(token["pos_x"], token["pos_y"]), Vector2(token["scale_x"], token["scale_y"]))
				else:
					create_token(token["name"], token["texture_name"], Vector2(token["pos_x"], token["pos_y"]), Vector2(token["scale_x"], token["scale_y"]))
	battlemap.close()
	

func _notification(what):
	if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		if (get_tree().get_network_peer() != null):
			if ClientVariables.dm:
				save_battlemap()
		else:
			save_battlemap()
