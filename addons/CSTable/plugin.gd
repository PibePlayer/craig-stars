tool
extends EditorPlugin

func _enter_tree():
    var texture = preload("res://addons/CSTable/icon.svg")
    add_custom_type("CSTable", "MarginContainer", preload("res://addons/CSTable/CSTable.cs"), texture)
    add_custom_type("CSLabelCell", "MarginContainer", preload("res://addons/CSTable/src/Table/CSLabelCell.cs"), texture)
    
func _exit_tree():
    remove_custom_type("CSTable")
    remove_custom_type("CSLabelCell")
	
