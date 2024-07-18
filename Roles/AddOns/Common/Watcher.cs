﻿using static TOZ.Options;

namespace TOZ.AddOns.Common
{
    internal class Watcher : IAddon
    {
        public AddonTypes Type => AddonTypes.Helpful;

        public void SetupCustomOption()
        {
            SetupAdtRoleOptions(15000, CustomRoles.Watcher, canSetNum: true, teamSpawnOptions: true);
        }
    }
}