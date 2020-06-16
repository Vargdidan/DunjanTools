extends Sprite

var tile_size = 64 # size in pixels of tiles on the grid
var target_position = Vector2() # desired position to move toward
onready var token_name = get_parent().get_node("UI/token_name")

func initialize(texture_name, postion):
	rset_config("target_position", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	rset_config("scale", MultiplayerAPI.RPC_MODE_REMOTESYNC)
	var image = Image.new()
	var loaded = image.load(ClientVariables.token_path + texture_name)
	if loaded == OK:
		texture = ImageTexture.new()
		texture.create_from_image(image, 0)
		set_default_size()
		var target_pos = postion
		var current_size_x=get_rect().size.x*scale.x
		var current_size_y=get_rect().size.y*scale.y
		target_pos.x = stepify(target_pos.x-current_size_x/2, tile_size)
		target_pos.y = stepify(target_pos.y-current_size_y/2, tile_size)
		target_position = target_pos
		global_position = target_position
		
		var name = get_parent().name
		var pos = name.rfindn("@")
		if (pos != -1):
			var right = name.right(pos+2)
			var number = 1 + int(right)
			token_name.set_text(String(number))
		else:
			token_name.set_text("0")
	

func _process(delta):
	check_selection()
	if (ClientVariables.selected_token == get_parent()):
		var dirty = false
		if (Input.is_action_just_pressed("ui_mouse_click")):
			if !get_rect().has_point(to_local(get_global_mouse_position())):
				ClientVariables.selected_token = null
				dirty = true
		set_use_parent_material(false)
		token_name.set_visible(true)
		if (!dirty):
			move_mouse()
			move()
			resize()
	else:
		set_use_parent_material(true)
		token_name.set_visible(false)
	global_position = lerp(global_position, target_position, 0.2)
	token_name.set_global_position(global_position)
	
	if ((get_tree().get_network_peer() != null) && get_tree().is_network_server()):
			rset_unreliable("scale", scale)
	

func check_selection():
	if (Input.is_action_just_pressed("ui_mouse_click")):
		if get_rect().has_point(to_local(get_global_mouse_position())):
			ClientVariables.selected_token = get_parent()
	

func move():
	var LEFT = Input.is_action_just_pressed("ui_move_left")
	var RIGHT = Input.is_action_just_pressed("ui_move_right")
	var UP = Input.is_action_just_pressed("ui_move_up")
	var DOWN = Input.is_action_just_pressed("ui_move_down")
	target_position.x += (-int(LEFT) + int(RIGHT))*64
	target_position.y += (-int(UP) + int(DOWN))*64
	if (get_tree().get_network_peer() != null):
			rset_unreliable("target_position", target_position)
	

func move_mouse():
	if (Input.is_action_pressed("ui_mouse_click")):
			var target_pos = get_global_mouse_position()
			var current_size_x=get_rect().size.x*scale.x
			var current_size_y=get_rect().size.y*scale.y
			target_pos.x = stepify(target_pos.x-current_size_x/2, tile_size)
			target_pos.y = stepify(target_pos.y-current_size_y/2, tile_size)
			target_position = target_pos
			if (get_tree().get_network_peer() != null):
				rset_unreliable("target_position", target_position)
		

func resize():
	if (Input.is_action_just_released("ui_scroll_up") && Input.is_action_pressed("ui_control")):
		var current_size_x=get_rect().size.x*scale.x
		var current_size_y=get_rect().size.y*scale.y
		var sizeto=Vector2(current_size_x+tile_size, current_size_y+tile_size)
		scale = sizeto/get_rect().size
		
	
	if (Input.is_action_just_released("ui_scroll_down") && Input.is_action_pressed("ui_control")):
		var current_size_x=get_rect().size.x*scale.x
		var current_size_y=get_rect().size.y*scale.y
		var sizeto=Vector2(current_size_x-tile_size, current_size_y-tile_size)
		if (sizeto.x > tile_size/2):
			scale = sizeto/get_rect().size
		
	

func set_default_size():
	var sizeto=Vector2(tile_size, tile_size)
	scale = sizeto/get_rect().size
