using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    [Serializable]
    public class MotionTransition : ClipTransition
    {
        [SerializeField,Tooltip("Should Root Motion be enabled when this animation plays?")]
        private bool _ApplyRootMotion;

        /// <inheritdoc/>
        public bool ApplyRootMotion
        {
            get => _ApplyRootMotion;
            set => _ApplyRootMotion = value;
        } 
        /************************************************************************************************************************/

        public override void Apply(AnimancerState state)
        {
            base.Apply(state);
            state.Root.Component.Animator.applyRootMotion = _ApplyRootMotion;
        }
    }

    
}
