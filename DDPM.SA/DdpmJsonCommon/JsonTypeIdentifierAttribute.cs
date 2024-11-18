/// <summary>
/// Add this attritube to supported DDM_COMMAND, DDM_RESPONSE, DDM_EVENT classes.
/// User can use reflection to get the collection of supported DDM_COMMAND, DDM_RESPONSE, DDM_EVENT classes easily.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class JsonTypeIdentifierAttribute : Attribute
{
}

/**
 * 
 * Apply the JsonTypeIdentifierAttribute to the DDM_COMMAND, DDM_RESPONSE, DDM_EVENT classes. Ex:
 * 
 * [JsonTypeIdentifier]
 * public class GET_CURRENT_MONITOR_INDEX : IDDM_COMMAND
 * {
 *     // ...
 * }
 * 
 * Use reflection to scan all assmebly, checks if the class has JsonTypeIdentifierAttribute, and then add the class to the collections
 *
 * foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
 * {
 *     var attribute = type.GetCustomAttribute<JsonTypeIdentifierAttribute>();
 *     if (attribute != null)
 *     {
 *         if (typeof(IDDM_COMMAND).IsAssignableFrom(type))
 *         {
 *             _commands.Add(type);
 *         }
 *         else if (typeof(IDDM_RESPONSE).IsAssignableFrom(type))
 *         {
 *             _responses.Add(type);
 *         }
 *         else if (typeof(IDDM_EVENT).IsAssignableFrom(type))
 *         {
 *             _events.Add(type);
 *         }
 *     }
 * }
 * 
 */