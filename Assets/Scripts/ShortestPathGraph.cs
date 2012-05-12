using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public interface IShortestPathGraph<T>
{
    void AddNode(T data);
    // call only after all nodes are added
    void AddEdge(T from, T to, float weight);
    // call only after all nodes and edges are added
    void PrecalculatePath(T fromData, T toData);
    IList<T> GetPath(T fromData, T toData);
}

public class FloydWarshallShortestPathGraph<T> : IShortestPathGraph<T>
{
    List<T> nodes = new List<T>();
    Dictionary<T, int> nodeIndices = new Dictionary<T, int>();

    float[,] pathLengths = null;
    // not the next node on the path, but the highest-indexed one (see
    // floyd-warshall algorithm)
    int[,] next = null;

    bool computed = false;

    public void AddNode(T data)
    {
        Assert.Condition(pathLengths == null, "AddNode called after AddEdge");
        Assert.Condition(!computed, "AddNode called after GetPath");

        nodeIndices[data] = nodes.Count;
        nodes.Add(data);
    }

    public void AddEdge(T from, T to, float weight)
    {
        Assert.Condition(!computed, "AddEdge called after GetPath");

        if(pathLengths == null)
        {
            pathLengths = new float[nodes.Count, nodes.Count];
            next = new int[nodes.Count, nodes.Count];

            for(int i = 0; i < nodes.Count; i++)
            {
                for(int j = 0; j < nodes.Count; j++)
                {
                    pathLengths[i, j] = Mathf.Infinity;
                    next[i, j] = -1;
                }
                pathLengths[i,i] = 0;
            }
        }

        pathLengths[nodeIndices[from], nodeIndices[to]] = weight;
    }

    public void PrecalculatePath(T fromData, T toData)
    {
        if(!computed)
        {
            for(int k = 0; k < nodes.Count; k++)
            for(int i = 0; i < nodes.Count; i++)
            for(int j = 0; j < nodes.Count; j++)
            if(pathLengths[i, k] + pathLengths[k, j] < pathLengths[i, j])
            {
                pathLengths[i, j] = pathLengths[i, k] + pathLengths[k, j];
                next[i, j] = k;
            }

            computed = true;
        }
    }

    public IList<T> GetPath(T fromData, T toData)
    {
        PrecalculatePath(fromData, toData);

        int i = nodeIndices[fromData];
        int j = nodeIndices[toData];

        if(pathLengths[i, j] == Mathf.Infinity)
            return null;

        List<T> path = new List<T>();
        Stack<KeyValuePair<int, int>> stack =
            new Stack<KeyValuePair<int, int>>();

        stack.Push(new KeyValuePair<int, int>(i, j));
        while(stack.Count > 0)
        {
            KeyValuePair<int, int> pair = stack.Pop();
            int mid = next[pair.Key, pair.Value];
            if(mid == -1)
            {
                path.Add(nodes[pair.Key]);
            }
            else
            {
                stack.Push(new KeyValuePair<int, int>(mid, pair.Value));
                stack.Push(new KeyValuePair<int, int>(pair.Key, mid));
            }
        }
        path.Add(nodes[j]);
        return path;
    }
}

public class DijkstraShortestPathGraph<T> : IShortestPathGraph<T>
{
    class Node : System.IComparable<Node>
    {
        public int index;
        public T data;
        public List<Edge> edges;

        // used by dijkstra's algorithm
        public float distance;

        public Node(int index, T data)
        {
            this.index = index;
            this.data = data;
            edges = new List<Edge>();
        }

        public int CompareTo(Node other)
        {
            return (distance != other.distance) ?
                distance.CompareTo(other.distance) :
                index.CompareTo(other.index);
        }
    }

    class Edge
    {
        public Node from;
        public Node to;
        public float weight;

        public Edge(Node from, Node to, float weight)
        {
            this.from = from;
            this.to = to;
            this.weight = weight;
        }
    }

    List<Node> nodes = new List<Node>();
    Dictionary<T, Node> nodesByData = new Dictionary<T, Node>();

    Dictionary<Node, Dictionary<Node, Node>> sourcePathData =
        new Dictionary<Node, Dictionary<Node, Node>>();

    public void AddNode(T data)
    {
        Node n = new Node(nodes.Count, data);
        nodes.Add(n);
        nodesByData[data] = n;
    }

    public void AddEdge(T from, T to, float weight)
    {
        nodesByData[from].edges.Add(
            new Edge(nodesByData[from], nodesByData[to], weight));
    }

    public void PrecalculatePath(T fromData, T toData)
    {
        Node from = nodesByData[fromData];

        if(!sourcePathData.ContainsKey(from))
        {
            // for each node, the node just previous to it on the path
            Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

            // there's no SortedSet<> in unity..
            SortedDictionary<Node, bool> closestNodes =
                new SortedDictionary<Node, bool>();

            foreach(Node node in nodes)
            {
                previous[node] = null;
                node.distance = (node == from) ? 0 : Mathf.Infinity;
                closestNodes[node] = true;
            }

            while(closestNodes.Count != 0)
            {
                Node node = closestNodes.First().Key;
                closestNodes.Remove(node);

                foreach(Edge edge in node.edges)
                {
                    float newDistance = node.distance + edge.weight;
                    if(newDistance < edge.to.distance)
                    {
                        closestNodes.Remove(edge.to);
                        edge.to.distance = node.distance + edge.weight;
                        previous[edge.to] = node;
                        closestNodes[edge.to] = true;
                    }
                }
            }

            sourcePathData[from] = previous;
        }
    }

    public IList<T> GetPath(T fromData, T toData)
    {
        PrecalculatePath(fromData, toData);

        Node from = nodesByData[fromData];
        Node to = nodesByData[toData];

        Dictionary<Node, Node> previous = sourcePathData[from];

        if(previous[to] == null)
            return null;

        // retreive the path
        List<T> path = new List<T>();
        Node node = to;
        while(node != from)
        {
            path.Add(node.data);
            node = previous[node];
        }
        path.Add(node.data);

        path.Reverse();

        return path;
    }
}
