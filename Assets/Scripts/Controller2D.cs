using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public LayerMask collisionMask;
    const float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigin raycastOrigin;

    void Start()
    {
        this.collider = GetComponent<BoxCollider2D>();
        CalculatorRaySpacing();
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigin();
        VerticalCollisions(ref velocity);

        transform.Translate(velocity);
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;


        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = directionY == -1 ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(raycastOrigin.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
            }
        }
    }

    void UpdateRaycastOrigin()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        raycastOrigin.bottomLeft = new Vector2(bound.min.x, bound.min.y);
        raycastOrigin.bottomRight = new Vector2(bound.max.x, bound.min.y);
        raycastOrigin.topLeft = new Vector2(bound.min.x, bound.max.y);
        raycastOrigin.topRight = new Vector2(bound.max.x, bound.max.y);
    }

    void CalculatorRaySpacing()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bound.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bound.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
