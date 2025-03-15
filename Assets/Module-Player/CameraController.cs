using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsGame.Player
{
    public class PlayerCameraSystem : MonoBehaviour
    {
        [SerializeField]
        private Transform m_Camera;

        [SerializeField]
        private float m_Sensitivity;

        PlayerInputSystem m_Input;

        private float m_Pitch;

        private Vector3 m_LerpRef;

        [SerializeField]
        private float m_SmoothTime = 10f;

        private void Start()
        {
            m_Input = GetComponent<PlayerInputSystem>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            Vector3 forward = m_Camera.forward;
            forward.y = 0f;
            transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * m_SmoothTime);
        }
    }
}
