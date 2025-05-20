
namespace Airudit.UpdateAssemblyInfo.UnitTests
{
    using System.IO;
    using System.Text;

    public class FakeFilesystemProvider : IFilesystemProvider
    {
        public DirectoryInfo DirectoryInfo(string path)
        {
            return null;
        }

        public bool FileExists(string path)
        {
            return true;
        }

        public Stream FileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return new MemoryStream();
        }

        public string FileReadAllText(string path, Encoding encoding)
        {
            return string.Empty;
        }
    }
}
