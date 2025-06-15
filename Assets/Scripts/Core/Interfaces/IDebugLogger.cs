namespace MarioGame.Core.Interfaces
{
    public interface IDebugLogger
    {
        bool EnableDebugLogs { get; }
        void DebugLog(params object[] messages);
        void DebugLogWarning(params object[] messages);
        void DebugLogError(params object[] messages);
    }
}