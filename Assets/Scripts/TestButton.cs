using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public DebugUIController debugUI;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    private void ButtonClicked() => debugUI.Log("The Test Button has been pressed!");
}