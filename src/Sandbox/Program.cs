using System;
using SimpleTasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var set = new SimpleTaskSet();
            var first = set.Create("first", "does a test thing").Run(() => Console.WriteLine("Running"));
            var second = set.Create("second", "does a test thing").DependsOn(first).Run((int version) => Console.WriteLine("Running " + version));
            var third = set.Create("third").DependsOn(second).Run(() => Console.WriteLine("third"));
            first.DependsOn(third);
            set.Invoke(args);
        }
    }
}
