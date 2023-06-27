using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace leadme_api
{
    public class ParentPipeServer
    {
        //This is the name of the server running on the imported dll
        public static string PipeName { private get; set; } = "leadme_parent_api";

        //PIPE SERVER
        private static NamedPipeServerStream _pipe;
        private static bool _cancelled = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static LogCallBack _logHandler;
        private static ActionCallBack _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Allows a third party application using this dll to specify a log handler for printing out status and alert messages as well as an action handler
        /// that interprets messages received on this pipe server. The code runs an loop of waiting for a connection, receiving a message, sending a response
        /// before reset the pipe for the next connection.
        /// </summary>
        /// <param name="logHandler">A callback to handle status messages</param>
        /// <param name="handler"><A callback to handle incoming messages from a pipe client</param>
        public static void Run(LogCallBack logHandler, ActionCallBack handler)
        {
            _logHandler = logHandler;
            _handler = handler;
            Task.Run(() =>
            {
                Loop().ConfigureAwait(false);
            });
        }

        private static async Task Loop()
        {
            while (!_cancelled)
            {
                var message = await Receive();
                if (message == "")
                    break;

                //Send a response back to the client
                await SendToClient("Command received");

                //Pass the message of to the action handler callback
                _ = Task.Factory.StartNew(() => _handler(message));
            }
        }

        public static async Task SendToClient(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            _logHandler($"Sending bytes: {bytes.Length}");

            if (_pipe is null || !_pipe.CanWrite)
                return;
            try
            {
                await Task.Run(() => _pipe.Write(bytes, 0, bytes.Length));
            }
            catch (Exception ex)
            {
                _logHandler(ex.ToString());
            }
            finally
            {
                _pipe.Dispose();
                _pipe = null;
            }
        }

        private static async Task<string> Receive()
        {
            try
            {
                //TODO test the new transmission mode with existing applications
                //Pipe transmission mode needs to be set to Message in order to handle C++ connections
                _pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);
                return await Task.Run(() =>
                {
                    //Pull this out to a message handler
                    _logHandler("Awaiting connection...");
                    _pipe.WaitForConnection();
                    var buffer = new byte[8192];
                    var len = _pipe.Read(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer, 0, len);
                });
            }
            catch (Exception e)
            {
                _pipe?.Dispose();
                _logHandler(e.Message);
                return "";
            }
        }

        public static void Close()
        {
            if (_pipe != null && _pipe.IsConnected)
                return;
            _cancelled = true;
            try
            {
                var pipe = new NamedPipeClientStream(PipeName);
                pipe.Connect(500);
                pipe.Flush();
                pipe.Close();
                _logHandler("Pipe server closed");
            }
            catch (Exception e)
            {
                _logHandler(e.Message);
            }
            finally
            {
                _pipe?.Close();
            }
        }
    }
}
