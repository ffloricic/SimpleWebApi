using System.Collections.Specialized;
using System.Web;

namespace HttpServer
{
    public sealed class HttpQueryCollection : HttpBaseDictionary
    {
        public HttpQueryCollection(string? rawQuery) : base()
        {
            if (string.IsNullOrEmpty(rawQuery))
                return;

            NameValueCollection parsedQuery = HttpUtility.ParseQueryString(rawQuery);

            foreach (string? key in parsedQuery.Keys)
            {
                if (String.IsNullOrEmpty(key))
                    continue;

                var values = parsedQuery.GetValues(key);

                Add(key, values);
            }
        }
    }
}
