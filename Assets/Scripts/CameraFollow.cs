using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [Range(0.0f, 1.0f)] [SerializeField] private float followSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0 , 0, -10);

    /// <summary>
    /// Set the current camera target.
    /// </summary>
    /// <param name="_target">The transform that will be targeted.</param>
    /// <param name="_followSpeed">The desired followm speed (Default = 0.125f)</param>
    public void SetTarget(Transform _target, float _followSpeed = 0.125f)
    {
        followTarget = _target;
        followSpeed = _followSpeed;
    }

    /// <summary>
    /// Follow the target.
    /// </summary>
    private void FollowTarget()
    {
        transform.position = Vector3.Lerp(transform.position, followTarget.position + offset, followSpeed);
    }

    private void FixedUpdate()
    {
        FollowTarget();
    }
}
