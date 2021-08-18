tool
extends EditorPlugin

func _enter_tree():
    var texture = preload("res://addons/CSTable/icon.svg")
    add_autoload_singleton("CSTableResourceLoader", "res://addons/CSTable/src/Table/CSTableResourceLoader.tscn")
    add_custom_type("CSTable", "MarginContainer", preload("res://addons/CSTable/CSTable.cs"), texture)
    add_custom_type("CSLabelCell", "MarginContainer", preload("res://addons/CSTable/src/Table/CSLabelCell.cs"), texture)
    add_custom_type("CSButtonCell", "MarginContainer", preload("res://addons/CSTable/src/Table/CSButtonCell.cs"), texture)

func _exit_tree():
    remove_autoload_singleton("CSTableResourceLoader")
    remove_custom_type("CSTable")
    remove_custom_type("CSLabelCell")
    remove_custom_type("CSButtonCell")
	
