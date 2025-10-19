# KeyRebind Scene Flow

**Game Mode** -> Game Interactions
**Menu Mode** -> Menu Interactions

----
## Game Mode

### when (setting btn is clicked)/(esc key is pressed) in the **Game Mode**
- game is paused (by setting Time.TimeScale to ~ 0f)
- setting menu(which contains KeyRebind) pop up opened

----
## Menu Mode
### when (close btn is clicked)/(esc key is pressed) in the **Menu Mode**
- close the settings menu
- resume the game (by setting Time.TimeScale to ~ 1f)

### when a certain btn for keybinding is pressed (enter any key pop-up)
- pop-up enter any key to assign the bindings
- if esc/backscape/delete is pressed clear the binding(note: enter any key pop-up is still open)
- if enter is pressed assign the binding and close the pop-up
- if enter is pressed with no bindings in there(i,e by clearing through esc/backsape/delete), screeshake to let the player know assign can't be made
- if close btn is pressed on the pop-up, assign the binding what was done until this point as you seem fit.

### when save and close is pressed
- save the new keybindings assigned
- close the settings menu
- resume the game (by setting Time.TimeScale to ~ 1f)

# KeyBinding Settings Visual

given an input action file for a certain character,
3 columns as follows

action | keyBind 0 | keyBind 1

keyBind 0 -> default keybindings which can be modified
keyBind 1 -> by default shall be empty can be later modified

## Descrepency with same keybindings
note: no 2 action can have same keyBindings

## Template provided for the following UI:
- key rebind row
- keybind (press any key) pop-up


# TODO
save the keybind 