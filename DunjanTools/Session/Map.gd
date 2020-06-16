extends Sprite
var tile_size = 64

func initialize(texture_name):
	rset_config("scale", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	var image = Image.new()
	var loaded = image.load(ClientVariables.map_path + texture_name)
	if loaded == OK:
		texture = ImageTexture.new()
		texture.create_from_image(image, 0)

func _process(delta):
	if (get_tree().get_network_peer() != null):
		if (get_tree().is_network_server()):
			resize()
	else:
		resize()

func resize():
	if (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_shift") && Input.is_action_pressed("ui_space")):
		var scale_x = scale.x+0.001
		var scale_y = scale.y+0.001
		scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_shift") && Input.is_action_pressed("ui_space")):
		var scale_x = scale.x-0.001
		var scale_y = scale.y-0.001
		scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_shift")):
		var scale_x = scale.x+0.1
		var scale_y = scale.y+0.1
		scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_shift")):
		var scale_x = scale.x-0.1
		var scale_y = scale.y-0.1
		scale = Vector2(scale_x, scale_y)
	
	if (get_tree().get_network_peer() != null):
		rset("scale", scale)
	
