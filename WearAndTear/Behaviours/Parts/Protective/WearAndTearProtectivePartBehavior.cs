﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Behaviours.Parts.Abstract;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours.Parts.Protective
{
    public class WearAndTearProtectivePartBehavior : WearAndTearPartBehavior , IWearAndTearProtectivePart
    {
        public WearAndTearProtectivePartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public WearAndTearProtectivePartProps ProtectiveProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            ProtectiveProps = properties.AsObject<WearAndTearProtectivePartProps>() ?? new();
        }
    }
}
