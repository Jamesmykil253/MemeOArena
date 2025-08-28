using UnityEngine;

namespace MOBA.Demo
{
    /// <summary>
    /// Simple demo target that can be used for testing abilities and interactions
    /// </summary>
    public class DemoTarget : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private Color highlightColor = Color.red;
        
        private Material originalMaterial;
        private Renderer targetRenderer;
        private bool isHighlighted = false;
        
        void Start()
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                originalMaterial = targetRenderer.material;
            }
        }
        
        void OnMouseEnter()
        {
            if (!isHighlighted && targetRenderer != null)
            {
                Material highlightMat = new Material(originalMaterial);
                highlightMat.color = highlightColor;
                targetRenderer.material = highlightMat;
                isHighlighted = true;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Target {name} highlighted");
                }
            }
        }
        
        void OnMouseExit()
        {
            if (isHighlighted && targetRenderer != null)
            {
                targetRenderer.material = originalMaterial;
                isHighlighted = false;
            }
        }
        
        void OnMouseDown()
        {
            if (showDebugInfo)
            {
                Debug.Log($"Target {name} clicked! This could trigger an ability or interaction.");
            }
            
            // Add visual feedback
            StartCoroutine(ClickFeedback());
        }
        
        private System.Collections.IEnumerator ClickFeedback()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;
            
            // Scale up
            float time = 0f;
            while (time < 0.1f)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, time / 0.1f);
                time += Time.deltaTime;
                yield return null;
            }
            
            // Scale back down
            time = 0f;
            while (time < 0.1f)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, time / 0.1f);
                time += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
    }
}
