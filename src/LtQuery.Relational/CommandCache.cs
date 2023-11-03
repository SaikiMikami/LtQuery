using System.Data.Common;

namespace LtQuery.Relational;

class CommandCache : IDisposable
{
    public IReadOnlyList<DbCommand>? SelectCommands { get; set; }
    public IReadOnlyList<DbCommand>? SignleCommands { get; set; }
    public IReadOnlyList<DbCommand>? FirstCommands { get; set; }
    public IReadOnlyList<DbCommand>? CountCommands { get; set; }

    public void Dispose()
    {
        var commands = SelectCommands;
        if (commands != null)
            foreach (var command in commands)
                command?.Dispose();

        commands = SignleCommands;
        if (commands != null)
            foreach (var command in commands)
                command?.Dispose();

        commands = FirstCommands;
        if (commands != null)
            foreach (var command in commands)
                command?.Dispose();

        commands = CountCommands;
        if (commands != null)
            foreach (var command in commands)
                command?.Dispose();
    }
}
