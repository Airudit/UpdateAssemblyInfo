
namespace Airudit.AssemblyInfo
{
    using System;
    using System.Reflection;

    [Obfuscation(Exclude = true)]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SourceControlRevisionAttribute : Attribute
    {
        /// <summary>
        /// Stores assembly source information.
        /// </summary>
        public SourceControlRevisionAttribute(string Revision, string Branch, string Repository)
        {
            this.Revision = Revision;
            this.Branch = Branch;
            this.Repository = Repository;
        }

        /// <summary>
        /// Stores assembly source information.
        /// </summary>
        public SourceControlRevisionAttribute(string Revision, string Branch)
            : this(Revision, Branch, null)
        {
        }

        /// <summary>
        /// Stores assembly source information.
        /// </summary>
        public SourceControlRevisionAttribute()
        {
        }

        /// <summary>
        /// The source control revision identifier.
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// The source control branch name.
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// The repository name.
        /// </summary>
        public string Repository { get; set; }

        /// <summary>
        /// The version tag (v1.2.6-xyz).
        /// </summary>
        public string VersionTag { get; set; }

        public override string ToString()
        {
            if (this.Revision == null)
            {
                return base.ToString();
            }
            else
            {
                return "Revision: " + this.Revision
                    + (this.Branch != null ? ("@" + this.Branch) : "")
                    + (this.Repository != null ? ("#" + this.Repository) : "")
                    + (this.VersionTag != null ? ("(" + this.VersionTag + ")") : "")
                    ;
            }
        }
    }
}
