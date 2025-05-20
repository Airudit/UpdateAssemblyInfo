
namespace Airudit.UpdateAssemblyInfo
{
    using System.IO;
    using System.Text;

    public class RealFilesystemProvider : IFilesystemProvider
    {
        public DirectoryInfo DirectoryInfo(string path)
        {
            return new DirectoryInfo(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream FileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return new FileStream(path, fileMode, fileAccess, fileShare);
        }

        public string FileReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(path, encoding);
        }
    }
}
