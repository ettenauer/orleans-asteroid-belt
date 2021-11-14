using System;

namespace Orleans.AsteriodBelt.Grains;
public static class Constants
{
    public const string StreamProvider = "all";
    public const string StateStreamNamespace = "default";
    public static Guid StateStreamId = Guid.NewGuid();
    public const string HubGroupId = "all";
}

