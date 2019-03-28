using Cinemachine;
using UnityEngine;

namespace BeyondPixels.Utilities
{
    /// <summary>
    /// An add-on module for Cinemachine Virtual Camera that locks the camera's Z co-ordinate 
    /// and Rounds position to PixelPerfect
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
        [Tooltip("PPU value")]
        public float m_PixelsPerUnit = 32;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (enabled && stage == CinemachineCore.Stage.Body)
            {
                var pos = state.RawPosition;
                var rot = state.RawOrientation;
                pos = new Vector3(Round(pos.x), Round(pos.y), this.m_ZPosition);
                rot = Quaternion.Euler(m_XRotation, 0, 0);
                state.RawPosition = pos;
                state.RawOrientation = rot;
            }
        }

        private float Round(float x)
        {
            return Mathf.Round(x * m_PixelsPerUnit) / m_PixelsPerUnit;
        }
    }
}
