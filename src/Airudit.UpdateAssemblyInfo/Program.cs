
namespace Airudit.UpdateAssemblyInfo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class Program
    {
        private readonly IFilesystemProvider filesystem;

        public Program(IFilesystemProvider filesystem)
        {
            this.filesystem = filesystem ?? throw new ArgumentNullException(nameof(filesystem));
        }

        /// <summary>
        /// CLI entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var me = new Program(new RealFilesystemProvider());
            var result = me.MainImpl(args);
            Environment.ExitCode = result.ErrorCode;
        }

        public bool Help { get; set; }

        public List<string> Errors { get; } = new List<string>();

        public List<string> Files { get; } = new List<string>();

        public UpdateAssemblyInfoContext MainImpl(string[] args)
        {
            var context = new UpdateAssemblyInfoContext(this.filesystem);
            context.Usings = new List<string>();
            context.OutputEncoding = Console.OutputEncoding ?? Encoding.Default;

            this.ParseArgs(args, context);

            if (this.Errors.Count > 0)
            {
                foreach (var error in this.Errors)
                {
                    Console.WriteLine("ERROR: " + error);
                }

                context.ErrorCode = 1;
                return context;
            }

            if (this.Help)
            {
                ShowUsage();
                context.ErrorCode = 0;
                return context;
            }

            context.DetectGitInfo();

            // output to file or console
            if (this.Files.Count > 0)
            {
                context.GenerateFiles(this.Files);
            }
            else
            {
                // NOTE: not sure why the file is generated in-memory before outputting?
                using (var memory = new MemoryStream())
                {
                    var writer = new StreamWriter(memory, context.OutputEncoding);
                    context.GenerateFile(writer);
                    writer.Flush();

                    memory.Position = 0L;
                    var reader = new StreamReader(memory, context.OutputEncoding);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            context.ErrorCode = 0;
            return context;
        }

        private void ParseArgs(string[] args, UpdateAssemblyInfoContext context)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var nextArg = (i + 1) < args.Length ? args[i + 1] : null;

                var knownArg = this.DetectArg(arg);
                if (knownArg == ProgramArgument.Help)
                {
                    this.Help = true;
                }
                else if (IsArg(arg, "--debug"))
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        Debugger.Launch();
                    }
                }
                else if (IsArg(arg, "--Build", "-b"))
                {
                    // --build $(ConfigurationName)
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Reflection");
                        context.Build = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Build must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Company"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Reflection");
                        context.Company = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Company must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Product"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Reflection");
                        context.Product = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Product must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Copyright"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Reflection");
                        context.Copyright = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Copyright must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Trademark"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Reflection");
                        context.Trademark = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Trademark must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Package"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.PackageFullName = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Package must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--PackageName"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.Package = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --PackageName must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--using", "-u"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing(nextArg);
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --using must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--Version"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Runtime.InteropServices");
                        context.AssemblyVersion = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --Version must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--FileVersion"))
                {
                    if (nextArg != null && this.DetectArg(nextArg) == ProgramArgument.Unknown)
                    {
                        context.RequireUsing("System.Runtime.InteropServices");
                        context.AssemblyFileVersion = nextArg;
                        i++;
                    }
                    else
                    {
                        this.Errors.Add("Argument --FileVersion must be followed by a value");
                    }
                }
                else if (IsArg(arg, "--SourceControlRevision", "--scv", "--scr"))
                {
                    context.IncludeSourceControlRevision = true;
                }
                else if (IsArg(arg, "--BuildInfo", "--BI"))
                {
                    context.IncludeBuildInfo = true;
                }
                else
                {
                    this.Files.Add(arg);
                }
            }
        }

        private ProgramArgument DetectArg(string arg)
        {
            if (IsArg(arg, "--help", "-h", "/?"))
            {
                return ProgramArgument.Help;
            }
            else if (IsArg(arg, "--debug"))
            {
                return ProgramArgument.Debug;
            }
            else if (IsArg(arg, "--Build", "-b"))
            {
                return ProgramArgument.Build;
            }
            else if (IsArg(arg, "--Company"))
            {
                return ProgramArgument.Company;
            }
            else if (IsArg(arg, "--Product"))
            {
                return ProgramArgument.Product;
            }
            else if (IsArg(arg, "--Copyright"))
            {
                return ProgramArgument.Copyright;
            }
            else if (IsArg(arg, "--Trademark"))
            {
                return ProgramArgument.Trademark;
            }
            else if (IsArg(arg, "--Package"))
            {
                return ProgramArgument.Package;
            }
            else if (IsArg(arg, "--PackageName"))
            {
                return ProgramArgument.PackageName;
            }
            else if (IsArg(arg, "--using", "-u"))
            {
                return ProgramArgument.Using;
            }
            else if (IsArg(arg, "--Version"))
            {
                return ProgramArgument.Version;
            }
            else if (IsArg(arg, "--FileVersion"))
            {
                return ProgramArgument.FileVersion;
            }
            else if (IsArg(arg, "--SourceControlRevision", "--scv", "--scr"))
            {
                return ProgramArgument.SourceControlRevision;
            }
            else if (IsArg(arg, "--BuildInfo", "--BI"))
            {
                return ProgramArgument.BuildInfo;
            }
            else
            {
                return ProgramArgument.Unknown;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage: UpdateAssemblyInfo.exe --help");
            Console.WriteLine("Usage: UpdateAssemblyInfo.exe <file> [options]");
            Console.WriteLine("");
            Console.WriteLine("<file>:            the file to generate");
            Console.WriteLine("                   ");
            Console.WriteLine("options:");
            Console.WriteLine("--build <x>        the build configuration (Debug, Release...)");
            Console.WriteLine("--using <x>        insert a using directive");
            Console.WriteLine("--company <x>      insert company name");
            Console.WriteLine("--product <x>      insert product name");
            Console.WriteLine("--Copyright <x>    insert copyright notice");
            Console.WriteLine("--Trademark <x>    insert trademark notice");
            Console.WriteLine("--Package <x>      the package id (MyPackageVariant-v1.2.0)");
            Console.WriteLine("--PackageName <x>  the package name (MyPackageVariant)");
            Console.WriteLine("--Version <x>      the assembly version");
            Console.WriteLine("--FileVersion <x>  the assembly file version");
            Console.WriteLine("--SourceControlRevision               ");
            Console.WriteLine("                   include source control information from the local git");
            Console.WriteLine("--BuildInfo        include build information (date, machine)");
            Console.WriteLine("--encoding <x>     the output file encoding");
            Console.WriteLine("");
        }

        private static bool IsArg(string arg, params string[] oks)
        {
            for (int i = 0; i < oks.Length; i++)
            {
                if (oks[i].Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum ProgramArgument
    {
        Unknown,
        Help,
        Debug,
        Build,
        Company,
        Product,
        Copyright,
        Trademark,
        Package,
        PackageName,
        Using,
        Version,
        FileVersion,
        SourceControlRevision,
        BuildInfo,
    }
}
