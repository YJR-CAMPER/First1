using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // 따라갈 대상 (플레이어)

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f; // 부드러움 정도 (0~1)
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // 카메라 오프셋

    [Header("Boundary Settings (Optional)")]
    [SerializeField] private bool useBoundary = false; // 맵 경계 사용 여부
    [SerializeField] private Vector2 minBoundary = new Vector2(-10, -10);
    [SerializeField] private Vector2 maxBoundary = new Vector2(10, 10);

    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 부드러운 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 경계 적용
        if (useBoundary)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBoundary.x, maxBoundary.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBoundary.y, maxBoundary.y);
        }

        transform.position = smoothedPosition;
    }

    // 외부에서 타겟 설정
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 즉시 타겟 위치로 이동 (씬 전환 시 유용)
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 snapPosition = target.position + offset;

        if (useBoundary)
        {
            snapPosition.x = Mathf.Clamp(snapPosition.x, minBoundary.x, maxBoundary.x);
            snapPosition.y = Mathf.Clamp(snapPosition.y, minBoundary.y, maxBoundary.y);
        }

        transform.position = snapPosition;
    }

    // 맵 경계 설정 (외부에서 호출 가능)
    public void SetBoundary(Vector2 min, Vector2 max)
    {
        useBoundary = true;
        minBoundary = min;
        maxBoundary = max;
    }
}
