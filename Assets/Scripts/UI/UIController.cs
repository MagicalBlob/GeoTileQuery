using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

/// <summary>
/// Controls the user interface
/// </summary>
public class UIController
{
    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map Map { get; }

    private Transform buttons;
    private Transform modal;
    private Transform currentModalContent;

    private Transform locationModal;
    private Transform layersModal;
    private Transform debugModal;
    private Transform aboutModal;

    /// <summary>
    /// Debug text display
    /// </summary>
    private Text debugTextDisplay;

    /// <summary>
    /// Log display
    /// </summary>
    private GameObject log;

    private float update = 0.0f;

    private int numFrames = 0;
    private float totalFps = 0;

    private bool arMode = false;

    /// <summary>
    /// Constructs a new UI Controller
    /// <param name="map">The map</param>
    /// </summary>
    public UIController(Map map)
    {
        this.Map = map;

        Transform UI = GameObject.Find("UI").transform;
        buttons = UI.Find("Buttons").transform;
        modal = UI.Find("Modal").transform;
        modal.Find("Overlay").GetComponent<Button>().onClick.AddListener(() => CloseModal());
        modal.Find("Content/Close").GetComponent<Button>().onClick.AddListener(() => CloseModal());

        // Location modal
        SetupLocationModal();

        // Layers modal
        layersModal = modal.Find("Content/Layers");
        buttons.Find("Layers").GetComponent<Button>().onClick.AddListener(() => { ShowModal(layersModal); });
        PopulateMapLayersList();

        // Debug modal
        debugModal = modal.Find("Content/Debug");
        buttons.Find("Debug").GetComponent<Button>().onClick.AddListener(() => { ShowModal(debugModal); });
        debugTextDisplay = debugModal.transform.Find("Text Display").GetComponent<Text>();
        log = debugModal.transform.Find("Log/Viewport/Content").gameObject;
        Logger.Subscribe(UpdateLog); // Listen for new log messages to display

        // About modal
        aboutModal = modal.Find("Content/About");
        buttons.Find("About").GetComponent<Button>().onClick.AddListener(() => { ShowModal(aboutModal, 500, 500); });

        // Navigation buttons
        buttons.Find("Navigation/Up").GetComponent<Button>().onClick.AddListener(() => Map.MoveCenter(new Vector2D(0, 100)));
        buttons.Find("Navigation/Down").GetComponent<Button>().onClick.AddListener(() => Map.MoveCenter(new Vector2D(0, -100)));
        buttons.Find("Navigation/Left").GetComponent<Button>().onClick.AddListener(() => Map.MoveCenter(new Vector2D(-100, 0)));
        buttons.Find("Navigation/Right").GetComponent<Button>().onClick.AddListener(() => Map.MoveCenter(new Vector2D(100, 0)));

        // Zoom buttons
        buttons.Find("Zoom/In").GetComponent<Button>().onClick.AddListener(() => Map.Zoom(1));
        buttons.Find("Zoom/Out").GetComponent<Button>().onClick.AddListener(() => Map.Zoom(-1));

        // Other buttons
        buttons.Find("AR").GetComponent<Button>().onClick.AddListener(ToggleAR);
        buttons.Find("Test").GetComponent<Button>().onClick.AddListener(TestButtonClicked);
        buttons.Find("Query").GetComponent<Button>().onClick.AddListener(ToggleQuery);
        buttons.Find("Filter").GetComponent<Button>().onClick.AddListener(ToggleFilters);
        buttons.Find("Ruler").GetComponent<Button>().onClick.AddListener(() => Debug.Log("//TODO: Ruler"));
        buttons.Find("Terrain").GetComponent<Button>().onClick.AddListener(() => Debug.Log("//TODO: Terrain"));
    }

    Vector2D position;
    /// <summary>
    /// Sets up the location modal
    /// </summary>
    private void SetupLocationModal()
    {
        // Get the modal
        locationModal = modal.Find("Content/Location");

        // Show modal button
        buttons.Find("Location").GetComponent<Button>().onClick.AddListener(() => { ShowModal(locationModal, 500, 205); });

        // Latitude/Longitude input fields
        InputField latitudeInput = locationModal.Find("Position/Latitude").GetComponent<InputField>();
        latitudeInput.onEndEdit.AddListener((string value) =>
        {
            try
            {
                position.X = Convert.ToDouble(value);
            }
            catch
            {
                position.X = 0;
            }
            latitudeInput.text = $"{position.X}";
        });
        InputField longitudeInput = locationModal.Find("Position/Longitude").GetComponent<InputField>();
        longitudeInput.onEndEdit.AddListener((string value) =>
        {
            try
            {
                position.Y = Convert.ToDouble(value);
            }
            catch
            {
                position.Y = 0;
            }
            longitudeInput.text = $"{position.Y}";
        });

        // Point of Interest dropdown
        Dropdown poiDropdown = locationModal.Find("Point of Interest").GetComponent<Dropdown>();
        List<Dropdown.OptionData> poiOptions = new List<Dropdown.OptionData>();
        foreach (Map.PointOfInterest poi in Map.POIs)
        {
            poiOptions.Add(new Dropdown.OptionData(poi.Name));
        }
        poiOptions.Add(new Dropdown.OptionData(""));
        poiDropdown.options = poiOptions;
        poiDropdown.value = poiOptions.Count - 1;
        poiDropdown.onValueChanged.AddListener((int value) =>
        {
            if (value == poiDropdown.options.Count - 1)
            {
                position = Vector2D.Zero;
            }
            else
            {
                position = Map.POIs[value].Coordinates;
            }
            latitudeInput.text = $"{position.X}";
            longitudeInput.text = $"{position.Y}";
        });

        // Go button
        locationModal.Find("Go").GetComponent<Button>().onClick.AddListener(() => { Map.MoveCenter(position.X, position.Y); });
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        UpdateAverageFps();

        update += Time.unscaledDeltaTime;
        if (update > 1.0f)
        {
            // Only update debug text about once per second
            update = 0.0f;
            UpdateDebugTextDisplay();
        }

        ProcessInput();
    }

    /// <summary>
    /// Processes input events
    /// </summary>
    public void ProcessInput()
    {
        // Query mode input
        if (queryMode)
        {
            // User clicked/tapped
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.touchCount > 0)
                {
                    // Touch input
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        QueryMap(touch.position);
                    }
                }
                else
                {
                    // Mouse input
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        QueryMap(Input.mousePosition);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Shows the current modal
    /// </summary>
    /// <param name="modalContent">The modal content</param>
    private void ShowModal(Transform modalContent)
    {
        ShowModal(modalContent, 1250, 1000);
    }

    /// <summary>
    /// Shows the current modal with a specific width and height
    /// </summary>
    /// <param name="modalContent">The modal content</param>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    private void ShowModal(Transform modalContent, float width, float height)
    {
        modal.Find("Content").GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        currentModalContent = modalContent;
        modal.Find("Content/Header").GetComponent<Text>().text = currentModalContent.name;
        currentModalContent.gameObject.SetActive(true);
        modal.gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the current modal
    /// </summary>
    private void CloseModal()
    {
        modal.gameObject.SetActive(false);
        if (currentModalContent != null)
        {
            currentModalContent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles AR mode
    /// </summary>
    private void ToggleAR()
    {
        if (arMode)
        {
            Map.SwitchTo2DMode();
            Debug.Log("Switched to 2D mode");
        }
        else
        {
            Map.SwitchToARMode();
            Debug.Log("Switched to AR mode");
        }
        arMode = !arMode;
    }

    bool queryMode = false;
    /// <summary>
    /// Toggles query mode
    /// </summary>
    private void ToggleQuery()
    {
        buttons.Find("Query/Text").GetComponent<Text>().text = queryMode ? "Query: Off" : "Query: On";
        queryMode = !queryMode;
    }

    int currentCameraAngle = 0;
    private void TestButtonClicked()
    {
        Debug.Log("Test button clicked!");
        switch (currentCameraAngle)
        {
            case 0:
                Map.Test2DCamera(new Vector3(350, 20, 235), new Vector3(20, 325, 0));
                currentCameraAngle = 1;
                break;
            case 1:
                Map.Test2DCamera(new Vector3(420, 120, 125), new Vector3(30, 310, 0));
                currentCameraAngle = 2;
                break;
            case 2:
                Map.Test2DCamera(new Vector3(600, 270, -60), new Vector3(40, 310, 0));
                currentCameraAngle = 3;
                break;
            case 3:
                Map.Test2DCamera(new Vector3(350, 760, -980), new Vector3(35, 330, 0));
                currentCameraAngle = 0;
                break;
        }
    }

    bool filterMode = false;
    /// <summary>
    /// Toggle filters
    /// </summary>
    private void ToggleFilters()
    {
        filterMode = !filterMode;

        // Make the layers filter settings match the current state
        foreach (ILayer layer in Map.Layers.Values)
        {
            if (layer is IFilterableLayer)
            {
                IFilterableLayer filterableLayer = layer as IFilterableLayer;
                filterableLayer.Filtered = filterMode;
            }
        }

        // Apply the filters
        Map.ApplyFilters();

        // Update the button
        if (filterMode)
        {
            buttons.Find("Filter/Text").GetComponent<Text>().text = "Filters: On";
        }
        else
        {
            buttons.Find("Filter/Text").GetComponent<Text>().text = "Filters: Off";
        }
    }

    /// <summary>
    /// Queries the map from the a given screen point and 
    /// </summary>
    /// <param name="screenPoint">The screen point to cast the query ray from</param>
    private void QueryMap(Vector2 screenPoint)
    {
        // Get the query model
        Transform queryModal = modal.Find("Content/Query");

        // Cast the ray at the screen point
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Hit something
            Vector2D hitLatLon = Map.WorldToLatLon(hit.point);
            string queryInfo = $"<b>Coordinates:</b>\n\t<b>- Latitude:</b> {hitLatLon.X}\n\t<b>- Longitude:</b> {hitLatLon.Y}\n\t<b>- Height:</b> {Map.GetHeight(hit.point).ToString("0.00")} m";

            // Check if the hit object is part of a map feature
            FeatureBehaviour featureBehaviour = hit.transform.GetComponentInParent<FeatureBehaviour>();
            if (featureBehaviour == null)
            {
                // Not a map feature
                queryModal.Find("Info").GetComponent<Text>().text = queryInfo;
                queryModal.Find("Feature Properties").gameObject.SetActive(false);

                // Show the query modal
                ShowModal(queryModal, 750, 195);
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
                ShowModal(queryModal, 750, 500);
            }
        }
        else
        {
            // Didn't hit anything
            queryModal.Find("Info").GetComponent<Text>().text = "<b>Here be dragons!</b>\n\n<i>Nothing was hit by the raycast</i>";
            queryModal.Find("Feature Properties").gameObject.SetActive(false);

            // Show the query modal
            ShowModal(queryModal, 750, 170);
        }
    }

    /// <summary>
    /// Populate the list of map layers
    /// </summary>
    private void PopulateMapLayersList()
    {
        // Get the layer list entry prefab
        GameObject list = layersModal.transform.Find("List/Viewport/Content").gameObject;
        GameObject listEntryPrefab = Resources.Load<GameObject>("UI/Layers List Entry");

        foreach (ILayer layer in Map.Layers.Values)
        {
            // Instantiate the layer list entry prefabs as children of the layers list
            GameObject listEntryObj = GameObject.Instantiate(listEntryPrefab, list.transform);
            listEntryObj.name = layer.Id;
            listEntryObj.transform.Find("View Info/Label").GetComponent<Text>().text = layer.DisplayName;

            // Setup the visibility toggle
            Toggle toggle = listEntryObj.transform.Find("Visibility").GetComponent<Toggle>();
            toggle.isOn = layer.Visible; // Set the initial state of the toggle to the layer's visibility
            toggle.onValueChanged.AddListener((bool value) => ToggleLayerVisibility(layer, value));

            // Setup the view info button
            Button viewInfoButton = listEntryObj.transform.Find("View Info").GetComponent<Button>();
            viewInfoButton.onClick.AddListener(() => ViewLayerInfo(layer));
        }
    }

    /// <summary>
    /// Toggles the visibility of the given layer
    /// </summary>
    /// <param name="layer">The layer to toggle the visibility of</param>
    /// <param name="toggleIsOn">Whether the toggle is on or off</param>
    private void ToggleLayerVisibility(ILayer layer, bool toggleIsOn)
    {
        // When the toggle is toggled, set the layer's visibility to the new state
        layer.Visible = toggleIsOn;

        // Update the layer visibility for all the tiles already in the map
        foreach (Tile tile in Map.Tiles.Values)
        {
            tile.Layers[layer.Id].GameObject.SetActive(layer.Visible);
        }

        Debug.Log(toggleIsOn ? $"Enabled layer '{layer.Id}'" : $"Disabled layer '{layer.Id}'");
    }

    /// <summary>
    /// View the info of the given layer
    /// </summary>
    /// <param name="layer">The layer to view the info of</param>
    private void ViewLayerInfo(ILayer layer)
    {
        // Get the layer view
        GameObject layerView = layersModal.transform.Find("Layer").gameObject;
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
                    Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueInput.onEndEdit.AddListener((string value) =>
                        {
                            StringFeatureProperty updatedProperty = (StringFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.FilterValue = value.Trim();
                            filterValueInput.text = updatedProperty.FilterValue;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
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
                            Map.ApplyFilters(filterableLayer.Id);
                        });
                        filterValueDropdown.onValueChanged.AddListener((int value) =>
                        {
                            CategoryFeatureProperty updatedProperty = (CategoryFeatureProperty)filterableLayer.FeatureProperties[propertyIndex];
                            updatedProperty.FilterValue = value;
                            filterableLayer.FeatureProperties[propertyIndex] = updatedProperty;
                            Map.ApplyFilters(filterableLayer.Id);
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
    /// Updates log to render new messages
    /// </summary>
    private void UpdateLog(object sender, System.EventArgs e)
    {
        Logger.Render(log);
    }

    /// <summary>
    /// Updates the debug text display
    /// </summary>
    private void UpdateDebugTextDisplay()
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

        debugText.Append("\n\nAvailable semaphore threads: ");
        debugText.Append(MainController.networkSemaphore.CurrentCount);
        debugText.Append("\nTiles: ");
        debugText.Append(Map.Tiles.Count);

        debugTextDisplay.text = debugText.ToString();
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
    /// Updates the counters for the average FPS
    /// </summary>
    private void UpdateAverageFps()
    {
        totalFps += GetInstantFps();
        numFrames++;
    }

    /// <summary>
    /// Gets the average FPS since the last call to GetAverageFps()
    /// </summary>
    /// <returns>Average FPS</returns>
    private float GetAverageFps()
    {
        float averageFps = totalFps / numFrames;
        totalFps = 0;
        numFrames = 0;
        return averageFps;
    }
}