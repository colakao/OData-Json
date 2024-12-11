using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicFilter : MonoBehaviour
{
	// Enum to define supported filter types
	public enum FilterType
	{
		Equals,
		GreaterThan,
		LessThan,
		Contains
	}

	[System.Serializable]
	public class Product
	{
		public int ProductID;
		public string ProductName;
		public float UnitPrice;
	}

	// Sample data list
	public List<Product> products = new List<Product>
	{
		new Product { ProductID = 1, ProductName = "Chai", UnitPrice = 18.0f },
		new Product { ProductID = 2, ProductName = "Chang", UnitPrice = 19.0f },
		new Product { ProductID = 3, ProductName = "Aniseed Syrup", UnitPrice = 10.0f },
		new Product { ProductID = 4, ProductName = "Chai", UnitPrice = 20.0f }
	};

	// Example of dynamic filters: (Key, Value, Type)
	private List<(string Key, object Value, FilterType Type)> filters;

	void Start()
	{
		// Example filters: Find Chai products with price greater than 18
		filters = new List<(string Key, object Value, FilterType Type)>
		{
			("ProductName", "Chai", FilterType.Equals),
			("UnitPrice", 18.0f, FilterType.GreaterThan)
		};

		// Apply dynamic filters
		var filteredProducts = FilterData(products, filters);

		// Display filtered data in the console
		foreach (var product in filteredProducts)
		{
			Debug.Log($"ProductID: {product.ProductID}, ProductName: {product.ProductName}, UnitPrice: {product.UnitPrice}");
		}
	}

	// Method to apply dynamic filters
	public static List<Product> FilterData(
		List<Product> data,
		List<(string Key, object Value, FilterType Type)> filters)
	{
		// Loop through each data item and check if it matches all filters
		return data.Where(item =>
			filters.All(filter => ApplyFilter(item, filter.Key, filter.Value, filter.Type))
		).ToList();
	}

	// Method to apply a single filter condition to an item
	private static bool ApplyFilter(Product item, string key, object value, FilterType type)
	{
		// Dynamically check and filter based on the key-value pair
		switch (key)
		{
			case "ProductName":
				string productName = item.ProductName;
				if (type == FilterType.Equals && productName.Equals(value))
				{
					return true;
				}
				if (type == FilterType.Contains && productName.Contains(value.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				break;

			case "UnitPrice":
				if (item.UnitPrice is float unitPrice && value is float targetPrice)
				{
					switch (type)
					{
						case FilterType.Equals:
							return unitPrice == targetPrice;
						case FilterType.GreaterThan:
							return unitPrice > targetPrice;
						case FilterType.LessThan:
							return unitPrice < targetPrice;
					}
				}
				break;
		}

		return false;
	}
}
