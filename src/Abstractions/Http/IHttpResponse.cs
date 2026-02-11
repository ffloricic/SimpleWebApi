namespace Abstractions.Http
{
    public interface IHttpResponse
    {
        int StatusCode { get; set; }
        IDictionary<string, IList<string>> Headers { get; }
        public Stream Body { get; }
    }
}
