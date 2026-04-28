using System;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;
using Vintagestory.ServerMods;
using WearAndTear.Config.Client;

namespace WearAndTear.Code.Rendering;

public class WindmillRenderer : MechBlockRenderer
{
    public static void UpdateForDurability(Shape shape, int randomNoise, int durability)
    {
        if (durability == 0)
        {
            RecursiveRemoveCloth(shape.Elements, 0);
        }
        else if (WearAndTearClientConfig.Instance.WindmillRotorDecayAutoGenShapes)
        {
            //Creating seeded random so it will be consistent
            var random = new Random(randomNoise); //55

            RecursiveRemoveCloth(shape.Elements, durability / 100d, random);
        }
    }

    public static bool ElementIsCloth(ShapeElement element)
    {
        if(element.Name is not null && element.Name.StartsWith("cloth", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        foreach(var face in element.FacesResolved!)
        {
            if(face is not null && face.Enabled && face.Texture.StartsWith("cloth", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public static ShapeElement[] RecursiveRemoveCloth(ShapeElement[] children, double durability, Random? random = null)
    {
        if(durability <= 0) random = null;

        foreach(var child in children)
        {
            if (child.FacesResolved is not null && ElementIsCloth(child) && (random is null || durability < random.NextDouble()))
            {
                for(var i = 0; i < child.FacesResolved.Length; i++)
                {
                    child.FacesResolved[i] = null!;
                }
            }

            if (child.Children is not null) child.Children = RecursiveRemoveCloth(child.Children, durability, random);
        }

        return children;
    }

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

        Vec3f rot = new(shapeLoc.rotateX, shapeLoc.rotateY, shapeLoc.rotateZ);

        capi.Tesselator.TesselateShape(textureSoureBlock, shape, out MeshData blockMesh, rot, null, null);
        if (shapeLoc.Overlays != null)
        {
            for (int i = 0; i < shapeLoc.Overlays.Length; i++)
            {
                CompositeShape ovShapeCmp = shapeLoc.Overlays[i];
                rot = new Vec3f(ovShapeCmp.rotateX, ovShapeCmp.rotateY, ovShapeCmp.rotateZ);
                Shape ovshape = Shape.TryGet(capi, ovShapeCmp.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                capi.Tesselator.TesselateShape(textureSoureBlock, ovshape, out MeshData overlayMesh, rot, null, null);
                blockMesh.AddMeshData(overlayMesh);
            }
        }
        blockMesh.CustomFloats = matrixAndLightFloats = new CustomMeshDataPartFloat(202000)
        {
            Instanced = true,
            InterleaveOffsets =
            [
                0,
                16,
                32,
                48,
                64
            ],
            InterleaveSizes =
            [
                4,
                4,
                4,
                4,
                4
            ],
            InterleaveStride = 80,
            StaticDraw = false
        };
        blockMesh.CustomFloats.SetAllocationSize(202000);
        blockMeshRef = capi.Render.UploadMesh(blockMesh);
    }

    protected override void UpdateLightAndTransformMatrix(int index, Vec3f distToCamera, float rotRad, IMechanicalPowerRenderable dev)
    {
        float rotX = rotRad * dev.AxisSign[0];
        float rotY = rotRad * dev.AxisSign[1];
        float rotZ = rotRad * dev.AxisSign[2];
        if (dev is BEBehaviorMPToggle tog && rotX == 0f ^ tog.IsRotationReversed())
        {
            rotY = 3.1415927f;
            rotZ = -rotZ;
        }
        UpdateLightAndTransformMatrix(matrixAndLightFloats.Values, index, distToCamera, dev.LightRgba, rotX, rotY, rotZ);
    }

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

    private readonly CustomMeshDataPartFloat matrixAndLightFloats;

    private readonly MeshRef blockMeshRef;
}