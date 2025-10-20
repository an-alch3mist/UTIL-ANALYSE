using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SPACE_UTIL;

// ===== HELPER EXTENSION METHOD (Add to your SPACE_UTIL namespace) =====
namespace SPACE_CHECK
{
	public static class InputActionExtensions
	{

		/// <summary>
		/// Get display string directly from binding path
		/// </summary>
		public static string GetDisplayString(this InputBinding binding)
		{
			if (string.IsNullOrEmpty(binding.effectivePath))
				return "[Not Bound]";

			string display = InputControlPath.ToHumanReadableString(
				binding.effectivePath,
				InputControlPath.HumanReadableStringOptions.OmitDevice
			);

			return string.IsNullOrEmpty(display) ? "[Not Bound]" : display;
		}
	}
}

// ===== QUICK REFERENCE TABLE =====
/*
BINDING PATH                    -> DISPLAY STRING
─────────────────────────────────────────────────
<Keyboard>/space                -> "Space"
<Keyboard>/a                    -> "A"
<Keyboard>/leftShift            -> "Left Shift"
<Keyboard>/rightShift           -> "Right Shift"
<Keyboard>/leftCtrl             -> "Left Ctrl"
<Keyboard>/numpad0              -> "Numpad 0"
<Keyboard>/digit0               -> "0"
<Mouse>/leftButton              -> "LMB"
<Mouse>/rightButton             -> "RMB"
<Keyboard>/escape               -> "Escape"
<Keyboard>/enter                -> "Enter"
<Keyboard>/backspace            -> "Backspace"
<Keyboard>/f1                   -> "F1"
*/
