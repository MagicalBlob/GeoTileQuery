using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

/// <summary>
/// Controls the modals
/// </summary>
public class Modals
{
    /// <summary>
    /// Whether the modal is currently open
    /// </summary>
    public bool IsOpen { get { return current.activeSelf; } }

    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map map;

    /// <summary>
    /// The UI controller
    /// </summary>
    private UIController ui;

    /// <summary>
    /// The modals root transform
    /// </summary>
    private Transform modals;

    /// <summary>
    /// The current modal content
    /// </summary>
    private GameObject current;

    /// <summary>
    /// Debug text display
    /// </summary>
    private Text debugTextDisplay;

    /// <summary>
    /// Log display
    /// </summary>
    private GameObject log;

    /// <summary>
    /// Number of frames since last update
    /// </summary>
    private int numFrames = 0;

    /// <summary>
    /// Sum of frame times since last update
    /// </summary>
    private float fpsSum = 0;

    /// <summary>
    /// Constructs the modals
    /// </summary>
    /// <param name="map">Reference to the map</param>
    /// <param name="ui">The UI controller</param>
    /// <param name="modals">The modals root transform</param>
    public Modals(Map map, UIController ui, Transform modals)
    {
        this.map = map;
        this.ui = ui;
        this.modals = modals;

        // Set up the close button
        modals.Find("Overlay").GetComponent<Button>().onClick.AddListener(() => Close());
        modals.Find("Content/Close").GetComponent<Button>().onClick.AddListener(() => Close());

        // Set up the location modal
        SetupLocationModal();

        // Set up the layers modal
        SetupLayersModal();

        // Set up the debug modal
        SetupDebugModal();
    }

    /// <summary>
    /// Shows the given modal with a specific width and height
    /// </summary>
    /// <param name="modal">The modal name</param>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    public void Show(string modal, float width, float height)
    {
        modals.Find("Content").GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        modals.Find("Content/Header").GetComponent<Text>().text = modal;
        current = modals.Find($"Content/{modal}").gameObject;
        current.SetActive(true);
        modals.gameObject.SetActive(true);
    }

    /// <summary>
    /// Shows the given modal
    /// </summary>
    /// <param name="modal">The modal name</param>
    public void Show(string modal)
    {
        Show(modal, 1250, 1000);
    }

    /// <summary>
    /// Closes the current modal
    /// </summary>
    public void Close()
    {
        modals.gameObject.SetActive(false);
        current.SetActive(false);
    }

    /// <summary>
    /// Sets up the location modal
    /// </summary>
    private void SetupLocationModal()
    {
        // Get the modal
        Transform locationModal = modals.Find("Content/Location");

        // Latitude/Longitude input fields
        InputField latitudeInput = locationModal.Find("Position/Latitude").GetComponent<InputField>();
        latitudeInput.onEndEdit.AddListener((string value) =>
        {
            double latitude;
            try
            {
                latitude = Convert.ToDouble(value);
            }
            catch
            {
                latitude = 0;
            }
            latitudeInput.text = $"{latitude}";
        });
        InputField longitudeInput = locationModal.Find("Position/Longitude").GetComponent<InputField>();
        longitudeInput.onEndEdit.AddListener((string value) =>
        {
            double longitude;
            try
            {
                longitude = Convert.ToDouble(value);
            }
            catch
            {
                longitude = 0;
            }
            longitudeInput.text = $"{longitude}";
        });

        // Point of Interest dropdown
        Dropdown poiDropdown = locationModal.Find("Point of Interest").GetComponent<Dropdown>();
        List<Dropdown.OptionData> poiOptions = new List<Dropdown.OptionData>();
        foreach (PointOfInterest poi in map.POIs)
        {
            poiOptions.Add(new Dropdown.OptionData(poi.Name));
        }
        poiDropdown.options = poiOptions;
        poiDropdown.value = 0;
        Vector2D position = map.POIs[0].Coordinates;
        latitudeInput.text = $"{position.X}";
        longitudeInput.text = $"{position.Y}";
        poiDropdown.onValueChanged.AddListener((int value) =>
        {
            Vector2D position = map.POIs[value].Coordinates;
            latitudeInput.text = $"{position.X}";
            longitudeInput.text = $"{position.Y}";
        });

        // Go button
        locationModal.Find("Go").GetComponent<Button>().onClick.AddListener(() =>
        {
            double latitude, longitude;
            try { latitude = Convert.ToDouble(latitudeInput.text); } catch { latitude = 0; }
            try { longitude = Convert.ToDouble(longitudeInput.text); } catch { longitude = 0; }
            map.MoveCenter(latitude, longitude);
        });
    }

    /// <summary>
    /// Sets up the layers modal
    /// </summary>
    private void SetupLayersModal()
    {
        // Get the modal
        Transform layersModal = modals.Find("Content/Layers");

        // Get the layer list entry prefab
        GameObject list = layersModal.transform.Find("List/Viewport/Content").gameObject;
        GameObject listEntryPrefab = Resources.Load<GameObject>("UI/Layers List Entry");

        foreach (ILayer layer in map.Layers.Values)
        {
            // Instantiate the layer list entry prefabs as children of the layers list
            GameObject listEntryObj = GameObject.Instantiate(listEntryPrefab, list.transform);
            listEntryObj.name = layer.Id;
            listEntryObj.transform.Find("View Info/Label").GetComponent<Text>().text = layer.DisplayName;

            // Setup the visibility toggle
            Toggle toggle = listEntryObj.transform.Find("Visibility").GetComponent<Toggle>();
            toggle.isOn = layer.Visible; // Set the initial state of the toggle to the layer's visibility
            toggle.onValueChanged.AddListener((bool value) => layer.Visible = value); // Set the layer's visibility when the toggle is changed

            // Setup the view info button
            Button viewInfoButton = listEntryObj.transform.Find("View Info").GetComponent<Button>();
            viewInfoButton.onClick.AddListener(() => ViewLayerInfo(layersModal, layer));
        }
    }

    /// <summary>
    /// View the info of the given layer
    /// </summary>
    /// <param name="modal">The layers modal</param>
    /// <param name="layer">The layer to view the info of</param>
    private void ViewLayerInfo(Transform modal, ILayer layer)
    {
        // Get the layer view
        GameObject layerView = modal.transform.Find("Layer").gameObject;
        layerView.SetActive(true);
        string lastUpdate = layer.LastUpdate == DateTime.MinValue ? "Unknown" : layer.LastUpdate.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture);
        layerView.transform.Find("Info").GetComponent<Text>().text = $"<b>{layer.DisplayName.ToUpper()}</b>\n<b>Description:</b> {layer.Description}\n<b>Source:</b> {layer.Source}\n<b>Last update:</b> {lastUpdate}";

        GameObject layerPropertiesView = layerView.transform.Find("Properties").gameObject;
        if (layer is IFilterableLayer)
        {
            IFilterableLayer filterableLayer = (IFilterableLayer)layer;

            // Get the layer properties list entry prefab
            GameObject propertiesList = layerPropertiesView.transform.Find("Viewport/Content").gameObject;
            GameObject listEntryPrefab = Resources.Load<GameObject>("UI/Layer Properties List Entry");

            // Remove all existing list entries
            foreach (Transform child in propertiesList.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // Add the properties to the list
            for (int i = 0; i < filterableLayer.FeatureProperties.Length; i++)
            {
                // Instantiate the layer property list entry prefabs as children of the layer properties list
                GameObject listEntryObj = GameObject.Instantiate(listEntryPrefab, propertiesList.transform);
                IFeatureProperty property = filterableLayer.FeatureProperties[i];
                int propertyIndex = i;
                listEntryObj.name = property.Key;

                // Setup the property filter toggle
                Toggle filtered = listEntryObj.transform.Find("Filtered").GetComponent<Toggle>();
                filtered.isOn = property.Filtered; // Set the initial state of the toggle to the property's filter state
                filtered.onValueChanged.AddListener((bool value) =>
                {
                    IFeatureProperty updatedProperty = filterableLayer.FeatureProperties[propertyIndex];
                    updatedProperty.Filtered = value;
                    filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                    map.ApplyFilters(filterableLayer.Id);
                });

                // Set the property name
                listEntryObj.transform.Find("Label").GetComponent<Text>().text = property.DisplayName;

                // Set the property filter options
                Dropdown filterDropdown = listEntryObj.transform.Find("Filter Dropdown").GetComponent<Dropdown>();
                InputField filterValueInput = listEntryObj.transform.Find("Filter Value Input Field").GetComponent<InputField>();
                Dropdown filterValueDropdown = listEntryObj.transform.Find("Filter Value Dropdown").GetComponent<Dropdown>();
                switch (property)
                {
                    case StringFeatureProperty stringP:
                        // Set the filter options and current filter value
                        List<Dropdown.OptionData> stringFilterOptions = new List<Dropdown.OptionData>();
                        foreach (string option in Enum.GetNames(typeof(StringFeatureProperty.FilterOperator)))
                        {
                            stringFilterOptions.Add(new Dropdown.OptionData(System.Text.RegularExpressions.Regex.Replace(option, "(\\B[A-Z])", " $1")));
                        }
                        filterDropdown.options = stringFilterOptions;
                        filterDropdown.value = (int)stringP.Filter;
                        filterValueInput.gameObject.SetActive(true);
                        filterValueInput.text = stringP.FilterValue;
                        filterValueInput.placeholder.GetComponent<Text>().text = "Filter value (case sensitive)";
                        filterValueDropdown.gameObject.SetActive(false);
                        // Add event listeners
                        filterDropdown.onValueChanged.AddListener((int value) =>
                        {
                            StringFeatureProperty updatedProperty = (StringFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.Filter = (StringFeatureProperty.FilterOperator)value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueInput.onEndEdit.AddListener((string value) =>
                        {
                            StringFeatureProperty updatedProperty = (StringFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.FilterValue = value.Trim();
                            filterValueInput.text = updatedProperty.FilterValue;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        break;
                    case NumberFeatureProperty numberP:
                        // Set the filter options and current filter value
                        List<Dropdown.OptionData> numberFilterOptions = new List<Dropdown.OptionData>();
                        foreach (string option in Enum.GetNames(typeof(NumberFeatureProperty.FilterOperator)))
                        {
                            numberFilterOptions.Add(new Dropdown.OptionData(System.Text.RegularExpressions.Regex.Replace(option, "(\\B[A-Z])", " $1")));
                        }
                        filterDropdown.options = numberFilterOptions;
                        filterDropdown.value = (int)numberP.Filter;
                        filterValueInput.gameObject.SetActive(true);
                        filterValueInput.text = $"{numberP.FilterValue}";
                        filterValueInput.placeholder.GetComponent<Text>().text = "Filter value (e.g. 1.5)";
                        filterValueDropdown.gameObject.SetActive(false);
                        // Add event listeners
                        filterDropdown.onValueChanged.AddListener((int value) =>
                        {
                            NumberFeatureProperty updatedProperty = (NumberFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.Filter = (NumberFeatureProperty.FilterOperator)value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueInput.onEndEdit.AddListener((string value) =>
                        {
                            NumberFeatureProperty updatedProperty = (NumberFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            try
                            {
                                updatedProperty.FilterValue = Convert.ToDouble(value);
                            }
                            catch
                            {
                                updatedProperty.FilterValue = 0;
                            }
                            filterValueInput.text = $"{updatedProperty.FilterValue}";
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        break;
                    case BooleanFeatureProperty booleanP:
                        // Set the filter options and current filter value
                        List<Dropdown.OptionData> booleanFilterOptions = new List<Dropdown.OptionData>();
                        foreach (string option in Enum.GetNames(typeof(BooleanFeatureProperty.FilterOperator)))
                        {
                            booleanFilterOptions.Add(new Dropdown.OptionData(System.Text.RegularExpressions.Regex.Replace(option, "(\\B[A-Z])", " $1")));
                        }
                        filterDropdown.options = booleanFilterOptions;
                        filterDropdown.value = (int)booleanP.Filter;
                        filterValueInput.gameObject.SetActive(true);
                        filterValueInput.text = $"{booleanP.FilterValue}";
                        filterValueInput.placeholder.GetComponent<Text>().text = $"Filter value (e.g. {true})";
                        filterValueDropdown.gameObject.SetActive(false);
                        // Add event listeners
                        filterDropdown.onValueChanged.AddListener((int value) =>
                        {
                            BooleanFeatureProperty updatedProperty = (BooleanFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.Filter = (BooleanFeatureProperty.FilterOperator)value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueInput.onEndEdit.AddListener((string value) =>
                        {
                            BooleanFeatureProperty updatedProperty = (BooleanFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            try
                            {
                                updatedProperty.FilterValue = Convert.ToBoolean(value);
                            }
                            catch
                            {
                                updatedProperty.FilterValue = false;
                            }
                            filterValueInput.text = $"{updatedProperty.FilterValue}";
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        break;
                    case DateFeatureProperty dateP:
                        // Set the filter options and current filter value
                        List<Dropdown.OptionData> dateFilterOptions = new List<Dropdown.OptionData>();
                        foreach (string option in Enum.GetNames(typeof(DateFeatureProperty.FilterOperator)))
                        {
                            dateFilterOptions.Add(new Dropdown.OptionData(System.Text.RegularExpressions.Regex.Replace(option, "(\\B[A-Z])", " $1")));
                        }
                        filterDropdown.options = dateFilterOptions;
                        filterDropdown.value = (int)dateP.Filter;
                        filterValueInput.gameObject.SetActive(true);
                        filterValueInput.text = $"{dateP.FilterValue}";
                        filterValueInput.placeholder.GetComponent<Text>().text = $"Filter value (e.g. {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture)})";
                        filterValueDropdown.gameObject.SetActive(false);
                        // Add event listeners
                        filterDropdown.onValueChanged.AddListener((int value) =>
                        {
                            DateFeatureProperty updatedProperty = (DateFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.Filter = (DateFeatureProperty.FilterOperator)value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueInput.onEndEdit.AddListener((string value) =>
                        {
                            DateFeatureProperty updatedProperty = (DateFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            try
                            {
                                updatedProperty.FilterValue = Convert.ToDateTime(value);
                            }
                            catch
                            {
                                updatedProperty.FilterValue = DateTime.MinValue;
                            }
                            filterValueInput.text = updatedProperty.FilterValue.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture);
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        break;
                    case CategoryFeatureProperty categoryP:
                        // Set the filter options and current filter value
                        List<Dropdown.OptionData> categoryFilterOptions = new List<Dropdown.OptionData>();
                        foreach (string option in Enum.GetNames(typeof(CategoryFeatureProperty.FilterOperator)))
                        {
                            categoryFilterOptions.Add(new Dropdown.OptionData(System.Text.RegularExpressions.Regex.Replace(option, "(\\B[A-Z])", " $1")));
                        }
                        filterDropdown.options = categoryFilterOptions;
                        filterDropdown.value = (int)categoryP.Filter;
                        filterValueInput.gameObject.SetActive(false);
                        filterValueDropdown.gameObject.SetActive(true);
                        List<Dropdown.OptionData> categoryOptions = new List<Dropdown.OptionData>();
                        foreach (string option in categoryP.Categories)
                        {
                            categoryOptions.Add(new Dropdown.OptionData(option));
                        }
                        filterValueDropdown.options = categoryOptions;
                        filterValueDropdown.value = categoryP.FilterValue;
                        // Add event listeners
                        filterDropdown.onValueChanged.AddListener((int value) =>
                        {
                            CategoryFeatureProperty updatedProperty = (CategoryFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.Filter = (CategoryFeatureProperty.FilterOperator)value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueDropdown.onValueChanged.AddListener((int value) =>
                        {
                            CategoryFeatureProperty updatedProperty = (CategoryFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.FilterValue = value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            map.ApplyFilters(filterableLayer.Id);
                        });
                        break;
                }
            }

            layerPropertiesView.SetActive(true);
        }
        else
        {
            layerPropertiesView.SetActive(false);
        }
    }

    /// <summary>
    /// Sets up the debug modal
    /// </summary>
    private void SetupDebugModal()
    {
        // Get the modal
        Transform debugModal = modals.Find("Content/Debug");
        debugTextDisplay = debugModal.transform.Find("Text Display").GetComponent<Text>();
        log = debugModal.transform.Find("Log/Viewport/Content").gameObject;
        Logger.Subscribe(UpdateLog); // Listen for new log messages to display
    }

    /// <summary>
    /// Calculates the instantaneous FPS
    /// </summary>
    /// <returns>Instantaneous FPS</returns>
    private float GetInstantFps()
    {
        return 1 / Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Gets the average FPS since the last call to GetAverageFps()
    /// </summary>
    /// <returns>Average FPS</returns>
    private float GetAverageFps()
    {
        float averageFps = fpsSum / numFrames;
        fpsSum = 0;
        numFrames = 0;
        return averageFps;
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        UpdateAverageFps();
    }

    /// <summary>
    /// Updates the counters for the average FPS
    /// </summary>
    private void UpdateAverageFps()
    {
        fpsSum += GetInstantFps();
        numFrames++;
    }

    /// <summary>
    /// Called on a fixed interval
    /// </summary>
    public void FixedUpdate()
    {
        UpdateDebugText();
    }

    /// <summary>
    /// Updates the debug text
    /// </summary>
    private void UpdateDebugText()
    {
        StringBuilder debugText = new StringBuilder();

        debugText.Append("Version: ");
        debugText.Append(Application.version);

        debugText.Append("\n\nInstant FPS: ");
        float instantFps = GetInstantFps();
        debugText.Append((int)instantFps);
        debugText.Append("\nAverage FPS: ");
        float averageFps = GetAverageFps();
        debugText.Append((int)averageFps);

        debugText.Append("\n\nSystem Memory: ");
        debugText.Append(SystemInfo.systemMemorySize);
        debugText.Append(" MB\nTotal Reserved: ");
        debugText.Append(Profiler.GetTotalReservedMemoryLong() / 1000000);
        debugText.Append(" MB\n- Allocated: ");
        debugText.Append(Profiler.GetTotalAllocatedMemoryLong() / 1000000);
        debugText.Append(" MB\n- Unused: ");
        debugText.Append(Profiler.GetTotalUnusedReservedMemoryLong() / 1000000);
        debugText.Append(" MB");

        debugText.Append("\n\nCPU: ");
        debugText.Append(SystemInfo.processorCount);
        debugText.Append("x ");
        debugText.Append(SystemInfo.processorType);
        debugText.Append(" @ ");
        debugText.Append(SystemInfo.processorFrequency / 1000f);
        debugText.Append(" GHz");

        debugText.Append("\n\nDisplay: ");
        debugText.Append(Screen.currentResolution);
        debugText.AppendLine();
        debugText.Append(SystemInfo.graphicsDeviceVendor);
        debugText.Append(" ");
        debugText.Append(SystemInfo.graphicsDeviceName);
        debugText.Append(" (");
        debugText.Append(SystemInfo.graphicsMemorySize);
        debugText.Append(" MB)\n");
        debugText.Append(SystemInfo.graphicsDeviceVersion);

        debugText.Append("\n\nDevice: ");
        debugText.Append(SystemInfo.operatingSystem);
        debugText.AppendLine();
        debugText.Append(SystemInfo.deviceModel);
        debugText.Append(" (");
        debugText.Append(SystemInfo.deviceType);
        debugText.Append(")\n");
        debugText.Append(SystemInfo.batteryLevel * 100);
        debugText.Append("% Battery (");
        debugText.Append(SystemInfo.batteryStatus);
        debugText.Append(")");

        debugText.Append("\n\nDebug level: ");
        debugText.Append(Debug.unityLogger.filterLogType);
        debugText.Append("\nAvailable semaphore threads: ");
        debugText.Append(MainController.networkSemaphore.CurrentCount);
        debugText.Append("\nTiles: ");
        debugText.Append(map.Tiles.Count);

        debugTextDisplay.text = debugText.ToString();
    }

    /// <summary>
    /// Updates log to render new messages
    /// </summary>
    private void UpdateLog(object sender, System.EventArgs e)
    {
        Logger.Render(log);
    }

    /// <summary>
    /// Queries the map from the a given screen point and displays the results in the query modal
    /// </summary>
    /// <param name="screenPoint">The screen point to cast the query ray from</param>
    public void ShowMapQuery(Vector2 screenPoint)
    {
        // Get the query model
        Transform queryModal = modals.Find("Content/Query");

        // Cast the ray at the screen point
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Hit something
            Vector2D hitLatLon = map.WorldToLatLon(hit.point);
            string queryInfo = $"<b>Coordinates:</b>\n\t<b>- Latitude:</b> {hitLatLon.X}\n\t<b>- Longitude:</b> {hitLatLon.Y}\n\t<b>- Height:</b> {map.GetHeight(hit.point).ToString("0.00")} m";

            // Check if the hit object is part of a map feature
            FeatureBehaviour featureBehaviour = hit.transform.GetComponentInParent<FeatureBehaviour>();
            if (featureBehaviour == null)
            {
                // Not a map feature
                queryModal.Find("Info").GetComponent<Text>().text = queryInfo;
                queryModal.Find("Feature Properties").gameObject.SetActive(false);

                // Show the query modal
                Show("Query", 750, 195);
            }
            else
            {
                // Hit a map feature
                Feature feature = featureBehaviour.Feature;
                queryInfo = $"{queryInfo}\n<b>Feature ID:</b> {feature.Id}\n<b>Layer:</b> {feature.TileLayer.Layer.Id}\n<b>Tile:</b> {feature.TileLayer.Tile.Id}";
                queryModal.Find("Info").GetComponent<Text>().text = queryInfo;

                // Feature properties
                string featureProperties = "";
                for (int i = 0; i < ((IFilterableLayer)feature.TileLayer.Layer).FeatureProperties.Length; i++)
                {
                    IFeatureProperty property = ((IFilterableLayer)feature.TileLayer.Layer).FeatureProperties[i];
                    string valueString;
                    switch (property)
                    {
                        case StringFeatureProperty stringP:
                            string valueStr = feature.GetPropertyAsNullableString(stringP.Key);
                            if (valueStr == null)
                            {
                                valueString = "<i>No value</i>";
                            }
                            else
                            {
                                valueString = valueStr;
                            }
                            break;
                        case NumberFeatureProperty numberP:
                            double? valueNum = feature.GetPropertyAsNullableDouble(numberP.Key);
                            if (valueNum == null)
                            {
                                valueString = "<i>No value</i>";
                            }
                            else
                            {
                                valueString = $"{System.Math.Round(valueNum.Value, 2)}";
                            }
                            break;
                        case BooleanFeatureProperty booleanP:
                            bool? valueBool = feature.GetPropertyAsNullableBool(booleanP.Key);
                            if (valueBool == null)
                            {
                                valueString = "<i>No value</i>";
                            }
                            else
                            {
                                valueString = $"{valueBool.Value}";
                            }
                            break;
                        case DateFeatureProperty dateP:
                            DateTime? valueDate = feature.GetPropertyAsNullableDateTime(dateP.Key);
                            if (valueDate == null)
                            {
                                valueString = "<i>No value</i>";
                            }
                            else
                            {
                                valueString = $"{valueDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture)}";
                            }
                            break;
                        case CategoryFeatureProperty categoryP:
                            string valueCat = feature.GetPropertyAsNullableString(categoryP.Key);
                            if (valueCat == null)
                            {
                                valueString = "<i>No value</i>";
                            }
                            else
                            {
                                valueString = valueCat;
                            }
                            break;
                        default:
                            valueString = "<i>Unknown property type</i>";
                            break;
                    }
                    featureProperties = $"{featureProperties}{(i > 0 ? "\n" : "")}<b>{property.DisplayName}:</b> {String.Format(property.FormatString, valueString)}";
                }

                queryModal.Find("Feature Properties").gameObject.SetActive(true);
                queryModal.Find("Feature Properties/Viewport/Content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                if (((IFilterableLayer)feature.TileLayer.Layer).FeatureProperties.Length > 0)
                {
                    queryModal.Find("Feature Properties/Viewport/Content/Text").GetComponent<Text>().text = featureProperties;
                }
                else
                {
                    queryModal.Find("Feature Properties/Viewport/Content/Text").GetComponent<Text>().text = "<i>Feature has no properties</i>";
                }

                // Show the query modal
                Show("Query", 750, 500);
            }
        }
        else
        {
            // Didn't hit anything
            queryModal.Find("Info").GetComponent<Text>().text = "<b>Here be dragons!</b>\n\n<i>Nothing was hit by the raycast</i>";
            queryModal.Find("Feature Properties").gameObject.SetActive(false);

            // Show the query modal
            Show("Query", 750, 170);
        }
    }
}
