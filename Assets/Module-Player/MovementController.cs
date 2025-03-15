using FpsGame.Player;
using UnityEngine;

namespace FpsGame.Player
{
    public class MovementController : MonoBehaviour
    {
        [SerializeField]
        private Transform m_Camera;
        private PlayerInputSystem m_Input;
        private Rigidbody m_Rigidbody;

        [SerializeField]
        private float speed = 5f;

        [SerializeField]
        private float m_SmoothTime = 40f;

        private void Start()
        {
            m_Input = GetComponent<PlayerInputSystem>();
            m_Rigidbody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            HandleMovement();
        }

        private void FixedUpdate()
        {
            HandleRotation();
        }

        private void HandleMovement()
        {
            Vector3 move =
                m_Camera.forward * m_Input.MoveInput.y + m_Camera.right * m_Input.MoveInput.x;
            move.y = 0;
            m_Rigidbody.AddForce(move.normalized * speed, ForceMode.VelocityChange);
        }

        private void HandleRotation()
        {
            Vector3 forward = m_Camera.forward;
            forward.y = 0f;
            transform.forward = Vector3.Slerp(
                transform.forward,
                forward,
                Time.deltaTime * m_SmoothTime
            );
        }
    }
}
