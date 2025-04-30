public class OrganizationNode
{
    public string Name { get; set; }
    public List<OrganizationNode> Children { get; set; } = new();

    public OrganizationNode(string name)
    {
        Name = name;
    }
}