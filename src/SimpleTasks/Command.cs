using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTasks
{
    /// <summary>
    /// Utility class for running processes
    /// </summary>
    public class Command
    {
        private readonly string command;
        private readonly string? args;

        /// <summary>
        /// Gets or sets whether to print the command being executed. Defaults to <c>true</c>
        /// </summary>
        public bool PrintCommand { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to throw a <see cref="SimpleTaskCommandFailedException"/> if the process
        /// returns a non-zero exit code. Defaults to <c>true</c>
        /// </summary>
        public bool ThrowOnError { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to print the process's standard output to console. Defaults to <c>true</c>
        /// </summary>
        public bool PrintStdout { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to print the process's standard error to console. Defaults to <c>true</c>
        /// </summary>
        public bool PrintStderr { get; set; } = true;

        /// <summary>
        /// Gets or sets a collection to which both standard output and error will be written
        /// </summary>
        public ICollection<string>? OutputLines { get; set; }

        /// <summary>
        /// Gets or sets a collection to which the process's standard output will be written
        /// </summary>
        public ICollection<string>? StdoutLines { get; set; }

        /// <summary>
        /// Gets or sets a collection to which the process's standard error will be written
        /// </summary>
        public ICollection<string>? StderrLines { get; set; }

        /// <summary>
        /// Gets or sets a delegate called once for each line of standard output of error written by the process
        /// </summary>
        public Action<string>? OnOutput { get; set; }

        /// <summary>
        /// Gets or sets a delegate called once for each line of standard output written by the process
        /// </summary>
        public Action<string>? OnStdout { get; set; }

        /// <summary>
        /// Gets or sets a delegate called once for each line of standard error written by the process
        /// </summary>
        public Action<string>? OnStderr { get; set; }

        /// <summary>
        /// Gets or sets the timeout, after which the process will be killed and a <see cref="SimpleTaskCommandTimedOutException"/>
        /// will be thrown
        /// </summary>
        public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

        /// <summary>
        /// The Current Working Directory in which to start the process
        /// </summary>
        public string? WorkingDirectory { get; set; }

        // Console.XXX are synchronized, but the rest of our ways of outputting things are not
        private readonly object outputLockObject = new();

        /// <summary>
        /// Initialises a new instance of the <see cref="Command"/> class, with the given command and args
        /// </summary>
        /// <param name="command">Process to run</param>
        /// <param name="args">Arguments to pass to the process</param>
        public Command(string command, string? args = null)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or whitespace", nameof(command));
            }

            this.command = command;
            this.args = args;
        }

        /// <summary>
        /// Executes the given command with the given args, using default options
        /// </summary>
        /// <param name="command">Process to run</param>
        /// <param name="args">Arguments to pass to the process</param>
        /// <exception cref="SimpleTaskCommandFailedException">
        /// The process returns a non-zero exit code and <see cref="ThrowOnError"/> is true
        /// </exception>
        /// <exception cref="SimpleTaskCommandTimedOutException">
        /// <see cref="Timeout"/> is not <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> and the process takes
        /// longer than this to execute
        /// </exception>
        public static void Run(string command, string? args = null)
        {
            // ThrowOnError defaults to true
            new Command(command, args).Run();
        }

        /// <summary>
        /// Executes the current command, providing a string containing all output
        /// </summary>
        /// <param name="output">The combined standard output and error written by the process</param>
        /// <returns>The process's exit code, if <see cref="ThrowOnError"/> is <c>false</c></returns>
        /// <exception cref="SimpleTaskCommandFailedException">
        /// The process returns a non-zero exit code and <see cref="ThrowOnError"/> is true
        /// </exception>
        /// <exception cref="SimpleTaskCommandTimedOutException">
        /// <see cref="Timeout"/> is not <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> and the process takes
        /// longer than this to execute
        /// </exception>
        public int Run(out string output)
        {
            var outputBuilder = new StringBuilder();
            int result = this.Run(outputBuilder, null, null);
            output = outputBuilder.ToString();
            return result;
        }

        /// <summary>
        /// Executes the current command, providing strings containing standard output and error
        /// </summary>
        /// <param name="stdout">The standard output written by the process</param>
        /// <param name="stderr">The standard error written by the process</param>
        /// <returns>The process's exit code, if <see cref="ThrowOnError"/> is <c>false</c></returns>
        /// <exception cref="SimpleTaskCommandFailedException">
        /// The process returns a non-zero exit code and <see cref="ThrowOnError"/> is true
        /// </exception>
        /// <exception cref="SimpleTaskCommandTimedOutException">
        /// <see cref="Timeout"/> is not <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> and the process takes
        /// longer than this to execute
        /// </exception>
        public int Run(out string stdout, out string stderr)
        {
            var (stdoutBuilder, stderrBuilder) = (new StringBuilder(), new StringBuilder());
            int result = this.Run(null, stdoutBuilder, stderrBuilder);
            (stdout, stderr) = (stdoutBuilder.ToString(), stderrBuilder.ToString());
            return result;
        }

        /// <summary>
        /// Executes the current command
        /// </summary>
        /// <returns>The process's exit code, if <see cref="ThrowOnError"/> is <c>false</c></returns>
        /// <exception cref="SimpleTaskCommandFailedException">
        /// The process returns a non-zero exit code and <see cref="ThrowOnError"/> is true
        /// </exception>
        /// <exception cref="SimpleTaskCommandTimedOutException">
        /// <see cref="Timeout"/> is not <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> and the process takes
        /// longer than this to execute
        /// </exception>
        public int Run() => this.Run(null, null, null);

        private int Run(StringBuilder? outputBuilder, StringBuilder? stdoutBuilder, StringBuilder? stderrBuilder)
        { 
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = this.command,
                    Arguments = this.args,
                    WorkingDirectory = this.WorkingDirectory,
                    UseShellExecute = false,
                }
            };

            bool customStdoutProcessing = this.OutputLines != null || this.StdoutLines != null
                || this.OnOutput != null || this.OnStdout != null
                || outputBuilder != null || stdoutBuilder != null;
            bool customStderrProcessing = this.OutputLines != null || this.StderrLines != null
                || this.OnOutput != null || this.OnStderr != null
                || outputBuilder != null || stderrBuilder != null;

            // If PrintStdout is true and customStdoutProcessing is false, we can just leave it un-redirected
            bool redirectStdout = !(this.PrintStdout && !customStdoutProcessing);
            bool redirectStderr = !(this.PrintStderr && !customStderrProcessing);

            process.StartInfo.RedirectStandardOutput = redirectStdout;
            process.StartInfo.RedirectStandardError = redirectStderr;

            if (this.PrintCommand)
            {
                Console.WriteLine($"{this.command} {this.args}");
            }

            process.Start();

            var readTasks = new List<Task>();
            if (redirectStdout)
            {
                var stdoutWriter = this.PrintStdout ? Console.Out : null;
                readTasks.Add(this.ReadStreamAsync(
                    process.StandardOutput,
                    stdoutWriter,
                    this.OutputLines,
                    this.StdoutLines,
                    this.OnOutput,
                    this.OnStdout,
                    outputBuilder,
                    stdoutBuilder));
            }
            if (redirectStderr)
            {
                var stderrWriter = this.PrintStderr ? Console.Error : null;
                readTasks.Add(this.ReadStreamAsync(
                    process.StandardError,
                    stderrWriter,
                    this.OutputLines,
                    this.StderrLines,
                    this.OnOutput,
                    this.OnStderr,
                    outputBuilder,
                    stderrBuilder));
            }

            bool completedWithinTimeout;
            if (readTasks.Count > 0)
            {
                // To avoid deadlocks, we need to read these first
                completedWithinTimeout = Task.WaitAll(readTasks.ToArray(), this.Timeout);
                if (completedWithinTimeout)
                {
                    process.WaitForExit();
                }
            }
            else
            {
                completedWithinTimeout = process.WaitForExit((int)this.Timeout.TotalMilliseconds);
            }

            if (!completedWithinTimeout)
            {
                process.Kill();
                throw new SimpleTaskCommandTimedOutException(this.Timeout, $"Command '{this.command}' timed out after '{this.Timeout}'");
            }

            int exitCode = process.ExitCode;

            if (this.ThrowOnError && exitCode != 0)
            {
                throw new SimpleTaskCommandFailedException(exitCode, $"Command '{this.command}' failed with exit code '{exitCode}'");
            }

            return exitCode;
        }

        private async Task ReadStreamAsync(
            StreamReader source,
            TextWriter? writer,
            ICollection<string>? output1,
            ICollection<string>? output2,
            Action<string>? onOutput1,
            Action<string>? onOutput2,
            StringBuilder? outputBuilder1,
            StringBuilder? outputBuilder2)
        {
            try
            {
                string? line;
                while ((line = await source.ReadLineAsync()) != null)
                {
                    writer?.WriteLine(line);
                    lock (this.outputLockObject)
                    {
                        output1?.Add(line);
                        output2?.Add(line);
                        onOutput1?.Invoke(line);
                        onOutput2?.Invoke(line);
                        outputBuilder1?.AppendLine(line);
                        outputBuilder2?.AppendLine(line);
                    }
                }
            }
            catch { }
        }
    }
}
