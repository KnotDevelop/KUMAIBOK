using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsGame.Player
{
    public class PlayerInputSystem : MonoBehaviour
    {
        private PlayerInputAction inputAction;
        [SerializeField] Vector2 lookInput;
        [SerializeField] Vector2 moveInput;
        public Vector2 LookInput { get => lookInput; private set => lookInput = value; }
        public Vector2 MoveInput { get => moveInput; private set => moveInput = value; }
        public PlayerInputAction InputAction { get => inputAction; private set => inputAction = value; }

        private void Awake()
        {
            InputAction = new PlayerInputAction();
        }
        private void OnEnable()
        {
            InputAction.Player.Enable();
            InputAction.Player.Look.performed += OnLook;
            InputAction.Player.Look.canceled += OnLook;
            InputAction.Player.Move.performed += OnMove;
            InputAction.Player.Move.canceled += OnMove;
        }
        private void OnDisable()
        {
            InputAction.Player.Look.performed -= OnLook;
            InputAction.Player.Look.canceled -= OnLook;
            InputAction.Player.Move.performed += OnMove;
            InputAction.Player.Move.canceled += OnMove;
            InputAction.Disable();
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