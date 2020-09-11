extends Sprite
var tile_size = 64
var current_map = null

func initialize(texture_name):
	current_map = texture_name
	rset_config("scale", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	
	var image = Image.new()
	var loaded = image.load(ClientVariables.MapFolder + texture_name)
	if loaded == OK:
		texture = ImageTexture.new()
		texture.create_from_image(image, 0)

func _process(_delta):
	if (get_tree().get_network_peer() != null):
		if ClientVariables.DMRole:
			resize()
	else:
		resize()

func resize():
	var _scale = scale
	if (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_shift") && Input.is_action_pressed("ui_space")):
		var scale_x = scale.x+0.001
		var scale_y = scale.y+0.001
		_scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_shift") && Input.is_action_pressed("ui_space")):
		var scale_x = scale.x-0.001
		var scale_y = scale.y-0.001
		_scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_shift")):
		var scale_x = scale.x+0.1
		var scale_y = scale.y+0.1
		_scale = Vector2(scale_x, scale_y)
	
	elif (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_shift")):
		var scale_x = scale.x-0.1
		var scale_y = scale.y-0.1
		_scale = Vector2(scale_x, scale_y)
	
	if (get_tree().get_network_peer() != null):
		rset("scale", _scale)
	else:
		scale = _scale
	

func set_scale(_scale):
	if (get_tree().get_network_peer() != null):
		rset("scale", _scale)
	else:
		scale = _scale

func save_map():
	var save_dict = {
		"map_scale_x" : scale.x,
		"map_scale_y" : scale.y
	}
	return save_dict
