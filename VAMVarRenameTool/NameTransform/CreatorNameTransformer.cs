using System.Collections.Generic;

public class CreatorNameTransformer : NameTransformer
{
    public CreatorNameTransformer(string fileName)
    {
        LoadMappings(fileName);
    }
}