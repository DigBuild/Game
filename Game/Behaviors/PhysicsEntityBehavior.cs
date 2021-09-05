using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Serialization;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Behaviors
{
    /// <summary>
    /// The contract for the physical entity behavior.
    /// </summary>
    public interface IPhysicsEntityBehavior
    {
        /// <summary>
        /// Whether the entity is in the world or not.
        /// </summary>
        public bool InWorld { get; set; }
        /// <summary>
        /// Whether the entity is on the ground or not.
        /// </summary>
        public bool OnGround { get; set; }
        
        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// The velocity.
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// The rotation pitch.
        /// </summary>
        public float Pitch { get; set; }
        /// <summary>
        /// The rotation yaw.
        /// </summary>
        public float Yaw { get; set; }
        /// <summary>
        /// The angular velocity for rotation pitch.
        /// </summary>
        public float AngularVelocityPitch { get; set; }
        /// <summary>
        /// The angular velocity for rotation yaw.
        /// </summary>
        public float AngularVelocityYaw { get; set; }

        /// <summary>
        /// The physics entity capability instance.
        /// </summary>
        public IPhysicsEntity? Capability { get; set; }
    }

    /// <summary>
    /// A physics entity behavior. Contract: <see cref="IPhysicsEntityBehavior"/>.
    /// <para>
    /// Gives the entity a bounding box, and allows it to move,
    /// colliding with other game objects in the world.
    /// </para>
    /// </summary>
    public sealed class PhysicsEntityBehavior : IEntityBehavior<IPhysicsEntityBehavior>
    {
        private readonly AABB _bounds;
        private readonly ushort _chunkLoadRadius;
        private readonly float _terminalVelocity, _groundDragFactor, _airDragFactor;
        private readonly float _jumpForce, _jumpKickSpeed, _movementSpeedGround, _movementSpeedAir, _rotationSpeed;

        public PhysicsEntityBehavior(
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
        
        public void Build(EntityBehaviorBuilder<IPhysicsEntityBehavior, IPhysicsEntityBehavior> entity)
        {
            entity.Add(GameEntityAttributes.Position, (_, data, _) => data.Position);
            entity.Add(GameEntityAttributes.Bounds, (_, _, _) => _bounds);
            entity.Add(GameEntityCapabilities.PhysicsEntity, (_, data, _) => data.Capability);
            entity.Add(ModelData.EntityAttribute, GetModelData);
            entity.Subscribe(OnJoinedWorld);
            entity.Subscribe(OnLeavingWorld);
        }

        private ModelData GetModelData(IReadOnlyEntityInstance instance, IPhysicsEntityBehavior data, Func<ModelData> next)
        {
            var modelData = next();
            modelData.CreateOrExtend<PhysicalEntityModelData>(d =>
            {
                d.Position = data.Position;
                d.Velocity = data.Capability!.PrevVelocity;
            });
            return modelData;
        }

        private void OnJoinedWorld(BuiltInEntityEvent.JoinedWorld evt, IPhysicsEntityBehavior data, Action next)
        {
            evt.Entity.World.GetChunk(new BlockPos(data.Position).ChunkPos)?.Get(ChunkEntities.Type).Add(evt.Entity);

            data.Capability = new PhysicsEntity(evt.Entity, _bounds, data, this);
            data.InWorld = true;
            evt.Entity.World.TickScheduler.After(1).Enqueue(GameJobs.PhysicsEntityMove, data.Capability);
            ((PhysicsEntity)data.Capability).LoadSurroundingChunks();
            next();
        }

        private void OnLeavingWorld(BuiltInEntityEvent.LeavingWorld evt, IPhysicsEntityBehavior data, Action next)
        {
            evt.Entity.World.GetChunk(new BlockPos(data.Position).ChunkPos)?.Get(ChunkEntities.Type).Remove(evt.Entity);

            data.InWorld = false;
            next();
        }

        /// <summary>
        /// Peforms a physics update on the given entity.
        /// </summary>
        /// <param name="scheduler">The scheduler</param>
        /// <param name="entity">The entity</param>
        public static void Update(Scheduler scheduler, IPhysicsEntity entity)
        {
            if (!entity.InWorld)
                return;

            entity.Move();
            scheduler.After(1).Enqueue(GameJobs.PhysicsEntityMove, entity);
        }

        private sealed class PhysicsEntity : IPhysicsEntity
        {
            private readonly EntityInstance _entity;
            private readonly IPhysicsEntityBehavior _data;
            private readonly PhysicsEntityBehavior _behavior;
            
            private IChunkLoadingClaim? _chunkLoadingTicket;

            public PhysicsEntity(
                EntityInstance entity,
                AABB bounds,
                IPhysicsEntityBehavior data,
                PhysicsEntityBehavior behavior)
            {
                _entity = entity;
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
                set
                {
                    var currentChunk = new BlockPos(_data.Position).ChunkPos;
                    var newChunk = new BlockPos(value).ChunkPos;
                    if (currentChunk != newChunk)
                    {
                        _entity.World.GetChunk(currentChunk)?.Get(ChunkEntities.Type).Remove(_entity);
                        _entity.World.GetChunk(newChunk)!.Get(ChunkEntities.Type).Add(_entity);
                    }

                    _data.Position = value;
                }
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
            
            public Vector3 PrevPosition { get; set; }
            public Vector3 PrevVelocity { get; set; }
            public float PrevPitch { get; set; }
            public float PrevYaw { get; set; }

            public void Rotate(float pitchDelta, float yawDelta)
            {
                PrevPitch = Pitch;
                PrevYaw = Yaw;
                AngularVelocityPitch = pitchDelta * _behavior._rotationSpeed;
                AngularVelocityYaw = yawDelta * _behavior._rotationSpeed;

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

            public void ApplyJumpMotion(float upwardsMotion)
            {
                if (!OnGround)
                    return;
                OnGround = false;
                Velocity += Vector3.Transform(
                    new Vector3(0, 0, upwardsMotion),
                    Matrix4x4.CreateRotationY(Yaw)
                ) * _behavior._jumpKickSpeed;
                Velocity += Vector3.UnitY * _behavior._jumpForce;
            }

            public void Move()
            {
                var world = _entity.World;

                OnGround = false;
                Velocity += -Vector3.UnitY * world.Gravity;
                
                var vel = Velocity;

                List<(ICollider Collider, AABB RelativeBounds, float Delta, Vector3 Intersection)> colliders = new(), colliders2 = new();
                var translatedBounds = Bounds + Position;
                var extendedBounds = translatedBounds + vel; //AABB.Containing(translatedBounds, translatedBounds + vel);
                foreach (var pos in extendedBounds.GetIntersectedBlockPositions())
                {
                    var block = world.GetBlock(pos);
                    if (block == null)
                        continue;

                    var collider = block.Get(world, pos, GameBlockAttributes.Collider);
                    var relativeBounds = translatedBounds - (Vector3) pos;

                    if (collider.Collide(relativeBounds, vel, out var delta, out var intersection))
                        colliders.Add((collider, relativeBounds, delta, intersection));
                }


                while (colliders.Count > 0)
                {
                    colliders.Sort((a, b) => a.Delta.CompareTo(b.Delta));
                    
                    var intersection = colliders[^1].Intersection;
                    if (intersection.Y > 0)
                        OnGround = true;

                    vel += intersection;

                    colliders2.Clear();
                    colliders.RemoveAt(colliders.Count - 1);
                    foreach (var (collider, relativeBounds, _, _) in colliders)
                    {
                        if (!collider.Collide(relativeBounds, vel, out var delta, out var intersection2))
                            continue;
                        colliders2.Add((collider, relativeBounds, delta, intersection2));
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

            internal void LoadSurroundingChunks()
            {
                var chunkPos = new BlockPos(Position).ChunkPos;
                var chunksToLoad = new HashSet<ChunkPos>();
                for (var x = -_behavior._chunkLoadRadius; x <= _behavior._chunkLoadRadius; x++)
                for (var z = -_behavior._chunkLoadRadius; z <= _behavior._chunkLoadRadius; z++)
                    chunksToLoad.Add(new ChunkPos(chunkPos.X + x, chunkPos.Z + z));

                var prevTicket = _chunkLoadingTicket;
                if (!_entity.World.ChunkManager.TryClaim(chunksToLoad, false, out _chunkLoadingTicket))
                {
                    throw new Exception("What.");
                }
                prevTicket?.Release();
            }
        }
    }

    /// <summary>
    /// A data class implementing the <see cref="IPhysicsEntityBehavior"/> contract.
    /// </summary>
    public sealed class PhysicsEntityData : IData<PhysicsEntityData>, IChangeNotifier, IPhysicsEntityBehavior
    {
        private Vector3 _position;

        public event Action? Changed;

        public bool InWorld { get; set; }
        public bool OnGround { get; set; }

        public Vector3 Position
        {
            get => _position;
            set { _position = value; Changed?.Invoke(); }
        }
        public Vector3 Velocity { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float AngularVelocityPitch { get; set; }
        public float AngularVelocityYaw { get; set; }

        public IPhysicsEntity? Capability { get; set; }

        public PhysicsEntityData Copy()
        {
            return new PhysicsEntityData
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

        /// <summary>
        /// The serdes.
        /// </summary>
        public static ISerdes<PhysicsEntityData> Serdes { get; } = new CompositeSerdes<PhysicsEntityData>()
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

    /// <summary>
    /// The physics entity capability interface.
    /// </summary>
    public interface IPhysicsEntity
    {
        /// <summary>
        /// Whether the entity is in the world or not.
        /// </summary>
        public bool InWorld { get; }
        /// <summary>
        /// The bounding box.
        /// </summary>
        public AABB Bounds { get; }
        /// <summary>
        /// Whether the entity is on the ground or not.
        /// </summary>
        public bool OnGround { get; set; }

        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// The velocity.
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// The rotation pitch.
        /// </summary>
        public float Pitch { get; set; }
        /// <summary>
        /// The rotation yaw.
        /// </summary>
        public float Yaw { get; set; }
        /// <summary>
        /// The angular velocity for rotation pitch.
        /// </summary>
        public float AngularVelocityPitch { get; set; }
        /// <summary>
        /// The angular velocity for rotation yaw.
        /// </summary>
        public float AngularVelocityYaw { get; set; }
        
        /// <summary>
        /// The position a tick ago.
        /// </summary>
        public Vector3 PrevPosition { get; set; }
        /// <summary>
        /// The velocity a tick ago.
        /// </summary>
        public Vector3 PrevVelocity { get; set; }
        /// <summary>
        /// The pitch a tick ago.
        /// </summary>
        public float PrevPitch { get; set; }
        /// <summary>
        /// The yaw a tick ago.
        /// </summary>
        public float PrevYaw { get; set; }

        /// <summary>
        /// Applies a rotation on the entity.
        /// </summary>
        /// <param name="pitchDelta">The pitch delta</param>
        /// <param name="yawDelta">The yaw delta</param>
        public void Rotate(float pitchDelta, float yawDelta);
        /// <summary>
        /// Applies a velocity on the entity.
        /// </summary>
        /// <param name="forwardMotion">The forward velocity</param>
        /// <param name="sidewaysMotion">The sideways velocity</param>
        public void ApplyMotion(float forwardMotion, float sidewaysMotion);
        /// <summary>
        /// Applies a vertical motion on the entity.
        /// </summary>
        /// <param name="upwardsMotion">The upwards motion</param>
        public void ApplyJumpMotion(float upwardsMotion);
        /// <summary>
        /// Updates the entity's physics state and computes collisions.
        /// </summary>
        public void Move();
    }

    /// <summary>
    /// A model data for a physics entity.
    /// </summary>
    public sealed class PhysicalEntityModelData
    {
        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// The velocity.
        /// </summary>
        public Vector3 Velocity { get; set; }
    }
}