# Unity Input System - Quick Reference Cheat Sheet

## 🎯 Core Concepts

```
InputActionAsset
  └── InputActionMap (e.g., "character", "ui")
	   └── InputAction (e.g., "jump", "shoot", "walk")
			└── InputBinding[] (e.g., "<Keyboard>/space", "<Mouse>/leftButton")
```

---

## 📖 Reading Input Actions

### Get Action Map
```csharp
// By name
InputActionMap map = inputActionAsset.FindActionMap("character");

// By index
InputActionMap firstMap = inputActionAsset.actionMaps[0];

// Iterate all maps
foreach (InputActionMap map in inputActionAsset.actionMaps)
{
	Debug.Log(map.name);
}
```

### Get Action
```csharp
// From action map
InputAction action = actionMap.FindAction("jump");

// Directly from asset
InputAction action = inputActionAsset.FindAction("character/jump");

// By index
InputAction firstAction = actionMap.actions[0];

// Iterate all actions
foreach (InputAction action in actionMap.actions)
{
	Debug.Log(action.name);
}
```

### Get Bindings
```csharp
InputAction action = inputActionAsset.FindAction("character/jump");

// Access binding by index
InputBinding binding = action.bindings[0];

// Get binding count
int count = action.bindings.Count;

// Iterate all bindings
for (int i = 0; i < action.bindings.Count; i++)
{
	InputBinding binding = action.bindings[i];
	Debug.Log($"Binding {i}: {binding.effectivePath}");
}
```

---

## 🔍 Binding Properties

### Essential Properties
```csharp
InputBinding binding = action.bindings[0];

// Original binding from InputActionAsset
string originalPath = binding.path;			  // "<Keyboard>/space"

// Current override (empty if no override)
string overridePath = binding.overridePath;	  // "<Keyboard>/e"

// The actual binding being used (path or overridePath)
string effectivePath = binding.effectivePath;	// "<Keyboard>/e"

// User-friendly display name
string displayString = action.GetBindingDisplayString(0); // "E"

// Unique ID
System.Guid id = binding.id;

// Binding name (useful for composites)
string name = binding.name;					  // "up", "down", "left", "right"

// Control scheme groups
string groups = binding.groups;				  // "Keyboard&Mouse"

// Additional properties
string interactions = binding.interactions;	  // "hold", "press", etc.
string processors = binding.processors;		  // "normalize", "scale", etc.
```

### Composite Bindings
```csharp
// Check if this is a composite (container)
bool isComposite = binding.isComposite;		  // true for "2D Vector"

// Check if this is part of a composite
bool isPartOfComposite = binding.isPartOfComposite; // true for "up", "down", "left", "right"

// Example check
if (binding.isComposite)
{
	Debug.Log($"This is a composite: {binding.name}");
}

if (binding.isPartOfComposite)
{
	Debug.Log($"This is part of composite: {binding.name}");
}
```

---

## ✏️ Modifying Bindings

### Override a Binding
```csharp
InputAction action = inputActionAsset.FindAction("character/jump");

// Method 1: Direct path string
action.ApplyBindingOverride(0, "<Keyboard>/e");

// Method 2: Using InputBinding
action.ApplyBindingOverride(0, new InputBinding 
{ 
	overridePath = "<Keyboard>/e" 
});

// Method 3: Using binding mask
InputBinding mask = new InputBinding { path = "<Keyboard>/space" };
action.ApplyBindingOverride(mask, "<Keyboard>/e");
```

### Remove Override
```csharp
// Remove specific binding override
action.RemoveBindingOverride(0);

// Remove all overrides for this action
action.RemoveAllBindingOverrides();

// Remove all overrides for entire action map
actionMap.RemoveAllBindingOverrides();

// Remove all overrides for entire asset
foreach (var map in inputActionAsset.actionMaps)
{
	map.RemoveAllBindingOverrides();
}
```

### Clear/Empty a Binding
```csharp
// Set to empty string
action.ApplyBindingOverride(0, new InputBinding { overridePath = "" });

// Or remove the override to restore default
action.RemoveBindingOverride(0);
```

---

## 🔎 Common Queries

### Check if Binding is Empty
```csharp
bool isEmpty = string.IsNullOrEmpty(binding.effectivePath);

if (isEmpty)
{
	Debug.Log("Binding is not set!");
}
```

### Find Binding Index by Path
```csharp
int targetIndex = -1;
for (int i = 0; i < action.bindings.Count; i++)
{
	if (action.bindings[i].effectivePath == "<Keyboard>/space")
	{
		targetIndex = i;
		break;
	}
}
```

### Get Only Non-Composite Bindings
```csharp
List<int> bindableIndices = new List<int>();

for (int i = 0; i < action.bindings.Count; i++)
{
	InputBinding binding = action.bindings[i];
	
	if (!binding.isComposite && !binding.isPartOfComposite)
	{
		bindableIndices.Add(i);
	}
}
```

### Check for Conflicting Bindings
```csharp
bool HasConflict(string targetPath, InputAction currentAction, int currentIndex)
{
	foreach (var map in inputActionAsset.actionMaps)
	{
		foreach (var action in map.actions)
		{
			for (int i = 0; i < action.bindings.Count; i++)
			{
				// Skip the current binding we're checking
				if (action == currentAction && i == currentIndex)
					continue;

				if (action.bindings[i].effectivePath == targetPath)
				{
					return true; // Conflict found!
				}
			}
		}
	}
	return false;
}
```

---

## 🎮 Action Properties

```csharp
InputAction action = inputActionAsset.FindAction("character/jump");

// Basic info
string name = action.name;					// "jump"
System.Guid id = action.id;				   // Unique identifier
InputActionType type = action.type;		   // Button, Value, PassThrough
string expectedControlType = action.expectedControlType; // "Button", "Vector2", etc.

// State
bool enabled = action.enabled;				// Is the action enabled?
InputActionPhase phase = action.phase;		// Current phase (Disabled, Waiting, Started, etc.)

// Read current value
float value = action.ReadValue<float>();	  // For Value type
Vector2 vector = action.ReadValue<Vector2>(); // For Vector2 type
bool pressed = action.ReadValue<float>() > 0; // For Button type

// Control info
InputControl activeControl = action.activeControl; // Currently active control
```

---

## 🎬 Action Callbacks

```csharp
InputAction action = inputActionAsset.FindAction("character/jump");

// Subscribe to events
action.started += OnJumpStarted;
action.performed += OnJumpPerformed;
action.canceled += OnJumpCanceled;

void OnJumpStarted(InputAction.CallbackContext ctx)
{
	Debug.Log("Jump started");
	float value = ctx.ReadValue<float>();
}

void OnJumpPerformed(InputAction.CallbackContext ctx)
{
	Debug.Log("Jump performed");
	// This is where you usually trigger the action
}

void OnJumpCanceled(InputAction.CallbackContext ctx)
{
	Debug.Log("Jump canceled");
}

// Unsubscribe (important!)
action.started -= OnJumpStarted;
action.performed -= OnJumpPerformed;
action.canceled -= OnJumpCanceled;
```

---

## 🔧 Enable/Disable Actions

```csharp
// Enable/Disable specific action
action.Enable();
action.Disable();

// Enable/Disable action map
actionMap.Enable();
actionMap.Disable();

// Enable/Disable entire asset
inputActionAsset.Enable();
inputActionAsset.Disable();

// Check if enabled
bool isEnabled = action.enabled;
bool isMapEnabled = actionMap.enabled;
```

---

## 💾 Save/Load System

### Save Binding Override
```csharp
void SaveBinding(string key, InputAction action, int bindingIndex)
{
	string path = action.bindings[bindingIndex].effectivePath;
	PlayerPrefs.SetString(key, path);
	PlayerPrefs.Save();
}
```

### Load Binding Override
```csharp
void LoadBinding(string key, InputAction action, int bindingIndex)
{
	if (PlayerPrefs.HasKey(key))
	{
		string savedPath = PlayerPrefs.GetString(key);
		action.ApplyBindingOverride(bindingIndex, savedPath);
	}
}
```

### Save All Bindings as JSON
```csharp
// Get overrides as JSON
string json = action.SaveBindingOverridesAsJson();
PlayerPrefs.SetString("SavedOverrides", json);

// Load overrides from JSON
string json = PlayerPrefs.GetString("SavedOverrides");
action.LoadBindingOverridesFromJson(json);
```

---

## 🎨 Display String Formatting

```csharp
// Default display
string display = action.GetBindingDisplayString(0);
// Output: "Space"

// With options
string display = action.GetBindingDisplayString(
	bindingIndex: 0,
	options: InputBinding.DisplayStringOptions.DontIncludeInteractions
);

// Display options flags:
// - DontIncludeInteractions
// - DontOmitDevice
// - DontUseShortDisplayNames
// - IgnoreBindingOverrides
```

---

## 🔄 Interactive Rebinding

```csharp
// Start interactive rebinding
var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
	.WithControlsExcluding("<Mouse>")		   // Exclude mouse
	.WithControlsExcluding("<Keyboard>/escape") // Exclude escape key
	.OnMatchWaitForAnother(0.1f)				// Wait for better match
	.OnComplete(operation =>
	{
		Debug.Log("Rebinding complete!");
		operation.Dispose();
	})
	.OnCancel(operation =>
	{
		Debug.Log("Rebinding canceled!");
		operation.Dispose();
	})
	.Start();

// Cancel rebinding
rebindOperation.Cancel();

// Cleanup
rebindOperation.Dispose();
```

---

## 📝 Common Paths

### Keyboard
```csharp
"<Keyboard>/space"
"<Keyboard>/w"
"<Keyboard>/a"
"<Keyboard>/s"
"<Keyboard>/d"
"<Keyboard>/escape"
"<Keyboard>/enter"
"<Keyboard>/shift"
"<Keyboard>/ctrl"
"<Keyboard>/alt"
"<Keyboard>/leftArrow"
"<Keyboard>/1"
```

### Mouse
```csharp
"<Mouse>/leftButton"
"<Mouse>/rightButton"
"<Mouse>/middleButton"
"<Mouse>/delta"		  // Mouse movement
"<Mouse>/position"	   // Mouse position
"<Mouse>/scroll"		 // Mouse scroll wheel
```

### Gamepad
```csharp
"<Gamepad>/buttonSouth"  // A on Xbox, X on PlayStation
"<Gamepad>/buttonEast"   // B on Xbox, Circle on PlayStation
"<Gamepad>/buttonWest"   // X on Xbox, Square on PlayStation
"<Gamepad>/buttonNorth"  // Y on Xbox, Triangle on PlayStation
"<Gamepad>/leftStick"
"<Gamepad>/rightStick"
"<Gamepad>/leftTrigger"
"<Gamepad>/rightTrigger"
"<Gamepad>/dpad/up"
```

---

## 🚀 Quick Start Template

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class InputExample : MonoBehaviour
{
	[SerializeField] private InputActionAsset inputActions;
	private InputAction jumpAction;

	void Awake()
	{
		// Get action
		jumpAction = inputActions.FindAction("character/jump");
		
		// Subscribe to event
		jumpAction.performed += OnJump;
	}

	void OnEnable()
	{
		inputActions.Enable();
	}

	void OnDisable()
	{
		inputActions.Disable();
	}

	void OnDestroy()
	{
		jumpAction.performed -= OnJump;
	}

	void OnJump(InputAction.CallbackContext ctx)
	{
		Debug.Log("Jump!");
	}
}
```

---

## 🛠️ Advanced Techniques

### Clone an Action
```csharp
// Create a runtime copy of an action
InputAction clonedAction = action.Clone();
```

### Get Action Type Info
```csharp
switch (action.type)
{
	case InputActionType.Button:
		// One-time press action
		break;
	case InputActionType.Value:
		// Continuous value (axis, stick)
		break;
	case InputActionType.PassThrough:
		// Fires on every input, even if same value
		break;
}
```

### Access Control Info
```csharp
// Get the device that triggered the action
InputDevice device = ctx.control.device;

// Get the specific control that was used
InputControl control = ctx.control;

// Check device type
if (device is Keyboard keyboard)
{
	Debug.Log("Input from keyboard");
}
else if (device is Mouse mouse)
{
	Debug.Log("Input from mouse");
}
else if (device is Gamepad gamepad)
{
	Debug.Log("Input from gamepad");
}
```

### Composite Binding Creation (Programmatic)
```csharp
// Example: Create WASD composite binding programmatically
var walkAction = actionMap.AddAction("walk", type: InputActionType.Value);

walkAction.AddCompositeBinding("2DVector")
	.With("Up", "<Keyboard>/w")
	.With("Down", "<Keyboard>/s")
	.With("Left", "<Keyboard>/a")
	.With("Right", "<Keyboard>/d");
```

### Multiple Binding Overrides
```csharp
// Override multiple bindings at once
var overrides = new List<(int, string)>
{
	(0, "<Keyboard>/e"),
	(1, "<Keyboard>/q")
};

foreach (var (index, path) in overrides)
{
	action.ApplyBindingOverride(index, path);
}
```

---

## 📊 Binding Analysis

### Count Bindings by Type
```csharp
int totalBindings = 0;
int compositeBindings = 0;
int partOfComposite = 0;
int regularBindings = 0;

foreach (var binding in action.bindings)
{
	totalBindings++;
	
	if (binding.isComposite)
		compositeBindings++;
	else if (binding.isPartOfComposite)
		partOfComposite++;
	else
		regularBindings++;
}

Debug.Log($"Total: {totalBindings}, Composites: {compositeBindings}, " +
		  $"Parts: {partOfComposite}, Regular: {regularBindings}");
```

### Find Duplicate Bindings
```csharp
Dictionary<string, List<string>> FindDuplicateBindings(InputActionAsset asset)
{
	var pathToActions = new Dictionary<string, List<string>>();
	
	foreach (var map in asset.actionMaps)
	{
		foreach (var action in map.actions)
		{
			for (int i = 0; i < action.bindings.Count; i++)
			{
				string path = action.bindings[i].effectivePath;
				if (string.IsNullOrEmpty(path)) continue;
				
				string actionKey = $"{map.name}/{action.name}[{i}]";
				
				if (!pathToActions.ContainsKey(path))
					pathToActions[path] = new List<string>();
					
				pathToActions[path].Add(actionKey);
			}
		}
	}
	
	// Filter to only duplicates
	return pathToActions
		.Where(kvp => kvp.Value.Count > 1)
		.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

### Get All Empty Bindings
```csharp
List<(string actionName, int bindingIndex)> GetEmptyBindings(InputActionAsset asset)
{
	var emptyBindings = new List<(string, int)>();
	
	foreach (var map in asset.actionMaps)
	{
		foreach (var action in map.actions)
		{
			for (int i = 0; i < action.bindings.Count; i++)
			{
				if (string.IsNullOrEmpty(action.bindings[i].effectivePath))
				{
					emptyBindings.Add(($"{map.name}/{action.name}", i));
				}
			}
		}
	}
	
	return emptyBindings;
}
```

---

## 🎯 Best Practices

### ✅ DO
- Always disable actions in `OnDisable()`
- Always unsubscribe from events in `OnDestroy()`
- Use `effectivePath` when checking current bindings (includes overrides)
- Use `path` when you need the original/default binding
- Cache action references in `Awake()` for performance
- Check `isComposite` and `isPartOfComposite` before processing bindings
- Use the same `InputActionAsset` instance everywhere (don't create new ones)

### ❌ DON'T
- Don't forget to enable/disable actions with component lifecycle
- Don't create multiple instances of the same `InputActionAsset`
- Don't modify bindings while action is enabled (disable first)
- Don't forget to dispose rebinding operations
- Don't assume binding indices are stable after modifications
- Don't process composite bindings as regular bindings

---

## 🐛 Common Issues & Solutions

### Issue: Bindings Not Saving
```csharp
// Problem: Not saving after modification
action.ApplyBindingOverride(0, "<Keyboard>/e");

// Solution: Call PlayerPrefs.Save()
string key = "MyAction_Binding_0";
PlayerPrefs.SetString(key, action.bindings[0].effectivePath);
PlayerPrefs.Save(); // ← Important!
```

### Issue: Input Not Working After Rebind
```csharp
// Problem: Action disabled during rebind and not re-enabled
bool wasEnabled = action.enabled;
action.Disable();
// ... rebind ...
if (wasEnabled) action.Enable(); // ← Don't forget!
```

### Issue: Composite Bindings Showing Up
```csharp
// Problem: Trying to rebind composite containers
for (int i = 0; i < action.bindings.Count; i++)
{
	var binding = action.bindings[i];
	
	// Skip composites and their parts
	if (binding.isComposite || binding.isPartOfComposite)
		continue;
		
	// Only process regular bindings
	ProcessBinding(binding);
}
```

### Issue: Memory Leaks
```csharp
// Problem: Not unsubscribing from events
void OnDestroy()
{
	// Always unsubscribe!
	action.started -= OnActionStarted;
	action.performed -= OnActionPerformed;
	action.canceled -= OnActionCanceled;
}
```

---

## 📚 Additional Resources

### Official Documentation
- [Unity Input System Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/index.html)
- [Input System API Reference](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/index.html)

### Key Classes
- `InputActionAsset` - Container for all input actions
- `InputActionMap` - Group of related actions
- `InputAction` - Single action (jump, shoot, etc.)
- `InputBinding` - Key/button binding for an action
- `InputActionRebindingExtensions` - Rebinding utilities

### Useful Namespaces
```csharp
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
```

---

## 🎓 Summary

**Reading:**
```csharp
InputActionAsset.FindActionMap("name")
InputActionMap.FindAction("name")
InputAction.bindings[index]
```

**Key Properties:**
```csharp
binding.path		   // Original
binding.overridePath   // Override
binding.effectivePath  // Actual (path or override)
```

**Modifying:**
```csharp
action.ApplyBindingOverride(index, path)
action.RemoveBindingOverride(index)
action.RemoveAllBindingOverrides()
```

**Lifecycle:**
```csharp
Awake()	→ Get actions & subscribe
OnEnable() → Enable actions
OnDisable() → Disable actions
OnDestroy() → Unsubscribe
```

---

**Happy coding! 🚀**