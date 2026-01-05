using System;
 using UnityEngine;
 
 public class Health : MonoBehaviour
 {
     [Header("Health Settings")]
     [SerializeField] private int maxHealth = 100;
 
     // Current health value
     private int _currentHealth;
 
     // Public read-only properties
     public int CurrentHealth => _currentHealth;
     public bool IsDead => _currentHealth <= 0;
 
     // Events so other scripts can react (enemy controller, game manager, UI etc.)
     public event Action<int> OnHealthChanged; // passes new health value
     public event Action OnDeath;
     public event Action OnDamaged;
 
     private void Awake()
     {
         // Initialize health to max on start
         _currentHealth = maxHealth;
         LogHealthToConsole();
     }
 
    
     // Apply damage to this object.
 
     // ReSharper disable Unity.PerformanceAnalysis
     public void TakeDamage(int damageAmount)
     {
         if (IsDead) return; // if already dead do nothing
 
         // Make sure damage is not negative
         damageAmount = Mathf.Max(0, damageAmount);
 
         // Reduce health (never below 0)
         _currentHealth = Mathf.Max(0, _currentHealth - damageAmount);
 
         // Log to console (required by assignment)
         LogHealthToConsole();
 
         // Notify listeners that it took damage
         OnDamaged?.Invoke();
         OnHealthChanged?.Invoke(_currentHealth);
 
         if (_currentHealth <= 0)
         {
             _currentHealth = 0;
             OnDeath?.Invoke();
         }
     }
 
     private void LogHealthToConsole()
     {
         // Requirement: print health whenever it changes
         Debug.Log($"{gameObject.name} health = {_currentHealth}/{maxHealth}");
     }
 }