using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class RigidBodyHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rigidBody;
    private Dictionary<string, Vector3> velocityList = new Dictionary<string, Vector3>();
    private Vector3 gravityVelocity;
    public bool doGravity = true;
    private Vector3 _combineVelocity;
    private bool m_IsGround;

    public Vector3 CombineVelocity
    {
        get { return _combineVelocity; }
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        rigidBody.useGravity = false;
    }

    void Update()
    {
        HandleIsGround();
        RecalculateCombinedVelocity();
        ApplyGravity();
        ApplyVelocity();
    }

    void ApplyGravity()
    {
        if (doGravity && !m_IsGround)
        {
            gravityVelocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            if (m_IsGround)
                gravityVelocity.y = 0;
        }
    }

    void ApplyVelocity()
    {
        rigidBody.linearVelocity = CombineVelocity;
    }

    public Vector3 RecalculateCombinedVelocity()
    {
        const float e = 2.7182818284590f;
        _combineVelocity = Vector3.zero;
        List<string> allKey = velocityList.Keys.ToList();
        foreach (var v in allKey)
        {
            _combineVelocity += velocityList[v];
            velocityList[v] =
                velocityList[v] * math.pow(e, -rigidBody.linearDamping * Time.deltaTime);
        }
        _combineVelocity += gravityVelocity;
        return _combineVelocity;
    }

    public void RegisterVelocity(string key)
    {
        if (!velocityList.TryAdd(key, new Vector3()))
        {
            Debug.LogError($"string key : {key} : is duplicated. ignoring process");
        }
    }

    public void SetVelocity(string key, Vector3 velocity)
    {
        if (velocityList.ContainsKey(key))
        {
            velocityList[key] = velocity;
        }
    }

    public void AddVelocity(string key, Vector3 velocity)
    {
        if (velocityList.ContainsKey(key))
        {
            velocityList[key] += velocity * Time.deltaTime;
        }
    }

    public void Clamp(string key, float min, float max)
    {
        if (velocityList.ContainsKey(key))
        {
            var v = velocityList[key];
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);
            velocityList[key] = v;
        }
    }

    public Vector3 GetVelocity(string key)
    {
        if (velocityList.TryGetValue(key, out var value))
        {
            return value;
        }
        else
        {
            Debug.LogError($"string key : {key} : not found. ignoring process");
            return Vector3.zero;
        }
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
            if (hit.collider.tag == "WhatIsGround")
            {
                m_IsGround = true;
                rigidBody.linearDamping = 5;
            }
            else
                rigidBody.linearDamping = 0.5f;
        }
        else
            rigidBody.linearDamping = 0.5f;
    }

    void OnCollisionEnter(Collision c)
    {
        List<string> allKey = velocityList.Keys.ToList();
        foreach (var v in allKey)
        {
            velocityList[v] = Vector3.Reflect(velocityList[v], c.contacts[0].normal) * 0.3f;
        }
    }
}
