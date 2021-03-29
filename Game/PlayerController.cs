using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Worlds;
using DigBuild.Recipes;
using DigBuild.Ui;

namespace DigBuild
{
    public sealed class PlayerController : ICraftingInventory, ICraftingInput
    {
        private const float Gravity = 2.5f * TickSource.TickDurationSeconds;
        private const float TerminalVelocity = 0.5f;
        private const float GroundDragFactor = 0.2F;
        private const float AirDragFactor = 0.3F;

        private const float JumpForce = 12f * TickSource.TickDurationSeconds;
        private const float JumpKickSpeed = 0.8f * TickSource.TickDurationSeconds;
        private const float MovementSpeedGround = 6 * TickSource.TickDurationSeconds;
        private const float MovementSpeedAir = 5 * TickSource.TickDurationSeconds;
        private const float CameraSpeed = 4 * TickSource.TickDurationSeconds;

        private static readonly AABB PlayerBounds = new(-0.4f, 0, -0.4f, 0.4f, 1.85f, 0.4f);

        private readonly IWorld _world;
        private IChunkLoadingTicket? _chunkLoadingTicket;
        
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

        public InventorySlot[] Hotbar { get; } = { new(), new(), new(), new(), new() };
        public uint ActiveHotbarSlot { get; set; } = 0;
        public ref InventorySlot Hand => ref Hotbar[ActiveHotbarSlot];

        public InventorySlot PickedItem { get; } = new();
        public bool HotbarTransfer { get; set; }


        
        public IReadOnlyList<IInventorySlot> ShapedSlots { get; } = new InventorySlot[]{ new(), new(), new(), new(), new(), new(), new() };
        public IReadOnlyList<IInventorySlot> ShapelessSlots { get; } = new InventorySlot[]{ new(), new(), new(), new() };
        public IInventorySlot CatalystSlot { get; } = new InventorySlot();
        public IInventorySlot OutputSlot { get; } = new InventorySlot();

        public ItemInstance GetCatalyst() => CatalystSlot.Item;
        public ItemInstance GetShaped(byte slot) => ShapedSlots[slot].Item;
        public ItemInstance GetShapeless(byte slot) => ShapelessSlots[slot].Item;

        private void UpdateCraftingInventory()
        {
            var result = Game.RecipeLookup.Find(this);
            OutputSlot.Item = result.HasValue ? result.Value.Output.Output : ItemInstance.Empty;
        }



        public PlayerCamera GetCamera(float partialTick)
        {
            return new(
                PrevPosition + PrevVelocity * partialTick + Vector3.UnitY * 1.75f,
                PrevPitch + AngularVelocityPitch * partialTick,
                PrevYaw + AngularVelocityYaw * partialTick,
                MathF.PI / 2
            );
        }

        public PlayerController(IWorld world, Vector3 position)
        {
            _world = world;
            Position = position;
            
            foreach (var slot in ShapedSlots)
                slot.Changed += UpdateCraftingInventory;
            foreach (var slot in ShapelessSlots)
                slot.Changed += UpdateCraftingInventory;
            CatalystSlot.Changed += UpdateCraftingInventory;
        }

        public void CycleHotbar(int amount)
        {
            var hotbarLength = Hotbar.Length;
            ActiveHotbarSlot = (uint) ((ActiveHotbarSlot + hotbarLength + (amount % hotbarLength)) % hotbarLength);
        }

        public void TransferHotbarUp()
        {
            if (PickedItem.Item.Count > 0) return;
            PickedItem.Item = Hand.Item;
            Hand.Item = ItemInstance.Empty;
            HotbarTransfer = true;
        }

        public void TransferHotbarDown()
        {
            if (PickedItem.Item.Count == 0) return;
            var hand = Hand.Item;
            Hand.Item = PickedItem.Item;
            PickedItem.Item = hand;
            HotbarTransfer = true;
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
                    PrevPitch + AngularVelocityPitch,
                    MathF.PI / 2
                )
            );
            AngularVelocityPitch = Pitch - PrevPitch;
            Yaw = (MathF.PI * 3 + PrevYaw + AngularVelocityYaw) % (MathF.PI * 2) - MathF.PI;
        }

        public void ApplyMotion(float forwardMotion, float sidewaysMotion)
        {
            Velocity += Vector3.Transform(
                new Vector3(sidewaysMotion, 0, forwardMotion),
                Matrix4x4.CreateRotationY(Yaw)
            ) * (!OnGround ? MovementSpeedAir : MovementSpeedGround);
        }

        public void Jump(float forwardMotion)
        {
            if (!OnGround)
                return;
            OnGround = false;
            Velocity += Vector3.Transform(
                new Vector3(0, 0, forwardMotion),
                Matrix4x4.CreateRotationY(Yaw)
            ) * JumpKickSpeed;
            Velocity += Vector3.UnitY * JumpForce;
        }

        public void Move()
        {
            OnGround = false;
            
            Velocity += -Vector3.UnitY * Gravity;
            
            var vel = Velocity;

            List<(ICollider Collider, AABB RelativeBounds, Vector3 Intersection)> colliders = new(), colliders2 = new();
            var translatedBounds = PlayerBounds + Position;
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
            
            var yVel = OnGround ? 0 : Math.Max(vel.Y, -TerminalVelocity);
            var dragFactor = OnGround ? GroundDragFactor : AirDragFactor;
            Position += vel;
            PrevVelocity = vel;
            Velocity = new Vector3(vel.X * dragFactor, yVel, vel.Z * dragFactor);

            var prevChunkPos = new BlockPos(PrevPosition).ChunkPos;
            var newChunkPos = new BlockPos(Position).ChunkPos;
            if (prevChunkPos != newChunkPos)
                LoadSurroundingChunks();
        }

        public void LoadSurroundingChunks()
        {
            var chunkPos = new BlockPos(Position).ChunkPos;
            var chunksToLoad = new HashSet<ChunkPos>();
            for (var x = -Game.ViewRadius; x < Game.ViewRadius; x++)
            for (var z = -Game.ViewRadius; z < Game.ViewRadius; z++)
                chunksToLoad.Add(new ChunkPos(chunkPos.X + x, 0, chunkPos.Z + z));

            var prevTicket = _chunkLoadingTicket;
            if (!_world.ChunkManager.RequestLoadingTicket(out _chunkLoadingTicket, chunksToLoad))
            {
                throw new Exception("What.");
            }
            prevTicket?.Release();
        }
    }

    public sealed class PlayerCamera : ICamera
    {
        public float FieldOfView { get; }

        public Vector3 Position { get; }

        public float Pitch { get; }
        public float Yaw { get; }

        public PlayerCamera(Vector3 position, float pitch, float yaw, float fieldOfView)
        {
            Position = position;
            Pitch = pitch;
            Yaw = yaw;
            FieldOfView = fieldOfView;
        }

        public Raycast.Ray Ray => new(Position, Forward * 5f);

        public Matrix4x4 Transform => 
            Matrix4x4.CreateTranslation(-Position) *
            Matrix4x4.CreateRotationY(MathF.PI - Yaw) *
            Matrix4x4.CreateRotationX(-Pitch);
        
        public Vector3 Forward => Vector3.TransformNormal(
            Vector3.UnitZ,
            Matrix4x4.CreateRotationX(-Pitch)
            * Matrix4x4.CreateRotationY(Yaw)
        );
        public Vector3 Up => Vector3.TransformNormal(
            Vector3.UnitY,
            Matrix4x4.CreateRotationX(-Pitch)
            * Matrix4x4.CreateRotationY(Yaw)
        );
    }
}