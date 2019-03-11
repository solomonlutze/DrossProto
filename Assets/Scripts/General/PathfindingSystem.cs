using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class Node {
    public int x;
    public int y;
    public int f;
    public int g;
    public int h;
    public string id;
    public Node parent;
    public Constants.FloorLayer floor;
}

// data structures we need backing this:
// dictionary of IDs (probably constructed from XY coords) to nodes
// priority queue of IDs; this is so the contains check will match
// (which it wouldn't if the queue contained the nodes themselves)
// if we find that our open list already has a node ID, we compare the node's
// g value (distance from start) with our own, and replace its parent/g if ours is better
public class PathfindingSystem : MonoBehaviour {

    public GridManager gridManager;

    public List<Node> CalculatePathToTarget(Vector3 startPosition, TileLocation targetLocation, CharacterAI ai) {
        SimplePriorityQueue<string> closedNodes = new SimplePriorityQueue<string>();
        SimplePriorityQueue<string> openNodes = new SimplePriorityQueue<string>();
        Dictionary<string, Node> nodeIdsToNodes = new Dictionary<string, Node>();
        List<Node> adjacentNodes;
        List<Node> finalPath = new List<Node>();
        Node startNode = InitNewNode(Mathf.FloorToInt(startPosition.x), Mathf.FloorToInt(startPosition.y), ai.currentFloor, null, targetLocation);
        string endNodeId = Mathf.RoundToInt(targetLocation.position.x) + ","+Mathf.RoundToInt(targetLocation.position.y)+","+targetLocation.floorLayer;
        openNodes.Enqueue(startNode.id, startNode.f);
        nodeIdsToNodes[startNode.id] = startNode;
        while (openNodes.Count > 0) {
            Node nextNode = nodeIdsToNodes[openNodes.Dequeue()];
            closedNodes.Enqueue(nextNode.id, nextNode.f);
            if (closedNodes.Contains(endNodeId)) {
                Node n = nodeIdsToNodes[endNodeId];
                while (n.parent != null) {
                    finalPath.Add(n);
                    n = n.parent;
                }
                finalPath.Reverse();
                Debug.Log("Found a path!");
                break;
            }
            adjacentNodes = GetAdjacentNodes(nextNode, targetLocation, ai);
            foreach(Node node in adjacentNodes) {
                if (closedNodes.Contains(node.id)) {
                    continue;
                }
                if (!openNodes.Contains(node.id)) {
                    openNodes.Enqueue(node.id, node.f);
                    nodeIdsToNodes[node.id] = node;
                } else {
                    if (nodeIdsToNodes[node.id].f > node.f) {
                        nodeIdsToNodes[node.id] = node;
                    }
                }
            }
        }
        return finalPath;
        // Each node has a score F, equal to G+H
        // G is cost to get to this node from original node
        // H is ESTIMATED cost from current node to final node
        // openNodes initially contains only startNode
        // Each cycle, we take the node in openNodes with the lowest cost.
        // We add it to closedNodes, then for each of its neighbors:
        //    if it's in closedNodes, ignore it;
        //    otherwise, calcualte its F score;
        //      if it isn't in the open list already, add it
        //      if it is in the open list, use whichever score is lower (and update parent accordingly)

    }


    List<Node> GetAdjacentNodes(Node originNode, TileLocation targetLocation, CharacterAI ai) {
        List<Node> nodes = new List<Node>();
        MaybeAddNode(nodes, originNode.x+1, originNode.y, originNode.floor, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.x-1, originNode.y, originNode.floor, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.x, originNode.y+1, originNode.floor, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.x, originNode.y-1, originNode.floor, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.x, originNode.y, originNode.floor+1, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.x, originNode.y, originNode.floor-1, originNode, targetLocation, ai);
        return nodes;
    }

    public bool IsPathClearOfHazards(Collider2D col, TileLocation target, CharacterAI ai) {
		if (target.floorLayer != ai.currentFloor) {
            Debug.Log("PathClearOfHazards - false, floors differ");
            return false;
        }
        Vector3[] colliderCorners = new Vector3[]{
			new Vector3 (col.bounds.extents.x, col.bounds.extents.y, 0),
			new Vector3 (-col.bounds.extents.x, col.bounds.extents.y, 0),
			new Vector3 (col.bounds.extents.x, -col.bounds.extents.y, 0),
			new Vector3 (-col.bounds.extents.x, -col.bounds.extents.y, 0),
		};
        HashSet<EnvironmentTile> tilesAlongPath = new HashSet<EnvironmentTile>();
        foreach (Vector3 pt in colliderCorners) {
            tilesAlongPath.UnionWith(GetAllTilesBetweenPoints(ai.transform.TransformPoint(pt), target));
        }
        foreach(EnvironmentTile et in tilesAlongPath) {
            if (!CanPassOverTile(et, ai)) {
                Debug.Log("PathClearOfHazards - false, can't pass over tile "+et.name);
                return false;
            }
        }
        Debug.Log("PathClearOfHazards - true!!");
        return true;
    }

    private bool CanPassOverTile(EnvironmentTile tile, CharacterAI ai) {
        return
            tile != null &&
            (ai.currentTileHeightLevel > tile.tileHeight || (tile.colliderType == Tile.ColliderType.None && !tile.dealsDamage) && !tile.ShouldRespawnPlayer())
        ;
    }

    // remorselessly borrowed from https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
    //TODO: in order to prevent enemies from deciding to step on hazards at low slopes, might need to conditionally floor or ceil instead of RoundToInt
    // This function assumes both points are on the same floor layer. You've been warned!!
    public HashSet<EnvironmentTile> GetAllTilesBetweenPoints(Vector3 origin, TileLocation target) {
        int w = Mathf.RoundToInt(target.position.x) - Mathf.RoundToInt(origin.x);
        int h = Mathf.RoundToInt(target.position.y) - Mathf.RoundToInt(origin.y);
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w<0) dx1 = -1 ; else if (w>0) dx1 = 1;
        if (h<0) dy1 = -1 ; else if (h>0) dy1 = 1;
        if (w<0) dx2 = -1 ; else if (w>0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest>shortest)) {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h<0) dy2 = -1 ; else if (h>0) dy2 = 1;
            dx2 = 0 ;
        }
        int numerator = longest >> 1 ;
        HashSet<EnvironmentTile> res = new HashSet<EnvironmentTile>();

        int currentX = Mathf.RoundToInt(origin.x);
        int currentY = Mathf.RoundToInt(origin.y);
        for (int i=0;i<=longest;i++) {
            LayerFloor layer = gridManager.layerFloors[target.floorLayer];
            Vector3Int pos = new Vector3Int (currentX, currentY, 0);
            res.Add((EnvironmentTile) layer.groundTilemap.GetTile (pos));
            EnvironmentTile objectTile = (EnvironmentTile) layer.objectTilemap.GetTile (new Vector3Int (currentX, currentY, 0));
            if (objectTile != null) {
                res.Add(objectTile);
            }
            numerator += shortest;
            if (!(numerator<longest)) {
                numerator -= longest;
                currentX += dx1;
                currentY += dy1;
            } else {
                currentX += dx2;
                currentY += dy2;
            }
        }
        return res;
    }

    bool ConnectionBetweenNodesOnDifferentFloorsExists(Node currentNode, Constants.FloorLayer newFloor) {
        LayerFloor layer = gridManager.layerFloors[currentNode.floor];
        if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null) {
            // this should not happen
            Debug.LogError("missing layer information for "+currentNode.floor);
            return false;
        }
        EnvironmentTile groundTile = (EnvironmentTile) layer.groundTilemap.GetTile (new Vector3Int (currentNode.x, currentNode.y, 0)); // is this in our floor?
        EnvironmentTile objectTile = (EnvironmentTile) layer.objectTilemap.GetTile (new Vector3Int (currentNode.x, currentNode.y, 0)); // is this in our floor?
        Constants.FloorLayer? targetLayer = null;
        if (objectTile != null && objectTile.changesFloorLayer) {
            targetLayer = objectTile.targetFloorLayer;
        } else if (groundTile != null && groundTile.changesFloorLayer) {
            targetLayer = objectTile.targetFloorLayer;
        }
        return (targetLayer != null && targetLayer == newFloor);
    }

    void MaybeAddNode(List<Node> nodeList, int x, int y, Constants.FloorLayer floor, Node originNode, TileLocation targetLocation, CharacterAI ai) {
        if (!gridManager.layerFloors.ContainsKey(floor)) { return; }
        LayerFloor layer = gridManager.layerFloors[floor];
        if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null) {
            // this floor doesn't exist, so don't worry about it
            return;
        }
        if (floor != originNode.floor && !ConnectionBetweenNodesOnDifferentFloorsExists(originNode, floor)) {
            return;
        }
        Vector3Int tilePos = new Vector3Int (x, y, 0);
        EnvironmentTile et = (EnvironmentTile) layer.groundTilemap.GetTile(tilePos);
        if (!CanPassOverTile(et, ai)) {
            return;
        }
        et = (EnvironmentTile) layer.objectTilemap.GetTile(tilePos);
        if (et != null && et.name == "RockTile") {
            Debug.Log("checking rockTile...");
            Debug.Log("canPassOverRockTile: "+CanPassOverTile(et, ai));
        }
        if (et != null && !CanPassOverTile(et, ai)) {
            return;
        }
        nodeList.Add(InitNewNode(x, y, floor, originNode, targetLocation));
    }

    Node InitNewNode(int x, int y, Constants.FloorLayer floor, Node parent, TileLocation targetLocation) {
        Node newNode = new Node();
        newNode.id = x+","+y+","+floor;
        newNode.x = x;
        newNode.y = y;
        newNode.floor = floor;
        newNode.g = 0;
        newNode.h = 0;
        newNode.parent = null;
        if (parent != null) {
            newNode.parent = parent;
            newNode.g = parent.g + 1;
            newNode.h = Mathf.Abs(Mathf.RoundToInt(targetLocation.position.x) - x)
                        + Mathf.Abs(Mathf.RoundToInt(targetLocation.position.y) - y)
                        + Mathf.Abs(targetLocation.floorLayer - floor);
            newNode.f = newNode.g + newNode.h;
        }
        return newNode;
    }
}