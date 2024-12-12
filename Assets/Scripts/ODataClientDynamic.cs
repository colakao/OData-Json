using System.Collections.Generic;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class ODataClientDynamic : MonoBehaviour
{
	// OData service URL
	private string serviceUrl = "https://services.odata.org/V4/Northwind/Northwind.svc/Products";

	// Reference to the TMP Text UI
	public TextMeshProUGUI dataDisplay; // Drag the TMP Text UI element here in the Inspector

	// Declare an event to notify when data is loaded
	public delegate void DataLoadedDelegate(List<Dictionary<string, object>> data);
	public event DataLoadedDelegate OnDataLoaded;

	// Original Data to store the JSON result
	private List<Dictionary<string, object>> originalData;

	// For dynamic property detection (filtering)
	private HashSet<string> availableProperties = new HashSet<string>();

	// For UI filter toggling
	public GameObject buttonPrefab; // Assign a Button prefab in the Inspector
	public Transform buttonContainer; // Assign a UI container (e.g., Vertical Layout Group)

	void Start()
	{
		// Start the query
		StartCoroutine(GetData());
	}

	// Coroutine to get data from the OData API
	IEnumerator GetData()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(serviceUrl))
		{
			yield return webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError($"Error: {webRequest.error}");
				dataDisplay.text = "Error fetching data.";
			}
			else
			{
				// Parse the response and store it in originalData
				ParseResponse(webRequest.downloadHandler.text);

				// Trigger the data loaded event
				OnDataLoaded?.Invoke(originalData);
			}
		}
	}

	void ParseResponse(string json)
	{
		var jsonObj = MiniJsonParser.Parse(json) as Dictionary<string, object>;

		if (jsonObj != null && jsonObj.ContainsKey("value"))
		{
			var valueList = jsonObj["value"] as List<object>;
			originalData = new List<Dictionary<string, object>>();

			foreach (var item in valueList)
			{
				if (item is Dictionary<string, object> product)
				{
					originalData.Add(product);

					// Collect available properties (keys)
					foreach (var key in product.Keys)
					{
						availableProperties.Add(key);
					}
				}
			}
		}

		if (originalData.Count == 0)
		{
			dataDisplay.text = "No data available.";
		}
	}

	// Add your filtering logic here (after data is loaded)
	void FilterData(List<Dictionary<string, object>> data)
	{
		// Example filter logic
		foreach (var item in data)
		{
			if (item.ContainsKey("Category"))
			{
				string category = item["Category"].ToString();
				Debug.Log("Filtered Category: " + category);
				// Your filtering logic here...
			}
		}
	}

	// Registering a listener for when data is loaded
	private void OnEnable()
	{
		OnDataLoaded += HandleDataLoaded;
	}

	private void OnDisable()
	{
		OnDataLoaded -= HandleDataLoaded;
	}

	// Handle the event when data is loaded
	private void HandleDataLoaded(List<Dictionary<string, object>> data)
	{
		InstantiateFilterButtons();
		UpdateDataDisplay();
	}


	void InstantiateFilterButtons()
	{
		foreach (var property in availableProperties)
		{
			GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
			newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = property;

			// Add a listener to handle button click
			string propertyName = property; // Capture property name in a local variable
			newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ToggleFilter(propertyName));
		}
	}

	private Dictionary<string, bool> activeFilters = new Dictionary<string, bool>();

	void ToggleFilter(string property)
	{
		if (activeFilters.ContainsKey(property))
		{
			activeFilters[property] = !activeFilters[property];
		}
		else
		{
			activeFilters[property] = true;
		}

		UpdateDataDisplay();
	}

	void UpdateDataDisplay()
	{
		string displayText = "Filtered Data:\n";

		foreach (var item in originalData)
		{
			foreach (var property in activeFilters.Keys)
			{
				if (activeFilters[property] && item.ContainsKey(property))
				{
					displayText += $"{property}: {item[property]}\n";
				}
			}

			displayText += "\n"; // Separate items
		}

		dataDisplay.text = displayText;
	}
}
