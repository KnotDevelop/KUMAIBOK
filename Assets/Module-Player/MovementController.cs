using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace FpsGame.Player
{
    public class MovementController : MonoBehaviour
    {
        [SerializeField]
        private Transform m_Camera;
        private InputSystem m_Input;
        private RigidBodyHandler m_Rigidbody;

        [SerializeField]
        private float m_SmoothRotTime = 40f;

        [SerializeField]
        private float m_WalkSpeed = 3f;

        [SerializeField]
        private float m_SprintSpeed = 6f;
        private float m_StartSprintTime; //Timestamp

        [SerializeField]
        private float m_DurationCanSilde = 1f;

        [SerializeField]
        private float m_CrouchSpeed = 1.5f;

        [SerializeField]
        private float m_SlideSpeed = 15f;

        [SerializeField]
        private float m_StartYScale = 1f;

        [SerializeField]
        private float m_CrouchYScale = 0.5f;

        [SerializeField]
        private float m_CurrentSpeed;

        [SerializeField]
        private bool m_IsGround = false;
        public bool IsGround
        {
            get => m_IsGround;
            set => m_IsGround = value;
        }

        private enum PlayerAction
        {
            DEFAULT,
            SPRINT,
            CROUCH,
        }

        [SerializeField]
        private PlayerAction m_Action = PlayerAction.DEFAULT;
        private bool m_IsMove = false;
        Vector3 debug_velo = new Vector3();

        private void Start()
        {
            m_Input = GetComponent<InputSystem>();
            m_Rigidbody = GetComponent<RigidBodyHandler>();
            m_Rigidbody.RegisterVelocity(nameof(MovementController));
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            m_CurrentSpeed = m_WalkSpeed;
        }

        void Update()
        {
            HandleIsGround();
            HandleSprint();
            HandleCrouch();
            HandleCameraNoise();
            debug_velo = m_Rigidbody.GetVelocity(nameof(MovementController));
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleRotation();
            HandleMovementSpeed();
        }

        /// <summary>
        /// Checks if the player is on the ground using a Raycast to detect the "WhatIsGround" tag.
        /// </summary>
        public void HandleIsGround()
        {
            RaycastHit hit;
            m_IsGround = false;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
            {
                if (hit.collider.tag == "WhatIsGround")
                {
                    m_IsGround = true;
                }
            }
        }

        /// <summary>
        /// Handles player movement based on input direction and speed.
        /// </summary>
        private void HandleMovement()
        {
            Vector3 move =
                m_Camera.forward * m_Input.MoveInput.y + m_Camera.right * m_Input.MoveInput.x;
            move.y = 0;
            move.Normalize();

            if (move == Vector3.zero)
                m_IsMove = false;
            else
                m_IsMove = true;

            Vector3 targetVelocity = move * m_CurrentSpeed;
            m_Rigidbody.SetVelocity(
                nameof(MovementController),
                new Vector3(targetVelocity.x, 0, targetVelocity.z)
            );
        }

        /// <summary>
        /// Smoothly rotates the player to match the camera's forward direction.
        /// </summary>
        private void HandleRotation()
        {
            Vector3 forward = m_Camera.forward;
            forward.y = 0f;
            transform.forward = Vector3.Slerp(
                transform.forward,
                forward,
                Time.deltaTime * m_SmoothRotTime
            );
        }

        /// <summary>
        /// Limits player movement speed to prevent exceeding the set speed.
        /// </summary>
        private void HandleMovementSpeed()
        {
            Vector3 flatVel = m_Rigidbody.GetVelocity(nameof(MovementController));

            if (flatVel.sqrMagnitude > m_CurrentSpeed * m_CurrentSpeed)
            {
                Vector3 limitedFlatVel = flatVel.normalized * m_CurrentSpeed;
                m_Rigidbody.SetVelocity(
                    nameof(MovementController),
                    new Vector3(limitedFlatVel.x, 0, limitedFlatVel.z)
                );
            }
        }

        /// <summary>
        /// Adjusts camera shake intensity based on player movement state.
        /// </summary>
        private void HandleCameraNoise()
        {
            CinemachineBasicMultiChannelPerlin cinemachine =
                m_Camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (m_IsMove)
            {
                switch (m_Action)
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

        /// <summary>
        /// Handles sprinting, increasing speed when the sprint key is pressed.
        /// </summary>
        private void HandleSprint()
        {
            if (m_Action == PlayerAction.CROUCH)
                return;

            if (m_Input.InputAction.Player.Sprint.WasPerformedThisFrame())
            {
                m_CurrentSpeed = m_SprintSpeed;
                m_StartSprintTime = Time.time;
                m_Action = PlayerAction.SPRINT;
            }
            else if (m_Input.InputAction.Player.Sprint.WasReleasedThisFrame())
            {
                m_CurrentSpeed = m_WalkSpeed;
                m_Action = PlayerAction.DEFAULT;
            }
        }

        /// <summary>
        /// Checks if the player can slide based on the sprint duration.
        /// </summary>
        bool CanSlide()
        {
            return Time.time > m_StartSprintTime + m_DurationCanSilde;
        }

        /// <summary>
        /// Handles crouching by changing the player's scale and adjusting movement speed.
        /// </summary>
        private void HandleCrouch()
        {
            if (m_Input.InputAction.Player.Crouch.WasPerformedThisFrame())
            {
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    m_CrouchYScale,
                    transform.localScale.z
                );
                // m_Rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                if (m_Action == PlayerAction.SPRINT && CanSlide())
                {
                    StartCoroutine(Sliding());
                }
                else
                {
                    m_CurrentSpeed = m_CrouchSpeed;
                }
                m_Action = PlayerAction.CROUCH;
            }
            else if (m_Input.InputAction.Player.Crouch.WasReleasedThisFrame())
            {
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    m_StartYScale,
                    transform.localScale.z
                );
                m_CurrentSpeed = m_WalkSpeed;
                m_Action = PlayerAction.DEFAULT;
            }
        }

        /// <summary>
        /// Initiates a sliding motion for 1 second, gradually reducing speed to crouch speed.
        /// </summary>
        IEnumerator Sliding()
        {
            m_CurrentSpeed = m_SlideSpeed;
            float duration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                m_CurrentSpeed = Mathf.Lerp(m_SlideSpeed, m_CrouchSpeed, elapsedTime / duration);
                elapsedTime += Time.deltaTime;

                if (m_Input.InputAction.Player.Crouch.WasReleasedThisFrame())
                {
                    transform.localScale = new Vector3(
                        transform.localScale.x,
                        m_StartYScale,
                        transform.localScale.z
                    );
                    m_CurrentSpeed = m_WalkSpeed;
                    m_Action = PlayerAction.DEFAULT;
                    yield break;
                }

                if (!m_IsMove)
                    break;

                yield return null;
            }

            m_CurrentSpeed = m_CrouchSpeed;
        }
    }
}
