using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models.Comments;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

/// <summary>
/// Реализация модуля работы с комментариями.
/// </summary>
internal sealed class CommentsModule : ICommentsModule
{
    private readonly IMaxApiTransport _core;

    internal CommentsModule(IMaxApiTransport core) => _core = core;

    public async Task<CommentMessageList> GetCommentsAsync(
        GetCommentsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var queryParams = new Dictionary<string, string>();

        if (request.CommentIds is { Count: > 0 })
            queryParams["comment_ids"] = string.Join(",", request.CommentIds);

        if (request.Before.HasValue)
            queryParams["before"] = request.Before.Value.ToString();

        if (request.After.HasValue)
            queryParams["after"] = request.After.Value.ToString();

        if (request.Count.HasValue)
            queryParams["count"] = request.Count.Value.ToString();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<CommentMessageList>(
            HttpMethod.Get,
            $"/messages/{Uri.EscapeDataString(request.MessageId)}/comments{queryString}",
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<CommentMessage> GetCommentByIdAsync(
        string messageId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("ID сообщения не должен быть пустым.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(commentId))
            throw new ArgumentException("ID комментария не должен быть пустым.", nameof(commentId));

        return await _core.SendRequestAsync<CommentMessage>(
            HttpMethod.Get,
            $"/messages/{Uri.EscapeDataString(messageId)}/comments/{Uri.EscapeDataString(commentId)}",
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<SendCommentResult> SendCommentAsync(
        SendCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var queryParams = new Dictionary<string, string>();

        if (request.DisableLinkPreview.HasValue)
            queryParams["disable_link_preview"] = request.DisableLinkPreview.Value.ToString().ToLower();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<SendCommentResult>(
            HttpMethod.Post,
            $"/messages/{Uri.EscapeDataString(request.MessageId)}/comments{queryString}",
            request.Body,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<SendCommentResult> SendCommentAsync(
        string messageId,
        string text,
        CancellationToken cancellationToken = default)
    {
        return SendCommentAsync(new SendCommentRequest
        {
            MessageId = messageId,
            Body = new NewCommentBody { Text = text }
        }, cancellationToken);
    }

    public async Task<BaseResponse> EditCommentAsync(
        EditCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var queryParams = new Dictionary<string, string>
        {
            ["comment_id"] = request.CommentId
        };

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Put,
            $"/messages/{Uri.EscapeDataString(request.MessageId)}/comments{queryString}",
            request.Body,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<BaseResponse> EditCommentAsync(
        string messageId,
        string commentId,
        string text,
        CancellationToken cancellationToken = default)
    {
        return EditCommentAsync(new EditCommentRequest
        {
            MessageId = messageId,
            CommentId = commentId,
            Body = new NewCommentBody { Text = text }
        }, cancellationToken);
    }

    public async Task<BaseResponse> DeleteCommentAsync(
        DeleteCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var queryParams = new Dictionary<string, string>
        {
            ["comment_id"] = request.CommentId
        };

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete,
            $"/messages/{Uri.EscapeDataString(request.MessageId)}/comments{queryString}",
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<BaseResponse> DeleteCommentAsync(
        string messageId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        return DeleteCommentAsync(new DeleteCommentRequest
        {
            MessageId = messageId,
            CommentId = commentId
        }, cancellationToken);
    }
}
