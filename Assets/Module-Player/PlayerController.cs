using UnityEngine;

namespace FpsGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        bool isGround = false;

        [SerializeField]
        private MovementController movement;
        public bool IsGround
        {
            get => isGround;
            set => isGround = value;
        }

        void Update()
        {
            HandleIsGround();
        }

        private void LateUpdate()
        {
            if (isGround)
                movement.HandleMovement();
        }

        private void FixedUpdate()
        {
            movement.HandleRotation();
        }

        public void HandleIsGround()
        {
            RaycastHit hit;
            isGround = false;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
            {
                if (hit.collider.tag == "WhatIsGround")
                {
                    isGround = true;
                }
            }
        }
    }
}
