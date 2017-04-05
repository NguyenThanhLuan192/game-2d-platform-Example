using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{

    public Vector3 move;
    public LayerMask passengerMask;

    public List<PassengerMovement> passengerMovement;
    public Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
        base.Start();
    }

    void Update()
    {
        UpdateRaycastOrigin();

        var velovity = move * Time.deltaTime;

        CalculatePassengerMovement(velovity);

        MovePassengers(true);

        // di cuhuyen platform theo van toc
        transform.Translate(velovity);
     
        MovePassengers(false);
    }

    void MovePassengers(bool beforeMovePassenger)
    {
        foreach (var passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }


            if (passenger.moveBeforePlatform == beforeMovePassenger)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }


    /// <summary>
    /// Moves the passenger.
    /// di chuyen doi tuong dung tren movingPlatform
    /// tinh toan viec di chuyen cua movingplatform
    /// </summary>
    /// <param name="velocity">Velocity : van toc di chuyen cua movingPlatform.</param>
    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassenger = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();
        var directionX = Mathf.Sign(velocity.x);
        var directionY = Mathf.Sign(velocity.y);


        // di chuyen theo truc y cua platform
        // vertically moving platform
        if (velocity.y != 0)
        {
            float raylength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            { 
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, raylength, passengerMask);
    
                if (hit)
                {
                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        // tinh van toc di chuyen theo truc x cua passenger
                        var pushX = (directionY == 1) ? velocity.x : 0;
                        // tinh van toc di chuyen theo truc y cua passenger
                        var pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        // di chuyen doi tuong
                        //hit.transform.Translate(new Vector3(pushX, pushY));

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }

            }
        }

        // di chuyen theo truc x cua platform
        // horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;


            for (int i = 0; i < horizontalRayCount; i++)
            {
                // gốc của tia raycast
                Vector2 rayOrigin = directionX == -1 ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                // tạo tia raycast 
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {

                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        // tinh van toc di chuyen theo truc x cua passenger
                        var pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        // tinh van toc di chuyen theo truc y cua passenger
                        var pushY = -skinWidth;
                       
                        // di chuyen doi tuong bị chạm vào
                        // hit.transform.Translate(new Vector3(pushX, pushY));
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // passenger on top of a horizontal or downward moving platform

        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float raylength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            { 
                Vector2 rayOrigin = raycastOrigin.topLeft + Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, raylength, passengerMask);

                if (hit)
                {
                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        // tinh van toc di chuyen theo truc x cua passenger
                        var pushX = velocity.x;
                        // tinh van toc di chuyen theo truc y cua passenger
                        var pushY = velocity.y;

                        // di chuyen doi tuong bị chạm vào
                        //hit.transform.Translate(new Vector3(pushX, pushY));
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }

            }
        }
    }

    public struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

}
