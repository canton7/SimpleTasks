using System;
using SimpleTasks;
#nullable enable
namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var set = new SimpleTaskSet();
            var first = set.Create("first", "does a test thing").Run(() => Console.WriteLine("Running"));
            var second = set.Create("second", "does a test thing").DependsOn(first).Run<string>(Thing);
            var third = set.Create("third").DependsOn(second).Run((string? foo) => Console.WriteLine($"third {foo}"));
            first.DependsOn(third);
            set.Invoke(args);

            
        }

        static void Thing([Option(Description = "Yay woo")] string version = null)
        {
            Console.WriteLine("Secone " + version);
        }
    }
}
