using System.Text;

public interface IExportable
{
    void Export(string filePath, Encoding encoding = null);
}
