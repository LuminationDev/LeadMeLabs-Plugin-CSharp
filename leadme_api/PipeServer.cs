using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace leadme_api
{
    public class PipeServer
    {
        //This is the name of the server running on the imported dll
        public static string PipeName { private get; set; } = "leadme_api";

        //PIPE SERVER
        private static NamedPipeServerStream _pipe;
        private static bool _cancelled = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static LogCallBack _logHandler;
        private static PauseCallBack _pauseHandler;
        private static ResumeCallBack _resumeHandler;
        private static ShutdownCallBack _shutdownHandler;
        private static DetailsCallBack _detailsHandler;
        private static ActionCallBack _handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Allows a third party application using this dll to specify a log handler for printing out status and alert messages as well as an action handler
        /// that interprets messages received on this pipe server. The code runs an loop of waiting for a connection, receiving a message, sending a response
        /// before reset the pipe for the next connection.
        /// </summary>
        /// <param name="logHandler">A callback to handle status messages</param>
        /// <param name="handler"><A callback to handle incoming messages from a pipe client</param>
        public static void Run(LogCallBack logHandler, PauseCallBack pHandler, ResumeCallBack rHandler, ShutdownCallBack sHandler, DetailsCallBack lHandler, ActionCallBack handler)
        {
            _logHandler = logHandler;
            _pauseHandler = pHandler;
            _resumeHandler = rHandler;
            _shutdownHandler = sHandler;
            _detailsHandler = lHandler;
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

                _logHandler("MESSAGE: " + message);

                //Send a response back to the client
                await SendToClient("Command received");

                //Determine the callback to run
                switch (message)
                {
                    case "pause":
                        _pauseHandler();
                        break;
                    case "resume":
                        _resumeHandler();
                        break;
                    case "shutdown":
                        _shutdownHandler();
                        break;
                    case "details":
                        _detailsHandler();
                        break;
                    default:
                        //Pass any other message off to the action handler callback
                        _handler(message);
                        break;
                }
            }
        }

        public static async Task SendToClient(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            if (_pipe is null || !_pipe.CanWrite)
                return;
            try
            {
                await Task.Run(() => _pipe.Write(bytes, 0, bytes.Length));
            }
            finally
            {
                _pipe.Dispose();
            }
        }

        private static async Task<string> Receive()
        {
            try
            {
                _pipe = new NamedPipeServerStream(PipeName);
                return await Task.Run(() =>
                {
                    //Pull this out to a message handler
                    _logHandler("Awaiting connection...");
                    var buffer = new byte[1024];
                    _pipe.WaitForConnection();
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
