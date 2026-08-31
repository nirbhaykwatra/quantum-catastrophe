using System;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float m_speed = 1;
    [SerializeField] private float SpeedMultiplier = 1;

    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, (m_speed * SpeedMultiplier) * Time.deltaTime));
    }
}
