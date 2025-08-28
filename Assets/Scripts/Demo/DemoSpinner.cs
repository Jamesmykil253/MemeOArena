using UnityEngine;

namespace MOBA.Demo
{
    /// <summary>
    /// Simple component that makes objects spin for visual flair
    /// </summary>
    public class DemoSpinner : MonoBehaviour
    {
        [Header("Spin Settings")]
        [SerializeField] private Vector3 spinAxis = Vector3.up;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private bool randomizeSpeed = true;
        [SerializeField] private bool randomizeAxis = false;
        
        private Vector3 actualSpinAxis;
        private float actualSpinSpeed;
        
        void Start()
        {
            // Set up spin parameters
            actualSpinAxis = randomizeAxis ? Random.onUnitSphere : spinAxis.normalized;
            actualSpinSpeed = randomizeSpeed ? spinSpeed * Random.Range(0.5f, 1.5f) : spinSpeed;
            
            // Add some random variation to avoid all objects spinning in sync
            if (randomizeSpeed)
            {
                actualSpinSpeed *= Random.Range(0.8f, 1.2f);
            }
        }
        
        void Update()
        {
            // Rotate the object
            transform.Rotate(actualSpinAxis, actualSpinSpeed * Time.deltaTime, Space.World);
        }
        
        public void SetSpinSpeed(float newSpeed)
        {
            actualSpinSpeed = newSpeed;
        }
        
        public void SetSpinAxis(Vector3 newAxis)
        {
            actualSpinAxis = newAxis.normalized;
        }
    }
}
