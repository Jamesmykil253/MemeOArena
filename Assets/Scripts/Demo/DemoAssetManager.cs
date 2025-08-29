using UnityEngine;
using System.Collections.Generic;

namespace MOBA.Demo
{
    /// <summary>
    /// Manages demo assets including materials and prefabs.
    /// Creates and assigns proper materials to demo objects automatically.
    /// </summary>
    public class DemoAssetManager : MonoBehaviour
    {
        [Header("Demo Materials")]
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private Material obstacleMaterial;
        [SerializeField] private Material pickupMaterial;
        
        [Header("Demo Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private GameObject pickupPrefab;
        
        [Header("Auto-Create Materials")]
        [SerializeField] private bool createMaterialsOnStart = true;
        
        private static DemoAssetManager instance;
        public static DemoAssetManager Instance => instance;
        
        // Material cache
        private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (createMaterialsOnStart)
            {
                CreateAllMaterials();
            }
        }
        
        /// <summary>
        /// Create all demo materials if they don't exist
        /// </summary>
        [ContextMenu("Create All Materials")]
        public void CreateAllMaterials()
        {
            Debug.Log("🎨 Creating demo materials...");
            
            // Create standard materials
            playerMaterial = GetOrCreateMaterial("Player", new Color(0.2f, 0.6f, 1.0f), 0.3f, 0.7f);
            groundMaterial = GetOrCreateMaterial("Ground", new Color(0.2f, 0.8f, 0.3f), 0.0f, 0.1f);
            enemyMaterial = GetOrCreateMaterial("Enemy", new Color(1.0f, 0.3f, 0.2f), 0.2f, 0.6f);
            obstacleMaterial = GetOrCreateMaterial("Obstacle", new Color(0.6f, 0.6f, 0.6f), 0.8f, 0.9f);
            pickupMaterial = GetOrCreateMaterial("Pickup", new Color(1.0f, 0.8f, 0.0f), 0.7f, 1.0f);
            
            Debug.Log("✅ Demo materials created!");
        }
        
        /// <summary>
        /// Get or create a material with specified properties
        /// </summary>
        private Material GetOrCreateMaterial(string name, Color color, float metallic = 0.0f, float smoothness = 0.5f)
        {
            string key = name.ToLower();
            
            if (materialCache.ContainsKey(key))
            {
                return materialCache[key];
            }
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = $"Demo_{name}_Material";
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            
            // Add some emission for pickups
            if (name == "Pickup")
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.2f);
            }
            
            materialCache[key] = mat;
            Debug.Log($"Created material: {name} - {color}");
            
            return mat;
        }
        
        /// <summary>
        /// Get a material by name
        /// </summary>
        public Material GetMaterial(string materialName)
        {
            string key = materialName.ToLower();
            
            if (materialCache.ContainsKey(key))
            {
                return materialCache[key];
            }
            
            // Try to find existing material first
            switch (key)
            {
                case "player": return playerMaterial ?? GetOrCreateMaterial("Player", new Color(0.2f, 0.6f, 1.0f));
                case "ground": return groundMaterial ?? GetOrCreateMaterial("Ground", new Color(0.2f, 0.8f, 0.3f));
                case "enemy": return enemyMaterial ?? GetOrCreateMaterial("Enemy", new Color(1.0f, 0.3f, 0.2f));
                case "obstacle": return obstacleMaterial ?? GetOrCreateMaterial("Obstacle", new Color(0.6f, 0.6f, 0.6f));
                case "pickup": return pickupMaterial ?? GetOrCreateMaterial("Pickup", new Color(1.0f, 0.8f, 0.0f));
                default:
                    Debug.LogWarning($"Unknown material: {materialName}. Creating default.");
                    return GetOrCreateMaterial(materialName, Color.white);
            }
        }
        
        /// <summary>
        /// Create a player GameObject with proper visual
        /// </summary>
        public GameObject CreatePlayer(Vector3 position = default)
        {
            GameObject playerObj;
            
            if (playerPrefab != null)
            {
                playerObj = Instantiate(playerPrefab, position, Quaternion.identity);
                Debug.Log("✓ Created player from prefab");
            }
            else
            {
                // Create from scratch
                playerObj = new GameObject("Demo Player");
                playerObj.transform.position = position;
                playerObj.tag = "Player";
                
                // Add visual representation
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.SetParent(playerObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(1f, 1.2f, 1f);
                visual.name = "Player Visual";
                
                // Apply material
                Renderer renderer = visual.GetComponent<Renderer>();
                renderer.material = GetMaterial("Player");
                
                // Remove collider from visual
                DestroyImmediate(visual.GetComponent<CapsuleCollider>());
                
                // Add controller
                playerObj.AddComponent<DemoPlayerController>();
                
                Debug.Log("✓ Created player from components with material");
            }
            
            return playerObj;
        }
        
        /// <summary>
        /// Create ground plane with proper material
        /// </summary>
        public GameObject CreateGround(Vector3 position = default, Vector3 scale = default)
        {
            if (scale == default) scale = new Vector3(10f, 1f, 10f);
            
            GameObject ground;
            
            if (groundPrefab != null)
            {
                ground = Instantiate(groundPrefab, position, Quaternion.identity);
                ground.transform.localScale = scale;
            }
            else
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Demo Ground";
                ground.transform.position = position;
                ground.transform.localScale = scale;
                
                // Apply material
                Renderer renderer = ground.GetComponent<Renderer>();
                renderer.material = GetMaterial("Ground");
            }
            
            Debug.Log("✓ Created ground with material");
            return ground;
        }
        
        /// <summary>
        /// Create obstacle with proper material
        /// </summary>
        public GameObject CreateObstacle(Vector3 position, Vector3 scale = default)
        {
            if (scale == default) scale = Vector3.one;
            
            GameObject obstacle;
            
            if (obstaclePrefab != null)
            {
                obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
                obstacle.transform.localScale = scale;
            }
            else
            {
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = "Demo Obstacle";
                obstacle.transform.position = position;
                obstacle.transform.localScale = scale;
                
                // Apply material
                Renderer renderer = obstacle.GetComponent<Renderer>();
                renderer.material = GetMaterial("Obstacle");
            }
            
            return obstacle;
        }
        
        /// <summary>
        /// Create pickup with proper material and effects
        /// </summary>
        public GameObject CreatePickup(Vector3 position)
        {
            GameObject pickup;
            
            if (pickupPrefab != null)
            {
                pickup = Instantiate(pickupPrefab, position, Quaternion.identity);
            }
            else
            {
                pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = "Demo Pickup";
                pickup.transform.position = position;
                pickup.transform.localScale = Vector3.one * 0.5f;
                
                // Apply material
                Renderer renderer = pickup.GetComponent<Renderer>();
                renderer.material = GetMaterial("Pickup");
                
                // Add spinner for visual appeal
                pickup.AddComponent<DemoSpinner>();
            }
            
            return pickup;
        }
        
        /// <summary>
        /// Fix any existing objects missing materials
        /// </summary>
        [ContextMenu("Fix All Missing Materials")]
        public void FixAllMissingMaterials()
        {
            Debug.Log("🔧 Fixing missing materials...");
            
            // Fix players
            DemoPlayerController[] players = FindObjectsByType<DemoPlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                FixPlayerMaterial(player.gameObject);
            }
            
            // Fix ground planes
            GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
            foreach (var ground in grounds)
            {
                FixGroundMaterial(ground);
            }
            
            // Fix any pink/missing materials
            Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var renderer in allRenderers)
            {
                if (renderer.material.name.Contains("Default-Material") || 
                    renderer.material.color == Color.magenta)
                {
                    // Try to determine appropriate material based on object name
                    string objName = renderer.gameObject.name.ToLower();
                    if (objName.Contains("player"))
                        renderer.material = GetMaterial("Player");
                    else if (objName.Contains("ground"))
                        renderer.material = GetMaterial("Ground");
                    else if (objName.Contains("obstacle"))
                        renderer.material = GetMaterial("Obstacle");
                    else if (objName.Contains("pickup"))
                        renderer.material = GetMaterial("Pickup");
                    else
                        renderer.material = GetMaterial("Player"); // Default to player material
                        
                    Debug.Log($"Fixed material for: {renderer.gameObject.name}");
                }
            }
            
            Debug.Log("✅ Material fixing complete!");
        }
        
        private void FixPlayerMaterial(GameObject player)
        {
            Renderer renderer = player.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = GetMaterial("Player");
                Debug.Log($"Fixed player material: {player.name}");
            }
        }
        
        private void FixGroundMaterial(GameObject ground)
        {
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = GetMaterial("Ground");
                Debug.Log($"Fixed ground material: {ground.name}");
            }
        }
        
        void OnGUI()
        {
            // Quick access buttons in-game
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Demo Assets</b>");
            
            if (GUILayout.Button("Create Materials"))
            {
                CreateAllMaterials();
            }
            
            if (GUILayout.Button("Fix Missing Materials"))
            {
                FixAllMissingMaterials();
            }
            
            if (GUILayout.Button("Create Test Player"))
            {
                Vector3 pos = UnityEngine.Camera.main ? UnityEngine.Camera.main.transform.position + UnityEngine.Camera.main.transform.forward * 3f : Vector3.zero;
                CreatePlayer(pos);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
