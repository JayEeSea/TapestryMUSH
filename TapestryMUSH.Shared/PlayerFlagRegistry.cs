using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapestryMUSH.Shared;

public static class PlayerFlagRegistry
{
    public static readonly HashSet<string> ValidFlags = new(StringComparer.OrdinalIgnoreCase)
    {
        "WIZARD",
        "ROYALTY",
        "BUILDER",
        "VISUAL",
        "PLAYER"
    };
}
