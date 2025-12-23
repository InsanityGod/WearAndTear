using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using WearAndTear.Code.Blocks;

namespace WearAndTear.Code.BlockEntities
{
    public class BlockEntityCharcoalBrickPit : BlockEntity
    {
        private static readonly float BurnHours = 18;

        // Key = horizontal location
        // Value = highest Y Position
        private readonly Dictionary<BlockPos, int> smokeLocations = new();

        private double finishedAfterTotalHours;
        private double startingAfterTotalHours;

        // 0 = warmup
        // 1 = burning
        private int state;

        private string startedByPlayerUid;

        public bool Lit { get; private set; }

        public int MaxPileSize { get; set; } = 11;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.Side == EnumAppSide.Client)
            {
                RegisterGameTickListener(OnClientTick, 150);
            }
            else RegisterGameTickListener(OnServerTick, 3000);

            if (Lit) FindHolesInPit();
        }

        private void OnClientTick(float dt)
        {
            if (!Lit || Block?.ParticleProperties == null) return;

            BlockPos pos = Pos.Copy();
            foreach (var val in smokeLocations)
            {
                if (Api.World.Rand.NextDouble() >= 0.2f || Block.ParticleProperties.Length == 0) continue;


                pos.Set(val.Key.X, val.Value + 1, val.Key.Z);

                Block upblock = Api.World.BlockAccessor.GetBlock(pos);
                AdvancedParticleProperties particles = Block.ParticleProperties[0];
                particles.basePos = BEBehaviorBurning.RandomBlockPos(Api.World.BlockAccessor, pos, upblock, BlockFacing.UP);

                particles.Quantity.avg = 1;
                Api.World.SpawnParticles(particles);
                particles.Quantity.avg = 0;
            }
        }

        private void OnServerTick(float dt)
        {
            if (!Lit) return;

            if (startingAfterTotalHours <= Api.World.Calendar.TotalHours && state == 0)
            {
                finishedAfterTotalHours = Api.World.Calendar.TotalHours + BurnHours;
                state = 1;
                MarkDirty(false);
            }

            if (state == 0) return;

            List<BlockPos> holes = FindHolesInPit();

            if (holes?.Count > 0)
            {
                Block fireblock = Api.World.GetBlock(new AssetLocation("fire"));
                finishedAfterTotalHours = Api.World.Calendar.TotalHours + BurnHours;

                foreach (BlockPos holePos in holes)
                {
                    BlockPos firePos = holePos.Copy();

                    Block block = Api.World.BlockAccessor.GetBlock(holePos);
                    if (block.BlockId != 0 && block.BlockId != Block.BlockId)
                    {
                        foreach (BlockFacing facing in BlockFacing.ALLFACES)
                        {
                            facing.IterateThruFacingOffsets(firePos);  // This must be the first command in the loop, to ensure all facings will be properly looped through regardless of any 'continue;' statements
                            if (Api.World.BlockAccessor.GetBlock(firePos).BlockId == 0 && Api.World.Rand.NextDouble() > 0.9f)
                            {
                                Api.World.BlockAccessor.SetBlock(fireblock.BlockId, firePos);
                                BlockEntity befire = Api.World.BlockAccessor.GetBlockEntity(firePos);
                                befire?.GetBehavior<BEBehaviorBurning>()?.OnFirePlaced(facing, startedByPlayerUid);
                            }
                        }
                    }
                }

                return;
            }

            if (finishedAfterTotalHours <= Api.World.Calendar.TotalHours) ConvertPit();
        }

        public void IgniteNow()
        {
            if (Lit) return;

            Lit = true;

            startingAfterTotalHours = this.Api.World.Calendar.TotalHours + 0.5f;
            MarkDirty(true);

            // To popuplate the smokeLocations
            FindHolesInPit();
        }

        private void ConvertPit()
        {
            HashSet<BlockPos> positionsToConvert = new();
            HashSet<BlockPos> visitedPositions = new();
            Queue<BlockPos> bfsQueue = new();
            bfsQueue.Enqueue(Pos);

            BlockPos minPos = Pos.Copy(), maxPos = Pos.Copy();

            while (bfsQueue.Count > 0)
            {
                BlockPos bpos = bfsQueue.Dequeue();
                BlockPos npos = bpos.Copy();

                BlockPos bposGround = bpos.Copy();
                bposGround.Y = 0;

                positionsToConvert.Add(bpos);

                foreach (BlockFacing facing in BlockFacing.ALLFACES)
                {
                    facing.IterateThruFacingOffsets(npos);  // This must be the first command in the loop, to ensure all facings will be properly looped through regardless of any 'continue;' statements

                    // Only traverse inside the dry sawdustbrick pile
                    if (!BlockCharcoalBrickPit.IsDrySawdustBrickPile(Api.World, npos))
                    {
                        IWorldChunk chunk = Api.World.BlockAccessor.GetChunkAtBlockPos(npos);
                        if (chunk == null) return; // Maybe at the endge of the loaded chunk, in which case return before changing any blocks and it can be converted next tick instead
                        continue;
                    }

                    if (InCube(npos, ref minPos, ref maxPos) && !visitedPositions.Contains(npos))
                    {
                        bfsQueue.Enqueue(npos.Copy());
                        visitedPositions.Add(npos.Copy());
                    }
                }
            }

            foreach(var pos in positionsToConvert) BlockCharcoalBrickPit.ConvertPile(Api.World, pos);

            Api.World.BlockAccessor.SetBlock(0, Pos);
        }

        internal void Init(IPlayer player)
        {
            startedByPlayerUid = player?.PlayerUID;
        }

        private List<BlockPos> FindHolesInPit()
        {
            smokeLocations.Clear();

            List<BlockPos> holes = new();
            HashSet<BlockPos> visitedPositions = new();
            Queue<BlockPos> bfsQueue = new();
            bfsQueue.Enqueue(Pos);

            int charcoalBrickPitBlockId = Api.World.GetBlock("wearandtear:charcoalbrickpit").BlockId;

            BlockPos minPos = Pos.Copy(), maxPos = Pos.Copy();

            while (bfsQueue.Count > 0)
            {
                BlockPos bpos = bfsQueue.Dequeue();
                BlockPos npos = bpos.Copy();
                BlockPos bposGround = bpos.Copy();
                bposGround.Y = 0;

                smokeLocations.TryGetValue(bposGround, out int yMax);
                smokeLocations[bposGround] = Math.Max(yMax, bpos.Y);

                foreach (BlockFacing facing in BlockFacing.ALLFACES)
                {
                    facing.IterateThruFacingOffsets(npos);  // This must be the first command in the loop, to ensure all facings will be properly looped through regardless of any 'continue;' statements
                     IWorldChunk chunk = Api.World.BlockAccessor.GetChunkAtBlockPos(npos);
                    if (chunk == null) return null;

                    Block nBlock = chunk.GetLocalBlockAtBlockPos(Api.World, npos);

                    bool solid = nBlock.GetLiquidBarrierHeightOnSide(facing.Opposite, npos) == 1 || nBlock.GetLiquidBarrierHeightOnSide(facing, bpos) == 1;
                    bool isDrySawdustBrickPile = BlockCharcoalBrickPit.IsDrySawdustBrickPile(Api.World, npos);

                    if (!isDrySawdustBrickPile && nBlock.BlockId != charcoalBrickPitBlockId)
                    {
                        if (IsCombustible(npos)) holes.Add(npos.Copy());
                        else if (!solid) holes.Add(bpos.Copy());
                    }

                    // Only traverse inside the dry sawdustbrick pile
                    if (!isDrySawdustBrickPile) continue;

                    if (InCube(npos, ref minPos, ref maxPos) && !visitedPositions.Contains(npos))
                    {
                        bfsQueue.Enqueue(npos.Copy());
                        visitedPositions.Add(npos.Copy());
                    }
                }
            }

            return holes;
        }

        private bool InCube(BlockPos npos, ref BlockPos minPos, ref BlockPos maxPos)
        {
            BlockPos nmin = minPos.Copy(), nmax = maxPos.Copy();

            if (npos.X < minPos.X) nmin.X = npos.X;
            else if (npos.X > maxPos.X) nmax.X = npos.X;

            if (npos.Y < minPos.Y) nmin.Y = npos.Y;
            else if (npos.Y > maxPos.Y) nmax.Y = npos.Y;

            if (npos.Z < minPos.Z) nmin.Z = npos.Z;
            else if (npos.Z > maxPos.Z) nmax.Z = npos.Z;

            // Only traverse within maxSize range
            if (nmax.X - nmin.X + 1 <= MaxPileSize && nmax.Y - nmin.Y + 1 <= MaxPileSize && nmax.Z - nmin.Z + 1 <= MaxPileSize)
            {
                minPos = nmin.Copy();
                maxPos = nmax.Copy();
                return true;
            }

            return false;
        }

        private bool IsCombustible(BlockPos pos)
        {
            Block block = Api.World.BlockAccessor.GetBlock(pos);
            if (block.CombustibleProps != null) return block.CombustibleProps.BurnDuration > 0;

            return block is ICombustible bic && bic.GetBurnDuration(Api.World, pos) > 0;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            int beforeState = state;
            bool beforeLit = Lit;
            base.FromTreeAttributes(tree, worldAccessForResolve);

            finishedAfterTotalHours = tree.GetDouble("finishedAfterTotalHours");
            startingAfterTotalHours = tree.GetDouble("startingAfterTotalHours");

            state = tree.GetInt("state");

            startedByPlayerUid = tree.GetString("startedByPlayerUid");
            Lit = tree.GetBool("lit", true);

            if ((beforeState != state || beforeLit != Lit) && Api?.Side == EnumAppSide.Client) FindHolesInPit();
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetDouble("finishedAfterTotalHours", finishedAfterTotalHours);
            tree.SetDouble("startingAfterTotalHours", startingAfterTotalHours);
            tree.SetInt("state", state);
            tree.SetBool("lit", Lit);

            if (startedByPlayerUid != null) tree.SetString("startedByPlayerUid", startedByPlayerUid);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            double minutesLeft = 60 * (startingAfterTotalHours - Api.World.Calendar.TotalHours);

            if (Lit)
            {
                if (minutesLeft <= 0)
                {
                    dsc.AppendLine(Lang.Get("Lit."));
                }
                else dsc.AppendLine(Lang.Get("lit-starting", (int)minutesLeft));
            }
            else dsc.AppendLine(Lang.Get("Unlit."));
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Lit) return false;

            MeshData litCharcoalMesh = ObjectCacheUtil.GetOrCreate(Api, "litCharcoalMesh", () =>
            {
                ITesselatorAPI tess = ((ICoreClientAPI)Api).Tesselator;

                tess.TesselateShape(Block, Shape.TryGet(Api, "shapes/block/wood/firepit/cold-normal.json"), out MeshData mesh);

                return mesh;
            });

            mesher.AddMeshData(litCharcoalMesh);
            return true;
        }
    }
}