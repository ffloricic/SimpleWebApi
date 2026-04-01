namespace Abstractions.Http
{
    public enum HttpRequestMethod
    {
        GET,
        POST,
        PUT,
        DELETE        
    }
    public enum ServerState
    {
        Created,
        Running,
        Stopping,
        Stopped
    }

    public enum ShutdownMode
    {
        Graceful,
        Immediate
    }
}
