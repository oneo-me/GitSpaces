using Avalonia.Collections;

namespace GitSpaces.Models;

public enum BranchTreeNodeType
{
    Remote,
    Folder,
    Branch
}

public class BranchTreeNode
{
    public string Name { get; set; }
    public BranchTreeNodeType Type { get; set; }
    public object Backend { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsFiltered { get; set; }
    public List<BranchTreeNode> Children { get; set; } = new();

    public bool IsUpstreamTrackStatusVisible => IsBranch && !string.IsNullOrEmpty((Backend as Branch).UpstreamTrackStatus);

    public string UpstreamTrackStatus => Type == BranchTreeNodeType.Branch ? (Backend as Branch).UpstreamTrackStatus : "";

    public bool IsRemote => Type == BranchTreeNodeType.Remote;

    public bool IsFolder => Type == BranchTreeNodeType.Folder;

    public bool IsBranch => Type == BranchTreeNodeType.Branch;

    public bool IsCurrent => IsBranch && (Backend as Branch).IsCurrent;

    public class Builder
    {
        public List<BranchTreeNode> Locals { get; } = new();

        public List<BranchTreeNode> Remotes { get; } = new();

        public void Run(List<Branch> branches, List<Remote> remotes)
        {
            foreach (var remote in remotes)
            {
                var path = $"remote/{remote.Name}";
                var node = new BranchTreeNode
                {
                    Name = remote.Name, Type = BranchTreeNodeType.Remote, Backend = remote, IsExpanded = _expanded.Contains(path)
                };

                _maps.Add(path, node);
                Remotes.Add(node);
            }

            foreach (var branch in branches)
            {
                var isFiltered = _filters.Contains(branch.FullName);
                if (branch.IsLocal)
                {
                    MakeBranchNode(branch, Locals, "local", isFiltered);
                }
                else
                {
                    var remote = Remotes.Find(x => x.Name == branch.Remote);
                    if (remote != null) MakeBranchNode(branch, remote.Children, $"remote/{remote.Name}", isFiltered);
                }
            }

            SortNodes(Locals);
            SortNodes(Remotes);
        }

        public void SetFilters(AvaloniaList<string> filters)
        {
            _filters.AddRange(filters);
        }

        public void CollectExpandedNodes(List<BranchTreeNode> nodes, bool isLocal)
        {
            CollectExpandedNodes(nodes, isLocal ? "local" : "remote");
        }

        void CollectExpandedNodes(List<BranchTreeNode> nodes, string prefix)
        {
            foreach (var node in nodes)
            {
                var path = prefix + "/" + node.Name;
                if (node.Type != BranchTreeNodeType.Branch && node.IsExpanded) _expanded.Add(path);
                CollectExpandedNodes(node.Children, path);
            }
        }

        void MakeBranchNode(Branch branch, List<BranchTreeNode> roots, string prefix, bool isFiltered)
        {
            var subs = branch.Name.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if (subs.Length == 1)
            {
                var node = new BranchTreeNode
                {
                    Name = subs[0],
                    Type = BranchTreeNodeType.Branch,
                    Backend = branch,
                    IsExpanded = false,
                    IsFiltered = isFiltered
                };
                roots.Add(node);
                return;
            }

            BranchTreeNode lastFolder = null;
            var path = prefix;
            for (var i = 0; i < subs.Length - 1; i++)
            {
                path = string.Concat(path, "/", subs[i]);
                if (_maps.ContainsKey(path))
                {
                    lastFolder = _maps[path];
                }
                else if (lastFolder == null)
                {
                    lastFolder = new()
                    {
                        Name = subs[i], Type = BranchTreeNodeType.Folder, IsExpanded = branch.IsCurrent || _expanded.Contains(path)
                    };
                    roots.Add(lastFolder);
                    _maps.Add(path, lastFolder);
                }
                else
                {
                    var folder = new BranchTreeNode
                    {
                        Name = subs[i], Type = BranchTreeNodeType.Folder, IsExpanded = branch.IsCurrent || _expanded.Contains(path)
                    };
                    _maps.Add(path, folder);
                    lastFolder.Children.Add(folder);
                    lastFolder = folder;
                }
            }

            var last = new BranchTreeNode
            {
                Name = subs[subs.Length - 1],
                Type = BranchTreeNodeType.Branch,
                Backend = branch,
                IsExpanded = false,
                IsFiltered = isFiltered
            };
            lastFolder.Children.Add(last);
        }

        void SortNodes(List<BranchTreeNode> nodes)
        {
            nodes.Sort((l, r) =>
            {
                if (l.Type == r.Type)
                {
                    return l.Name.CompareTo(r.Name);
                }

                return (int)l.Type - (int)r.Type;
            });

            foreach (var node in nodes) SortNodes(node.Children);
        }

        readonly HashSet<string> _expanded = new();
        readonly List<string> _filters = new();
        readonly Dictionary<string, BranchTreeNode> _maps = new();
    }
}
