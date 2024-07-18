using static TOZ.Options;

namespace TOZ.AddOns.Common
{
    internal class DualPersonality : IAddon
    {
        public AddonTypes Type => AddonTypes.Mixed;

        public void SetupCustomOption()
        {
            SetupAdtRoleOptions(14700, CustomRoles.DualPersonality, canSetNum: true, teamSpawnOptions: true);
            DualVotes = new BooleanOptionItem(14712, "DualVotes", true, TabGroup.Addons)
                .SetParent(CustomRoleSpawnChances[CustomRoles.DualPersonality]);
        }
    }
}