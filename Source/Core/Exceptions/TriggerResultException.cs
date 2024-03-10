namespace Sanguosha.Core.Exceptions;

public enum TriggerResult
{
    Retry,
    Fail,
    Success,
    End,
    Abort,
}
public class TriggerResultException(TriggerResult r) : SgsException
{
    public TriggerResult Status { get; set; } = r;
}
