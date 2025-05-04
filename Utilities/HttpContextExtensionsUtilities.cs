namespace MinimalAPIsMovies.Utilities;

public static class HttpContextExtensionsUtilities
{
    public static T ExtractValueOrDefault<T>(this HttpContext context, string field, T defaultValue)
        where T : IParsable<T>
    {
        var value = context.Request.Query[field];

        return T.TryParse(value, null, out var result) ? result : defaultValue;
    }
}
