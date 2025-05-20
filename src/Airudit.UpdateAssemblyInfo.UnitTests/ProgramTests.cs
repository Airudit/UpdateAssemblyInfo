
namespace Airudit.UpdateAssemblyInfo.UnitTests
{
    using Airudit.UpdateAssemblyInfo;
    using System.Linq;
    using Xunit;

    public class ProgramTests
    {
        private IFilesystemProvider filesystem;
        ////private Mock<IFilesystemProvider> filesystem;

        public IFilesystemProvider Filesystem
        {
            ////get { return this.filesystem?.Object ?? (this.filesystem = new Mock<IFilesystemProvider>(MockBehavior.Loose)).Object; }
            get { return this.filesystem ?? (this.filesystem = new FakeFilesystemProvider()); }
        }
        
        [Fact]
        public void AcceptsFilePaths()
        {
            var args = new string[] { "file.cs", "x", };
            var program = new Program(this.Filesystem);
            var result = program.MainImpl(args);
            Assert.Equal(0, result.ErrorCode);
            Assert.Collection(
                program.Files,
                x => Assert.Equal(args[0], x),
                x => Assert.Equal(args[1], x));
        }
        
        [Fact]
        public void EmptyArgDoesNotFail()
        {
            // this CLI: tools/UpdateAssemblyInfo.exe Properties/AssemblyInfo.auto.cs  --build  --company MyCompany
            // misses a value for the --build argument
            var args = new string[] { "--build", "--company", "EA4T", "file.cs", };
        
            // what to do? fail? yes!
            var program = new Program(this.Filesystem);
            var result = program.MainImpl(args);
            Assert.Equal(1, result.ErrorCode);

            Assert.Single(program.Errors);
            Assert.Single(program.Files, args.Last());
        }
    }
}
