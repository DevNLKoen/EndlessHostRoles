using static TOZ.Options;

namespace TOZ.AddOns.Common
{
    internal class Autopsy : IAddon
    {
        public AddonTypes Type => AddonTypes.Helpful;

        public void SetupCustomOption()
        {
            SetupAdtRoleOptions(13600, CustomRoles.Autopsy, canSetNum: true, teamSpawnOptions: true);
        }
    }
}