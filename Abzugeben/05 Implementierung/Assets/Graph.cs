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

        public bool hasContent()
        {
            return this.information.hasContent();
        }
    }

    public class Edge
    {
        private Node start;
        private Node end;
        private bool isBothWays;
        public Edge(Node start, Node end)
        {
            this.start = start;
            this.end = end;
            isBothWays = false;
        }

        public Node getFrom()
        {
            return start;
        }

        public Node getTo()
        {
            return end;
        }

        public void setBothWays(bool contrary)
        {
            isBothWays = contrary;
        }

        public bool getBothWays()
        {
            return isBothWays;
        }
    }

    public class Information
    {
        //use this class to save information from Wikipedia
        //this is ONLY used for information from Wikipedia!!
        private string name;
        private int pageviews;
        
        private Chapter chapters; //this will save all internal chapters
        //including the very first one, meaning content of <h1>
        //this actually represents the highest level chapter


        public Information()
        {
            name = "NONAME";
        }

        public Information(string name)
        {
            this.name = name;
        }

        public Information(string name, int pageviews)
        {
            this.name = name;
            this.pageviews = pageviews;
        }

        public string getName()
        {
            return name;
        }

        public int getPageviews()
        {
            return pageviews;
        }

        public bool hasContent()
        {
            if (this.chapters != null)
            {
                return true;
            }
            return false;
        }

        public class Chapter
        {
            //A chapter needs a list of subchapters
            //content representation as string, if the deepest level is reached
            //and a list of media files, if there are any
            private List<Chapter> subchapters;
            private Chapter parent;
            private string content;

            private string title;

            // It is possible to parse data from the HTML parser in Webhandler.cs
            // to lists containing textures, videos etc.
            // However this is not easy and requires a much more fleshed out parser.

            // private List<string> pictures;

            public Chapter(string name, Chapter parent)
            {
                title = name;
                this.parent = parent;
                subchapters = null;
            }

            public void setSubchapters(List<Chapter> subs)
            {
                this.subchapters = subs;
            }
            public void setContent(string cont)
            {
                this.content = cont;
            }
            public void setTitle(string t)
            {
                this.title = t;
            }

            public string getTitle()
            {
                return this.title;
            }

            public List<Chapter> getSubchapters()
            {
                return this.subchapters;
            }
            public string getContent()
            {
                return this.content;
            }
            public Chapter getParent()
            {
                return parent;
            }
        }
    }
}
