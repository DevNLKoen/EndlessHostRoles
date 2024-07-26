using UnityEngine;

namespace TOZ;

public static class DebugModeManager
{
    public static OptionItem EnableDebugMode;
    public static bool AmDebugger { get; private set; } =
#if DEBUG
        true;
#else
        false;
#endif
    public static bool IsDebugMode => AmDebugger && EnableDebugMode != null && EnableDebugMode.GetBool();

    public static void Auth(HashAuth auth, string input)
    {
        AmDebugger = true; //|= auth.CheckString(input);
    }

    public static void SetupCustomOption()
    {
        EnableDebugMode = new BooleanOptionItem(2, "EnableDebugMode", false, TabGroup.ZloosSettings, true)
            .SetHeader(true)
            .SetColor(Color.green)
            .SetHidden(!AmDebugger);
    }
}