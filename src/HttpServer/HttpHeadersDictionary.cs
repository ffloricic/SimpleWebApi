using System.Collections.Specialized;

namespace HttpServer
{
    public sealed class HttpHeadersDictionary: HttpBaseDictionary 
    {
        public HttpHeadersDictionary(NameValueCollection headers): base()
        {
            ArgumentNullException.ThrowIfNull(headers);

            foreach (string? key in headers.AllKeys) {
                if (key is null)
                    continue;

                var values = headers.GetValues(key);

                Add(key, values);
                
                }
        }
    }
}
