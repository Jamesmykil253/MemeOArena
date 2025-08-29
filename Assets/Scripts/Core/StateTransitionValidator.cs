using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// Enhanced state transition validator that checks preconditions before allowing state changes.
    /// Provides comprehensive validation, logging, and telemetry for FSM transitions.
    /// </summary>
    public class StateTransitionValidator
    {
        public delegate bool ValidationRule(IState from, IState to, string reason);
        
        private readonly Dictionary<Type, Dictionary<Type, ValidationRule>> transitionRules = new();
        private readonly List<string> validationErrors = new();
        
        /// <summary>
        /// Add a validation rule for transitions from one state type to another.
        /// </summary>
        public void AddRule<TFrom, TTo>(ValidationRule rule)
            where TFrom : IState where TTo : IState
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);
            
            if (!transitionRules.ContainsKey(fromType))
                transitionRules[fromType] = new Dictionary<Type, ValidationRule>();
            
            transitionRules[fromType][toType] = rule;
        }
        
        /// <summary>
        /// Validate a proposed state transition.
        /// </summary>
        public bool ValidateTransition(IState from, IState to, string reason, out string errorMessage)
        {
            validationErrors.Clear();
            errorMessage = null;
            
            if (from == null && to == null)
            {
                errorMessage = "Both from and to states are null";
                return false;
            }
            
            if (to == null)
            {
                errorMessage = "Target state cannot be null";
                return false;
            }
            
            // Allow initial state transitions (from null)
            if (from == null)
                return true;
            
            var fromType = from.GetType();
            var toType = to.GetType();
            
            // Check if we have specific rules for this transition
            if (transitionRules.ContainsKey(fromType) && 
                transitionRules[fromType].ContainsKey(toType))
            {
                var rule = transitionRules[fromType][toType];
                if (!rule(from, to, reason))
                {
                    errorMessage = $"Validation rule failed for transition {fromType.Name} -> {toType.Name}";
                    return false;
                }
            }
            
            // Check general validation rules
            if (!ValidateGeneralRules(from, to, reason))
            {
                errorMessage = string.Join("; ", validationErrors);
                return false;
            }
            
            return true;
        }
        
        private bool ValidateGeneralRules(IState from, IState to, string reason)
        {
            bool isValid = true;
            
            // Rule 1: Cannot transition to the same state instance
            if (ReferenceEquals(from, to))
            {
                validationErrors.Add("Cannot transition to the same state instance");
                isValid = false;
            }
            
            // Rule 2: Critical transitions should have reasons
            if (IsCriticalTransition(from, to) && string.IsNullOrEmpty(reason))
            {
                validationErrors.Add($"Critical transition {from.GetType().Name} -> {to.GetType().Name} requires a reason");
                isValid = false;
            }
            
            return isValid;
        }
        
        private bool IsCriticalTransition(IState from, IState to)
        {
            // Define critical transitions that always require reasons
            var criticalStateNames = new HashSet<string>
            {
                "ErrorState", "DisabledState", "KnockbackState", 
                "InterruptedState", "FailedState"
            };
            
            return criticalStateNames.Contains(to.GetType().Name) ||
                   criticalStateNames.Contains(from.GetType().Name);
        }
    }
}
