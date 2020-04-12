using System;
using SimpleTasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var set = new SimpleTaskSet();
            set.Create("testy", "does a test thing").Run((int version) => Console.WriteLine("Running " + version));
            set.Create("bar", "does a test thing").Run(() => Console.WriteLine("Running"));
            set.Invoke(args);
        }
    }
}
