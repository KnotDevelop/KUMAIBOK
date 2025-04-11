using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class RigidBodyHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rigidBody;
    private bool m_IsGround;
    public Vector3 CurrentSpeed
    {
        get => rigidBody.linearVelocity;
    }
    public bool doGravity
    {
        get => rigidBody.useGravity;
        set => rigidBody.useGravity = value;
    }

    void Update()
    {
        HandleIsGround();
    }

    public void AddForceClamp(Vector3 velocity, float maxSpeed)
    {
        var velocityOnPlane = Vector3.Project(rigidBody.linearVelocity, velocity.normalized);
        if (velocityOnPlane.magnitude < maxSpeed)
        {
            rigidBody.AddForce(
                velocity.normalized * (maxSpeed - velocityOnPlane.magnitude),
                ForceMode.VelocityChange
            );
        }
    }

    public void AddForce(Vector3 force)
    {
        rigidBody.AddForce(force);
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
            // if (hit.collider.tag == "WhatIsGround")
            // {
            //     m_IsGround = true;
            // }
            m_IsGround = true;
            rigidBody.linearDamping = 5;
        }
        else
        {
            m_IsGround = false;
            rigidBody.linearDamping = 0.1f;
        }
    }
}
