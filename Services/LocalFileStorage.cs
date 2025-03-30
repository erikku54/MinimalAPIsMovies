namespace MinimalAPIsMovies.Services;

public class LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : IFileStorage
{
    public Task Delete(string? route, string container)
    {
        if (string.IsNullOrEmpty(route))
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(route);
        var filePath = Path.Combine(env.WebRootPath, container, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public async Task<string> Store(string container, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        string folder = Path.Combine(env.WebRootPath, container);

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        string filePath = Path.Combine(folder, fileName);
        // using (var stream = new FileStream(filePath, FileMode.Create))
        // {
        //     await file.CopyToAsync(stream);
        // }
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            var content = stream.ToArray();
            await File.WriteAllBytesAsync(filePath, content);
        }

        var scheme = httpContextAccessor.HttpContext!.Request.Scheme;
        var host = httpContextAccessor.HttpContext!.Request.Host;
        var url = $"{scheme}://{host}/{container}/{fileName}";

        return url;
    }
}
