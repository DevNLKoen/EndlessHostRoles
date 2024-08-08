using System;
using AmongUs.GameOptions;
using TOZ.Crewmate;
using TOZ.Impostor;

namespace TOZ.Modules;

public class MeetingTimeManager
{
    private static int DiscussionTime;
    private static int VotingTime;
    private static int DefaultDiscussionTime;
    private static int DefaultVotingTime;

    public static void Init()
    {
        DefaultDiscussionTime = Main.RealOptionsData.GetInt(Int32OptionNames.DiscussionTime);
        DefaultVotingTime = Main.RealOptionsData.GetInt(Int32OptionNames.VotingTime);
        Logger.Info($"DefaultDiscussionTime: {DefaultDiscussionTime}s, DefaultVotingTime: {DefaultVotingTime}s", "MeetingTimeManager.Init");
        ResetMeetingTime();
    }

    public static void ApplyGameOptions(IGameOptions opt)
    {
        opt.SetInt(Int32OptionNames.DiscussionTime, DiscussionTime);
        opt.SetInt(Int32OptionNames.VotingTime, VotingTime);
    }

    private static void ResetMeetingTime()
    {
        DiscussionTime = DefaultDiscussionTime;
        VotingTime = DefaultVotingTime;
    }

    public static void OnReportDeadBody()
    {
        if (Options.AllAliveMeeting.GetBool() && Utils.IsAllAlive)
        {
            DiscussionTime = 0;
            VotingTime = Options.AllAliveMeetingTime.GetInt();
            Logger.Info($"Discussion Time: {DiscussionTime}s, Voting Time: {VotingTime}s", "MeetingTimeManager.OnReportDeadBody");
            return;
        }

        ResetMeetingTime();
        int BonusMeetingTime = 0;


        if (BonusMeetingTime >= 0)
        {
            VotingTime += BonusMeetingTime;
        }
        else
        {
            DiscussionTime += BonusMeetingTime;
            if (DiscussionTime < 0)
            {
                VotingTime += DiscussionTime;
                DiscussionTime = 0;
            }
        }

        Logger.Info($"Discussion Time: {DiscussionTime}s, Voting Time: {VotingTime}s", "MeetingTimeManager.OnReportDeadBody");
    }
}