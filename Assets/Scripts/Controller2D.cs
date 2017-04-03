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
    public CollisionInfo collision;

    void Start()
    {
        this.collider = GetComponent<BoxCollider2D>();
        CalculatorRaySpacing();
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigin();
        collision.Reset();
        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
            VerticalCo1llisions(ref velocity);

        transform.Translate(velocity);
    }

    /// <summary>
    /// Horizontals the collisions.
    /// va chạm ngang
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;


        for (int i = 0; i < horizontalRayCount; i++)
        {
            // gốc của tia raycast
            Vector2 rayOrigin = directionX == -1 ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            // tạo tia raycast 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                // tính toán lại vận tốc
                velocity.x = (hit.distance - skinWidth) * directionX;
                // tính lại độ dài tia raycast
                rayLength = hit.distance;

                collision.left = directionX == -1;
                collision.right = directionX == 1;
            }
        }
    }

    /// <summary>
    /// Verticals the co1llisions.
    /// va chạm đứng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void VerticalCo1llisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;


        for (int i = 0; i < verticalRayCount; i++)
        {
            // gốc của tia raycast
            Vector2 rayOrigin = directionY == -1 ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            // tạo tia raycast 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                // tính toán lại vận tốc
                velocity.y = (hit.distance - skinWidth) * directionY;
                // tính lại độ dài tia raycast
                rayLength = hit.distance;
            
                collision.above = directionY == 1;
                collision.below = directionY == -1;
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

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}
