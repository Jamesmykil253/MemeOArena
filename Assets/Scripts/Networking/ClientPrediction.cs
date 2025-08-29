using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;

namespace MOBA.Networking
{
    /// <summary>
    /// DEPRECATED - Client prediction system removed as part of networking cleanup.
    /// This file remains for reference but is no longer functional.
    /// The demo now uses simplified direct input without client-server prediction.
    /// </summary>
    public class ClientPrediction : MonoBehaviour
    {
        [Header("Prediction Settings - DEPRECATED")]
        #pragma warning disable CS0414 // Field assigned but never used - deprecated component
        [SerializeField] private int maxPredictionFrames = 60;
        [SerializeField] private float correctionThreshold = 0.1f;
        [SerializeField] private float smoothingSpeed = 10f;
        #pragma warning restore CS0414
        
        private void Awake()
        {
            Debug.LogWarning("ClientPrediction is deprecated and disabled. Remove this component for optimal performance.");
            enabled = false;
        }
    }
}
