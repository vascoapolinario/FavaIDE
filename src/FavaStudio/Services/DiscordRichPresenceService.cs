using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace FavaStudio.Services;

public sealed class DiscordRichPresenceService : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly long _startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private NamedPipeClientStream? _pipe;
    private string _connectedClientId = "";

    public async Task SetPresenceAsync(string clientId, string details, string state, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureConnectedAsync(clientId.Trim(), cancellationToken);
            if (_pipe is null || !_pipe.IsConnected)
                return;

            var activity = new Dictionary<string, object?>
            {
                ["timestamps"] = new Dictionary<string, object?> { ["start"] = _startedAt },
                ["instance"] = false
            };
            if (!string.IsNullOrWhiteSpace(details))
                activity["details"] = Limit(details, 128);
            if (!string.IsNullOrWhiteSpace(state))
                activity["state"] = Limit(state, 128);

            await SendActivityAsync(activity, cancellationToken);
        }
        catch
        {
            Disconnect();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_pipe is { IsConnected: true })
                await SendActivityAsync(null, cancellationToken);
        }
        catch
        {
            // Discord may already be gone; clearing presence should never interrupt the IDE.
        }
        finally
        {
            Disconnect();
            _gate.Release();
        }
    }

    private async Task EnsureConnectedAsync(string clientId, CancellationToken cancellationToken)
    {
        if (_pipe is { IsConnected: true } && string.Equals(_connectedClientId, clientId, StringComparison.Ordinal))
            return;

        Disconnect();

        for (var i = 0; i < 10; i++)
        {
            var pipe = new NamedPipeClientStream(".", $"discord-ipc-{i}", PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await pipe.ConnectAsync(150, cancellationToken);
                _pipe = pipe;
                _connectedClientId = clientId;
                await SendFrameAsync(0, new Dictionary<string, object?>
                {
                    ["v"] = 1,
                    ["client_id"] = clientId
                }, cancellationToken);
                return;
            }
            catch
            {
                pipe.Dispose();
            }
        }
    }

    private Task SendActivityAsync(Dictionary<string, object?>? activity, CancellationToken cancellationToken) =>
        SendFrameAsync(1, new Dictionary<string, object?>
        {
            ["cmd"] = "SET_ACTIVITY",
            ["args"] = new Dictionary<string, object?>
            {
                ["pid"] = Environment.ProcessId,
                ["activity"] = activity
            },
            ["nonce"] = Guid.NewGuid().ToString("N")
        }, cancellationToken);

    private async Task SendFrameAsync(int opcode, object payload, CancellationToken cancellationToken)
    {
        if (_pipe is null)
            return;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var header = new byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(0, 4), opcode);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4, 4), bytes.Length);
        await _pipe.WriteAsync(header, cancellationToken);
        await _pipe.WriteAsync(bytes, cancellationToken);
        await _pipe.FlushAsync(cancellationToken);
    }

    private void Disconnect()
    {
        _connectedClientId = "";
        _pipe?.Dispose();
        _pipe = null;
    }

    private static string Limit(string value, int maxLength)
    {
        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public void Dispose()
    {
        Disconnect();
        _gate.Dispose();
    }
}
