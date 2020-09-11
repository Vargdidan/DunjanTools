extends Sprite

var tile_size = 64 # size in pixels of tiles on the grid
var target_position = Vector2() # desired position to move toward
onready var token_name = get_parent().get_node("token_name")
onready var color = Color(1, 0.2, 0.2, 0.2)
var texture_name = ""

func initialize(_texture_name, postion, _scale):
	rset_config("target_position", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	rset_config("scale", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	texture_name = _texture_name
	var image = Image.new()
	var loaded = image.load(ClientVariables.TokenFolder + texture_name)
	if loaded == OK:
		texture = ImageTexture.new()
		texture.create_from_image(image, 0)
		token_name.set_text(get_parent().name)
		
		if _scale == null:
			set_default_size()
		else:
			scale = _scale
		
		var target_pos = postion
		target_pos.x = stepify(target_pos.x, tile_size)
		target_pos.y = stepify(target_pos.y, tile_size)
		target_position = target_pos
		global_position = target_position
		
		if (get_tree().get_network_peer() != null) && !get_tree().is_network_server():
			rpc("request_position_scale")
	

func _process(_delta):
	update()
	
	check_selection()
	if (ClientVariables.selected_token == get_parent()):
		var dirty = false
		if (Input.is_action_just_pressed("ui_mouse_click")):
			if !get_rect().has_point(to_local(get_global_mouse_position())):
				ClientVariables.selected_token = null
				dirty = true
		if (!dirty):
			move_mouse()
			move()
			resize()
	global_position = lerp(global_position, target_position, 0.2)
	token_name.set_global_position(global_position)
	

func _draw():
	if (ClientVariables.selected_token == get_parent()):
		draw_rect(get_rect(), color, true)

func check_selection():
	if get_rect().has_point(to_local(get_global_mouse_position())):
		token_name.set_visible(true)
		if (Input.is_action_just_released("ui_mouse_click")):
			ClientVariables.selected_token = get_parent()
	else:
		token_name.set_visible(false)

func move():
	if (Input.is_action_just_pressed("ui_move_left") ||
		Input.is_action_just_pressed("ui_move_right") ||
		Input.is_action_just_pressed("ui_move_up") ||
		Input.is_action_just_pressed("ui_move_down")):
			var LEFT = Input.is_action_just_pressed("ui_move_left")
			var RIGHT = Input.is_action_just_pressed("ui_move_right")
			var UP = Input.is_action_just_pressed("ui_move_up")
			var DOWN = Input.is_action_just_pressed("ui_move_down")
			if (get_tree().get_network_peer() != null):
				var pos = Vector2(target_position.x, target_position.y)
				pos.x += (-int(LEFT) + int(RIGHT))*64
				pos.y += (-int(UP) + int(DOWN))*64
				rpc("request_movement", pos)
			else:
				target_position.x += (-int(LEFT) + int(RIGHT))*64
				target_position.y += (-int(UP) + int(DOWN))*64
	

func move_mouse():
	if (Input.is_action_pressed("ui_mouse_click")):
			var target_pos = get_global_mouse_position()
			var current_size_x=get_rect().size.x*scale.x
			var current_size_y=get_rect().size.y*scale.y
			target_pos.x = stepify(target_pos.x-current_size_x/2, tile_size)
			target_pos.y = stepify(target_pos.y-current_size_y/2, tile_size)
			target_position = target_pos
			
			if (get_tree().get_network_peer() != null):
				rpc("request_movement", target_pos)
			else:
				target_position = target_pos
		

func resize():
	if (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_control")):
		var current_size_x=get_rect().size.x*scale.x
		var current_size_y=get_rect().size.y*scale.y
		var sizeto=Vector2(current_size_x+tile_size, current_size_y+tile_size)
		var _scale = sizeto/get_rect().size
		
		if (get_tree().get_network_peer() != null):
			rpc("request_scale", _scale)
		else:
			scale = _scale
		
	
	if (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_control")):
		var current_size_x=get_rect().size.x*scale.x
		var current_size_y=get_rect().size.y*scale.y
		var sizeto=Vector2(current_size_x-tile_size, current_size_y-tile_size)
		if (sizeto.x > tile_size/2):
			var _scale = sizeto/get_rect().size
			if (get_tree().get_network_peer() != null):
				rpc("request_scale", _scale)
			else:
				scale = _scale
		
	

func set_default_size():
	var sizeto=Vector2(tile_size, tile_size)
	scale = sizeto/get_rect().size

remotesync func request_movement(pos):
	if get_tree().is_network_server():
		rset("target_position", pos)
	

remotesync func request_scale(_scale):
	if get_tree().is_network_server():
		rset("scale", _scale)
	

remote func request_position_scale():
	if get_tree().is_network_server():
		rset("scale", scale)
		rset("target_position", target_position)
	

func save_token():
	var save_dict = {
		"name" : token_name.get_text(),
		"texture_name" : texture_name,
		"pos_x" : global_position.x,
		"pos_y" : global_position.y,
		"scale_x" : scale.x,
		"scale_y" : scale.y
	}
	return save_dict
