using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace ConsoleWriteLineVsConsoleLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var iterations = 10000;
            var sv = new Stopwatch();

            var random = new Random(1000);

            sv.Start();

            for (int i = 0; i < iterations/10; i++)
            {
                await Task.WhenAll(Enumerable.Range(0, 10).Select(async x =>
                {
                    await Task.Delay(0);
                    Console.WriteLine($"{i*10 + x + 1} - Hello World!");
                }));
            }
            
            sv.Stop();

            var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("ConsoleWriteLineVsConsoleLogger.Program", LogLevel.Debug)
                        .AddConsoleFormatter<Formatter, ConsoleFormatterOptions>(options =>
                        {
                            options.IncludeScopes = false;
                        })
                        .AddConsole(options =>
                        {
                            options.FormatterName = nameof(Formatter);
                        });
                }
            );

            var logger = loggerFactory.CreateLogger(typeof(Program));

            var sv1 = new Stopwatch();

            random = new Random(1000);

            sv1.Start();

            for (int i = 0; i < iterations/10; i++)
            {
                await Task.WhenAll(Enumerable.Range(0, 10).Select(async x =>
                {
                    await Task.Delay(0);
                    logger.LogInformation("{SomeData} - Hello World!", i * 10 + x + 1);
                }));
            }

            loggerFactory.Dispose();

            sv1.Stop();

            Console.WriteLine($"Console.WriteLine elapsed {sv.ElapsedMilliseconds} ms.");
            Console.WriteLine($"logger.LogInformation {sv1.ElapsedMilliseconds} ms.");
        }

        public class Formatter : ConsoleFormatter
        {
            public Formatter() : base("Formatter")
            {
            }

            public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
            {
                textWriter.WriteLine(logEntry.State);
            }
        }
    }
}
