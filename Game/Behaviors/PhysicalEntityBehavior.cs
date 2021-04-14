using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worlds;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    public interface IPhysicalEntityBehavior
    {
        public bool InWorld { get; set; }
        public bool OnGround { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float AngularVelocityPitch { get; set; }
        public float AngularVelocityYaw { get; set; }

        public IPhysicalEntity? Capability { get; set; }
    }

    public sealed class PhysicalEntityBehavior : IEntityBehavior<IPhysicalEntityBehavior>
    {
        private readonly AABB _bounds;
        private readonly ushort _chunkLoadRadius;
        private readonly float _terminalVelocity, _groundDragFactor, _airDragFactor;
        private readonly float _jumpForce, _jumpKickSpeed, _movementSpeedGround, _movementSpeedAir, _rotationSpeed;

        public PhysicalEntityBehavior(
            AABB bounds, ushort chunkLoadRadius = 0,
            float terminalVelocity = 1.5f, float groundDragFactor = 0.2f, float airDragFactor = 0.3f, float jumpForce = 0,
            float jumpKickSpeed = 0, float movementSpeedGround = 0, float movementSpeedAir = 0, float rotationSpeed = 0
        )
        {
            _bounds = bounds;
            _chunkLoadRadius = chunkLoadRadius;
            _terminalVelocity = terminalVelocity;
            _groundDragFactor = groundDragFactor;
            _airDragFactor = airDragFactor;
            _jumpForce = jumpForce;
            _jumpKickSpeed = jumpKickSpeed;
            _movementSpeedGround = movementSpeedGround;
            _movementSpeedAir = movementSpeedAir;
            _rotationSpeed = rotationSpeed;
        }
        
        public void Build(EntityBehaviorBuilder<IPhysicalEntityBehavior, IPhysicalEntityBehavior> entity)
        {
            entity.Add(EntityAttributes.Position, (_, data, _) => data.Position);
            entity.Add(EntityCapabilities.PhysicalEntity, (_, data, _) => data.Capability);
            entity.Subscribe(OnJoinedWorld);
            entity.Subscribe(OnLeavingWorld);
        }
        
        private void OnJoinedWorld(BuiltInEntityEvent.JoinedWorld evt, IPhysicalEntityBehavior data, Action next)
        {
            data.Capability = new PhysicalEntity(evt.Entity.World, _bounds, data, this);
            data.InWorld = true;
            evt.Entity.World.TickScheduler.After(1).Enqueue(GameJobs.PhysicalEntityMove, data.Capability);
        }

        private void OnLeavingWorld(BuiltInEntityEvent.LeavingWorld evt, IPhysicalEntityBehavior data, Action next)
        {
            data.InWorld = false;
        }

        public static void Update(Scheduler scheduler, IPhysicalEntity entity)
        {
            if (!entity.InWorld)
                return;

            entity.Move();
            scheduler.After(1).Enqueue(GameJobs.PhysicalEntityMove, entity);
        }

        private sealed class PhysicalEntity : IPhysicalEntity
        {
            private readonly IWorld _world;
            private readonly IPhysicalEntityBehavior _data;
            private readonly PhysicalEntityBehavior _behavior;
            
            private IChunkClaim? _chunkLoadingTicket;

            public PhysicalEntity(
                IWorld world, AABB bounds, IPhysicalEntityBehavior data,
                PhysicalEntityBehavior behavior
            )
            {
                _world = world;
                Bounds = bounds;
                _data = data;
                _behavior = behavior;
                PrevPosition = data.Position;
            }

            public bool InWorld => _data.InWorld;

            public AABB Bounds { get; }

            public bool OnGround
            {
                get => _data.OnGround;
                set => _data.OnGround = value;
            }

            public Vector3 Position
            {
                get => _data.Position;
                set => _data.Position = value;
            }

            public Vector3 Velocity
            {
                get => _data.Velocity;
                set => _data.Velocity = value;
            }

            public float Pitch
            {
                get => _data.Pitch;
                set => _data.Pitch = value;
            }

            public float Yaw
            {
                get => _data.Yaw;
                set => _data.Yaw = value;
            }

            public float AngularVelocityPitch
            {
                get => _data.AngularVelocityPitch;
                set => _data.AngularVelocityPitch = value;
            }

            public float AngularVelocityYaw
            {
                get => _data.AngularVelocityYaw;
                set => _data.AngularVelocityYaw = value;
            }
            
            public Vector3 PrevPosition { get; private set; }
            public Vector3 PrevVelocity { get; private set; }
            public float PrevPitch { get; private set; }
            public float PrevYaw { get; private set; }

            public void Rotate(float pitchDelta, float yawDelta)
            {
                PrevPitch = Pitch;
                PrevYaw = Yaw;
                AngularVelocityPitch = pitchDelta * pitchDelta * MathF.Sign(pitchDelta) * _behavior._rotationSpeed;
                AngularVelocityYaw = yawDelta * yawDelta * MathF.Sign(yawDelta) * _behavior._rotationSpeed;

                Pitch = MathF.Max(
                    -MathF.PI / 2,
                    MathF.Min(
                        Pitch + AngularVelocityPitch,
                        MathF.PI / 2
                    )
                );
                AngularVelocityPitch = Pitch - PrevPitch;
                Yaw = (MathF.PI * 3 + Yaw + AngularVelocityYaw) % (MathF.PI * 2) - MathF.PI;
            }

            public void ApplyMotion(float forwardMotion, float sidewaysMotion)
            {
                Velocity += Vector3.Transform(
                    new Vector3(sidewaysMotion, 0, forwardMotion),
                    Matrix4x4.CreateRotationY(Yaw)
                ) * (!OnGround ? _behavior._movementSpeedAir : _behavior._movementSpeedGround);
            }

            public void ApplyJumpMotion(float forwardMotion)
            {
                if (!OnGround)
                    return;
                OnGround = false;
                Velocity += Vector3.Transform(
                    new Vector3(0, 0, forwardMotion),
                    Matrix4x4.CreateRotationY(Yaw)
                ) * _behavior._jumpKickSpeed;
                Velocity += Vector3.UnitY * _behavior._jumpForce;
            }

            public void Move()
            {
                OnGround = false;
                
                Velocity += -Vector3.UnitY * _world.Gravity;
                
                var vel = Velocity;

                List<(ICollider Collider, AABB RelativeBounds, Vector3 Intersection)> colliders = new(), colliders2 = new();
                var translatedBounds = Bounds + Position;
                var extendedBounds = translatedBounds + vel; //AABB.Containing(translatedBounds, translatedBounds + vel);
                foreach (var pos in extendedBounds.GetIntersectedBlockPositions())
                {
                    var block = _world.GetBlock(pos);
                    if (block == null)
                        continue;

                    var collider = block.Get(new BlockContext(_world, pos, block), BlockAttributes.Collider);
                    var relativeBounds = translatedBounds - (Vector3) pos;

                    if (collider.Collide(relativeBounds, vel, out var intersection))
                        colliders.Add((collider, relativeBounds, intersection));
                }


                while (colliders.Count > 0)
                {
                    colliders.Sort((a, b) => a.Intersection.LengthSquared().CompareTo(b.Intersection.LengthSquared()));
                    
                    var intersection = colliders[^1].Intersection;
                    if (intersection.Y > 0)
                        OnGround = true;

                    vel += intersection;

                    colliders2.Clear();
                    colliders.RemoveAt(colliders.Count - 1);
                    foreach (var (collider, relativeBounds, _) in colliders)
                    {
                        if (!collider.Collide(relativeBounds, vel, out var intersection2))
                            continue;
                        colliders2.Add((collider, relativeBounds, intersection2));
                    }

                    (colliders, colliders2) = (colliders2, colliders);
                }

                PrevPosition = Position;
                
                var yVel = OnGround ? 0 : Math.Max(vel.Y, -_behavior._terminalVelocity);
                var dragFactor = OnGround ? _behavior._groundDragFactor : _behavior._airDragFactor;
                Position += vel;
                PrevVelocity = vel;
                Velocity = new Vector3(vel.X * dragFactor, yVel, vel.Z * dragFactor);

                if (_behavior._chunkLoadRadius == 0)
                    return;

                var prevChunkPos = new BlockPos(PrevPosition).ChunkPos;
                var newChunkPos = new BlockPos(Position).ChunkPos;
                if (prevChunkPos != newChunkPos)
                    LoadSurroundingChunks();
            }

            private void LoadSurroundingChunks()
            {
                var chunkPos = new BlockPos(Position).ChunkPos;
                var chunksToLoad = new HashSet<ChunkPos>();
                for (var x = -_behavior._chunkLoadRadius; x <= _behavior._chunkLoadRadius; x++)
                for (var y = -2; y <= 2; y++)
                for (var z = -_behavior._chunkLoadRadius; z <= _behavior._chunkLoadRadius; z++)
                    chunksToLoad.Add(new ChunkPos(chunkPos.X + x, chunkPos.Y + y, chunkPos.Z + z));

                var prevTicket = _chunkLoadingTicket;
                if (!_world.ChunkManager.TryLoad(chunksToLoad, false, out _chunkLoadingTicket))
                {
                    throw new Exception("What.");
                }
                prevTicket?.Release();
            }
        }
    }

    public sealed class PhysicalEntityData : IData<PhysicalEntityData>, IPhysicalEntityBehavior
    {
        public bool InWorld { get; set; }
        public bool OnGround { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float AngularVelocityPitch { get; set; }
        public float AngularVelocityYaw { get; set; }

        public IPhysicalEntity? Capability { get; set; }

        public PhysicalEntityData Copy()
        {
            return new()
            {
                InWorld = false,
                OnGround = OnGround,
                Position = Position,
                Velocity = Velocity,
                Pitch = Pitch,
                Yaw = Yaw,
                AngularVelocityPitch = AngularVelocityPitch,
                AngularVelocityYaw = AngularVelocityYaw
            };
        }

        public static ISerdes<PhysicalEntityData> Serdes { get; } =
            new CompositeSerdes<PhysicalEntityData>(() => new PhysicalEntityData())
            {
                {1u, d => d.InWorld, UnmanagedSerdes<bool>.NotNull},
                {2u, d => d.OnGround, UnmanagedSerdes<bool>.NotNull},
                {3u, d => d.Position, UnmanagedSerdes<Vector3>.NotNull},
                {4u, d => d.Velocity, UnmanagedSerdes<Vector3>.NotNull},
                {5u, d => d.Pitch, UnmanagedSerdes<float>.NotNull},
                {6u, d => d.Yaw, UnmanagedSerdes<float>.NotNull},
                {7u, d => d.AngularVelocityPitch, UnmanagedSerdes<float>.NotNull},
                {8u, d => d.AngularVelocityYaw, UnmanagedSerdes<float>.NotNull},
            };
    }

    public interface IPhysicalEntity
    {
        public bool InWorld { get; }
        public AABB Bounds { get; }
        public bool OnGround { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float AngularVelocityPitch { get; set; }
        public float AngularVelocityYaw { get; set; }
        
        public Vector3 PrevPosition { get; }
        public Vector3 PrevVelocity { get; }
        public float PrevPitch { get; }
        public float PrevYaw { get; }

        public void Rotate(float pitchDelta, float yawDelta);
        public void ApplyMotion(float forwardMotion, float sidewaysMotion);
        public void ApplyJumpMotion(float forwardMotion);
        public void Move();
    }
}