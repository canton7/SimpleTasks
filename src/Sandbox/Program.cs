using System;
using SimpleTasks;
using static SimpleTasks.SimpleTask;
#nullable enable
namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var first = CreateTask("first", "does a test thing").Run(() => Console.WriteLine("Running"));
            var second = CreateTask("second", "does a test thing").DependsOn(first).Run<string>(Thing);
            var third = CreateTask("third").Run((string? foo) => Console.WriteLine($"third {foo}"));
            //first.DependsOn(third);
            InvokeTask(args);

            
        }

        static void Thing([Option(Description = "Yay woo")] string version = null)
        {
            Console.WriteLine("Secone " + version);
        }
    }
}
