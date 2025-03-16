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
    Rigidbody rigidBody;

    void Start()
    {
        shoot.Enable();
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
    }

    /// <summary>
    /// Flicker all dots in line at the start of swing for aesthetic purpose.
    /// </summary>
    /// <returns></returns>
    IEnumerator FlickerLine()
    {
        line.positionCount = lineResolution;
        for (int i = 0; i < lineResolution; i++)
        {
            line.SetPosition(i, transform.position);
        }
        Vector3 center = Vector2.zero;
        center.y = Screen.height / 2;
        center.x = Screen.width / 2;
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
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized * upBoostForce;
        rigidBody.AddForce(displacement * Time.deltaTime, ForceMode.VelocityChange);
    }

    /// <summary>
    /// single Jump (appllied force) to at the start of swing for boosting purpose.
    /// </summary>
    private void JumpToSwingPoint()
    {
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized;
        rigidBody.AddForce(displacement * boostForce, ForceMode.Impulse);
    }
}
