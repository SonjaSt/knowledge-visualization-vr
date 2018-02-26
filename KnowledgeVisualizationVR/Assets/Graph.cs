using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph {

    //use lists for easier comprehension in the implementation of the Fruchterman-Reingold algorithm
    private List<Node> nodes;
    private List<Edge> edges;

    public Graph()
    {
        nodes = new List<Node>();
        edges = new List<Edge>();
    }

    public List<Node> getNodes() { return nodes; }
    public List<Edge> getEdges() { return edges; }

    public void addNode(Node node)
    {
        nodes.Add(node);
    }

    public void addEdge(Edge edge)
    {
        edges.Add(edge);
    }
    public class Node
    {
        private Information information;
        private List<Node> neighbors;
        private Position position; //see v.pos in Fruchterman-Reingold paper
        private Position displacement; //see v.disp in Fruchterman-Reingold paper

        public struct Position
        {
            public float x;
            public float y;
            public float z;
        }

        public Node(Information info)
        {
            this.information = info;
            neighbors = new List<Node>();
            setPosition(0.0f, 0.0f, 0.0f);
        }
        public Node(Information info, List<Node> neighbors)
        {
            this.information = info;
            this.neighbors = neighbors;
            setPosition(0.0f, 0.0f, 0.0f);
        }

        public Node(Information info, List<Node> neighbors, float x, float y, float z)
        {
            this.information = info;
            this.neighbors = neighbors;
            setPosition(x, y, z);
        }

        public void setPosition(float x, float y, float z)
        {
            position.x = x;
            position.y = y;
            position.z = z;
        }

        public void setDisplacement(float x, float y, float z)
        {
            displacement.x = x;
            displacement.y = y;
            displacement.z = z;
        }

        public void addDisplacement(float x, float y, float z)
        {
            displacement.x += x;
            displacement.y += y;
            displacement.z += z;
        }

        /**
         * This method adds a neighbor to the neighborlist
         **/
        public void addNeighbor(Node neighbor)
        {
            neighbors.Add(neighbor);
        }

        /**
        * This method removes a neighbor from the neighborlist
        * The neighbor is given as type Node
        **/
        public void removeNeighbor(Node node)
        {
            neighbors.Remove(node);
        }

        /**
         * This method removes a neighbor from the neighborlist
         * The neighbor is found by name (titles on Wikipedia are unique!)
         **/
        public void removeNeighbor(string name)
        {
            foreach (Node node in neighbors)
            {
                if (node.getName().Equals(""))
                {
                    neighbors.Remove(node);
                    return;
                }
            }
        }

        public Information getInformation() { return this.information; }
        public string getName() { return this.information.getName(); }
        public List<Node> getNeighbors() { return neighbors; }
        public float getX() { return position.x; }
        public float getY() { return position.y; }
        public float getZ() { return position.z; }
        public float getDisplacementX() { return displacement.x; }
        public float getDisplacementY() { return displacement.y; }
        public float getDisplacementZ() { return displacement.z; }
    }

    public class Edge
    {
        private Node start;
        private Node end;
        public Edge(Node start, Node end)
        {
            this.start = start;
            this.end = end;
        }

        public Node getFrom()
        {
            return start;
        }

        public Node getTo()
        {
            return end;
        }
    }

    public class Information
    {
        //STUB
        //use this class to save information from Wikipedia
        //this is ONLY used for information from Wikipedia!!
        private string name;

        public Information()
        {
            name = "NONAME";
        }

        public string getName()
        {
            return name;
        }
    }
}
