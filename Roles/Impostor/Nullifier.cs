using System.Collections.Generic;
using TOZ.Crewmate;
using static TOZ.Options;

namespace TOZ.Impostor
{
    internal class Nullifier : RoleBase
    {
        private const int Id = 642000;
        public static List<byte> playerIdList = [];

        public static OptionItem NullCD;
        private static OptionItem KCD;
        private static OptionItem Delay;

        public override bool IsEnable => playerIdList.Count > 0;

        public static void SetupCustomOption()
        {
            SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Nullifier);
            NullCD = new FloatOptionItem(Id + 10, "NullCD", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Nullifier])
                .SetValueFormat(OptionFormat.Seconds);
            KCD = new FloatOptionItem(Id + 11, "KillCooldown", new(0f, 180f, 2.5f), 25f, TabGroup.ImpostorRoles)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Nullifier])
                .SetValueFormat(OptionFormat.Seconds);
            Delay = new IntegerOptionItem(Id + 12, "NullifierDelay", new(0, 90, 1), 5, TabGroup.ImpostorRoles)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Nullifier])
                .SetValueFormat(OptionFormat.Seconds);
        }

        public override void Init()
        {
            playerIdList = [];
        }

        public override void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }

        public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KCD.GetFloat();

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (!IsEnable || killer == null || target == null) return false;

            return killer.CheckDoubleTrigger(target, () =>
            {
                killer.SetKillCooldown(time: NullCD.GetFloat());
                killer.Notify(Translator.GetString("NullifierUseRemoved"));
                LateTask.New(() =>
                {
                    switch (target.GetCustomRole())
                    {
                        case CustomRoles.SabotageMaster:
                            if (Main.PlayerStates[target.PlayerId].Role is not SabotageMaster { IsEnable: true } sm) return;
                            sm.UsedSkillCount++;
                            sm.SendRPC();
                            break;
                        default:
                            target.RpcRemoveAbilityUse();
                            break;
                    }

                    if (GameStates.IsInTask) Utils.NotifyRoles(SpecifySeer: target, SpecifyTarget: target);
                }, Delay.GetInt(), "Nullifier Remove Ability Use");
            });
        }
    }
}