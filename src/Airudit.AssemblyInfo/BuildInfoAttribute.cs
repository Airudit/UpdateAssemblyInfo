
namespace Airudit.AssemblyInfo
{
    using System;
    using System.Reflection;

    [Obfuscation(Exclude = true)]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildInfoAttribute : Attribute
    {
        private string package;
        private string packageName;

        public BuildInfoAttribute()
        {
        }

        public BuildInfoAttribute(string Date, string MachineName)
        {
            this.Date = Date;
            this.MachineName = MachineName;
        }

        public string Date { get; set; }

        public string MachineName { get; set; }

        public string Package
        {
            get { return this.package; }
            set { this.package = string.IsNullOrEmpty(value) ? null : value; }
        }

        public string PackageName
        {
            get { return this.packageName; }
            set { this.packageName = string.IsNullOrEmpty(value) ? null : value; }
        }

        public override string ToString()
        {
            if (this.Date == null)
            {
                return base.ToString();
            }
            else
            {
                return "Build: " + this.Date
                    + (this.MachineName != null ? ("@" + this.MachineName) : "");
            }
        }
    }
}
