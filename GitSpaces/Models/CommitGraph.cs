using System;
using System.Collections.Generic;
using Avalonia;

namespace GitSpaces.Models;

public class CommitGraph
{
    public class Path
    {
        public List<Point> Points = new();
        public int Color;
    }

    public class PathHelper
    {
        public string Next;
        public bool IsMerged;
        public double LastX;
        public double LastY;
        public double EndY;
        public Path Path;

        public PathHelper(string next, bool isMerged, int color, Point start)
        {
            Next = next;
            IsMerged = isMerged;
            LastX = start.X;
            LastY = start.Y;
            EndY = LastY;

            Path = new();
            Path.Color = color;
            Path.Points.Add(start);
        }

        public PathHelper(string next, bool isMerged, int color, Point start, Point to)
        {
            Next = next;
            IsMerged = isMerged;
            LastX = to.X;
            LastY = to.Y;
            EndY = LastY;

            Path = new();
            Path.Color = color;
            Path.Points.Add(start);
            Path.Points.Add(to);
        }

        public void Add(double x, double y, double halfHeight, bool isEnd = false)
        {
            if (x > LastX)
            {
                Add(new(LastX, LastY));
                Add(new(x, y - halfHeight));
                if (isEnd) Add(new(x, y));
            }
            else if (x < LastX)
            {
                if (y > LastY + halfHeight) Add(new(LastX, LastY + halfHeight));
                Add(new(x, y));
            }
            else if (isEnd)
            {
                Add(new(x, y));
            }

            LastX = x;
            LastY = y;
        }

        void Add(Point p)
        {
            if (EndY < p.Y)
            {
                Path.Points.Add(p);
                EndY = p.Y;
            }
        }
    }

    public class Link
    {
        public Point Start;
        public Point Control;
        public Point End;
        public int Color;
    }

    public class Dot
    {
        public Point Center;
        public int Color;
    }

    public List<Path> Paths { get; set; } = new();
    public List<Link> Links { get; set; } = new();
    public List<Dot> Dots { get; set; } = new();

    public static CommitGraph Parse(List<Commit> commits, double rowHeight, int colorCount)
    {
        double UNIT_WIDTH = 12;
        double HALF_WIDTH = 6;
        var UNIT_HEIGHT = rowHeight;
        var HALF_HEIGHT = rowHeight / 2;

        var temp = new CommitGraph();
        var unsolved = new List<PathHelper>();
        var mapUnsolved = new Dictionary<string, PathHelper>();
        var ended = new List<PathHelper>();
        var offsetY = -HALF_HEIGHT;
        var colorIdx = 0;

        foreach (var commit in commits)
        {
            var major = null as PathHelper;
            var isMerged = commit.IsMerged;
            var oldCount = unsolved.Count;

            // Update current y offset
            offsetY += UNIT_HEIGHT;

            // Find first curves that links to this commit and marks others that links to this commit ended.
            var offsetX = -HALF_WIDTH;
            foreach (var l in unsolved)
            {
                if (l.Next == commit.SHA)
                {
                    if (major == null)
                    {
                        offsetX += UNIT_WIDTH;
                        major = l;

                        if (commit.Parents.Count > 0)
                        {
                            major.Next = commit.Parents[0];
                            if (!mapUnsolved.ContainsKey(major.Next)) mapUnsolved.Add(major.Next, major);
                        }
                        else
                        {
                            major.Next = "ENDED";
                            ended.Add(l);
                        }

                        major.Add(offsetX, offsetY, HALF_HEIGHT);
                    }
                    else
                    {
                        ended.Add(l);
                    }

                    isMerged = isMerged || l.IsMerged;
                }
                else
                {
                    if (!mapUnsolved.ContainsKey(l.Next)) mapUnsolved.Add(l.Next, l);
                    offsetX += UNIT_WIDTH;
                    l.Add(offsetX, offsetY, HALF_HEIGHT);
                }
            }

            // Create new curve for branch head
            if (major == null && commit.Parents.Count > 0)
            {
                offsetX += UNIT_WIDTH;
                major = new(commit.Parents[0], isMerged, colorIdx, new(offsetX, offsetY));
                unsolved.Add(major);
                temp.Paths.Add(major.Path);
                colorIdx = (colorIdx + 1) % colorCount;
            }

            // Calculate link position of this commit.
            var position = new Point(offsetX, offsetY);
            if (major != null)
            {
                major.IsMerged = isMerged;
                position = new(major.LastX, offsetY);
                temp.Dots.Add(new()
                {
                    Center = position, Color = major.Path.Color
                });
            }
            else
            {
                temp.Dots.Add(new()
                {
                    Center = position, Color = 0
                });
            }

            // Deal with parents
            for (var j = 1; j < commit.Parents.Count; j++)
            {
                var parent = commit.Parents[j];
                if (mapUnsolved.ContainsKey(parent))
                {
                    var l = mapUnsolved[parent];
                    var link = new Link();

                    link.Start = position;
                    link.End = new(l.LastX, offsetY + HALF_HEIGHT);
                    link.Control = new(link.End.X, link.Start.Y);
                    link.Color = l.Path.Color;
                    temp.Links.Add(link);
                }
                else
                {
                    offsetX += UNIT_WIDTH;

                    // Create new curve for parent commit that not includes before
                    var l = new PathHelper(commit.Parents[j], isMerged, colorIdx, position, new(offsetX, position.Y + HALF_HEIGHT));
                    unsolved.Add(l);
                    temp.Paths.Add(l.Path);
                    colorIdx = (colorIdx + 1) % colorCount;
                }
            }

            // Remove ended curves from unsolved
            foreach (var l in ended)
            {
                l.Add(position.X, position.Y, HALF_HEIGHT, true);
                unsolved.Remove(l);
            }

            // Margins & merge state (used by datagrid).
            commit.IsMerged = isMerged;
            commit.Margin = new(Math.Max(offsetX + HALF_WIDTH, oldCount * UNIT_WIDTH), 0, 0, 0);

            // Clean up
            ended.Clear();
            mapUnsolved.Clear();
        }

        // Deal with curves haven't ended yet.
        for (var i = 0; i < unsolved.Count; i++)
        {
            var path = unsolved[i];
            var endY = (commits.Count - 0.5) * UNIT_HEIGHT;

            if (path.Path.Points.Count == 1 && path.Path.Points[0].Y == endY) continue;
            path.Add((i + 0.5) * UNIT_WIDTH, endY + HALF_HEIGHT, HALF_HEIGHT, true);
        }

        unsolved.Clear();

        return temp;
    }
}
