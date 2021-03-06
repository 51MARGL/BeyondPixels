﻿using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.Utilities
{
    public static class Utilities
    {
        /// <summary>
        /// Custom extension for Animator
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="layerName"></param>
        public static void ActivateLayer(this Animator animator, string layerName)
        {
            if (animator.GetLayerIndex(layerName) == -1)
                return;

            for (var i = 0; i < animator.layerCount; i++)
                animator.SetLayerWeight(i, 0);

            animator.SetLayerWeight(animator.GetLayerIndex(layerName), 1);
        }
    }

    [DisableAutoCreation]
    public class FixedUpdateSystemGroup : ComponentSystemGroup { }

}
