using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform followTarget;
    [Range(0.0f, 1.0f)] public float followSpeed = 0.125f;
    public Vector3 offset = new Vector3(0 , 0, -10);

    public void SetTarget(Transform _target, float _followSpeed = 0.125f)
    {
        followTarget = _target;
        followSpeed = _followSpeed;
    }

    public void FollowTarget()
    {
        transform.position = Vector3.Lerp(transform.position, followTarget.position + offset, followSpeed);
    }

    private void FixedUpdate()
    {
        FollowTarget();
    }
}
