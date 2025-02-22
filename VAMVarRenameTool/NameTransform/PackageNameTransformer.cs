public class PackageNameTransformer : NameTransformer
{
    public PackageNameTransformer(string fileName)
    {
        LoadMappings(fileName);
    }
}