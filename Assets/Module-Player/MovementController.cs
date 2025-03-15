using FpsGame.Player;
using UnityEngine;

namespace FpsGame.Player.Camera
{
    public class MovementController : MonoBehaviour
    {
        [SerializeField] Transform m_Camera;
        PlayerInputSystem m_Input;
        Rigidbody m_Rigidbody;
        [SerializeField] float speed = 5f;

        private void Start()
        {
            m_Input = GetComponent<PlayerInputSystem>();
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            Vector3 move = m_Camera.forward * m_Input.MoveInput.y + m_Camera.right * m_Input.MoveInput.x;
            move.y = 0;
            m_Rigidbody.AddForce(move.normalized * speed, ForceMode.VelocityChange);
        }
    }
}
