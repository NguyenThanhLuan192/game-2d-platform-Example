using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    public LayerMask collisionMask;
    public const float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D collider;
    public RaycastOrigin raycastOrigin;

    public virtual void Start()
    {
        this.collider = GetComponent<BoxCollider2D>();
        CalculatorRaySpacing();
    }

    /// <summary>
    /// Updates the raycast origin.
    /// các gốc tọa độ của tia ray cast
    /// được tính toán trong update
    /// </summary>
    public  void UpdateRaycastOrigin()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        raycastOrigin.bottomLeft = new Vector2(bound.min.x, bound.min.y);
        raycastOrigin.bottomRight = new Vector2(bound.max.x, bound.min.y);
        raycastOrigin.topLeft = new Vector2(bound.min.x, bound.max.y);
        raycastOrigin.topRight = new Vector2(bound.max.x, bound.max.y);
    }

    /// <summary>
    /// Calculators the ray spacing.
    /// tính khoảng cách giữa các tia ray cast
    /// </summary>
    public void CalculatorRaySpacing()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bound.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bound.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
