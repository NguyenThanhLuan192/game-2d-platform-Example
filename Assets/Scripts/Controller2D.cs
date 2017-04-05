using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{

    float maxSlopeAngle = 80;
    // góc xuống tối đa
    float maxDescentAngle = 80;

    public CollisionInfo collision;

    public override void Start()
    {
        base.Start();
    }

    public void Move(Vector3 velocity, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigin();

        collision.Reset();
        collision.velocityOld = velocity;

        if (velocity.x < 0)
            DescendSlope(ref velocity);

        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
            VerticalCo1llisions(ref velocity);
        
        // tinh toan vi tri cua nhan vat theo van toc
        transform.Translate(velocity);

        // kiem tra player co dung tren platform ko
        if (standingOnPlatform)
        {
            collision.below = true;
        }
    }

    /// <summary>
    /// Horizontals the collisions.
    /// va chạm ngang
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void HorizontalCollisions(ref Vector3 velocity)
    {
        // huong theo truc X. mathf.sign tra ve -1 hoac 1
        float directionX = Mathf.Sign(velocity.x);
        // độ dài của tia bằng vận tốc của vật thể + skinWidth
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

                if (hit.distance == 0)
                {
                    continue;
                }

                // tính góc của con dốc 
                // hit.normal là vector pháp tuyến từ mặt phẳng
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); 

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collision.descendingSlope)
                    {
                        collision.descendingSlope = false;
                        velocity = collision.velocityOld;
                    }


                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collision.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimpSlope(ref velocity, slopeAngle); 
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!collision.climbingSlope || slopeAngle > maxSlopeAngle)
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
    }

    /// <summary>
    /// Verticals the co1llisions.
    /// va chạm đứng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void VerticalCo1llisions(ref Vector3 velocity)
    {
        // hướng di chuyển của nhân vật =1 đi lên; =-1 đi xuống
        float directionY = Mathf.Sign(velocity.y);
        // độ dài của tia bằng với vận tốc của vận tốc + skinWidth
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;


        for (int i = 0; i < verticalRayCount; i++)
        {
            // tinh toan gốc của tia raycast
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

                if (collision.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                
                collision.above = directionY == 1;
                collision.below = directionY == -1;
            }
        }

        // kiểm tra khi đi dang di chuyển trên dốc có gặp 1 con dốc khác
        if (collision.climbingSlope)
        {
            // hướng di chuyển
            float directionX = Mathf.Sign(velocity.x);
            // độ dài của tia
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            // gốc các tia
            Vector2 originRay = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight + Vector2.up * directionX;
            RaycastHit2D hit = Physics2D.Raycast(originRay, directionX * Vector2.right, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collision.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collision.slopeAngle = slopeAngle;
                }
            }
        }
    }

    /// <summary>'
    /// xử lý khi di chuyển lên dốc
    /// tính vận tốc của nhân vật khi di chuyển trên dốc
    /// khi di chuyển lên dốc vận tốc sẽ chậm hơn khi đi trên mặt bằng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    /// <param name="slopeAngle">Slope angle.</param>
    void ClimpSlope(ref Vector3 velocity, float slopeAngle)
    {
        // vận tốc theo trục x
        float moveDistance = Mathf.Abs(velocity.x);
        // van toc theo truc y tren doc
        float climVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climVelocityY)
        {
            // tính lại vận tốc trục trục x
            velocity.y = climVelocityY;
            // tính lại vận tốc của trục y
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            collision.below = true;

            collision.climbingSlope = true;
            collision.slopeAngle = slopeAngle;
        }
    }

    /// <summary>
    /// Descends the slope.
    /// xử lý khi di chuyển xuống dốc
    /// góc xuống dốc
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void DescendSlope(ref Vector3 velocity)
    {
        // hướng di chuyển 
        float directionX = Mathf.Sign(velocity.x);
        // gốc của tia
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescentAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collision.slopeAngle = slopeAngle;
                        collision.descendingSlope = false;
                        collision.below = true;
                    }
                }
            }
        }
    }



    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;

        public bool descendingSlope;

        public Vector3 velocityOld;

        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngle = slopeAngleOld;
            slopeAngleOld = 0;
        }
    }
}
