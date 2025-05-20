namespace Airudit.UpdateAssemblyInfo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class UpdateAssemblyInfoContext
    {
        private readonly IFilesystemProvider filesystem;

        public UpdateAssemblyInfoContext(IFilesystemProvider filesystem)
        {
            this.filesystem = filesystem;
        }

        public DirectoryInfo GitRoot { get; private set; }

        public DirectoryInfo GitDatabaseDirectory { get; private set; }

        public string RepositoryName { get; private set; }
        public string BranchName { get; private set; }
        public string BranchFullName { get; private set; }
        public string Revision { get; private set; }
        
        /// <summary>
        /// The build configuration (Debug, Release...)
        /// </summary>
        public string Build { get; internal set; }
    
        /// <summary>
        /// The "using" directives to use.
        /// </summary>
        public List<string> Usings { get; internal set; }
        public string Company { get; internal set; }
        public string Product { get; internal set; }
        public string Trademark { get; internal set; }
        public string Copyright { get; internal set; }
        public string AssemblyVersion { get; internal set; }
        public string AssemblyFileVersion { get; internal set; }
        public bool IncludeSourceControlRevision { get; internal set; }
        public bool IncludeBuildInfo { get; internal set; }
        public string Package { get; internal set; }
        public string PackageFullName { get; internal set; }
        public string VersionTag { get; private set; }
        public Encoding OutputEncoding { get; set; }
        public int ErrorCode { get; set; }

        internal void DetectGitInfo()
        {
            // detect git
            bool ok = false;
            var dir = this.filesystem.DirectoryInfo(Environment.CurrentDirectory);
            DirectoryInfo parent = null, dotGit = null;
            while (dir != null && dir.Exists && !dir.Equals(parent))
            {
                dotGit = this.filesystem.DirectoryInfo(Path.Combine(dir.FullName, ".git"));
                if (dotGit.Exists)
                {
                    ok = true;
                    break;
                }

                parent = dir;
                dir = dir.Parent;
            }

            if (!ok)
            {
                return;
            }

            // keep some info
            this.GitRoot = dir;
            this.GitDatabaseDirectory = dotGit;
            this.RepositoryName = this.GitRoot.Name;

            string path, branchHeadPath = null;

            // read branch name in `.git/HEAD`
            // into: branchHeadPath       = "refs/heads/dev/abcd/my-feature"
            // into: this.BranchName     = "my-feature";
            // into: this.BranchFullName = "dev/abcd/my-feature;
            path = Path.Combine(this.GitDatabaseDirectory.FullName, "HEAD");
            var commitIdRegex = new Regex("^[0-9a-fA-F]+$"); // "8efadf219440b8166cafc2ca6ea3925a03053770"
            var commitRefRegex = new Regex("^ref: ?(.+)$");  // "ref: refs/heads/master"
            if (this.filesystem.FileExists(path))
            {
                using (var stream = this.filesystem.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    Match match;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            var prefix = "ref: ";
                            if (line.StartsWith(prefix))
                            {
                                var contents = line.Substring(prefix.Length).Trim();
                                branchHeadPath = contents;

                                prefix = "refs/heads/";
                                if (contents.StartsWith(prefix))
                                {
                                    contents = contents.Substring(prefix.Length);
                                }
                                
                                var slash = new char[] { '/', };
                                var parts = contents.Split(slash);
                                var part = parts.Last();
                                if (!string.IsNullOrEmpty(part))
                                {
                                    this.BranchName = part;
                                    this.BranchFullName = contents;
                                }

                                break;
                            }
                            else if ((match = commitIdRegex.Match(line)).Success)
                            {
                                this.Revision = line; // "8efadf219440b8166cafc2ca6ea3925a03053770"
                                break;
                            }
                        }
                    }
                }
            }

            // read HEAD (current commit id) for this.Revision
            if (branchHeadPath != null)
            {
                string headRef = null;
                Match match;

                path = Path.Combine(this.GitDatabaseDirectory.FullName, branchHeadPath);
                if (this.filesystem.FileExists(path))
                {
                    using (var stream = this.filesystem.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                if ((match = commitIdRegex.Match(line)).Success)
                                {
                                    this.Revision = line; // "8efadf219440b8166cafc2ca6ea3925a03053770"
                                    break;
                                }
                                else if ((match = commitRefRegex.Match(line)).Success)
                                {
                                    headRef = match.Groups[1].Value; // "refs/heads/master"
                                    break;
                                }
                                else
                                {
                                    this.Revision = "UNKNOWN";
                                }
                            }
                        }
                    }
                }

                if (headRef != null)
                {
                    path = Path.Combine(this.GitDatabaseDirectory.FullName, headRef);
                    if (this.filesystem.FileExists(path))
                    {
                        using (var stream = this.filesystem.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    if ((match = commitIdRegex.Match(line)).Success)
                                    {
                                        this.Revision = line;
                                        break;
                                    }
                                    else
                                    {
                                        this.Revision = "UNKNOWN";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // HACK: how to do this? git describe --tags --abbrev=0 --match 'v*'
            this.VersionTag = default(string);
            path = Path.Combine(this.GitRoot.FullName, "next-version-tag");
            if (this.filesystem.FileExists(path))
            {
                var contents = this.filesystem.FileReadAllText(path, Encoding.UTF8).Trim();
                if (!string.IsNullOrEmpty(contents))
                {
                    this.VersionTag = contents.Trim();
                }
            }
        }

        internal void GenerateFiles(List<string> files)
        {
            for (int f = 0; f < files.Count; f++)
            {
                var file = files[f];
                this.GenerateFile(file);
            }
        }

        private void GenerateFile(string file)
        {
            using (var stream = this.filesystem.FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.SetLength(0L);
                using (var writer = new StreamWriter(stream, this.OutputEncoding ?? Encoding.UTF8))
                {
                    this.GenerateFile(writer);
                }
            }
        }

        internal void GenerateFile(StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("//------------------------------------------------------------------------------");
            writer.WriteLine("// <auto-generated>");
            writer.WriteLine("//     This code was generated by a tool.");
            writer.WriteLine("//");
            writer.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
            writer.WriteLine("//     the code is regenerated.");
            writer.WriteLine("//");
            writer.WriteLine("//     Generated by: Airudit.UpdateAssemblyInfo");
            writer.WriteLine("//     Generated on: " + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            writer.WriteLine("//     Generated on: " + Environment.MachineName);
            writer.WriteLine("//     Generated at: " + Environment.CurrentDirectory);
            writer.WriteLine("//     Encoding:     " + writer.Encoding.EncodingName);
            writer.WriteLine("// </auto-generated>");
            writer.WriteLine("//------------------------------------------------------------------------------");
            writer.WriteLine();

            // namespaces
            foreach (var directive in this.Usings)
            {
                writer.Write("using ");
                writer.Write(directive);
                writer.WriteLine(";");
            }

            // Assembly Configuration
            if (this.Build != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyConfiguration(""");
                writer.Write(this.EscapeString(this.Build));
                writer.WriteLine(@""")]");
            }

            // Assembly Company
            if (this.Company != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyCompany(""");
                writer.Write(this.EscapeString(this.Company));
                writer.WriteLine(@""")]");
            }

            // Assembly Company
            if (this.Product != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyProduct(""");
                writer.Write(this.EscapeString(this.Product));
                writer.WriteLine(@""")]");
            }

            // Assembly Copyright
            if (this.Copyright != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyCopyright(""");
                writer.Write(this.EscapeString(this.Copyright));
                writer.WriteLine(@""")]");
            }

            // Assembly Trademark
            if (this.Trademark != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyTrademark(""");
                writer.Write(this.EscapeString(this.Trademark));
                writer.WriteLine(@""")]");
            }

            // Assembly Version
            if (this.AssemblyVersion != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyVersion(""");
                writer.Write(this.EscapeString(this.AssemblyVersion));
                writer.WriteLine(@""")]");
            }

            // Assembly Trademark
            if (this.AssemblyFileVersion != null)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: AssemblyFileVersion(""");
                writer.Write(this.EscapeString(this.AssemblyFileVersion));
                writer.WriteLine(@""")]");
            }

            // SourceControlRevision
            if (this.IncludeSourceControlRevision)
            {
                writer.WriteLine();
                writer.Write(@"[assembly: SourceControlRevision(");

                string sep = string.Empty;
                if (this.Revision != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Revision", this.Revision);
                    sep = ", ";
                }

                if (this.BranchFullName != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Branch", this.BranchFullName);
                    sep = ", ";
                }
                else if (this.BranchName != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Branch", this.BranchName);
                    sep = ", ";
                }

                if (this.RepositoryName != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Repository", this.RepositoryName);
                    sep = ", ";
                }

                if (this.VersionTag != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "VersionTag", this.VersionTag);
                    sep = ", ";
                }

                writer.WriteLine(@")]");
            }

            // BuildInfo
            if (this.IncludeBuildInfo || !string.IsNullOrEmpty(this.Package) || !string.IsNullOrEmpty(this.PackageFullName))
            {
                writer.WriteLine();
                writer.Write(@"[assembly: BuildInfo(");

                string sep = string.Empty;

                if (this.IncludeBuildInfo)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Date", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                    sep = ", ";

                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "MachineName", Environment.MachineName);
                    sep = ", ";
                }

                if (this.Package != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "PackageName", this.Package);
                    sep = ", ";
                }

                if (this.PackageFullName != null)
                {
                    writer.Write(sep);
                    this.WriteAttributeKeyValue(writer, "Package", this.PackageFullName);
                    sep = ", ";
                }

                writer.WriteLine(@")]");
            }

            writer.WriteLine();
            writer.WriteLine();
            writer.Flush();
        }

        private void WriteAttributeKeyValue(StreamWriter writer, string key, object value)
        {
            writer.Write(key);
            writer.Write(" = ");
            this.WriteValue(writer, value);
        }

        private void WriteValue(StreamWriter writer, object value)
        {
            if (value is string)
            {
                writer.Write("\"");
                writer.Write(this.EscapeString((string)value));
                writer.Write("\"");
            }
            else
            {
                writer.Write(value);
            }
        }

        internal void RequireUsing(string value)
        {
            if (!this.Usings.Contains(value))
            {
                this.Usings.Add(value);
            }
        }

        private string EscapeString(string value)
        {
            var newValue = value.Replace("\"", "\\\"");
            return newValue;
        }
    }
}
