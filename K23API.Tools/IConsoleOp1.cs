namespace K23API.Tools;

public interface IConsoleOp1
{
    string Label { get; }

    Task RunAsync(ConsoleOpCtx1 context, CancellationToken cancellationToken);
}
