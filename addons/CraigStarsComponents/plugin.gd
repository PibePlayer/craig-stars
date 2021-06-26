tool
extends EditorPlugin

func _enter_tree():
    var texture = preload("res://addons/CraigStarsComponents/icon.svg")
    add_custom_type("ProductionQueueItemsTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/ProductionQueueItemsTable.cs"), texture)
    
func _exit_tree():
    remove_custom_type("ProductionQueueItemsTable")
	
