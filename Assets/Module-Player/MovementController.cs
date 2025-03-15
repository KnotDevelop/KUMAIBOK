using Unity.Cinemachine;
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
        private float walkSpeed = 3f;
        [SerializeField]
        private float sprintSpeed = 6f;
        [SerializeField]
        private float crouchSpeed = 1.5f;
        [SerializeField]
        private float startYScale = 1f;
        [SerializeField]
        private float crouchYScale = 0.5f;
        private float currentSpeed;

        [SerializeField]
        private float m_SmoothTime = 40f;

        enum PlayerAction { DEFAULT, SPRINT, CROUCH }
        [SerializeField]
        private PlayerAction action = PlayerAction.DEFAULT;
        bool isMove = false;

        private void Start()
        {
            m_Input = GetComponent<PlayerInputSystem>();
            m_Rigidbody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currentSpeed = walkSpeed;
        }
        void Update()
        {
            HandleSprint();
            HandleCrouch();
            HandleCameraNoise();
        }
        private void FixedUpdate()
        {
            HandleMovement();
            HandleRotation();
            HandleMovementSpeed();
        }

        private void HandleMovement()
        {
            Vector3 move = m_Camera.forward * m_Input.MoveInput.y + m_Camera.right * m_Input.MoveInput.x;
            move.y = 0;
            move.Normalize();

            if (move == Vector3.zero)
                isMove = false;
            else
                isMove = true;

            // กำหนด velocity โดยตรงเพื่อให้ความเร็วสม่ำเสมอ
            Vector3 targetVelocity = move * currentSpeed;
            m_Rigidbody.linearVelocity = new Vector3(targetVelocity.x, m_Rigidbody.linearVelocity.y, targetVelocity.z);
        }

        private void HandleRotation()
        {
            Vector3 forward = m_Camera.forward;
            forward.y = 0f;
            transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * m_SmoothTime);
        }

        private void HandleMovementSpeed()
        {
            Vector3 flatVel = new Vector3(m_Rigidbody.linearVelocity.x, 0, m_Rigidbody.linearVelocity.z);

            if (flatVel.sqrMagnitude > currentSpeed * currentSpeed)
            {
                Vector3 limitedFlatVel = flatVel.normalized * currentSpeed;
                m_Rigidbody.linearVelocity = new Vector3(limitedFlatVel.x, m_Rigidbody.linearVelocity.y, limitedFlatVel.z);
            }
        }
        private void HandleCameraNoise()
        {
            CinemachineBasicMultiChannelPerlin cinemachine = m_Camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (isMove)
            {
                switch (action)
                {
                    case PlayerAction.DEFAULT:
                        cinemachine.FrequencyGain = 2f;
                        break;
                    case PlayerAction.SPRINT:
                        cinemachine.FrequencyGain = 4f;
                        break;
                    case PlayerAction.CROUCH:
                        cinemachine.FrequencyGain = 1f;
                        break;
                }
            }
            else
            {
                cinemachine.FrequencyGain = 1f;
            }
        }
        private void HandleSprint()
        {
            if (action == PlayerAction.CROUCH) return;

            if (m_Input.InputAction.Player.Sprint.WasPerformedThisFrame())
            {
                currentSpeed = sprintSpeed;

                action = PlayerAction.SPRINT;
            }
            else if (m_Input.InputAction.Player.Sprint.WasReleasedThisFrame())
            {
                currentSpeed = walkSpeed;
                action = PlayerAction.DEFAULT;
            }
        }

        private void HandleCrouch()
        {
            if (action == PlayerAction.SPRINT) return;

            if (m_Input.InputAction.Player.Crouch.WasPerformedThisFrame())
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                m_Rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                currentSpeed = crouchSpeed;
                action = PlayerAction.CROUCH;
            }
            else if (m_Input.InputAction.Player.Crouch.WasReleasedThisFrame())
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                currentSpeed = walkSpeed;
                action = PlayerAction.DEFAULT;
            }
        }
    }
}
