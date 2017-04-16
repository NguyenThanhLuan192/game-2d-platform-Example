using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Controller2D target;
    public Vector2 forcusAreaSize;
    public float verticleOffset;
    public float lookAheadDstX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;


    ForcusArea forcusArea;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;
    float smoothLookVelocity;
    float smoothVelocityY;

    bool lookAheadStopped;

    void Start()
    {
        forcusArea = new ForcusArea(target.collider.bounds, forcusAreaSize);
    }

    void LateUpdate()
    {
        forcusArea.Update(target.collider.bounds);
        // tính toán di chuyển camera smooth
        Vector2 forcusPosition = forcusArea.centre + Vector2.up * verticleOffset;
        if (forcusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(forcusArea.velocity.x);
            if (Mathf.Sign(target.playerInput.x) == Mathf.Sign(forcusArea.velocity.x) && target.playerInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4;
                }
            }
        }
    
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocity, lookSmoothTimeX);
        forcusPosition.y = Mathf.SmoothDamp(transform.position.y, forcusPosition.y, ref smoothVelocityY, verticalSmoothTime);
        forcusPosition += Vector2.right * currentLookAheadX;
        transform.position = (Vector3)forcusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(forcusArea.centre, forcusAreaSize);
    }

    struct ForcusArea
    {
        public Vector2 centre;
        public Vector2 velocity;

        float left, right;
        float bottom, top;

        public ForcusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;

            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (bottom + top) / 2);
        }

        /// <summary>
        /// Update the specified targetBounds.
        /// tính toán phạm vi duyển vùng không gian mà player di chuyển
        /// nếu di chuyển ra ngoài cùng này thì camera sẽ di chuyển theo
        /// </summary>
        /// <param name="targetBounds">Target bounds.</param>
        public void Update(Bounds targetBounds)
        {
            // pham vi truc X
            float shiftX = 0;
            if (targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            }
            else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;
            // pham vi truc Y
            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;
            // tinh tam cua vung di chuyen
            centre = new Vector2((left + right) / 2, (bottom + top) / 2);
            // tinh van toc di chuyen cua
            velocity = new Vector2(shiftX, shiftY);
        }
    }

}
