using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsGame.Player
{
    public class PlayerInputSystem : MonoBehaviour
    {
        PlayerInputAction inputAction;
        [SerializeField] Vector2 lookInput;
        [SerializeField] Vector2 moveInput;
        public Vector2 LookInput { get => lookInput; private set => lookInput = value; }
        public Vector2 MoveInput { get => moveInput; private set => moveInput = value; }

        private void Awake()
        {
            inputAction = new PlayerInputAction();
        }
        private void OnEnable()
        {
            inputAction.Player.Enable();
            inputAction.Player.Look.performed += OnLook;
            inputAction.Player.Look.canceled += OnLook;
            inputAction.Player.Move.performed += OnMove;
            inputAction.Player.Move.canceled += OnMove;
        }
        private void OnDisable()
        {
            inputAction.Player.Look.performed -= OnLook;
            inputAction.Player.Look.canceled -= OnLook;
            inputAction.Player.Move.performed += OnMove;
            inputAction.Player.Move.canceled += OnMove;
            inputAction.Disable();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }
}