using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public DebugUIController debugUI;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    class Enemy
    {
        public string Name { get; set; }
        public int AttackDamage { get; set; }
        public int MaxHealth { get; set; }
    }

    private void ButtonClicked()
    {
        string json = @"{
            'Name': 'Ninja',
            'AttackDamage': '40'
            }";

        var enemy = JsonConvert.DeserializeObject<Enemy>(json);

        debugUI.Log("The Test Button has been pressed!");

        debugUI.Log($"{enemy.Name} deals {enemy.AttackDamage} damage.");
    }
}