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
    private Coroutine instanceCoroutine;
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

    void Start()
    {
        shoot.Enable();
        control.Enable();
        rigidBody.RegisterVelocity("JumpSwing");
        rigidBody.RegisterVelocity("BoostSwing");
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
            instanceCoroutine = StartCoroutine(FlickerLine());
            JumpToSwingPoint();
        }
        if (shoot.IsPressed())
        {
            BoostToSwingPoint();
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
        if (instanceCoroutine != null)
        {
            StopCoroutine(instanceCoroutine);
        }
        foreach (var d in dotInstancesCoroutine)
        {
            StopCoroutine(d);
        }
        dotInstancesCoroutine.Clear();
        line.positionCount = 1;
        rigidBody.doGravity = true;
    }

    /// <summary>
    /// Flicker all dots in line at the start of swing for aesthetic purpose.
    /// </summary>
    /// <returns></returns>
    IEnumerator FlickerLine()
    {
        rigidBody.doGravity = false;

        line.positionCount = lineResolution;
        for (int i = 0; i < lineResolution; i++)
        {
            line.SetPosition(i, transform.position);
        }
        Vector3 center = Vector2.zero;
        center.y = Screen.height / 2;
        center.x = Screen.width / 2 + Screen.width * offsetPercentX;
        Ray r = Camera.main.ScreenPointToRay(center);

        if (Physics.Raycast(r, out var hit, 200))
        {
            swingPoint = hit.point;
            for (int i = lineResolution; i > 0; i--)
            {
                dotInstancesCoroutine.Add(StartCoroutine(FlickerSingleDot(i, hit.point)));
                yield return new WaitForSeconds(0.02f);
            }
        }
        instanceCoroutine = null;
    }

    /// <summary>
    /// Flicker the dot in line to create aesthetic purpose.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    IEnumerator FlickerSingleDot(int i, Vector3 endPoint)
    {
        float lerp = 0;
        while (lerp < 1)
        {
            lerp += Time.deltaTime * speed;
            var p = Vector3.Lerp(startPoint.position, endPoint, lerp);
            p.y += flickerCurve.Evaluate(lerp);
            line.SetPosition(i, p);
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Continous applied force during swing to the target position.
    /// </summary>
    private void BoostToSwingPoint()
    {
        var c = control.ReadValue<float>();
        Vector3 toCenterForce = swingPoint - transform.position;
        toCenterForce = toCenterForce.normalized * upBoostForce;
        rigidBody.AddVelocity("BoostSwing", toCenterForce );
        rigidBody.Clamp("BoostSwing", -maxSpeed, maxSpeed);
    }

    /// <summary>
    /// single Jump (appllied force) to at the start of swing for boosting purpose.
    /// </summary>
    private void JumpToSwingPoint()
    {
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized;
        rigidBody.SetVelocity("JumpSwing", displacement * boostForce);
    }
}
