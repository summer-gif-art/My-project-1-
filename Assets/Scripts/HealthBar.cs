using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health; // The Health component we track (player or enemy)
    [SerializeField] private Slider slider; // The UI Slider that shows the value

    private void Start()
    {
        // If not set in Inspector, try to find the Slider on this GameObject
        if (slider == null)
            slider = GetComponent<Slider>();

        if (health == null || slider == null)
        {
            Debug.LogWarning("HealthBar: Missing Health or Slider reference.");
            return;
        }

        // Initialize slider values
        slider.maxValue = health.CurrentHealth;
        slider.value = health.CurrentHealth;

        // Subscribe to health changes
        health.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int newHealth)
    {
        // Update the slider when health changes
        slider.value = newHealth;
    }
}