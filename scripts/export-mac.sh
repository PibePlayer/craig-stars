#!/bin/zsh
DIR=${0:a}

/Applications/Godot_mono.app/Contents/MacOS/Godot  --no-window --verbose --path ${DIR}/../project.godot --export "mac" "${HOME}/Downloads/craig-stars.dmg"
