using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Code.Rendering
{
    public class WindmillRenderer : MechBlockRenderer
    {
        public void UpdateForDurability(Shape shape, int randomNoise, int durability)
        {
            if (durability == 0)
            {
                RecursiveRemoveCloth(shape.Elements);
            }
            else if (WearAndTearModSystem.Config.SpecialParts.WindmillRotorDecayAutoGenShapes)
            {
                //Creating seeded random so it will be consistent
                var random = new Random(randomNoise); //55 //TODO use block position
                RecursiveRemoveCloth(shape.Elements, random, durability / 100d);
            }
        }

        public ShapeElement[] RecursiveRemoveCloth(ShapeElement[] children) =>
            children.Where(element => !element.Name.StartsWith("cloth")).Select(element =>
            {
                if (element.Children != null) element.Children = RecursiveRemoveCloth(element.Children);
                return element;
            }).ToArray();

        public ShapeElement[] RecursiveRemoveCloth(ShapeElement[] children, Random random, double durability) =>
            children.Where(element => !element.Name.StartsWith("cloth") || durability > random.NextDouble()).Select(element =>
            {
                if (element.Children != null) element.Children = RecursiveRemoveCloth(element.Children, random, durability);
                return element;
            }).ToArray();

        public WindmillRenderer(ICoreClientAPI capi, MechanicalPowerMod mechanicalPowerMod, Block textureSoureBlock, CompositeShape shapeLoc) : base(capi, mechanicalPowerMod)
        {
            AssetLocation loc = shapeLoc.Base.Clone();

            var shape = Shape.TryGet(capi, loc.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
            if (shape == null)
            {
                var parts = loc.Path.Split('-');

                int randomNoise = 0;
                var lastPart = parts[^1];

                var rolledUp = (lastPart == "rolledup");

                if (rolledUp) lastPart = "100";
                else int.TryParse(parts[^2], out randomNoise);

                loc.Path = string.Join("-", parts[..^(rolledUp ? 1 : 2)]);
                shape = Shape.TryGet(capi, loc.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                if (int.TryParse(lastPart, out int durability))
                {
                    UpdateForDurability(shape, randomNoise, durability);
                }
            }

            Vec3f rot = new Vec3f(shapeLoc.rotateX, shapeLoc.rotateY, shapeLoc.rotateZ);

            MeshData blockMesh;
            capi.Tesselator.TesselateShape(textureSoureBlock, shape, out blockMesh, rot, null, null);
            if (shapeLoc.Overlays != null)
            {
                for (int i = 0; i < shapeLoc.Overlays.Length; i++)
                {
                    CompositeShape ovShapeCmp = shapeLoc.Overlays[i];
                    rot = new Vec3f(ovShapeCmp.rotateX, ovShapeCmp.rotateY, ovShapeCmp.rotateZ);
                    Shape ovshape = Shape.TryGet(capi, ovShapeCmp.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                    MeshData overlayMesh;
                    capi.Tesselator.TesselateShape(textureSoureBlock, ovshape, out overlayMesh, rot, null, null);
                    blockMesh.AddMeshData(overlayMesh);
                }
            }
            blockMesh.CustomFloats = matrixAndLightFloats = new CustomMeshDataPartFloat(202000)
            {
                Instanced = true,
                InterleaveOffsets = new int[]
                {
                    0,
                    16,
                    32,
                    48,
                    64
                },
                InterleaveSizes = new int[]
                {
                    4,
                    4,
                    4,
                    4,
                    4
                },
                InterleaveStride = 80,
                StaticDraw = false
            };
            blockMesh.CustomFloats.SetAllocationSize(202000);
            blockMeshRef = capi.Render.UploadMesh(blockMesh);
        }

        // Token: 0x06002330 RID: 9008 RVA: 0x00136BDC File Offset: 0x00134DDC
        protected override void UpdateLightAndTransformMatrix(int index, Vec3f distToCamera, float rotation, IMechanicalPowerRenderable dev)
        {
            float rotX = rotation * dev.AxisSign[0];
            float rotY = rotation * dev.AxisSign[1];
            float rotZ = rotation * dev.AxisSign[2];
            BEBehaviorMPToggle tog = dev as BEBehaviorMPToggle;
            if (tog != null && rotX == 0f ^ tog.isRotationReversed())
            {
                rotY = 3.1415927f;
                rotZ = -rotZ;
            }
            UpdateLightAndTransformMatrix(matrixAndLightFloats.Values, index, distToCamera, dev.LightRgba, rotX, rotY, rotZ);
        }

        // Token: 0x06002331 RID: 9009 RVA: 0x00136C54 File Offset: 0x00134E54
        public override void OnRenderFrame(float deltaTime, IShaderProgram prog)
        {
            UpdateCustomFloatBuffer();
            if (quantityBlocks > 0)
            {
                matrixAndLightFloats.Count = quantityBlocks * 20;
                updateMesh.CustomFloats = matrixAndLightFloats;
                capi.Render.UpdateMesh(blockMeshRef, updateMesh);
                capi.Render.RenderMeshInstanced(blockMeshRef, quantityBlocks);
            }
        }

        // Token: 0x06002332 RID: 9010 RVA: 0x00136CCD File Offset: 0x00134ECD
        public override void Dispose()
        {
            base.Dispose();
            MeshRef meshRef = blockMeshRef;
            if (meshRef == null)
            {
                return;
            }
            meshRef.Dispose();
        }

        // Token: 0x04001200 RID: 4608
        private CustomMeshDataPartFloat matrixAndLightFloats;

        // Token: 0x04001201 RID: 4609
        private MeshRef blockMeshRef;
    }
}