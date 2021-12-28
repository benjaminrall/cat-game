using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraController : MonoBehaviour
{
    // Target transform for camera to follow
    public Transform target;
    
    // Smoothing speed of camera motion
    [Range(0.0f, 1.0f)] public float smoothingSpeed = 0.125f;
    
    // How much the camera should be offset to the player
    public Vector3 offset;

    // Late update so that camera follow calculations do not interfere with player movement etc.
    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothingSpeed);
    }

    // Coroutine for shaking the camera
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
