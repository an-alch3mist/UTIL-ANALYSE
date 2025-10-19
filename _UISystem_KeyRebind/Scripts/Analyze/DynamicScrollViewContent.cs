using UnityEngine;
using UnityEngine.UI;

using SPACE_UTIL;

public class DynamicScrollViewContent : MonoBehaviour
{
	[SerializeField] private GameObject buttonPrefab;
	[SerializeField] private Transform content; // Assign the Content GameObject here
	[SerializeField] private int _btnCount = 10;


	private void Awake()
	{
		Debug.Log(("Awake(): " + this).colorTag("lime"));


	}
	void Start()
	{
		Debug.Log(("Start(): " + this).colorTag("lime"));
		// Ensure Content has required components
		SetupContentComponents();
	}

	private void Update()
	{
		if (INPUT.M.InstantDown(0))
			// Example: Add 10 buttons dynamically
			for (int i = 0; i < this._btnCount; i++)
			{
				AddButton($"Button {i + 1}");
			}
	}



	private void SetupContentComponents()
	{
		// Add VerticalLayoutGroup if not present
		if (content.GetComponent<VerticalLayoutGroup>() == null)
		{
			var layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
			layoutGroup.spacing = 10f;
			layoutGroup.padding = new RectOffset(10, 10, 10, 10);
			layoutGroup.childControlHeight = true;
			layoutGroup.childControlWidth = true;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.childForceExpandWidth = true;
		}

		// Add ContentSizeFitter if not present
		if (content.GetComponent<ContentSizeFitter>() == null)
		{
			var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		}
	}

	public void AddButton(string buttonText)
	{
		// Instantiate button as child of content
		GameObject btn = Instantiate(buttonPrefab, content);

		// Optional: Set button text
		Text txtComponent = btn.GetComponentInChildren<Text>();
		if (txtComponent != null)
		{
			txtComponent.text = buttonText;
		}

		// Force layout rebuild (usually happens automatically, but can help)
		LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
	}

	public void RemoveButton(GameObject button)
	{
		Destroy(button);

		// Force layout rebuild after removal
		LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
	}

	public void ClearAllButtons()
	{
		foreach (Transform child in content)
		{
			Destroy(child.gameObject);
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
	}
}