using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace leadme_api
{
    /// <summary>
    /// A class designed to hold the details about the entire project. This includes Gobal actions, Levels and level specific actions.
    /// </summary>
    public class Details
    {
        public string name { get; set; }
        public List<GlobalAction> globalActions { get; set; }
        public List<Level> levels { get; set; }

        /// <summary>
        /// Turn a supplied Details class into a stringified JSON version of itself ready to be sent through a pipe server.
        /// </summary>
        /// <param name="details">An instiated Details class.</param>
        /// <returns>A stringified version of the class.</returns>
        public static string Serialize(Details details)
        {
            return $"details,{JsonConvert.SerializeObject(details, Formatting.Indented)}";
        }
    }

    /// <summary>
    /// Actions that can be taken regardless of what scene a player/user is currently on.
    /// </summary>
    public class GlobalAction
    {
        public string name { get; set; }
        public string trigger { get; set; }
    }

    /// <summary>
    /// A specific scene within the project that can be loaded through the trigger field. It contains a list of actions that are 
    /// unique to the level.
    /// </summary>
    public class Level
    {
        public string name { get; set; }
        public string trigger { get; set; }
        public List<Action> actions { get; set; }
    }

    /// <summary>
    /// An action to be performed on a particular scene. The name is how a user will see it and the trigger is how the project will
    /// interpret the action. The extra variable is for any additional information that may be required for a particular project.
    /// This may include: file type, video duration etc..
    /// </summary>
    public class Action
    {
        public string name { get; set; }
        public string trigger { get; set; }
        public JArray extra { get; set; }
    }
}
