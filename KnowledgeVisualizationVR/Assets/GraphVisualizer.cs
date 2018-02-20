using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour {
    public GameObject ball;

    //THESE VALUES NEED TO BE SET TO SPECIFIC VALUES AND MADE PRIVATE
    public float DEFAULT_LENGTH = 0.0f;
    public float DEFAULT_WIDTH = 0.0f;
    public float DEFAULT_HEIGHT = 0.0f;
    public float DEFAULT_VOLUME = 0.0f; //IMPORTANT: WHEN VALUES FOUND, SET THIS TO DEFAULT VOLUME
    public float DEFAULT_CUBE_ROOT = 0.0f; //IMPORTANT: WHEN VALUES FOUND, SET THIS TO DEFAULT CUBE ROOT

    public float DEFAULT_STEP = 0.10f;
    public int NUMBER_OF_ITERATIONS = 100;

    private Graph graph;
    private float volume; //this defines the boundaries of the drawn Graph
    private float optimalDistance;
    private Boundaries boundaries;

    public struct Boundaries
    {
        public float length;
        public float width;
        public float height;
    }

    public void setGraph()
    {
        graph = new Graph();
        setBoundaries();
        volume = DEFAULT_VOLUME;
        optimalDistance = DEFAULT_CUBE_ROOT;
    }
    public void setGraph(Graph graph)
    {
        this.graph = graph;
        setBoundaries();
        volume = DEFAULT_VOLUME;
        optimalDistance = DEFAULT_CUBE_ROOT;
    }

    public void setGraph(Graph graph, float x, float y, float z)
    {
        this.graph = graph;
        setBoundaries(x, y, z);
        volume = boundaries.width * boundaries.length * boundaries.height; //get volume
        optimalDistance = Mathf.Pow(volume, (1.0f / 3.0f)); //get cube root of volume
    }

    public void setBoundaries(float l, float w, float h)
    {
        boundaries.length = l;
        boundaries.width = w;
        boundaries.height = h;
    }

    public void setBoundaries()
    {
        boundaries.length = DEFAULT_LENGTH;
        boundaries.width = DEFAULT_WIDTH;
        boundaries.height = DEFAULT_HEIGHT;
    }

    public float getAttractiveForce(float x) { return x * x / optimalDistance; }
    public float getRepulsiveForce(float x) { return optimalDistance * optimalDistance / x; }

    public float cool(float t)
    {
        return t - DEFAULT_STEP;
    }

    //this algorithm does NOT use the grid variant
    //it is a replica of the pseudocode given in the Fruchterman Reingold paper
    //this is a guideline for the grid based algorithm
    public void FruchtermanReingold()
    {
        float temperature = 10.0f;
        for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
        {
            //this is the foreach for repulsive forces
            //it adds to the displacement of a node by calculating repulsive forces for each other node in the graph
            foreach (Graph.Node currentNode in graph.getNodes())
            {
                currentNode.setDisplacement(0.0f, 0.0f, 0.0f);
                //calculate forces for currentNode with all other nodes and add to displacement
                foreach (Graph.Node node in graph.getNodes())
                {
                    if (currentNode != node)
                    {
                        //difference := v.pos - u.pos
                        float differenceX = currentNode.getX() - node.getX();
                        float differenceY = currentNode.getY() - node.getY();
                        float differenceZ = currentNode.getZ() - node.getZ();

                        //this is |difference|
                        float differenceLength = Mathf.Sqrt(differenceX*differenceX +
                            differenceY*differenceY +
                            differenceZ*differenceZ);

                        //v.disp := v.disp + (difference / |difference) * repForce(|difference|)
                        float repulsiveForce = getRepulsiveForce(differenceLength);
                        currentNode.setDisplacement(
                            currentNode.getDisplacementX() + (differenceX / differenceLength) * repulsiveForce,
                            currentNode.getDisplacementY() + (differenceY / differenceLength) * repulsiveForce,
                            currentNode.getDisplacementZ() + (differenceZ / differenceLength) * repulsiveForce);
                    }
                }
                //this is the foreach for attractive forces
                //similar to above but with edges
                foreach (Graph.Edge edge in graph.getEdges())
                {
                    //difference := e.v.pos - e.u.pos
                    float differenceX = edge.getFrom().getX() - edge.getTo().getX();
                    float differenceY = edge.getFrom().getY() - edge.getTo().getY();
                    float differenceZ = edge.getFrom().getZ() - edge.getTo().getZ();

                    //|difference|
                    float differenceLength = Mathf.Sqrt(differenceX * differenceX +
                        differenceY * differenceY +
                        differenceZ * differenceZ);

                    float attractiveForce = getAttractiveForce(differenceLength);
                    
                    //e.v.disp := e.v.disp - (difference / |difference|) * attractiveForce(|difference|)
                    edge.getFrom().setDisplacement(edge.getFrom().getDisplacementX() - (differenceX / differenceLength) * attractiveForce,
                        edge.getFrom().getDisplacementY() - (differenceY / differenceLength) * attractiveForce,
                        edge.getFrom().getDisplacementZ() - (differenceZ / differenceLength) * attractiveForce);

                    //e.u.disp := e.u.disp + (difference / |difference|) * attractiveForce(|difference)
                    edge.getTo().setDisplacement(edge.getTo().getDisplacementX() - (differenceX / differenceLength) * attractiveForce,
                       edge.getTo().getDisplacementY() - (differenceY / differenceLength) * attractiveForce,
                       edge.getTo().getDisplacementZ() - (differenceZ / differenceLength) * attractiveForce);
                }

                //last foreach, limit displacement by boundaries and temperature
                foreach (Graph.Node node in graph.getNodes())
                {
                    float displacementLength = Mathf.Sqrt(Mathf.Pow(node.getDisplacementX(), 2.0f)+
                        Mathf.Pow(node.getDisplacementY(), 2.0f)+
                        Mathf.Pow(node.getDisplacementZ(), 2.0f));

                    node.setPosition(node.getX() + (node.getDisplacementX() / displacementLength) * Mathf.Min(node.getDisplacementX(), temperature),
                        node.getY() + (node.getDisplacementY() / displacementLength) * Mathf.Min(node.getDisplacementY(), temperature),
                        node.getZ() + (node.getDisplacementZ() / displacementLength) * Mathf.Min(node.getDisplacementZ(), temperature));
                    
                    node.setPosition(Mathf.Min(boundaries.width/2.0f, Mathf.Max(-boundaries.width/2, node.getX())),
                        Mathf.Min(boundaries.height/2.0f, Mathf.Max(-boundaries.height/2.0f, node.getY())),
                        Mathf.Min(boundaries.length/2.0f, Mathf.Max(-boundaries.length/2.0f, node.getZ())));
                }
                temperature = cool(temperature);
            }
        }
    }

    public void drawGraph()
    {
        List<GameObject> lines = new List<GameObject>();
        foreach (Graph.Node currentNode in graph.getNodes())
        {
            Instantiate(ball, new Vector3(currentNode.getX(), currentNode.getY(), currentNode.getZ()), Quaternion.identity);
        }
        foreach (Graph.Edge currentEdge in graph.getEdges())
        {
            //maybe use GL.LINES instead...?
            GameObject lineObject = new GameObject();
            lineObject.AddComponent<LineRenderer>();
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            //add more stuff?
        }
    }

    //to use this, first get a list of usable neighbors
    //this implementation uses a different approach, check paper to see differences
    //volume is calculated as volume = w*l*h / |nodes|
    //optimalDistance is calculated as optimaleDistance = Mathf.pow(volume, (1.0/3.0))
    public void FruchtermanReingoldGridBased()
    {

    }
}
