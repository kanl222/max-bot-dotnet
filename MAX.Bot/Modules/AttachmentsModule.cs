using System.Net.Http.Headers;
using MAX.Bot.Exceptions;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class AttachmentsModule : IAttachmentsModule
{
#if !NET462
    private static readonly HttpClient DownloadHttpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    })
    {
        Timeout = TimeSpan.FromSeconds(120)
    };
#else
    private static readonly HttpClient DownloadHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(120)
    };
#endif

    private readonly MaxBotCore _core;

    internal AttachmentsModule(MaxBotCore core) => _core = core;

    public async Task<VideoInfoResponse> GetVideoAsync(
        string videoToken,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
    {
        ValidateVideoToken(videoToken);
        return await _core.RetryWhileAttachmentNotReadyAsync(
            () => GetVideoInfoAsync(videoToken, cancellationToken),
            retryOptions,
            cancellationToken);
    }

    private static void ValidateVideoToken(string videoToken)
    {
        if (string.IsNullOrWhiteSpace(videoToken))
            throw new ArgumentException("Токен видео-вложения не должен быть пустым.", nameof(videoToken));

        if (!videoToken.All(c => Char_IsAsciiLetterOrDigit(c) || c is '_' or '-'))
            throw new ArgumentException("Токен видео-вложения может содержать только латинские буквы, цифры, '_' и '-'.", nameof(videoToken));
    }

    private Task<VideoInfoResponse> GetVideoInfoAsync(string videoToken, CancellationToken cancellationToken) =>
        _core.SendRequestAsync<VideoInfoResponse>(
            HttpMethod.Get, $"/videos/{Uri.EscapeDataString(videoToken)}", null, cancellationToken);

    public async Task<string?> TryGetAttachmentDownloadUrlAsync(
        Attachment attachment,
        VideoQuality videoQuality = VideoQuality.Mp4_480,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(attachment);

        try
        {
            return await GetAttachmentDownloadUrlAsync(attachment, videoQuality, cancellationToken);
        }
        catch (MaxBotClientException ex) when (ex.IsAttachmentNotReady)
        {
            return null;
        }
    }

    public async Task<AttachmentDownloadResult> DownloadAttachmentAsync(
        Attachment attachment,
        DownloadAttachmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(attachment);
        options ??= new DownloadAttachmentOptions();

        var maxAttempts = Math.Max(1, options.MaxAttempts);

        string? url = null;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            url = await TryGetAttachmentDownloadUrlAsync(attachment, options.VideoQuality, cancellationToken);
            if (!string.IsNullOrWhiteSpace(url)) break;
            if (attempt < maxAttempts - 1)
                await Task.Delay(options.RetryDelay, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new MaxBotClientException(
                "Вложение ещё не готово к скачиванию или URL недоступен.",
                HttpStatusCode.ServiceUnavailable,
                isAttachmentNotReady: true);
        }

        var fileName = ResolveDownloadFileName(options, url, attachment);

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
#if NET462
            var content = await DownloadHttpClient.GetByteArrayAsync(url).ConfigureAwait(false);
#else
            var content = await DownloadHttpClient.GetByteArrayAsync(url, cancellationToken);
#endif
            return new AttachmentDownloadResult { Content = content, FileName = fileName };
        }

        var savedFilePath = ResolveSavedFilePath(options.FilePath, url, attachment, options.DefaultExtension);
        var directory = Path.GetDirectoryName(savedFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        // Потоковое скачивание непосредственно в файл без загрузки всего файла в RAM
        using (var response = await DownloadHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
            using Stream streamToWriteTo = File.Create(savedFilePath);
#if NET462
            await streamToReadFrom.CopyToAsync(streamToWriteTo);
#else
            await streamToReadFrom.CopyToAsync(streamToWriteTo, cancellationToken);
#endif
        }

        return new AttachmentDownloadResult
        {
            SavedFilePath = savedFilePath,
            FileName = Path.GetFileName(savedFilePath),
        };
    }

    private static string ResolveDownloadFileName(DownloadAttachmentOptions options, string url, Attachment attachment)
    {
        var preferredFileName = attachment.GetFileName();
        if (!string.IsNullOrWhiteSpace(preferredFileName)) return preferredFileName;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var fileName = Path.GetFileName(uri.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(fileName)) return fileName;
        }

        var extension = options.DefaultExtension ?? GetDefaultExtension(attachment) ?? ".bin";
        if (!extension.StartsWith('.')) extension = "." + extension;
        return $"download{extension}";
    }

    private static string ResolveSavedFilePath(string filePath, string url, Attachment attachment, string? defaultExtension)
    {
        if (HasRecognizedFileExtension(filePath)) return filePath;

        var extension = Path.GetExtension(attachment.GetFileName() ?? string.Empty);
        if (string.IsNullOrWhiteSpace(extension) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            extension = Path.GetExtension(uri.AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = defaultExtension ?? GetDefaultExtension(attachment) ?? ".bin";
        if (!extension.StartsWith('.')) extension = "." + extension;
        return filePath + extension;
    }

    private static bool HasRecognizedFileExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension) || extension.Length < 2 || extension.Length > 11) return false;
        for (var i = 1; i < extension.Length; i++)
            if (!char.IsLetterOrDigit(extension[i])) return false;
        return true;
    }

    private static string? GetDefaultExtension(Attachment attachment) => attachment switch
    {
        ImageAttachment => ".jpg",
        VideoAttachment => ".mp4",
        FileAttachment  => ".bin",
        _               => null,
    };

    private async Task<string?> GetAttachmentDownloadUrlAsync(
        Attachment attachment,
        VideoQuality videoQuality,
        CancellationToken cancellationToken)
    {
        switch (attachment)
        {
            case ImageAttachment image: return image.Payload.Url;
            case FileAttachment file:   return file.Payload.Url;
            case ShareAttachment share: return share.Payload.Url;
            case VideoAttachment video:
                return (await GetVideoInfoAsync(video.Payload.Token, cancellationToken))
                    .TryGetDownloadUrl(videoQuality);
            default:
                throw new NotSupportedException(
                    $"Скачивание не поддерживается для вложения типа {attachment.GetType().Name}.");
        }
    }

    [Obsolete("Используйте UploadAsync вместо UploadsAsync.")]
    public Task<string> UploadsAsync(UploadRequest request, CancellationToken cancellationToken = default)
        => UploadAsync(request, cancellationToken);

    public async Task<string> UploadAsync(UploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var uploadType = request.GetUploadTypeValue();
        var uploadInfo = await _core.SendRequestAsync<UploadResponse>(
            HttpMethod.Post,
            $"/uploads?type={HttpUtility_UrlEncode(uploadType)}",
            null,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(uploadInfo.Url))
            throw new InvalidOperationException("API MAX вернул пустой URL для загрузки файла");

        var uploadUri = CreateUploadUri(uploadInfo.Url);
        var uploadResponse = await UploadFileAsync(uploadUri, request, cancellationToken);
        return ResolveUploadToken(request, uploadInfo, uploadResponse);
    }

    private async Task<UploadResponse> UploadFileAsync(
        Uri uploadUri,
        UploadRequest request,
        CancellationToken cancellationToken)
    {
        using var fileContent = new StreamContent(new NonDisposingStream(request.Content!));

        if (!string.IsNullOrWhiteSpace(request.ContentType))
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);

        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "data", request.FileName);

        var response = await _core.HttpClient.PostAsync(uploadUri, multipartContent, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new MaxBotClientException($"HTTP {response.StatusCode}: {responseContent}", response.StatusCode);

        if (string.IsNullOrWhiteSpace(responseContent))
            return new UploadResponse();

        if (!IsJsonResponse(responseContent))
        {
            if (request.Type is UploadType.Video or UploadType.Audio)
                return new UploadResponse();

            throw new InvalidOperationException(
                $"API MAX вернул не JSON-ответ после загрузки файла: {CreateResponsePreview(responseContent)}");
        }

        return JsonSerializer.Deserialize<UploadResponse>(responseContent, MaxBotCore.DeserializeOptions)
            ?? throw new InvalidOperationException("Не удалось десериализовать ответ загрузки файла");
    }

    private static Uri CreateUploadUri(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uploadUri) ||
            (uploadUri.Scheme != Uri.UriSchemeHttps && uploadUri.Scheme != Uri.UriSchemeHttp))
            throw new InvalidOperationException("API MAX вернул некорректный URL для загрузки файла");

        return uploadUri;
    }

    private static string ResolveUploadToken(UploadRequest request, UploadResponse uploadInfo, UploadResponse uploadResponse)
    {
        var token = request.Type switch
        {
            UploadType.Video or UploadType.Audio => uploadInfo.Token ?? uploadResponse.Token,
            UploadType.Image or UploadType.File  => uploadResponse.Token ?? uploadInfo.Token,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(token)) return token!;

        var expectedStep = request.Type is UploadType.Video or UploadType.Audio
            ? "первого шага загрузки video/audio"
            : "ответа загрузки image/file";

        throw new InvalidOperationException($"API MAX не вернул token из {expectedStep}.");
    }

    private static bool IsJsonResponse(string responseContent)
    {
        var trimmed = responseContent.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }

    private static string CreateResponsePreview(string responseContent)
    {
        const int maxLength = 200;
        var normalized = responseContent.Replace("\r", string.Empty).Replace("\n", " ");
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength] + "...";
    }

    private sealed class NonDisposingStream : Stream
    {
        private readonly Stream _inner;
        public NonDisposingStream(Stream inner) => _inner = inner;

        public override bool CanRead  => _inner.CanRead;
        public override bool CanSeek  => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length   => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken ct) => _inner.FlushAsync(ct);

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) =>
            _inner.ReadAsync(buffer, offset, count, ct);

#if !NET462
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) =>
            _inner.ReadAsync(buffer, ct);
#endif

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct) =>
            _inner.WriteAsync(buffer, offset, count, ct);

#if !NET462
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) =>
            _inner.WriteAsync(buffer, ct);
#endif

        protected override void Dispose(bool disposing) { }
    }
}
