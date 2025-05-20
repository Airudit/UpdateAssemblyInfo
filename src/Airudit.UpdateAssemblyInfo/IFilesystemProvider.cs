
namespace Airudit.UpdateAssemblyInfo
{
    using System.IO;
    using System.Text;

    public interface IFilesystemProvider
    {
        DirectoryInfo DirectoryInfo(string path);
        bool FileExists(string path);
        Stream FileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
        string FileReadAllText(string path, Encoding encoding);
    }
}
