# Transparent Overlay Using Window API user32.dll

## Make Sure The Following:
- in Build Build Settings > Player Settings
	- Use DXGI flip model swapchain for D3D11 -> unchecked
	- Fullscreen Mode -> FullScreen Window
	- Run In Backgroung -> checked
	- Visible In Backgrounf -> checked

## in a scene Main Camera -> Camera Component
	- Clear Flags -> Solid COlor
	- Background Color -> 0, 0, 0, 0

## create a new empty gameObject
	-> TransparentOverlayUsingWindowAPI.cs 
```cs
using System;
using System.Runtime.InteropServices; // for DllImport, varialbe names is sensitive

using UnityEngine;

namespace SPACE_TransparentOverlayUsingWindowAPI
{
	public class TransparentOverlayUsingWindowAPI : MonoBehaviour
	{
#if !UNITY_EDITOR
		[DllImport("user32.dll")]
		private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong); // helps to set attributes for window

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); // helps to set attributes for window, to make window always on top

		[DllImport("user32.dll")]
		private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags); 
	
		private struct MARGINS
		{
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}

		[DllImport("Dwmapi.dll")]
		private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

		const int GWL_EXSTYLE = -20;
		const uint WS_EX_LAYERED		= 0x00080000;
		const uint WS_EX_TRANSPARENT	= 0x00000020;
		static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		const uint LWA_COLORKEY = 0x00000001;

		private void Start()
		{
			Debug.Log("Start(): " + this);
			// MessageBox(new IntPtr(0), "Hello Text1", "Hello Caption", 0x0);

			IntPtr hWnd = GetActiveWindow();
			MARGINS margins = new MARGINS { cxLeftWidth = -1 };
			DwmExtendFrameIntoClientArea(hWnd, ref margins);

			SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
			SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY); // clickable only those which has no alpha
			SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);

			Application.runInBackground = true;
		}
#endif
	} 
}
```