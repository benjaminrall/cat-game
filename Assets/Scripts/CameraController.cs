using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraController : MonoBehaviour
{
    // target transform for camera to follow
    public Transform target;
    
    // smoothing speed of camera motion
    [Range(0.0f, 1.0f)]
    public float smoothingSpeed = 0.125f;

    // camera offset to target
    public Vector3 offset;

    private void Start()
    {
        StartCoroutine(Shake(10, 0.05f, 15));
    }

    // late update so that camera follow calculations do not interfere with player movement etc.
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothingSpeed);
    }

    // coroutine for shake function
    public IEnumerator Shake(float duration, float magnitude, float speed)
    {
        float elapsed = 0.0f;
        float delay = 1 / speed;

        while (elapsed < duration)
        {
            Vector3 targetPosition = target.position + offset;

            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            Transform t = transform;
            t.localPosition = new Vector3(targetPosition.x + x, targetPosition.y + y, t.localPosition.z);

            elapsed += delay;

            yield return new WaitForSeconds(delay);
        }
    }
}
