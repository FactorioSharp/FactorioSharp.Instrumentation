namespace FactorioSharp.Instrumentation.Extensions;

public static class TaskExtensions
{
    public static void RunSync(this Task task) => task.GetAwaiter().GetResult();
    public static T RunSync<T>(this Task<T> task) => task.GetAwaiter().GetResult();
}
