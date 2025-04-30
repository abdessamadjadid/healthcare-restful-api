using System.Globalization;
using CsvHelper;

public class OrgHierarchyService
{
    private readonly string _csvPath = "FictionalHealthCareSystem.csv";
    private OrganizationNode? _root = null;
    private Dictionary<string, OrganizationNode> _nodeMap = new();

    public OrgHierarchyService()
    {
        LoadCsv();
    }

    private void LoadCsv()
    {
        using var reader = new StreamReader(_csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<OrgRecord>().ToList();

        foreach (var record in records)
        {
            if (!string.IsNullOrEmpty(record.OrgName) && !_nodeMap.ContainsKey(record.OrgName))
                _nodeMap[record.OrgName] = new OrganizationNode(record.OrgName);

            if (!string.IsNullOrEmpty(record.SubOrgName) && !_nodeMap.ContainsKey(record.SubOrgName))
                _nodeMap[record.SubOrgName] = new OrganizationNode(record.SubOrgName);

            if (!string.IsNullOrEmpty(record.OrgName) && !string.IsNullOrEmpty(record.SubOrgName) &&
                record.OrgName != record.SubOrgName && !_nodeMap[record.OrgName].Children.Contains(_nodeMap[record.SubOrgName]))
                _nodeMap[record.OrgName].Children.Add(_nodeMap[record.SubOrgName]);
        }

        // Determine the root node (a node that is not a child of any other node)
        var allChildren = _nodeMap.Values.SelectMany(node => node.Children).ToHashSet();
        _root = _nodeMap.Values.FirstOrDefault(node => !allChildren.Contains(node));

    }

    private void SaveToCsv()
    {
        if (_nodeMap.Count == 0)
        {
            Console.WriteLine("Warning: No data to save to CSV.");
            return;
        }

        using var writer = new StreamWriter(_csvPath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        var records = _nodeMap.Values.SelectMany(node =>
            node.Children.Select(child => new OrgRecord
            {
                OrgName = node.Name,
                SubOrgName = child.Name
            })).ToList();

        csv.WriteRecords(records);
    }

    public List<string> GetAllNodes()
    {
        var result = new List<string>();
        if (_root != null)
        {
            Traverse(_root, result);
        }
        return result;
    }

    public bool RenameHospital(string oldName, string newName)
    {
        if (!_nodeMap.ContainsKey(oldName))
        {
            Console.WriteLine($"Rename failed: Node with name '{oldName}' does not exist.");
            return false;
        }

        var node = _nodeMap[oldName];
        node.Name = newName;

        // Update the parent references
        foreach (var parent in _nodeMap.Values)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] == node)
                {
                    parent.Children[i] = node; // Update the reference in the parent's Children list
                }
            }
        }

        // Update the dictionary
        _nodeMap.Remove(oldName);
        _nodeMap[newName] = node;

        // If the renamed node is the root, update the root reference
        if (_root == node)
        {
            _root = node;
        }

        Console.WriteLine($"Renamed {oldName} to {newName}");
        Console.WriteLine("Updated hierarchy:");
        foreach (var nodeEntry in _nodeMap.Values)
        {
            Console.WriteLine($"Node: {nodeEntry.Name}, Children: {string.Join(", ", nodeEntry.Children.Select(c => c.Name))}");
        }

        SaveToCsv();
        return true;
    }

    public void AddRecord(string parent, string child)
    {
        if (!_nodeMap.ContainsKey(parent))
            _nodeMap[parent] = new OrganizationNode(parent);

        if (!_nodeMap.ContainsKey(child))
        {
            var childNode = new OrganizationNode(child);
            _nodeMap[parent].Children.Add(childNode);
            _nodeMap[child] = childNode;
        }
        else
        {
            // Ensure the child is not already in the parent's Children list
            var childNode = _nodeMap[child];
            if (!_nodeMap[parent].Children.Contains(childNode))
            {
                _nodeMap[parent].Children.Add(childNode);
            }
        }
    }

    private void Traverse(OrganizationNode node, List<string> result)
    {
        if (node == null)
            return;

        result.Add(node.Name);

        foreach (var child in node.Children ?? new List<OrganizationNode>())
        {
            Traverse(child, result);
        }
    }

}