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

	// Parse the JSON response
	void ParseResponse(string json)
	{
		// Parse the JSON manually
		var jsonObj = MiniJsonParser.Parse(json) as Dictionary<string, object>;

		// Ensure the "value" key exists
		if (jsonObj != null && jsonObj.ContainsKey("value"))
		{
			var valueList = jsonObj["value"] as List<object>;
			originalData = new List<Dictionary<string, object>>();

			foreach (var item in valueList)
			{
				if (item is Dictionary<string, object> product)
				{
					originalData.Add(product);
				}
			}
		}

		// If no data is available
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
		// Do something with the data, like displaying or filtering
		Debug.Log("Data Loaded: " + data.Count + " items.");
		// Call your filter or other logic here
		FilterData(data);
	}
}
