extends Node2D

onready var length_label = get_node("Length")
onready var color = Color(0.2, 0.2, 1, 0.7)
onready var color_origin = Color(1, 0.2, 0.2, 0.7)
var start_pos = Vector2(0,0)
var start_pos_raw = Vector2(0,0)
var end_pos = Vector2(0,0)
var pos_is_set = false

func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	update()
	
	if (Input.is_action_pressed("ui_space") && !Input.is_action_pressed("ui_shift")):
		var mouse_position = get_global_mouse_position()
		var pos_x = stepify(mouse_position.x-32, 64)
		var pos_y = stepify(mouse_position.y-32, 64)
		end_pos = Vector2(pos_x/64, pos_y/64)
		if (!pos_is_set):
			start_pos = end_pos
			start_pos_raw = mouse_position
			pos_is_set = true
			global_position = mouse_position
		length_label.set_global_position(Vector2(mouse_position.x, mouse_position.y + 15))
		
	
	if (Input.is_action_just_released("ui_space")):
		pos_is_set = false
		length_label.set_visible(false)

func _draw():
	if (pos_is_set):
		length_label.set_visible(true)
		
		var local_start_pos = to_local(Vector2(start_pos.x*64+32,start_pos.y*64+32))
		var local_end_pos = to_local(Vector2(end_pos.x*64+32, end_pos.y*64+32))
		var length = local_start_pos.distance_to(local_end_pos)
		var length_ft = round(start_pos.distance_to(end_pos))*5
		
		if (Input.is_action_pressed("circle")):
			var circle_origin = to_local(start_pos_raw.snapped(Vector2(64,64)))
			var end_ = to_local(Vector2(end_pos.x*64, end_pos.y*64))
			var cirle_radius = circle_origin.distance_to(end_)
			draw_circle(circle_origin, cirle_radius, color)
			draw_circle(circle_origin, 3, color_origin)
		elif (Input.is_action_pressed("cone")):
			var cone_start_pos = to_local(start_pos_raw.snapped(Vector2(64,64)))
			var mouse_position = get_global_mouse_position()
			var cone_end_pos = to_local(mouse_position)
			var cone_width = cone_start_pos.distance_to(cone_end_pos)/2
			var cone_end_pos_1 = cone_end_pos+cone_end_pos.tangent().normalized()*cone_width
			var cone_end_pos_2 = cone_end_pos+cone_end_pos.tangent().normalized()*-cone_width
			length_ft = int((Vector2(cone_start_pos.x/64, cone_start_pos.y/64).distance_to(Vector2(cone_end_pos.x/64, cone_end_pos.y/64)))*5)
			draw_line(cone_start_pos, cone_end_pos, color, 3, true)
			draw_line(cone_start_pos, cone_end_pos_1, color, 3, true)
			draw_line(cone_end_pos_1, cone_end_pos_2, color, 3, true)
			draw_line(cone_start_pos, cone_end_pos_2, color, 3, true)
			draw_circle(cone_start_pos, 3, color_origin)
		elif (Input.is_action_pressed("box")):
			var box_origin = to_local(start_pos_raw.snapped(Vector2(64,64)))
			var end_ = to_local(Vector2(end_pos.x*64, end_pos.y*64))
			var box_width = box_origin.distance_to(end_)
			draw_rect(Rect2(box_origin, Vector2(box_width, box_width)),color, true, 3,true)
			draw_circle(box_origin, 3, color_origin)
		elif (Input.is_action_pressed("line_aoe")):
			var line_origin = to_local(start_pos_raw.snapped(Vector2(64,64)))
			var end_ = to_local(get_global_mouse_position())
			length_ft = int((Vector2(line_origin.x/64, line_origin.y/64).distance_to(Vector2(end_.x/64, end_.y/64)))*5)
			draw_line(line_origin, end_, color, 64, true)
			draw_circle(line_origin, 3, color_origin)
		else:
			draw_circle(local_start_pos, 3, color_origin)
			draw_line(local_start_pos, local_end_pos, color, 3, true)
		
		length_label.set_text(String(length_ft) + " ft")
		
