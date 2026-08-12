using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class MessagesModule : IMessagesModule
{
    private readonly MaxBotCore _core;

    internal MessagesModule(MaxBotCore core) => _core = core;

    public async Task<SendMessageResponse> SendMessageAsync(
        SendMessageRequest request,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>();

        if (request.UserId.HasValue)
            queryParams["user_id"] = request.UserId.Value.ToString();

        if (request.ChatId.HasValue)
            queryParams["chat_id"] = request.ChatId.Value.ToString();

        if (request.DisableLinkPreview.HasValue)
            queryParams["disable_link_preview"] = request.DisableLinkPreview.Value.ToString().ToLower();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        var send = () => _core.SendRequestAsync<SendMessageResponse>(
            HttpMethod.Post, $"/messages{queryString}", request, cancellationToken);

        if (RequiresAttachmentRetry(request))
            return await _core.RetryWhileAttachmentNotReadyAsync(send, retryOptions, cancellationToken);

        return await send();
    }

    private static bool RequiresAttachmentRetry(SendMessageRequest request) =>
        request.Attachments?.Any(static a => a.RequiresUploadProcessing) == true;

    public async Task<BaseResponse> AnswerCallbackAsync(
        AnswerCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Post,
            $"/answers?callback_id={HttpUtility_UrlEncode(request.CallbackId)}",
            request,
            cancellationToken);
    }

    public async Task<GetMessagesResponse> GetMessagesAsync(
        GetMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        var queryParams = new Dictionary<string, string>();

        if (request.ChatId.HasValue)
            queryParams["chat_id"] = request.ChatId.Value.ToString();

        if (request.MessageIds is { Count: > 0 })
            queryParams["message_ids"] = string.Join(",", request.MessageIds);

        if (request.Count.HasValue)
            queryParams["count"] = request.Count.Value.ToString();

        if (request.From.HasValue)
            queryParams["from"] = request.From.Value.ToString();

        if (request.To.HasValue)
            queryParams["to"] = request.To.Value.ToString();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<GetMessagesResponse>(
            HttpMethod.Get, $"/messages{queryString}", null, cancellationToken);
    }

    public async Task<Message> GetMessageByIdAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("ID сообщения не должен быть пустым.", nameof(messageId));

        return await _core.SendRequestAsync<Message>(
            HttpMethod.Get, $"/messages/{HttpUtility_UrlEncode(messageId)}", null, cancellationToken);
    }

    public async Task<BaseResponse> EditMessageByIdAsync(
        string messageId,
        SendMessageRequest messageRequest,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("ID сообщения не должен быть пустым.", nameof(messageId));

        var edit = () => _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Put, $"/messages?message_id={HttpUtility_UrlEncode(messageId)}", messageRequest, cancellationToken);

        if (RequiresAttachmentRetry(messageRequest))
            return await _core.RetryWhileAttachmentNotReadyAsync(edit, retryOptions, cancellationToken);

        return await edit();
    }

    public async Task<BaseResponse> DeleteMessageByIdAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("ID сообщения не должен быть пустым.", nameof(messageId));

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/messages?message_id={HttpUtility_UrlEncode(messageId)}", null, cancellationToken);
    }
}
