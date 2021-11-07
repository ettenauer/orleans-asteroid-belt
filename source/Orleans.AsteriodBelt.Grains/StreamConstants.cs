using System;

namespace Orleans.AsteriodBelt.Grains;
public static class StreamConstants
{
    public const string StreamProvider = "all";
    public const string StateStreamNamespace = "default";
    public const string MoveStreamNamespace = "default";
    public static Guid StateStreamId = Guid.NewGuid();
    public static Guid MoveStreamId = Guid.NewGuid();
}

