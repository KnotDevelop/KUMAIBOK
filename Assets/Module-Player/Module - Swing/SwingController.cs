using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwingController : MonoBehaviour
{
    const int lineResolution = 10;

    [SerializeField]
    Transform startPoint;

    [SerializeField]
    AnimationCurve flickerCurve;

    [SerializeField]
    LineRenderer line;

    [SerializeField]
    InputAction shoot;

    [SerializeField]
    InputAction jumpboost;
    Coroutine instanceCoroutine;
    List<Coroutine> dotInstancesCoroutine = new List<Coroutine>();

    [SerializeField]
    [Range(0, 3)]
    float speed = 2;
    Vector3 swingPoint;

    [SerializeField]
    float upForce = 5;

    [SerializeField]
    float boostForce = 30;

    [SerializeField]
    Rigidbody rb;

    void Start()
    {
        shoot.Enable();
        jumpboost.Enable();
    }

    void Update()
    {
        if (shoot.WasPressedThisFrame())
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
            instanceCoroutine = StartCoroutine(FlickerLine());
            JumpToSwingPoint();
        }

        if (shoot.IsPressed())
        {
            BoostToSwingPoint();
        }
        if (shoot.WasReleasedThisFrame())
        {
            line.positionCount = 1;
        }
        line.SetPosition(0, startPoint.position);
    }

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

    private void BoostToSwingPoint()
    {
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized * upForce;
        rb.AddForce(displacement * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void JumpToSwingPoint()
    {
        Vector3 displacement = swingPoint - transform.position;
        displacement = displacement.normalized;
        rb.AddForce(displacement * boostForce, ForceMode.Impulse);
    }
}
