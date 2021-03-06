extends Camera2D

var target_position = Vector2() # desired position to move towards
var target_zoom = Vector2()
var move_direction = Vector2()
var speed = 700

func _ready():
	target_position = position
	target_zoom = zoom
	

func _process(delta):
	zoom()
	position += speed * move_direction * delta
	move_direction = Vector2()
	
	zoom = lerp(zoom, target_zoom, 0.6)
	

func _input(event):
	if Input.is_action_pressed("ui_right_click"):
		if event is InputEventMouseMotion:
			move_direction -= event.relative * zoom * 0.1
		
	if Input.is_action_pressed("ui_control") && Input.is_action_just_pressed("zero_camera"):
		global_position = Vector2(960, 540)

func zoom():
	if (!Input.is_action_pressed("ui_control") && !Input.is_action_pressed("ui_shift") && Input.is_action_just_released("ui_scroll_up")):
		target_zoom.x -= 0.2
		target_zoom.y -= 0.2
	elif (!Input.is_action_pressed("ui_control") && !Input.is_action_pressed("ui_shift") && Input.is_action_just_released("ui_scroll_down")):
		target_zoom.x += 0.2
		target_zoom.y += 0.2
	
	if target_zoom.x < 0.1:
		target_zoom.x = 0.1
	if target_zoom.y < 0.1:
		target_zoom.y = 0.1
