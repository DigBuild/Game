using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Voxel;

namespace DigBuild
{
    public class PlayerController : ICamera
    {
        private const float Gravity = 2.0f * TickManager.TickDurationSeconds;
        private const float TerminalVelocity = 0.5f;
        private const float GroundDragFactor = 0.2F;
        private const float AirDragFactor = 0.3F;

        private const float JumpForce = 12f * TickManager.TickDurationSeconds;
        private const float JumpKickSpeed = 0.8f * TickManager.TickDurationSeconds;
        private const float MovementSpeedGround = 6 * TickManager.TickDurationSeconds;
        private const float MovementSpeedAir = 5 * TickManager.TickDurationSeconds;
        private const float CameraSpeed = 4 * TickManager.TickDurationSeconds;

        private static readonly AABB PlayerBounds = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f);

        private readonly IWorld _world;
        
        public Vector3 PrevPosition { get; private set; }
        public Vector3 PrevVelocity { get; private set; }
        public float PrevPitch { get; private set; }
        public float PrevYaw { get; private set; }

        public Vector3 Position { get; private set; }
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        public Vector3 Velocity { get; private set; }
        public float AngularVelocityPitch { get; private set; }
        public float AngularVelocityYaw { get; private set; }
        public bool OnGround { get; private set; }

        Vector3 ICamera.Position => Position + Vector3.UnitY * 1.75f;

        Matrix4x4 ICamera.Transform =>
            Matrix4x4.CreateTranslation(-((ICamera) this).Position) *
            Matrix4x4.CreateRotationY(Yaw) *
            Matrix4x4.CreateRotationX(Pitch);

        public Matrix4x4 GetInterpolatedTransform(float partialTick)
        {
            return 
                Matrix4x4.CreateTranslation(-(PrevPosition + PrevVelocity * partialTick + Vector3.UnitY * 1.75f)) *
                Matrix4x4.CreateRotationY(PrevYaw + AngularVelocityYaw * partialTick) *
                Matrix4x4.CreateRotationX(PrevPitch + AngularVelocityPitch * partialTick);
        }

        public RayCaster.Ray GetInterpolatedRay(float partialTick)
        {
            var start = PrevPosition + PrevVelocity * partialTick + Vector3.UnitY * 1.75f;
            var direction = Vector3.TransformNormal(
                new Vector3(0, 0, 1),
                Matrix4x4.CreateRotationX(PrevPitch + AngularVelocityPitch * partialTick)
                * Matrix4x4.CreateRotationY(MathF.PI - (PrevYaw + AngularVelocityYaw * partialTick))
            );
            return new RayCaster.Ray(start, start + direction * 5f);
        }

        public PlayerController(IWorld world, Vector3 position)
        {
            _world = world;
            Position = position;
        }

        public void UpdateRotation(float pitchDelta, float yawDelta)
        {
            PrevPitch = Pitch;
            PrevYaw = Yaw;
            AngularVelocityPitch = pitchDelta * pitchDelta * MathF.Sign(pitchDelta) * CameraSpeed;
            AngularVelocityYaw = yawDelta * yawDelta * MathF.Sign(yawDelta) * CameraSpeed;

            Pitch = MathF.Max(
                -MathF.PI / 2,
                MathF.Min(
                    Pitch + AngularVelocityPitch,
                    MathF.PI / 2
                )
            );
            Yaw = (Yaw + AngularVelocityYaw) % (MathF.PI * 2);
        }

        public void ApplyMotion(float forwardMotion, float sidewaysMotion)
        {
            Velocity += Vector3.Transform(
                new Vector3(forwardMotion, 0, sidewaysMotion),
                Matrix4x4.CreateRotationY(MathF.PI / 2 - Yaw)
            ) * (!OnGround ? MovementSpeedAir : MovementSpeedGround);
        }

        public void Jump(float forwardMotion)
        {
            if (!OnGround)
                return;
            OnGround = false;
            Velocity += Vector3.Transform(
                new Vector3(forwardMotion, 0, 0),
                Matrix4x4.CreateRotationY(MathF.PI / 2 - Yaw)
            ) * JumpKickSpeed;
            Velocity += Vector3.UnitY * JumpForce;
        }

        public void Move()
        {
            OnGround = false;
            
            Velocity += -Vector3.UnitY * Gravity;
            
            var vel = Velocity;

            List<(ICollider collider, AABB relativeBounds, Vector3 intersection)> colliders = new(), colliders2 = new();
            foreach (var pos in PlayerBounds.WithOffset(Position + vel).GetIntersectedBlockPositions())
            {
                var block = _world.GetBlock(pos);
                if (block == null)
                    continue;

                var collider = block.Collider;
                var relativeBounds = PlayerBounds.WithOffset(Position - new Vector3(pos.X, pos.Y, pos.Z));

                if (collider.Collide(relativeBounds.WithOffset(vel), vel, out var intersection))
                    colliders.Add((collider, relativeBounds, intersection));
            }


            while (colliders.Count > 0)
            {
                colliders.Sort((a, b) => -a.intersection.LengthSquared().CompareTo(b.intersection.LengthSquared()));
                
                var intersection = colliders[^1].intersection;
                if (intersection.Y > 0)
                    OnGround = true;

                vel += intersection;

                colliders2.Clear();
                colliders.RemoveAt(colliders.Count - 1);
                foreach (var (collider, relativeBounds, _) in colliders)
                {
                    if (!collider.Collide(relativeBounds.WithOffset(vel), vel, out var intersection2))
                        continue;
                    colliders2.Add((collider, relativeBounds, intersection2));
                }

                (colliders, colliders2) = (colliders2, colliders);
            }

            PrevPosition = Position;
            
            var yVel = OnGround ? 0 : Math.Max(vel.Y, -TerminalVelocity);
            var dragFactor = OnGround ? GroundDragFactor : AirDragFactor;
            Position += vel;
            PrevVelocity = vel;
            Velocity = new Vector3(vel.X * dragFactor, yVel, vel.Z * dragFactor);
        }
    }
}