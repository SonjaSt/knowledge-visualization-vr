using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Most of the comments are for myself, so I can keep track of what I've done.
 * Focusing is hard.
 */
public class GraphVisualizer : MonoBehaviour {
    WebHandler communicator;
    bool printed = false;

    bool waiting = false;

    //this is the model for the ball
    public GameObject ball;

    //THESE VALUES NEED TO BE SET TO SPECIFIC VALUES AND MADE PRIVATE
    public float DEFAULT_LENGTH = 100.0f;
    public float DEFAULT_WIDTH = 100.0f;
    public float DEFAULT_HEIGHT = 100.0f;
    public float DEFAULT_VOLUME = 1000000.0f; //IMPORTANT: WHEN VALUES FOUND, SET THIS TO DEFAULT VOLUME
    public float DEFAULT_CUBE_ROOT = 100.0f; //IMPORTANT: WHEN VALUES FOUND, SET THIS TO DEFAULT CUBE ROOT

    public float DEFAULT_STEP = 0.10f;
    public int NUMBER_OF_ITERATIONS = 50;

    private Graph graph;
    private float volume; //this defines the boundaries of the drawn Graph
    private float optimalDistance;
    private Boundaries boundaries;

    //this extracts the body of a wikipedia article in an easy to parse style
    string message = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&explaintext=&titles=";
    //this string gets the title of a random wikipedia page
    string random = "https://de.wikipedia.org/w/api.php?action=query&list=random&format=json&rnnamespace=0&rnlimit=1";

    public struct Boundaries
    {
        public float length;
        public float width;
        public float height;
    }

    public void Start()
    {
        /*
        communicator = new WebHandler();
        //The string "Graphentheorie" is merely a teststring
        StartCoroutine(communicator.requestContent(message + "Graphentheorie"));
        
        string[] content = communicator.getSectionedContent();
        for (int i = 0; i < content.Length; i++)
        {
            Debug.Log(content[i]);
        }
        */

        /* This is for testing
         */
        temperature = 5.0f;
        makeStar();

        communicator = gameObject.AddComponent<WebHandler>();
    }

    public void makeStar()
    {
        Graph newGraph = new Graph();
        Graph.Node newNode = new Graph.Node(null);
        newNode.setPosition(0.0f, 0.0f, 0.0f);
        newGraph.addNode(newNode);
        for (int i = 0; i < 49; i++)
        {
            Graph.Node nextNode = new Graph.Node(null);
            nextNode.setPosition(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));
            Graph.Edge edge = new Graph.Edge(newNode, nextNode);

            newGraph.addNode(nextNode);
            newGraph.addEdge(edge);
        }

        setGraph(newGraph);
    }

    float time = 0;
    int counter = 0;
    List<GameObject> drawn;
    public void Update()
    {
        /*
        if (!communicator.getContent().Equals("") && printed == false)
        {
            string[] content = communicator.getSectionedContent();
            for (int i = 0; i < content.Length; i++)
            {
                Debug.Log(content[i]);
            }
            printed = true;
        }
        */

        
        time += Time.deltaTime;
        if (time > 0.05f && counter <10000)
        {
            if (drawn != null) { foreach (GameObject obj in drawn)
                {
                    Destroy(obj);
                }
            }
            FruchtermanReingold();
            drawn = drawGraph();
            counter++;
            time = 0;
        }
        

        if (Input.GetKeyDown(KeyCode.Return))
        {
            waiting = true;
            StartCoroutine(communicator.requestRandomArticle());
        }
        if (waiting)
        {
            if (communicator.isFinished()) { Debug.Log("Finished fetching random article: " + communicator.getContent()); waiting = false; }
        }
    }

    public void setGraph(Graph graph)
    {
        this.graph = graph;
        if (boundaries.height == 0 && boundaries.width == 0 && boundaries.length == 0)
        {
            setBoundaries();
        }
        volume = DEFAULT_VOLUME;
        optimalDistance = Mathf.Pow(volume/graph.getNodes().Count, (1.0f/3.0f));
    }

    public void setGraph(Graph graph, float x, float y, float z)
    {
        this.graph = graph;
        setBoundaries(x, y, z);
        volume = boundaries.width * boundaries.length * boundaries.height; //get volume
        optimalDistance = Mathf.Pow(volume/graph.getNodes().Count, (1.0f / 3.0f)); //get cube root of volume
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
        return t - DEFAULT_STEP > 0 ? t - DEFAULT_STEP : 0;
    }

    float temperature;


    //this algorithm does NOT use the grid variant
    //it is a replica of the pseudocode given in the Fruchterman Reingold paper
    //this is a guideline for the grid based algorithm

    //IMPORTANT: check divisions! differences may be 0
    public void FruchtermanReingold()
    {
        List<Graph.Node> nodes = graph.getNodes();
        List<Graph.Edge> edges = graph.getEdges();
        //float temperature = 5.0f;
        for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
        {
            //this is the foreach for repulsive forces
            //it adds to the displacement of a node by calculating repulsive forces for each other node in the graph
            foreach (Graph.Node currentNode in nodes)
            {
                currentNode.setDisplacement(0.0f, 0.0f, 0.0f);
                //calculate forces for currentNode with all other nodes and add to displacement
                foreach (Graph.Node node in nodes)
                {
                    if (currentNode != node)
                    {
                        //difference := v.pos - u.pos
                        float differenceX = currentNode.getX() - node.getX();
                        float differenceY = currentNode.getY() - node.getY();
                        float differenceZ = currentNode.getZ() - node.getZ();

                        //this is |difference|
                        float differenceLength = Mathf.Sqrt(differenceX * differenceX +
                            differenceY * differenceY +
                            differenceZ * differenceZ);

                        //v.disp := v.disp + (difference / |difference|) * repForce(|difference|)
                        float repulsiveForce = getRepulsiveForce(differenceLength);
                        currentNode.addDisplacement((differenceX / differenceLength) * repulsiveForce,
                            (differenceY / differenceLength) * repulsiveForce,
                            (differenceZ / differenceLength) * repulsiveForce);
                    }
                }

            }

            //this is the foreach for attractive forces
            //similar to above but with edges
            foreach (Graph.Edge edge in edges)
            {
                Debug.Log("Reached edge loop");
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

            int wasauchimmer = 0;    
            foreach (Graph.Node node in nodes)
            {

                if (float.IsNaN(node.getX()) || float.IsNaN(node.getY()) || float.IsNaN(node.getZ()) || float.IsNaN(node.getDisplacementX()) || float.IsNaN(node.getDisplacementY()) || float.IsNaN(node.getDisplacementZ()))
                {
                    Debug.Log("If Pos or Disp NaN then position: " + node.getX() + ", " + node.getY() + ", " + node.getZ() + "\n" +
                        "Displacement: " + node.getDisplacementX() + ", " + node.getDisplacementY() + ", " + node.getDisplacementZ() + "\n" +
                        "Temperatur: " + temperature + "\n" + 
                        "Knotenindex: " + wasauchimmer);

                    return;
                }
                float displacementLength = Mathf.Sqrt(Mathf.Pow(node.getDisplacementX(), 2.0f)+
                    Mathf.Pow(node.getDisplacementY(), 2.0f)+
                    Mathf.Pow(node.getDisplacementZ(), 2.0f));

                Debug.Log("DisplacementLength: " + displacementLength);


                //NOTE: displacement length?
                float newPosX = node.getX() + Mathf.Min(displacementLength, temperature) * node.getDisplacementX() / displacementLength;
                float newPosY = node.getY() + Mathf.Min(displacementLength, temperature) * node.getDisplacementY() / displacementLength;
                float newPosZ = node.getZ() + Mathf.Min(displacementLength, temperature) * node.getDisplacementZ() / displacementLength;

                if (float.IsNaN(newPosX) || float.IsNaN(newPosY) || float.IsNaN(newPosZ))
                {
                    Debug.Log("If newPos NaN then old Position: " + node.getX() + ", " + node.getY() + ", " + node.getZ() + "\nDisplacement: " + node.getDisplacementX() + ", " + node.getDisplacementY() + ", " + node.getDisplacementZ());
                    Debug.Log("If newPos NaN then new Position: " + newPosX + ", " + newPosY + ", " + newPosZ);
                }

                node.setPosition(newPosX,
                    newPosY,
                    newPosZ);
                    
                node.setPosition(Mathf.Min(boundaries.width/2.0f, Mathf.Max(-boundaries.width/2, node.getX())),
                    Mathf.Min(boundaries.height/2.0f, Mathf.Max(-boundaries.height/2.0f, node.getY())),
                    Mathf.Min(boundaries.length/2.0f, Mathf.Max(-boundaries.length/2.0f, node.getZ())));
                wasauchimmer++;      
            }
                
                temperature = cool(temperature);
            
        }

        Debug.Log("Reached end of Fruchterman Reingold");
    }

    public List<GameObject> drawGraph()
    {
        //draw nodes
        List<GameObject> drawnComponents = new List<GameObject>();
        List<GameObject> lines = new List<GameObject>();
        List<Graph.Node> nodes = graph.getNodes();
        List<Graph.Edge> edges = graph.getEdges();
        foreach (Graph.Node currentNode in nodes)
        {
            GameObject newBall = Instantiate(ball, new Vector3(currentNode.getX(), currentNode.getY(), currentNode.getZ()), Quaternion.identity);
            drawnComponents.Add(newBall);
        }
        
        foreach (Graph.Edge currentEdge in edges)
        {
            //maybe use GL.LINES instead...?
            GameObject lineObject = new GameObject();
            lineObject.AddComponent<LineRenderer>();
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            line.SetPosition(0, new Vector3(currentEdge.getFrom().getX(), currentEdge.getFrom().getY(), currentEdge.getFrom().getZ()));
            line.SetPosition(0, new Vector3(currentEdge.getTo().getX(), currentEdge.getTo().getY(), currentEdge.getTo().getZ()));
            
            line.SetWidth(0.01f, 0.01f);
            drawnComponents.Add(lineObject);
        }
        

        return drawnComponents;
    }

    /**
     * This method uses the title of a starting article and the amount of hops (distance from the start) to generate a graph
     * 
     * !!!ATTENTION: THIS METHOD NEEDS REVISION AS IT HAS NOT BEEN TESTED OR CHECKED IF IT IS COMPLETE!!!
     */
    IEnumerator getGraph(string startingNode, int hops)
    {
        //Note: We identify graph nodes by string because of easier comprehension
        //Note: Don't confuse the lists graph is taking care of with the list of strings of neighbors...
        Graph graph = new Graph();
        Graph.Information info = new Graph.Information(startingNode);
        Graph.Node firstNode = new Graph.Node(info);
        graph.addNode(firstNode); //this will keep track of ALL accumulated nodes and edges
        
        List<Graph.Node> nodesToCheck = new List<Graph.Node>(); //these are nodes that are found in the neighborhood
        nodesToCheck.Add(firstNode); //this only keeps track of neighborhoods and deletes nodes that have been explored already

        //NOTE: This version of BFS does currently not keep track of a "visited" list. Since we are limited by hops (which shouldn't ever go above 3 for sanity's sake), we finish searching at some point anyway
        for (int i = 0; i < hops; i++)
        {
            int currentNodesToCheck = nodesToCheck.Count;
            //this is the number of nodes to check the neighborhoods for.
            //All neighbors that are found will be added to "nodesToCheck" in the next loop
            for (int j = 0; j < currentNodesToCheck; j++)
            { 
                //this for iterates a list of nodes that will be explored
                //KEEP IN MIND it checks the amount of nodes beforehand and works on the first x found nodes because nodes are being added to the list while working on it
                //i.e. the list nodesToCheck has 1 node in the beginning (starting node), so "currentNodesToCheck" is 1
                //we now explore this node and find all neighbors and add them to the graph and to nodesToCheck
                //after the next for loop, nodesToCheck will have a lot of new nodes
                //when the hop is done, meaning the for loop iterating j has come to an end, delete the first currentNodesToCheck-many elements in nodesToCheck
                yield return StartCoroutine(communicator.requestNeighbors(nodesToCheck[j]));
                List<string> tempNeighbors = communicator.getNeighbors();
                for (int z = 0; z < tempNeighbors.Count; z++)
                {
                    //this for adds all neighbors of a node that is currently being explored to the list of nodes to be explored and the graph
                    Graph.Information neighborInfo = new Graph.Information(tempNeighbors[z]);
                    Graph.Node nextNode = new Graph.Node(neighborInfo);
                    Graph.Edge nextedge = new Graph.Edge(nodesToCheck[j], nextNode);
                    if (!graph.getNodes().Exists(node => node.getName().Equals(tempNeighbors[z])))
                    {
                        graph.addNode(nextNode); //add neighbor to graph if not already in graph
                    }
                    if (!graph.getEdges().Exists(edge => edge.getFrom().getName().Equals(nodesToCheck[j]) && edge.getTo().getName().Equals(tempNeighbors[z]))) //I don't want to talk about this
                    {
                        //above if should check if edge is already in list

                        /**
                        int oppositeEdge = graph.getEdges().FindIndex(edge => edge.getFrom().getName().Equals(tempNeighbors[z]) && edge.getTo().getName().Equals(nodesToCheck[j]));
                        if (oppositeEdge >= 0)
                        {
                            //check if the edge exists in the opposite direction (try to find an edge where FROM is the neighbor that is looked at and TO is the node that is being explored)
                            graph.getEdges()[oppositeEdge]...
                        }
                        
                        IMPORTANT NOTE: THE ABOVE STEP IS NECESSARY WHEN AN EDGE IS SAVED WITH A FLAG THAT INDICATES IT GOES IN BOTH DIRECTIONS
                        AN EDGE IS CURRENTLY MODELED SO THAT EDGES BETWEEN TWO NODES CAN BE ADDED TWICE IF THEY ARE OPPOSITES
                        I only realised I could've left this out after I almost finished it.
                        */
                        graph.addEdge(nextedge); //add edge to graph if not already in graph
                    }
                    nodesToCheck.Add(nextNode); //this adds the currently looked at node to the list of nodes to explore
                }
            }
            nodesToCheck.RemoveRange(0, currentNodesToCheck); //see comment at start of for loop iterating j for more information on why this is necessary
        }
        this.graph = graph;
    }


    //This method downsizes a graph to a given number of maximum nodes
    //The metric used is the number of pageviews an article has (relates to interest)
    //To make things easier, when passing Information to the graph, add a number of pageviews to the Information

    //This method works in-place on the graph saved in this class
    public void downsizeGraph(int maxNodes)
    {
        if (graph.getNodes().Count <= maxNodes) { return; }
        else
        {
            //the order of nodes is not important to the algorithms we use
            //therefore we take the easy way out and sort our node-list in-place by pageviews
            //make sure to not forget edges

            graph.getNodes().Sort((x, y) => x.getInformation().getPageviews().CompareTo(y.getInformation().getPageviews()));
            //What now?
            //Go through first x many Elements to delete every node that isn't relevant enough and also corresponding edges
            //x here is graph.getNodes().Count - maxNodes
            int numberToErase = graph.getNodes().Count - maxNodes;
            for (int i = 0; i < numberToErase; i++)
            {
                string currentName = graph.getNodes()[0].getName(); //we are always looking at the first Index, 0
                graph.getEdges().RemoveAll(x => x.getFrom().getName().Equals(currentName) || x.getTo().getName().Equals(currentName)); //delete all edges with 0 involved
                graph.getNodes().RemoveAt(0); //finally remove the node itself, the next node (1) now becomes 0 and is being looked at.
            }
            //after finishing this loop, all unnecessary nodes and edges will be gone
            //hopefully.
        }
    }

    //to use this, first get a list of usable neighbors
    //this implementation uses a different approach, check paper to see differences
    //volume is calculated as volume = w*l*h / |nodes|
    //optimalDistance is calculated as optimaleDistance = Mathf.pow(volume, (1.0/3.0))
    /*public void FruchtermanReingoldGridBased()
    {

    }*/
}
