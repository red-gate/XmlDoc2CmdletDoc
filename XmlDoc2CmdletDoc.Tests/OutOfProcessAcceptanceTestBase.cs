using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using XmlDoc2CmdletDoc.Core;

namespace XmlDoc2CmdletDoc.Tests
{
    public abstract class OutOfProcessAcceptanceTestBase : AcceptanceTestBase
    {
        protected abstract string TestAssemblyFrameworkName { get; }

        private string SolutionDir => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));

#if DEBUG
        private static string Configuration = "Debug";
#else        
        private static string Configuration = "Release";
#endif

        protected override string TestAssemblyPath =>
            Path.Combine(SolutionDir, "XmlDoc2CmdletDoc.TestModule", "bin", Configuration, TestAssemblyFrameworkName, "XmlDoc2CmdletDoc.TestModule.dll");

        protected override void GenerateHelpForTestAssembly(string assemblyPath)
        {
#if NET461
            var toolFrameworkName = "net461";
#elif NET472
            var toolFrameworkName = "net472";
#elif NET48
            var toolFrameworkName = "net48";
#elif NETCOREAPP2_1
            var toolFrameworkName = "netcoreapp2.1";
#elif NETCOREAPP3_1
            var toolFrameworkName = "netcoreapp3.1";
#elif NET5_0
            var toolFrameworkName = "net5.0";
#endif

            var toolDir = Path.Combine(SolutionDir, "XmlDoc2CmdletDoc", "bin", Configuration, toolFrameworkName);

#if NETCOREAPP
            var toolPath = Path.Combine(toolDir, "XmlDoc2CmdletDoc.dll");
#else
            var toolPath = Path.Combine(toolDir, "XmlDoc2CmdletDoc.exe");
#endif
            Assert.That(File.Exists(toolPath), $"Tool not found: {toolPath}");

            var workingDirectory = Path.GetDirectoryName(toolPath);
            var startInfo = new ProcessStartInfo
                            {
#if NETCOREAPP                                
                                FileName = "dotnet",
                                Arguments = $"\"{toolPath}\" \"{assemblyPath}\"",
#else
                                FileName = toolPath,
                                Arguments = assemblyPath,
#endif
                                WorkingDirectory = workingDirectory,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };

            using (var process = Process.Start(startInfo))
            {
                Assert.That(process, Is.Not.Null, $"Process failed to start: {toolPath}");
                
                var stdoutTask = ReadLines(process.StandardOutput, Console.WriteLine);
                var stderrTask = ReadLines(process.StandardError, Console.Error.WriteLine);

                var streamsClosed = Task.WaitAll(new [] {stdoutTask, stderrTask}, 10000); // 10 seconds.
                var processExited = process.WaitForExit(1000); // A further 1 second.
                
                if (!processExited || !streamsClosed)
                {
                    Assert.Fail($"Process didn't complete in a timely manner: {toolPath}");
                }

                Assert.That(process.ExitCode, Is.EqualTo(0), "Process failed");
            }
        }

        private static async Task ReadLines(TextReader reader, Action<string> onLineRead)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                onLineRead(line);
            }
        }
    }
}
