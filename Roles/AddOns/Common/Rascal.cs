using static TOZ.Options;

namespace TOZ.AddOns.Common
{
    internal class Rascal : IAddon
    {
        public AddonTypes Type => AddonTypes.Harmful;

        public void SetupCustomOption()
        {
            SetupAdtRoleOptions(15600, CustomRoles.Rascal, canSetNum: true, tab: TabGroup.Addons, teamSpawnOptions: true);
        }
    }
}