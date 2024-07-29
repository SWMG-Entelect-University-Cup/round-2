using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntelectUniCup2_Level1
{
    class Planet
    {
        int maxVisits = 0;

        int[] Biomes = new int[] { 1, 14, 28, 42, 57, 85, 100 };

        Node NorthPole;
        Node SouthPole;

        //Private class of nodes - only accessible within the Graph class
        private class Node
        {
            public string Coordinate { get; set; }
            public int Biome { get; set; }
            public double Quality { get; set; }
            public List<Node> Successors { get; set; }

            public Node(string coordinate, int biome, double quality)
            {
                this.Coordinate = coordinate;
                this.Biome = biome;
                Successors = new List<Node>();
                this.Quality = quality;

            } //Constructor

            public override string ToString()
            {
                return Biome.ToString();
            } //ToString
        } //class Node

        //List of nodes
        private Node[] nodes;

        //Constructor
        public Planet()
        {
            nodes = new Node[0]; 

        } //Constructor

        public int Size
        {
            get { return nodes.Length; }
        } //Size

        #region admin
        public void AddNode(string coordinate, int biome, double quality)
        {
            Node U = GetNode(coordinate);
            if (U == null) //If not already in the array of nodes
            {
                //Increase the size of the array to accommodate the new node
                Array.Resize(ref nodes, nodes.Length + 1);
                int u_ = nodes.Length - 1; //Last index in the array

                //Create new node with the value u and assign to the last spot in the array
                nodes[u_] = new Node(coordinate, biome, quality);
            }
        } //AddNode

        //Private helper method to get the node that contains a specific value
        private Node GetNode(string coordinates)
        {
            foreach (Node node in nodes)
                if (node.Coordinate.Equals(coordinates))
                    return node;
            return null;
        } //GetNode

        public void AddSuccessor(string coordinates1, string coordinates2)
        {
            Node U = GetNode(coordinates1);
            Node V = GetNode(coordinates2);
            if (U != null && V != null) //Ensure that both nodes exist
            {
                U.Successors.Add(V); //Adds V as a successor of U
            }
        } //AddSuccessor

        #endregion admin

        #region GetOptimalPath
        public string GetOptimalPath(int days)
        {
            if (NorthPole == null || SouthPole == null)
                return "poles are null";

            maxVisits = days * 3;
            List<List<Node>> allPaths = new List<List<Node>>();
            List<Node> currentPath = new List<Node>();
            Dictionary<Node, double> nodeScores = new Dictionary<Node, double>();

            // Precompute scores for each node
            foreach (var node in nodes)
            {
                nodeScores[node] = node.Quality * Biomes[node.Biome];
            }

            // Use a priority queue to explore nodes with higher potential scores first
            var priorityQueue = new SortedSet<PathNode>(Comparer<PathNode>.Create((a, b) => b.Score.CompareTo(a.Score)));
            priorityQueue.Add(new PathNode(NorthPole, 0, new List<Node> { NorthPole }));

            while (priorityQueue.Count > 0)
            {
                var currentPathNode = priorityQueue.Max;
                priorityQueue.Remove(currentPathNode);

                var currentNode = currentPathNode.Node;
                var currentPathList = currentPathNode.Path;

                if (currentNode == SouthPole && currentPathList.Count <= maxVisits)
                {
                    allPaths.Add(currentPathList);
                    continue;
                }

                foreach (var successor in currentNode.Successors)
                {
                    if (!currentPathList.Contains(successor))
                    {
                        var newPath = new List<Node>(currentPathList) { successor };
                        var newScore = CalculateScore(newPath, nodeScores);
                        priorityQueue.Add(new PathNode(successor, newScore, newPath));
                    }
                }
            }

            if (allPaths.Count == 0)
                return "No valid path found";

            List<Node> bestPath = allPaths.OrderByDescending(path => CalculateScore(path, nodeScores)).First();
            double bestScore = CalculateScore(bestPath, nodeScores);

            string result = "[";
            foreach (Node node in bestPath)
            {
                result += "(" + node.Coordinate + ")";
            }
            result += "] with score: " + bestScore;
            return result;
        }

        private double CalculateScore(List<Node> path, Dictionary<Node, double> nodeScores)
        {
            double score = 0;
            foreach (Node node in path)
            {
                score += nodeScores[node];
            }
            return score;
        }

        private class PathNode
        {
            public Node Node { get; }
            public double Score { get; }
            public List<Node> Path { get; }

            public PathNode(Node node, double score, List<Node> path)
            {
                Node = node;
                Score = score;
                Path = path;
            }
        }
        #endregion GetOptimalPath



        public void ImportFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            // Read nodes from the file and add them to the planet
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Trim('{', '}').Split(';');
                string coordinate = parts[0].Trim('(', ')');
                int biome = parts.Length > 1 ? int.Parse(parts[1]) : 0;
                double quality = parts.Length > 2 ? double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) : 0.0;

                AddNode(coordinate, biome, quality);
            }

            // Get the range of x and y coordinates
            var yCoords = nodes.Select(n => int.Parse(n.Coordinate.Split(',')[1])).Distinct().OrderBy(y => y).ToList();
            int minY = yCoords.First();
            int maxY = yCoords.Last();

            // Determine North and South pole coordinates based on the level
            string northPole = $"0,{maxY}";
            string southPole = $"0,{minY}";

            NorthPole = nodes.First(n => n.Coordinate == northPole);
            SouthPole = nodes.First(n => n.Coordinate == southPole);

            var xCoords = nodes.Select(n => int.Parse(n.Coordinate.Split(',')[0])).Distinct().OrderBy(x => x).ToList();
            int minX = xCoords.First();
            int maxX = xCoords.Last();

            // Connect nodes vertically and horizontally
            foreach (var node in nodes)
            {
                var coords = node.Coordinate.Split(',');
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                // Connect vertically
                Node northNode = GetNode($"{x},{y + 1}");
                if (northNode != null)
                    AddSuccessor(node.Coordinate, northNode.Coordinate);

                Node southNode = GetNode($"{x},{y - 1}");
                if (southNode != null)
                    AddSuccessor(node.Coordinate, southNode.Coordinate);

                // Connect horizontally with wrapping
                int eastX = x == maxX ? minX : xCoords[xCoords.IndexOf(x) + 1];
                Node eastNode = GetNode($"{eastX},{y}");
                if (eastNode != null)
                    AddSuccessor(node.Coordinate, eastNode.Coordinate);

                int westX = x == minX ? maxX : xCoords[xCoords.IndexOf(x) - 1];
                Node westNode = GetNode($"{westX},{y}");
                if (westNode != null)
                    AddSuccessor(node.Coordinate, westNode.Coordinate);
            }

            // Connect second row nodes to the South Pole
            foreach (Node node in nodes.Where(n => int.Parse(n.Coordinate.Split(',')[1]) == minY + 1))
            {
                AddSuccessor(node.Coordinate, SouthPole.Coordinate);
                AddSuccessor(SouthPole.Coordinate, node.Coordinate);
            }

            // Connect second-to-last row nodes to the North Pole
            foreach (var node in nodes.Where(n => int.Parse(n.Coordinate.Split(',')[1]) == maxY - 1))
            {
                AddSuccessor(node.Coordinate, NorthPole.Coordinate);
                AddSuccessor(NorthPole.Coordinate, node.Coordinate);
            }
        }


    }
}
