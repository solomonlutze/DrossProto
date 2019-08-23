using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class Node {
    public int f;
    public int g;
    public int h;
    public TileLocation loc;
    public Node parent;
}

// data structures we need backing this:
// dictionary of IDs (probably constructed from XY coords) to nodes
// priority queue of IDs; this is so the contains check will match
// (which it wouldn't if the queue contained the nodes themselves)
// if we find that our open list already has a node ID, we compare the node's
// g value (distance from start) with our own, and replace its parent/g if ours is better
public class PathfindingSystem : Singleton<PathfindingSystem> {

    public GridManager gridManager;

    public IEnumerator CalculatePathToTarget(Vector3 startPosition, TileLocation targetLocation, CharacterAI ai) {
        Stopwatch timeSpentThisFrame = new Stopwatch();
        SimplePriorityQueue<TileLocation> closedNodes = new SimplePriorityQueue<TileLocation>();
        SimplePriorityQueue<TileLocation> openNodes = new SimplePriorityQueue<TileLocation>();
        Dictionary<TileLocation, Node> nodeLocationsToNodes = new Dictionary<TileLocation, Node>();
        List<Node> adjacentNodes;
        List<Node> finalPath = new List<Node>();
        Node startNode = InitNewNode(Mathf.FloorToInt(startPosition.x), Mathf.FloorToInt(startPosition.y), ai.currentFloor, null, targetLocation);
        openNodes.Enqueue(startNode.loc, startNode.f);
        nodeLocationsToNodes[startNode.loc] = startNode;
        timeSpentThisFrame.Start();
        while (openNodes.Count > 0) {
            if (timeSpentThisFrame.ElapsedMilliseconds > 15) {
                yield return null;
                timeSpentThisFrame.Restart();
            }
            if (openNodes.Count > 80 || closedNodes.Count > 80) {
                ai.SetPathToTarget(null);
                yield break;
            }
            Node nextNode = nodeLocationsToNodes[openNodes.Dequeue()];
            closedNodes.Enqueue(nextNode.loc, nextNode.f);
            if (closedNodes.Contains(targetLocation)) {
                Node n = nodeLocationsToNodes[targetLocation];
                while (n.parent != null) {
                    finalPath.Add(n);
                    n = n.parent;
                }
                finalPath.Reverse();
                break;
            }
            adjacentNodes = GetAdjacentNodes(nextNode, targetLocation, ai);
            foreach(Node node in adjacentNodes) {
                if (closedNodes.Contains(node.loc)) {
                    continue;
                }
                if (!openNodes.Contains(node.loc)) {
                    openNodes.Enqueue(node.loc, node.f);
                    nodeLocationsToNodes[node.loc] = node;
                } else {
                    if (nodeLocationsToNodes[node.loc].f > node.f) {
                        nodeLocationsToNodes[node.loc] = node;
                    }
                }
            }
        }
        ai.SetPathToTarget(finalPath);
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
        MaybeAddNode(nodes, originNode.loc.position.x+1, originNode.loc.position.y, originNode.loc.floorLayer, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.loc.position.x-1, originNode.loc.position.y, originNode.loc.floorLayer, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.loc.position.x, originNode.loc.position.y+1, originNode.loc.floorLayer, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.loc.position.x, originNode.loc.position.y-1, originNode.loc.floorLayer, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.loc.position.x, originNode.loc.position.y, originNode.loc.floorLayer+1, originNode, targetLocation, ai);
        MaybeAddNode(nodes, originNode.loc.position.x, originNode.loc.position.y, originNode.loc.floorLayer-1, originNode, targetLocation, ai);
        return nodes;
    }

    public bool IsPathClearOfHazards(Collider2D col, TileLocation target, CharacterAI ai) {
      if (target.floorLayer != ai.currentFloor) {
              return false;
          }
          Vector3[] colliderCorners = new Vector3[]{
        new Vector3 (col.bounds.extents.x, col.bounds.extents.y, 0),
        new Vector3 (-col.bounds.extents.x, col.bounds.extents.y, 0),
        new Vector3 (col.bounds.extents.x, -col.bounds.extents.y, 0),
        new Vector3 (-col.bounds.extents.x, -col.bounds.extents.y, 0),
      };
      HashSet<EnvironmentTileInfo> tilesAlongPath = new HashSet<EnvironmentTileInfo>();
      foreach (Vector3 pt in colliderCorners) {
        tilesAlongPath.UnionWith(GetAllTilesBetweenPoints(ai.transform.TransformPoint(pt), target));
      }
      bool debugFoundAnObject = false;
      foreach(EnvironmentTileInfo eti in tilesAlongPath) {
        eti.DebugHighlightSquare();
        if (eti.objectTileType != null) { debugFoundAnObject = true;}
        if (!CanPassOverTile(eti, ai)) {
          return false;
        }
      }
      if (debugFoundAnObject) {
        UnityEngine.Debug.LogError("what the hell??");
        foreach(EnvironmentTileInfo eti in tilesAlongPath) {
          if (eti.objectTileType != null) {
            UnityEngine.Debug.Log("found object tile type "+eti.objectTileType+" at "+eti.tileLocation);
          }
        }
      }
      // UnityEngine.Debug.Break();
      return true;
    }

    private bool CanPassOverTile(EnvironmentTileInfo tile, CharacterAI ai) {
        return
            tile != null
            && tile.GetColliderType() == Tile.ColliderType.None
            && !tile.dealsDamage
            && !tile.CanRespawnPlayer()
        ;
    }

    private bool TakesDamageFromTile(EnvironmentTile tile, CharacterAI ai) {
        // return !tile.dealsDamage || ai.damageTypeResistances[tile.environmentalDamage.damageType] == 100;
        return !tile.dealsDamage;
    }
    // remorselessly borrowed from https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
    //TODO: in order to prevent enemies from deciding to step on hazards at low slopes, might need to conditionally floor or ceil instead of RoundToInt
    // This function assumes both points are on the same floor layer. You've been warned!!
    public HashSet<EnvironmentTileInfo> GetAllTilesBetweenPoints(Vector3 origin, TileLocation target) {
        int w = Mathf.RoundToInt(target.center.x) - Mathf.RoundToInt(origin.x);
        int h = Mathf.RoundToInt(target.center.y) - Mathf.RoundToInt(origin.y);
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
        HashSet<EnvironmentTileInfo> res = new HashSet<EnvironmentTileInfo>();

        int currentX = Mathf.RoundToInt(origin.x);
        int currentY = Mathf.RoundToInt(origin.y);
        res.Add(GridManager.Instance.GetTileAtLocation(Mathf.FloorToInt(origin.x), Mathf.FloorToInt(origin.y), target.floorLayer));
        UnityEngine.Debug.DrawLine(origin, new Vector3(target.position.x, target.position.y, 0), Color.green, .1f);
        for (int i=0;i<=longest;i++) {
            Vector3Int pos = new Vector3Int (currentX, currentY, 0);
            res.Add((EnvironmentTileInfo) GridManager.Instance.GetTileAtLocation(currentX, currentY, target.floorLayer));
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

    bool ConnectionBetweenNodesOnDifferentFloorsExists(Node currentNode, FloorLayer newFloor) {
        LayerFloor layer = gridManager.layerFloors[currentNode.loc.floorLayer];
        if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null) {
            // this should not happen
            UnityEngine.Debug.LogError("missing layer information for "+currentNode.loc.floorLayer);
            return false;
        }
        EnvironmentTileInfo tileInfo = GridManager.Instance.GetTileAtLocation(currentNode.loc);
        if (tileInfo == null) { return false; }
        FloorLayer? targetLayer = null;
        if (tileInfo.ChangesFloorLayer()) {
          targetLayer = tileInfo.GetTargetFloorLayer(currentNode.loc.floorLayer);
        }
        return (targetLayer != null && targetLayer == newFloor);
    }

    void MaybeAddNode(List<Node> nodeList, int x, int y, FloorLayer floor, Node originNode, TileLocation targetLocation, CharacterAI ai) {
        if (!gridManager.layerFloors.ContainsKey(floor)) { return; }
        LayerFloor layer = gridManager.layerFloors[floor];
        if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null) {
            // this floor doesn't exist, so don't worry about it
            return;
        }
        if (floor != originNode.loc.floorLayer && !ConnectionBetweenNodesOnDifferentFloorsExists(originNode, floor)) {
            return;
        }
        Vector2Int tilePos = new Vector2Int (x, y);
        EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(new TileLocation(tilePos, floor));
        if (eti == null) { return; }
        if (!CanPassOverTile(eti, ai)) {
            return;
        }
        nodeList.Add(InitNewNode(x, y, floor, originNode, targetLocation));
    }

    Node InitNewNode(int x, int y, FloorLayer floor, Node parent, TileLocation targetLocation) {
        Node newNode = new Node();
        newNode.loc = new TileLocation(new Vector2Int(x, y), floor);
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