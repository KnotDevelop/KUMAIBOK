using UnityEngine;

namespace FpsGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        bool isGround = false;
        void Update()
        {
            HandleIsGround();
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
