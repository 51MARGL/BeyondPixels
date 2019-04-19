using Cinemachine;

using UnityEngine;

namespace BeyondPixels.Utilities
{
    /// <summary>
    /// An add-on module for Cinemachine Virtual Camera that locks the camera's Z co-ordinate 
    /// </summary>
    [ExecuteInEditMode]
    [SaveDuringPlay]
    [AddComponentMenu("")] // Hide in menu
    public class CinemachineCustomCameraExtension : CinemachineExtension
    {
        [Tooltip("Lock the camera's Z position to this value")]
        public float m_ZPosition = -9;
        [Tooltip("Lock the camera's X rotation to this value")]
        public float m_XRotation = -15;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (this.enabled && stage == CinemachineCore.Stage.Body)
            {
                var pos = state.RawPosition;
                var rot = state.RawOrientation;
                pos = new Vector3(pos.x, pos.y, this.m_ZPosition);
                rot = Quaternion.Euler(this.m_XRotation, 0, 0);
                state.RawPosition = pos;
                state.RawOrientation = rot;
            }
        }
    }
}
