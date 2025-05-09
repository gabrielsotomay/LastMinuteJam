﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Implements game physics for some in game entity.
    /// </summary>
    public class NetworkedKinematicObject : NetworkBehaviour
    {
        /// <summary>
        /// The minimum normal (dot product) considered suitable for the entity sit on.
        /// </summary>
        public float minGroundNormalY = .65f;

        /// <summary>
        /// A custom gravity coefficient applied to this entity.
        /// </summary>
        public float gravityModifier = 1f;

        /// <summary>
        /// The current velocity of the entity.
        /// </summary>
        public Vector2 velocity = Vector2.zero;

        /// <summary>
        /// Is the entity currently sitting on a surface?
        /// </summary>
        /// <value></value>
        public bool IsGrounded = false;
        public bool SimpleMovement = false;

        protected Vector2 targetVelocity;
        protected Vector2 groundNormal;
        protected Rigidbody2D body;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        
        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;

        public NetworkTransform networkTransform;
        /// <summary>
        /// Bounce the object's vertical velocity.
        /// </summary>
        /// <param name="value"></param>
        public void Bounce(float value)
        {
            velocity.y = value;
        }

        /// <summary>
        /// Bounce the objects velocity in a direction.
        /// </summary>
        /// <param name="dir"></param>
        public void Bounce(Vector2 dir)
        {
            velocity.y = dir.y;
            velocity.x = dir.x;
        }

        /// <summary>
        /// Teleport to some position.
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            velocity *= 0;
            body.linearVelocity *= 0;
            networkTransform.Teleport(position, rotation);
            body.position = position;
            transform.position = position;
        }
        public void Teleport(Vector3 position)
        {
            velocity *= 0;
            body.linearVelocity *= 0;
            networkTransform.Teleport(position);
            body.position = position;
            transform.position = position;
        }

        protected virtual void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        protected virtual void OnDisable()
        {
            body.bodyType = RigidbodyType2D.Dynamic;
        }

        public override void NetworkStart()
        {
            if (!SimpleMovement)
            {
                contactFilter.useTriggers = false;
                contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
                contactFilter.useLayerMask = true;
            }
            base.NetworkStart();
        }

        protected virtual void ComputeVelocity()
        {

        }
         
        public override void NetworkFixedUpdate()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity();
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            if (SimpleMovement)
            {
                velocity.y = targetVelocity.y;
            }
            else if (velocity.y < 0 && !IsGrounded)
            {
                velocity += gravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
            }
            else if (!IsGrounded)
            {
                velocity += Physics2D.gravity * Time.fixedDeltaTime;
            }
            velocity.x = targetVelocity.x;

            IsGrounded = false;

            var deltaPosition = velocity * Time.fixedDeltaTime;

            var moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;
            
            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);
        }

        void PerformMovement(Vector2 move, bool yMovement)
        {
            var distance = move.magnitude;

            if (distance > minMoveDistance && !SimpleMovement)
            {
                //check if we hit anything in current direction of travel
                var count = body.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
                for (var i = 0; i < count; i++)
                {
                    var currentNormal = hitBuffer[i].normal;
                    //is this surface flat enough to land on?
                    if (currentNormal.y > minGroundNormalY)
                    {
                        IsGrounded = true;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement)
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }
                    if (IsGrounded)
                    {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                        {
                            //slower velocity if moving against the normal (up a hill).
                            //velocity = velocity - projection * currentNormal;
                        }
                    }
                    else
                    {
                        // We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        velocity.x *= 0;
                        velocity.y = Mathf.Min(velocity.y, 0);
                    }
                    // remove shellDistance from actual move distance.
                    var modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            Vector2 moveVector = move.normalized * distance;
            if (IsServer) //if (!(!IsServer && InputSource == Sandbox.LocalPlayer && yMovement) && (IsServer || InputSource != null) )
            {
                body.position = body.position + moveVector;
                transform.position = transform.position + new Vector3(moveVector.x, moveVector.y, 0);
            }
        }

    }
}