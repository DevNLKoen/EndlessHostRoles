using static TOZ.Options;

namespace TOZ.AddOns.Common
{
    internal class Fool : IAddon
    {
        public AddonTypes Type => AddonTypes.Harmful;

        public void SetupCustomOption()
        {
            SetupAdtRoleOptions(19200, CustomRoles.Fool, canSetNum: true, teamSpawnOptions: true);
        }
    }
}