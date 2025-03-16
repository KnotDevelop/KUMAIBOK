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
        private Rigidbody m_Rigidbody;

        [SerializeField]
        private float m_SmoothRotTime = 40f;

        [SerializeField]
        private float walkSpeed = 3f;
        [SerializeField]
        private float sprintSpeed = 6f;
        float startSprintTime; //Timestamp
        [SerializeField]
        float durationCanSilde = 1f;
        [SerializeField]
        private float crouchSpeed = 1.5f;
        [SerializeField]
        private float slideSpeed = 15f;
        [SerializeField]
        private float startYScale = 1f;
        [SerializeField]
        private float crouchYScale = 0.5f;
        [SerializeField]
        private float currentSpeed;



        private enum PlayerAction { DEFAULT, SPRINT, CROUCH }
        [SerializeField]
        private PlayerAction action = PlayerAction.DEFAULT;
        private bool isMove = false;

        private void Start()
        {
            m_Input = GetComponent<InputSystem>();
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
        /// <summary>
        /// Handles player movement based on input direction and speed.
        /// </summary>
        private void HandleMovement()
        {
            Vector3 move = m_Camera.forward * m_Input.MoveInput.y + m_Camera.right * m_Input.MoveInput.x;
            move.y = 0;
            move.Normalize();

            if (move == Vector3.zero)
                isMove = false;
            else
                isMove = true;

            Vector3 targetVelocity = move * currentSpeed;
            m_Rigidbody.linearVelocity = new Vector3(targetVelocity.x, m_Rigidbody.linearVelocity.y, targetVelocity.z);
        }
        /// <summary>
        /// Smoothly rotates the player to match the camera's forward direction.
        /// </summary>
        private void HandleRotation()
        {
            Vector3 forward = m_Camera.forward;
            forward.y = 0f;
            transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * m_SmoothRotTime);
        }
        /// <summary>
        /// Limits player movement speed to prevent exceeding the set speed.
        /// </summary>
        private void HandleMovementSpeed()
        {
            Vector3 flatVel = new Vector3(m_Rigidbody.linearVelocity.x, 0, m_Rigidbody.linearVelocity.z);

            if (flatVel.sqrMagnitude > currentSpeed * currentSpeed)
            {
                Vector3 limitedFlatVel = flatVel.normalized * currentSpeed;
                m_Rigidbody.linearVelocity = new Vector3(limitedFlatVel.x, m_Rigidbody.linearVelocity.y, limitedFlatVel.z);
            }
        }
        /// <summary>
        /// Adjusts camera shake intensity based on player movement state.
        /// </summary>
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
        /// <summary>
        /// Handles sprinting, increasing speed when the sprint key is pressed.
        /// </summary>
        private void HandleSprint()
        {
            if (action == PlayerAction.CROUCH) return;

            if (m_Input.InputAction.Player.Sprint.WasPerformedThisFrame())
            {
                currentSpeed = sprintSpeed;
                startSprintTime = Time.time;
                action = PlayerAction.SPRINT;
            }
            else if (m_Input.InputAction.Player.Sprint.WasReleasedThisFrame())
            {
                currentSpeed = walkSpeed;
                action = PlayerAction.DEFAULT;
            }
        }
        bool CanSlide()
        {
            return Time.time > startSprintTime + durationCanSilde;
        }
        /// <summary>
        /// Handles crouching, adjusting player scale and speed accordingly.
        /// </summary>
        private void HandleCrouch()
        {
            if (m_Input.InputAction.Player.Crouch.WasPerformedThisFrame())
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                m_Rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                if (action == PlayerAction.SPRINT && CanSlide())
                {
                    StartCoroutine(Sliding());
                }
                else
                {
                    currentSpeed = crouchSpeed;
                }
                action = PlayerAction.CROUCH;
            }
            else if (m_Input.InputAction.Player.Crouch.WasReleasedThisFrame())
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                currentSpeed = walkSpeed;
                action = PlayerAction.DEFAULT;
            }
        }
        IEnumerator Sliding()
        {
            currentSpeed = slideSpeed;
            float duration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                currentSpeed = Mathf.Lerp(slideSpeed, crouchSpeed, elapsedTime / duration);
                elapsedTime += Time.deltaTime;

                if (m_Input.InputAction.Player.Crouch.WasReleasedThisFrame())
                {
                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                    currentSpeed = walkSpeed;
                    action = PlayerAction.DEFAULT;
                    yield break;
                }

                if (!isMove) break;

                yield return null;
            }

            currentSpeed = crouchSpeed;
        }
    }
}
