using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace leadme_api
{
    public class PipeClient
    {
        //This is used to talk to other applications runnings a pipe server.
        public static string PipeName { private get; set; } = "leadme_api";

        private NamedPipeClientStream _client;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private LogCallBack _logHandler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public PipeClient(LogCallBack logHandler, int ReceiveTimeout)
        {
            _logHandler = logHandler;
            _receiveTimeout = ReceiveTimeout;
        }

        //A timeout in seconds for the pipe to wait when connecting to a server
        private int _receiveTimeout { get; set; }

        public void Send(string message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Connect();
                    await SendToApplication(message);
                    _logHandler(await ReceiveFromApplication());
                }
                catch (Exception e)
                {
                    _logHandler(e.Message);
                }
                finally
                {
                    if (_client != null)
                        _client.Dispose();
                }
            });
        }

        private async Task Connect()
        {
            _client = new NamedPipeClientStream(PipeName);
            await _client.ConnectAsync(1000);
        }

        private async Task SendToApplication(string message)
        {
            if (_client == null)
                throw new InvalidOperationException();
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client.WriteAsync(bytes, 0, bytes.Length);
            await _client.FlushAsync();
        }

        private async Task<string> ReceiveFromApplication()
        {
            if (_client == null)
                throw new InvalidOperationException();
            var buffer = new byte[8192];
            var cancel = new CancellationTokenSource();
            if (_receiveTimeout > 0)
                cancel.CancelAfter(_receiveTimeout);
            var len = await _client.ReadAsync(buffer, 0, buffer.Length, cancel.Token);
            return Encoding.UTF8.GetString(buffer, 0, len);
        }
    }
}
