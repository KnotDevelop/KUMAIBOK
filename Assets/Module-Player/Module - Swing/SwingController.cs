using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwingController : MonoBehaviour
{
    /// <summary>
    /// The amount of dot in line renderer (positionCount)
    /// </summary>
    const int lineResolution = 10;

    /// <summary>
    /// Start position of the line
    /// </summary>
    [SerializeField]
    Transform startPoint;

    /// <summary>
    /// Amount of jittering at the beginning of Swing
    /// </summary>
    [SerializeField]
    AnimationCurve flickerCurve;

    /// <summary>
    /// Line renderer component in Unity
    /// </summary>
    [SerializeField]
    LineRenderer line;

    /// <summary>
    /// Input for Shooting swing line
    /// </summary>
    [SerializeField]
    InputAction shoot;

    [SerializeField]
    InputAction control;

    [SerializeField]
    float maxSpeed = 10;
    private List<Coroutine> dotInstancesCoroutine = new List<Coroutine>();

    /// <summary>
    /// Speed of shooting of swing line.
    /// </summary>
    [SerializeField]
    [Range(0, 3)]
    float speed = 2;

    /// <summary>
    /// the current swinging point (target position)
    /// </summary>
    Vector3 swingPoint;

    /// <summary>
    /// force added of the starting of swing. Jump Boost
    /// </summary>
    [SerializeField]
    float upBoostForce = 5;

    /// <summary>
    /// boosting force applied continously during swing.
    /// </summary>
    [SerializeField]
    float boostForce = 30;

    /// <summary>
    /// rigidbody unity Component
    /// </summary>
    [SerializeField]
    RigidBodyHandler rigidBody;

    [SerializeField]
    private float offsetPercentX;

    SpringJoint joint;

    void Start()
    {
        shoot.Enable();
        control.Enable();
    }

    void Update()
    {
        InputHandler();
        UpdateCurrentPosition();
    }

    /// <summary>
    /// Apply event on each input.
    /// </summary>
    private void InputHandler()
    {
        if (shoot.WasPressedThisFrame())
        {
            ResetSwing();
            FlickerLine();
            JumpToSwingPoint();
        }

        if (shoot.WasReleasedThisFrame())
        {
            ResetSwing();
        }
    }

    /// <summary>
    /// Update the first dot on line renderer to current position
    /// </summary>
    void UpdateCurrentPosition()
    {
        line.SetPosition(0, startPoint.position);
    }

    /// <summary>
    /// Reset the swing coroutine.
    /// </summary>
    private void ResetSwing()
    {
        if (joint)
            Destroy(joint);

        foreach (var d in dotInstancesCoroutine)
        {
            if (d == null)
                continue;
            StopCoroutine(d);
        }
        dotInstancesCoroutine.Clear();
        line.positionCount = 1;
    }

    /// <summary>
    /// Flicker all dots in line at the start of swing for aesthetic purpose.
    /// </summary>
    /// <returns></returns>
    void FlickerLine()
    {
        const float maxDist = 300;
        const float maxDistSwing = 8;
        Vector3 center = Vector2.zero;
        center.y = Screen.height / 2;
        center.x = (Screen.width / 2);
        Ray r = Camera.main.ScreenPointToRay(center);

        if (Physics.Raycast(r, out var hit, maxDist))
        {
            line.positionCount = 2;
            joint = rigidBody.gameObject.AddComponent<SpringJoint>();
            joint.connectedAnchor = hit.point;
            joint.spring = 100;
            joint.damper = 1;
            joint.minDistance = 2;
            joint.maxDistance = hit.distance * 0.9f;
            joint.autoConfigureConnectedAnchor = false;
            swingPoint = hit.point;
            line.SetPosition(1, hit.point);
        }
    }

    /// <summary>
    /// single Jump (appllied force) to at the start of swing for boosting purpose.
    /// </summary>
    private void JumpToSwingPoint()
    {
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized;
        rigidBody.AddForceClamp(displacement * boostForce, boostForce);
    }
}
