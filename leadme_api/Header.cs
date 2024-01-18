namespace leadme_api
{
    /// <summary>
    /// Generic logger functions to print/display information to the screen.
    /// </summary>
    /// <param name="message"></param>
    public delegate void LogCallBack(string message);

    /// <summary>
    /// A basic action callback for handling messages within a program that 
    /// is importing the dynamic link library.
    /// </summary>
    /// <param name="message"></param>
    public delegate void ActionCallBack(string message);

    /// <summary>
    /// A callback that is responsible for pausing the currently running
    /// application.
    /// </summary>
    public delegate void PauseCallBack();

    /// <summary>
    /// A callback that is responsible for resuming a paused session that
    /// is currently running.
    /// </summary>
    public delegate void ResumeCallBack();

    /// <summary>
    /// A callback that is responsible for gently exiting an applications as
    /// another developer intended.
    /// </summary>
    public delegate void ShutdownCallBack();

    /// <summary>
    /// A callback that collects the levels contained within the current
    /// application.
    /// </summary>
    public delegate void DetailsCallBack();
}
