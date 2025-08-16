using UnityEngine;
using System;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Gameplay
{
    [Serializable]
    public class LootItem
    {
        public string itemId;
        public string itemName;
        public LootType type;
        public int quantity;
        public float rarity; // 0-1, gdzie 1 = bardzo rzadkie
    }

    public enum LootType
    {
        Currency,
        VehiclePart,
        Customization,
        Boost,
        Experience
    }

    public class LootPickup : MonoBehaviour
    {
        [Header("Loot Configuration")]
        [SerializeField] private LootItem[] _possibleLoot;
        [SerializeField] private int _minItems = 1;
        [SerializeField] private int _maxItems = 3;
        [SerializeField] private bool _randomizeLoot = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject _pickupEffect;
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.5f;

        [Header("Respawn")]
        [SerializeField] private bool _canRespawn = true;
        [SerializeField] private float _respawnTime = 300f; // 5 minut

        private Vector3 _startPosition;
        private bool _isCollected = false;
        private Renderer[] _renderers;
        private Collider _collider;

        public event Action<LootItem[]> OnLootCollected;

        private void Start()
        {
            _startPosition = transform.position;
            _renderers = GetComponentsInChildren<Renderer>();
            _collider = GetComponent<Collider>();

            // Randomizuj loot jeśli potrzeba
            if (_randomizeLoot && _possibleLoot.Length == 0)
            {
                GenerateRandomLoot();
            }
        }

        private void Update()
        {
            if (!_isCollected)
            {
                // Obróć skrzynkę
                transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0);
                
                // Bobbing effect
                float newY = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            // Sprawdź czy to gracz
            if (other.CompareTag("Player") || other.GetComponent<ExtremeRacing.Vehicles.VehicleController>())
            {
                CollectLoot();
            }
        }

        private void CollectLoot()
        {
            if (_isCollected) return;

            _isCollected = true;

            // Wygeneruj loot
            LootItem[] collectedLoot = GenerateLootReward();

            // Powiadom o zebraniu
            OnLootCollected?.Invoke(collectedLoot);

            // Dodaj do ekonomii gracza
            AddLootToPlayer(collectedLoot);

            // Efekty wizualne i dźwiękowe
            PlayPickupEffects();

            // Ukryj skrzynkę
            SetVisible(false);

            // Zaplanuj respawn jeśli możliwy
            if (_canRespawn)
            {
                Invoke(nameof(Respawn), _respawnTime);
            }
            else
            {
                Destroy(gameObject, 1f);
            }

            Debug.Log($"[LootPickup] Collected {collectedLoot.Length} items from loot crate");
        }

        private LootItem[] GenerateLootReward()
        {
            if (_possibleLoot.Length == 0)
            {
                return new LootItem[0];
            }

            int itemCount = UnityEngine.Random.Range(_minItems, _maxItems + 1);
            LootItem[] reward = new LootItem[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                // Wybierz losowy item z uwzględnieniem rzadkości
                LootItem selectedItem = SelectRandomLoot();
                
                // Sklonuj item żeby móc zmienić quantity
                reward[i] = new LootItem
                {
                    itemId = selectedItem.itemId,
                    itemName = selectedItem.itemName,
                    type = selectedItem.type,
                    quantity = selectedItem.quantity + UnityEngine.Random.Range(0, selectedItem.quantity),
                    rarity = selectedItem.rarity
                };
            }

            return reward;
        }

        private LootItem SelectRandomLoot()
        {
            // Weighted random selection based on rarity (lower rarity = higher chance)
            float totalWeight = 0f;
            foreach (var item in _possibleLoot)
            {
                totalWeight += (1f - item.rarity) + 0.1f; // +0.1 żeby bardzo rzadkie miały szansę
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in _possibleLoot)
            {
                currentWeight += (1f - item.rarity) + 0.1f;
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }

            return _possibleLoot[0]; // Fallback
        }

        private void GenerateRandomLoot()
        {
            // Generuj podstawowe typy loot na podstawie regionu lub poziomu gracza
            _possibleLoot = new LootItem[]
            {
                new LootItem { itemId = "credits", itemName = "Credits", type = LootType.Currency, quantity = 100, rarity = 0.1f },
                new LootItem { itemId = "engine_part", itemName = "Engine Part", type = LootType.VehiclePart, quantity = 1, rarity = 0.7f },
                new LootItem { itemId = "paint_job", itemName = "Custom Paint", type = LootType.Customization, quantity = 1, rarity = 0.5f },
                new LootItem { itemId = "nitro_boost", itemName = "Nitro Boost", type = LootType.Boost, quantity = 3, rarity = 0.3f },
                new LootItem { itemId = "experience", itemName = "Experience Points", type = LootType.Experience, quantity = 50, rarity = 0.2f }
            };
        }

        private void AddLootToPlayer(LootItem[] loot)
        {
            var economyManager = FindObjectOfType<EconomyManager>();
            if (economyManager != null)
            {
                foreach (var item in loot)
                {
                    switch (item.type)
                    {
                        case LootType.Currency:
                            // economyManager.AddCurrency(item.quantity);
                            break;
                        case LootType.Experience:
                            // economyManager.AddExperience(item.quantity);
                            break;
                        case LootType.VehiclePart:
                        case LootType.Customization:
                        case LootType.Boost:
                            // economyManager.AddItem(item);
                            break;
                    }
                }
            }

            // Pokaż UI notification
            ShowLootNotification(loot);
        }

        private void ShowLootNotification(LootItem[] loot)
        {
            // To będzie implementowane przez UI system
            foreach (var item in loot)
            {
                Debug.Log($"[Loot] Collected: {item.itemName} x{item.quantity}");
            }
        }

        private void PlayPickupEffects()
        {
            // Particle effect
            if (_pickupEffect != null)
            {
                Instantiate(_pickupEffect, transform.position, Quaternion.identity);
            }

            // Sound effect
            if (_pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);
            }
        }

        private void SetVisible(bool visible)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                    renderer.enabled = visible;
            }

            if (_collider != null)
                _collider.enabled = visible;
        }

        private void Respawn()
        {
            _isCollected = false;
            SetVisible(true);
            transform.position = _startPosition;

            // Może zregenerować loot
            if (_randomizeLoot)
            {
                GenerateRandomLoot();
            }

            Debug.Log("[LootPickup] Loot crate respawned");
        }

        // Editor methods
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
            
            if (_canRespawn)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
            }
        }
    }
}