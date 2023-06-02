using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateUpdatedFollow : MonoBehaviour
{
    [SerializeField]
    Transform _targetToFollow;

    void LateUpdate()
    {
        transform.position = _targetToFollow.position;
        transform.rotation = _targetToFollow.rotation;
    }
}
