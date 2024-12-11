using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Rendering
{
    public class WindmillRenderer : MechBlockRenderer
    {
        public void UpdateForDurability(Shape shape, int durability)
        {
            if (durability == 0)
            {
                RecursiveRemoveCloth(shape.Elements);
            }
            else if(WearAndTearModSystem.Config.SpecialParts.WindmillRotorDecayAutoGenShapes)
            {
                //Creating seeded random so it will be consistent
                var random = new Random(55); //55
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

                var lastPart = parts[^1];
                if(lastPart == "rolledup") lastPart = "0";

                loc.Path = string.Join("-", parts[..^1]);
                shape = Shape.TryGet(capi, loc.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                if (int.TryParse(lastPart, out int durability))
                {
                    UpdateForDurability(shape, durability);
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
            blockMesh.CustomFloats = (this.matrixAndLightFloats = new CustomMeshDataPartFloat(202000)
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
            });
            blockMesh.CustomFloats.SetAllocationSize(202000);
            this.blockMeshRef = capi.Render.UploadMesh(blockMesh);
        }

        // Token: 0x06002330 RID: 9008 RVA: 0x00136BDC File Offset: 0x00134DDC
        protected override void UpdateLightAndTransformMatrix(int index, Vec3f distToCamera, float rotation, IMechanicalPowerRenderable dev)
        {
            float rotX = rotation * (float)dev.AxisSign[0];
            float rotY = rotation * (float)dev.AxisSign[1];
            float rotZ = rotation * (float)dev.AxisSign[2];
            BEBehaviorMPToggle tog = dev as BEBehaviorMPToggle;
            if (tog != null && (rotX == 0f ^ tog.isRotationReversed()))
            {
                rotY = 3.1415927f;
                rotZ = -rotZ;
            }
            this.UpdateLightAndTransformMatrix(this.matrixAndLightFloats.Values, index, distToCamera, dev.LightRgba, rotX, rotY, rotZ);
        }

        // Token: 0x06002331 RID: 9009 RVA: 0x00136C54 File Offset: 0x00134E54
        public override void OnRenderFrame(float deltaTime, IShaderProgram prog)
        {
            this.UpdateCustomFloatBuffer();
            if (this.quantityBlocks > 0)
            {
                this.matrixAndLightFloats.Count = this.quantityBlocks * 20;
                this.updateMesh.CustomFloats = this.matrixAndLightFloats;
                this.capi.Render.UpdateMesh(this.blockMeshRef, this.updateMesh);
                this.capi.Render.RenderMeshInstanced(this.blockMeshRef, this.quantityBlocks);
            }
        }

        // Token: 0x06002332 RID: 9010 RVA: 0x00136CCD File Offset: 0x00134ECD
        public override void Dispose()
        {
            base.Dispose();
            MeshRef meshRef = this.blockMeshRef;
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