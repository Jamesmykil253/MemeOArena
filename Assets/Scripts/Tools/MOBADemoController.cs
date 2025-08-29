using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Core;
using MOBA.Input;

namespace MOBA.Tools
{
    /// <summary>
    /// Interactive MOBA demonstration controller aligned with TDD/GDD documentation.
    /// Provides comprehensive showcase of all core MOBA systems and mechanics.
    /// </summary>
    public class MOBADemoController : MonoBehaviour
    {
        [Header("MOBA Demo Configuration")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoSpeed = 1f;
        [SerializeField] private bool verboseLogging = true;
        
        [Header("ScriptableObject Assets")]
        [SerializeField] private BaseStatsTemplate baseStats;
        [SerializeField] private JumpPhysicsDef jumpPhysics;
        [SerializeField] private ScoringDef scoringDef;
        [SerializeField] private UltimateEnergyDef ultimateEnergyDef;
        [SerializeField] private AbilityDef basicAttack;
        [SerializeField] private AbilityDef ultimateBlast;
        
        [Header("Scene References")]
        [SerializeField] private Transform[] orbSpawnPoints;
        [SerializeField] private Transform[] scoringPads;
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Demo Statistics")]
        [SerializeField] private int orbsSpawned = 0;
        [SerializeField] private int orbsCollected = 0;
        [SerializeField] private int enemiesDefeated = 0;
        [SerializeField] private float totalDamageDealt = 0f;
        [SerializeField] private float ultimateEnergyGenerated = 0f;
        
        // Demo state
        private GameObject player;
        private UnifiedLocomotionController locomotionController;
        private bool isDemoRunning = false;
        private float nextOrbSpawn = 0f;
        private float nextEnemySpawn = 0f;
        private List<GameObject> spawnedOrbs = new List<GameObject>();
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        
        // UI Elements
        private Canvas demoCanvas;
        private Text statsText;
        
        void Start()
        {
            InitializeDemo();
            if (autoStartDemo)
            {
                StartDemo();
            }
        }
        
        void Update()
        {
            if (isDemoRunning)
            {
                UpdateDemo();
                UpdateUI();
            }
            
            // Demo control keys using proper Input System
            if (Keyboard.current != null)
            {
                if (Keyboard.current[Key.F1].wasPressedThisFrame)
                {
                    ToggleDemo();
                }
                if (Keyboard.current[Key.F2].wasPressedThisFrame)
                {
                    SpawnOrb();
                }
                if (Keyboard.current[Key.F3].wasPressedThisFrame)
                {
                    SpawnEnemy();
                }
                if (Keyboard.current[Key.F4].wasPressedThisFrame)
                {
                    DemonstrateUltimate();
                }
            }
        }
        
        /// <summary>
        /// Initialize demo components and validate scene setup
        /// </summary>
        private void InitializeDemo()
        {
            // Find player
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[MOBADemo] No player found! Please ensure scene has player with 'Player' tag");
                return;
            }
            
            locomotionController = player.GetComponent<UnifiedLocomotionController>();
            if (locomotionController == null)
            {
                Debug.LogError("[MOBADemo] Player missing UnifiedLocomotionController!");
                return;
            }
            
            // Find or create orb spawn points
            if (orbSpawnPoints == null || orbSpawnPoints.Length == 0)
            {
                CreateOrbSpawnPoints();
            }
            
            // Find or create scoring pads
            if (scoringPads == null || scoringPads.Length == 0)
            {
                CreateScoringPads();
            }
            
            // Create demo UI
            CreateDemoUI();
            
            // Validate ScriptableObject assets
            ValidateAssets();
            
            if (verboseLogging)
            {
                Debug.Log("[MOBADemo] Demo initialized successfully!");
                Debug.Log("[MOBADemo] Controls: F1=Toggle Demo, F2=Spawn Orb, F3=Spawn Enemy, F4=Ultimate Demo");
            }
        }
        
        /// <summary>
        /// Create orb spawn points if none exist
        /// </summary>
        private void CreateOrbSpawnPoints()
        {
            GameObject orbSpawnContainer = new GameObject("OrbSpawnPoints");
            orbSpawnContainer.transform.SetParent(transform);
            
            // Create spawn points in a circle around player
            int spawnCount = 8;
            float radius = 15f;
            List<Transform> spawns = new List<Transform>();
            
            for (int i = 0; i < spawnCount; i++)
            {
                float angle = (i * 360f / spawnCount) * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
                
                GameObject spawn = new GameObject($"OrbSpawn_{i}");
                spawn.transform.SetParent(orbSpawnContainer.transform);
                spawn.transform.position = position;
                spawn.tag = "OrbSpawn";
                spawns.Add(spawn.transform);
                
                // Add visual indicator
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator.transform.SetParent(spawn.transform);
                indicator.transform.localPosition = Vector3.zero;
                indicator.transform.localScale = Vector3.one * 0.5f;
                indicator.GetComponent<Renderer>().material.color = Color.yellow;
                indicator.name = "SpawnIndicator";
            }
            
            orbSpawnPoints = spawns.ToArray();
            
            if (verboseLogging)
            {
                Debug.Log($"[MOBADemo] Created {spawnCount} orb spawn points");
            }
        }
        
        /// <summary>
        /// Create scoring pads if none exist
        /// </summary>
        private void CreateScoringPads()
        {
            GameObject scoringContainer = new GameObject("ScoringPads");
            scoringContainer.transform.SetParent(transform);
            
            List<Transform> pads = new List<Transform>();
            
            // Create team scoring pads
            Vector3[] padPositions = {
                new Vector3(-20f, 0f, 0f), // Team 1 pad
                new Vector3(20f, 0f, 0f)   // Team 2 pad
            };
            
            for (int i = 0; i < padPositions.Length; i++)
            {
                GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name = $"ScoringPad_Team{i + 1}";
                pad.transform.SetParent(scoringContainer.transform);
                pad.transform.position = padPositions[i];
                pad.transform.localScale = new Vector3(4f, 0.2f, 4f);
                pad.tag = "ScoringPad";
                
                // Color coding
                pad.GetComponent<Renderer>().material.color = i == 0 ? Color.blue : Color.red;
                
                pads.Add(pad.transform);
            }
            
            scoringPads = pads.ToArray();
            
            if (verboseLogging)
            {
                Debug.Log($"[MOBADemo] Created {pads.Count} scoring pads");
            }
        }
        
        /// <summary>
        /// Create demo UI for statistics and controls
        /// </summary>
        private void CreateDemoUI()
        {
            // Create canvas
            GameObject canvasGO = new GameObject("MOBADemoCanvas");
            demoCanvas = canvasGO.AddComponent<Canvas>();
            demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            demoCanvas.sortingOrder = 100;
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create stats panel
            GameObject statsPanel = new GameObject("StatsPanel");
            statsPanel.transform.SetParent(demoCanvas.transform, false);
            
            RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 1);
            statsRect.anchorMax = new Vector2(0, 1);
            statsRect.anchoredPosition = new Vector2(10, -10);
            statsRect.sizeDelta = new Vector2(300, 200);
            
            Image statsBackground = statsPanel.AddComponent<Image>();
            statsBackground.color = new Color(0, 0, 0, 0.7f);
            
            // Create stats text
            GameObject statsTextGO = new GameObject("StatsText");
            statsTextGO.transform.SetParent(statsPanel.transform, false);
            
            RectTransform textRect = statsTextGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            statsText = statsTextGO.AddComponent<Text>();
            statsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statsText.fontSize = 12;
            statsText.color = Color.white;
            statsText.alignment = TextAnchor.UpperLeft;
        }
        
        /// <summary>
        /// Validate that required ScriptableObject assets are assigned
        /// </summary>
        private void ValidateAssets()
        {
            int missingAssets = 0;
            
            if (baseStats == null)
            {
                Debug.LogWarning("[MOBADemo] BaseStatsTemplate not assigned!");
                missingAssets++;
            }
            
            if (jumpPhysics == null)
            {
                Debug.LogWarning("[MOBADemo] JumpPhysicsDef not assigned!");
                missingAssets++;
            }
            
            if (scoringDef == null)
            {
                Debug.LogWarning("[MOBADemo] ScoringDef not assigned!");
                missingAssets++;
            }
            
            if (ultimateEnergyDef == null)
            {
                Debug.LogWarning("[MOBADemo] UltimateEnergyDef not assigned!");
                missingAssets++;
            }
            
            if (basicAttack == null)
            {
                Debug.LogWarning("[MOBADemo] Basic Attack AbilityDef not assigned!");
                missingAssets++;
            }
            
            if (ultimateBlast == null)
            {
                Debug.LogWarning("[MOBADemo] Ultimate Blast AbilityDef not assigned!");
                missingAssets++;
            }
            
            if (missingAssets > 0)
            {
                Debug.LogError($"[MOBADemo] {missingAssets} ScriptableObject assets missing! Please assign them in the inspector.");
            }
            else
            {
                Debug.Log("[MOBADemo] All ScriptableObject assets validated successfully!");
            }
        }
        
        /// <summary>
        /// Start the interactive MOBA demo
        /// </summary>
        public void StartDemo()
        {
            isDemoRunning = true;
            nextOrbSpawn = Time.time + 2f;
            nextEnemySpawn = Time.time + 5f;
            
            if (verboseLogging)
            {
                Debug.Log("[MOBADemo] Demo started! Demonstrating MOBA core loop: Farm → Fight → Score → Regroup");
            }
        }
        
        /// <summary>
        /// Stop the demo and clean up
        /// </summary>
        public void StopDemo()
        {
            isDemoRunning = false;
            
            // Clean up spawned objects
            foreach (GameObject orb in spawnedOrbs)
            {
                if (orb != null) DestroyImmediate(orb);
            }
            spawnedOrbs.Clear();
            
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null) DestroyImmediate(enemy);
            }
            spawnedEnemies.Clear();
            
            if (verboseLogging)
            {
                Debug.Log("[MOBADemo] Demo stopped and cleaned up");
            }
        }
        
        /// <summary>
        /// Toggle demo on/off
        /// </summary>
        public void ToggleDemo()
        {
            if (isDemoRunning)
            {
                StopDemo();
            }
            else
            {
                StartDemo();
            }
        }
        
        /// <summary>
        /// Update demo systems
        /// </summary>
        private void UpdateDemo()
        {
            // Spawn orbs periodically
            if (Time.time >= nextOrbSpawn && orbSpawnPoints.Length > 0)
            {
                SpawnOrb();
                nextOrbSpawn = Time.time + (3f / demoSpeed);
            }
            
            // Spawn enemies periodically
            if (Time.time >= nextEnemySpawn && spawnedEnemies.Count < 3)
            {
                SpawnEnemy();
                nextEnemySpawn = Time.time + (8f / demoSpeed);
            }
            
            // Clean up null references
            spawnedOrbs.RemoveAll(orb => orb == null);
            spawnedEnemies.RemoveAll(enemy => enemy == null);
        }
        
        /// <summary>
        /// Spawn an orb for collection
        /// </summary>
        public void SpawnOrb()
        {
            if (orbSpawnPoints == null || orbSpawnPoints.Length == 0) return;
            
            Transform spawnPoint = orbSpawnPoints[Random.Range(0, orbSpawnPoints.Length)];
            
            // Create orb
            GameObject orb = orbPrefab != null ? 
                Instantiate(orbPrefab, spawnPoint.position + Vector3.up, Quaternion.identity) :
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            if (orbPrefab == null)
            {
                // Configure default orb
                orb.name = "CollectableOrb";
                orb.transform.localScale = Vector3.one * 0.8f;
                orb.GetComponent<Renderer>().material.color = Color.cyan;
                orb.AddComponent<Rigidbody>().isKinematic = true;
                
                // Add collection trigger
                SphereCollider trigger = orb.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 1.5f;
            }
            
            spawnedOrbs.Add(orb);
            orbsSpawned++;
            
            if (verboseLogging)
            {
                Debug.Log($"[MOBADemo] Spawned orb #{orbsSpawned} at {spawnPoint.name}");
            }
        }
        
        /// <summary>
        /// Spawn an enemy for combat demonstration
        /// </summary>
        public void SpawnEnemy()
        {
            if (player == null) return;
            
            // Spawn enemy near player but not too close
            Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 8f;
            spawnPos.y = 0f;
            
            GameObject enemy = enemyPrefab != null ?
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity) :
                GameObject.CreatePrimitive(PrimitiveType.Capsule);
            
            if (enemyPrefab == null)
            {
                // Configure default enemy
                enemy.name = "TestEnemy";
                enemy.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
                enemy.GetComponent<Renderer>().material.color = Color.red;
                enemy.tag = "Enemy";
                
                // Add health component (simplified)
                var healthComponent = enemy.AddComponent<BasicHealthComponent>();
                healthComponent.maxHealth = 100f;
                healthComponent.currentHealth = 100f;
            }
            
            spawnedEnemies.Add(enemy);
            
            if (verboseLogging)
            {
                Debug.Log($"[MOBADemo] Spawned enemy at {spawnPos} for combat demonstration");
            }
        }
        
        /// <summary>
        /// Demonstrate ultimate ability with dramatic effects
        /// </summary>
        public void DemonstrateUltimate()
        {
            if (player == null || ultimateBlast == null) return;
            
            // Create ultimate effect
            GameObject ultimateEffect = new GameObject("UltimateDemo");
            ultimateEffect.transform.position = player.transform.position;
            
            // Add particle system for dramatic effect
            ParticleSystem particles = ultimateEffect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 10f;
            main.startSize = 2f;
            main.startColor = Color.yellow;
            main.maxParticles = 100;
            
            // Calculate RSB damage
            if (baseStats != null)
            {
                float rawDamage = ultimateBlast.Ratio * baseStats.Attack + 
                                 ultimateBlast.Slider * 1f + // Assume level 1
                                 ultimateBlast.Base;
                totalDamageDealt += rawDamage;
                ultimateEnergyGenerated += ultimateEnergyDef != null ? ultimateEnergyDef.scoreDepositEnergy : 25f;
                
                if (verboseLogging)
                {
                    Debug.Log($"[MOBADemo] Ultimate Blast! RSB Damage: {rawDamage:F1} ({ultimateBlast.Ratio}×{baseStats.Attack} + {ultimateBlast.Slider}×1 + {ultimateBlast.Base})");
                }
            }
            
            // Clean up after 3 seconds
            Destroy(ultimateEffect, 3f);
        }
        
        /// <summary>
        /// Update demo UI with current statistics
        /// </summary>
        private void UpdateUI()
        {
            if (statsText == null) return;
            
            string stats = $"=== MOBA DEMO STATISTICS ===\n";
            stats += $"Demo Status: {(isDemoRunning ? "RUNNING" : "STOPPED")}\n";
            stats += $"Demo Speed: {demoSpeed:F1}x\n\n";
            
            stats += $"--- FARM PHASE ---\n";
            stats += $"Orbs Spawned: {orbsSpawned}\n";
            stats += $"Orbs Collected: {orbsCollected}\n";
            stats += $"Active Orbs: {spawnedOrbs.Count}\n\n";
            
            stats += $"--- FIGHT PHASE ---\n";
            stats += $"Enemies Spawned: {spawnedEnemies.Count}\n";
            stats += $"Enemies Defeated: {enemiesDefeated}\n";
            stats += $"Total Damage: {totalDamageDealt:F1}\n\n";
            
            stats += $"--- ENERGY SYSTEM ---\n";
            stats += $"Ultimate Energy: {ultimateEnergyGenerated:F1}\n";
            stats += $"Energy Requirement: {(ultimateEnergyDef != null ? ultimateEnergyDef.energyRequirement : 85):F1}\n\n";
            
            stats += $"--- CONTROLS ---\n";
            stats += $"F1: Toggle Demo\n";
            stats += $"F2: Spawn Orb\n";
            stats += $"F3: Spawn Enemy\n";
            stats += $"F4: Ultimate Demo";
            
            statsText.text = stats;
        }
        
        /// <summary>
        /// Context menu method for manual demo start
        /// </summary>
        [ContextMenu("Start MOBA Demo")]
        public void StartDemoFromMenu()
        {
            StartDemo();
        }
        
        /// <summary>
        /// Context menu method for manual demo stop
        /// </summary>
        [ContextMenu("Stop MOBA Demo")]
        public void StopDemoFromMenu()
        {
            StopDemo();
        }
        
        /// <summary>
        /// Context menu method to validate all systems
        /// </summary>
        [ContextMenu("Validate MOBA Systems")]
        public void ValidateMOBASystems()
        {
            ValidateAssets();
            
            if (verboseLogging)
            {
                Debug.Log("[MOBADemo] MOBA systems validation complete!");
            }
        }
    }
    
    /// <summary>
    /// Simple health component for enemy demonstration
    /// </summary>
    public class BasicHealthComponent : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        public void Die()
        {
            Debug.Log($"[MOBADemo] {gameObject.name} defeated!");
            Destroy(gameObject);
        }
    }
}
