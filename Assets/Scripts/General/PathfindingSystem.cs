using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class Node
{
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
public class PathfindingSystem : Singleton<PathfindingSystem>
{

  public GridManager gridManager;

  public IEnumerator CalculatePathToTarget(Vector3 startPosition, TileLocation targetLocation, AiStateController ai, AiAction initiatingAction, WorldObject objectOfInterest = null)
  {
    ai.SetIsCalculatingPath(true);
    bool foundPath = false;
    Stopwatch timeSpentThisFrame = new Stopwatch();
    SimplePriorityQueue<TileLocation> closedNodes = new SimplePriorityQueue<TileLocation>();
    SimplePriorityQueue<TileLocation> openNodes = new SimplePriorityQueue<TileLocation>();
    Dictionary<TileLocation, Node> nodeLocationsToNodes = new Dictionary<TileLocation, Node>();
    List<Node> adjacentNodes;
    List<Node> finalPath = new List<Node>();
    Node startNode = InitNewNode(Mathf.FloorToInt(startPosition.x), Mathf.FloorToInt(startPosition.y), 0, ai.currentFloor, null, targetLocation);
    openNodes.Enqueue(startNode.loc, startNode.f);
    nodeLocationsToNodes[startNode.loc] = startNode;
    timeSpentThisFrame.Start();
    while (openNodes.Count > 0)
    {
      if (timeSpentThisFrame.ElapsedMilliseconds > 1)
      {
        yield return null;
        timeSpentThisFrame.Restart();
      }
      if (openNodes.Count > 80 || closedNodes.Count > 80)
      {
        // We should give up on finding a path
        ai.SetIsCalculatingPath(false);
        ai.SetPathToTarget(null);
        yield break;
      }
      Node nextNode = nodeLocationsToNodes[openNodes.Dequeue()];
      closedNodes.Enqueue(nextNode.loc, nextNode.f);
      if (closedNodes.Contains(targetLocation))
      {
        Node n = nodeLocationsToNodes[targetLocation];
        while (n.parent != null)
        {
          finalPath.Add(n);
          n = n.parent;
        }
        finalPath.Reverse();
        foundPath = true;
        break;
      }
      adjacentNodes = GetAdjacentNodes(nextNode, targetLocation, ai, initiatingAction);
      foreach (Node node in adjacentNodes)
      {
        if (closedNodes.Contains(node.loc))
        {
          continue;
        }
        if (!openNodes.Contains(node.loc))
        {
          openNodes.Enqueue(node.loc, node.f);
          nodeLocationsToNodes[node.loc] = node;
        }
        else
        {
          if (nodeLocationsToNodes[node.loc].f > node.f)
          {
            nodeLocationsToNodes[node.loc] = node;
          }
        }
      }
    }
    ai.SetIsCalculatingPath(false);
    if (foundPath)
    {
      DebugDrawPath(finalPath, .01f);
      if (objectOfInterest != null) { ai.objectOfInterest = objectOfInterest; }
      ai.SetPathToTarget(finalPath);
    }
    else
    {
      ai.SetPathToTarget(null);
    }
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


  private void DebugDrawPath(List<Node> path, float t)
  {
    for (int i = 0; i < path.Count - 1; i++)
    {
      UnityEngine.Debug.DrawLine(path[i].loc.position3D, path[i + 1].loc.position3D, Color.red, t);
    }
  }

  List<Node> GetAdjacentNodes(Node originNode, TileLocation targetLocation, AiStateController ai, AiAction initiatingAction)
  {
    List<Node> nodes = new List<Node>();
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x + 1, originNode.loc.tilemapPosition.y, originNode.loc.floorLayer, originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x - 1, originNode.loc.tilemapPosition.y, originNode.loc.floorLayer, originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x, originNode.loc.tilemapPosition.y + 1, originNode.loc.floorLayer, originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x, originNode.loc.tilemapPosition.y - 1, originNode.loc.floorLayer, originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x, originNode.loc.tilemapPosition.y, originNode.loc.floorLayer + 1, originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, originNode.loc.tilemapPosition.x, originNode.loc.tilemapPosition.y, originNode.loc.floorLayer - 1, originNode, targetLocation, ai, initiatingAction);
    return nodes;
  }

  public bool IsPathClearOfHazards(Vector3 targetPosition, FloorLayer targetFloor, AiStateController ai)
  {
    if (targetFloor != ai.currentFloor)
    {
      return false;
    }
    Vector3[] colliderCorners = new Vector3[]{
      new Vector3 (ai.circleCollider.radius, 0, 0),
      new Vector3 (-ai.circleCollider.radius, 0, 0),
      new Vector3 (0, ai.circleCollider.radius, 0),
      new Vector3 (0, -ai.circleCollider.radius, 0),
			// new Vector3 (col.bounds.extents.x, col.bounds.extents.y, 0),
			// new Vector3 (-col.bounds.extents.x, col.bounds.extents.y, 0),
			// new Vector3 (col.bounds.extents.x, -col.bounds.extents.y, 0),
			// new Vector3 (-col.bounds.extents.x, -col.bounds.extents.y, 0),
		};
    HashSet<EnvironmentTileInfo> tilesAlongPath = new HashSet<EnvironmentTileInfo>();
    foreach (Vector3 pt in colliderCorners)
    {
      tilesAlongPath.UnionWith(GetAllTilesBetweenPoints(ai.transform.TransformPoint(pt), targetPosition + pt, targetFloor));
    }
    // foreach (EnvironmentTileInfo eti in tilesAlongPath)
    // {
    // GridManager.Instance.DEBUGHighlightTile(eti.tileLocation);
    // }
    foreach (EnvironmentTileInfo eti in tilesAlongPath)
    {
      if (eti == null || eti.dealsDamage)
      {
        return false;
      }
      if (!CanPassOverTile(eti, ai))
      {
        return false;
      }
    }
    return true;
  }

  // USUALLY returns either 1 (can cross) or -1 (can't cross).
  // MAY return a higher value if the tile deals damage; edit this function to adjust how hard that's weighed
  private int GetNodeTravelCost(EnvironmentTileInfo tileInfo, AiStateController ai, AiAction initiatingAction)
  {
    if (!tileInfo.CharacterCanOccupyTile(ai) || !tileInfo.CharacterCanCrossTile(ai))
    {
      return -1;
    }
    int cost = 1;
    if (tileInfo.dealsDamage)
    {
      foreach (EnvironmentalDamage envDamage in tileInfo.environmentalDamageSources)
      {

        if (ai.GetDamageTypeResistanceLevel(envDamage.damageType) < envDamage.GetResistanceRequiredForImmunity())
        {
          if (initiatingAction.hazardCrossingCost == -1) { return -1; }
          cost += initiatingAction.hazardCrossingCost;
        }
      }
    }
    return cost;
  }
  private bool CanPassOverTile(EnvironmentTileInfo tile, AiStateController ai)
  {
    return
        tile != null
        && ((tile.GetColliderType() == Tile.ColliderType.None && !tile.dealsDamage) && !tile.CanRespawnPlayer())
    ;
  }

  private bool TakesDamageFromTile(EnvironmentTile tile, AiStateController ai)
  {
    // return !tile.dealsDamage || ai.damageTypeResistances[tile.environmentalDamage.damageType] == 100;
    return !tile.dealsDamage;
  }

  //TODO: in order to prevent enemies from deciding to step on hazards at low slopes, might need to conditionally floor or ceil instead of RoundToInt
  // This function assumes both points are on the same floor layer. You've been warned!!
  public HashSet<EnvironmentTileInfo> GetAllTilesBetweenPoints(Vector3 origin, Vector3 target, FloorLayer floor)
  {
    UnityEngine.Debug.DrawLine(origin, target, Color.blue);
    HashSet<EnvironmentTileInfo> res = new HashSet<EnvironmentTileInfo>();
    float x0 = origin.x;
    float x1 = target.x;
    float y0 = origin.y;
    float y1 = target.y;
    float dx = Mathf.Abs(x1 - x0);
    float dy = Mathf.Abs(y1 - y0);

    int x = Mathf.FloorToInt(x0);
    int y = Mathf.FloorToInt(y0);

    int n = 1;
    int x_inc, y_inc;
    float difference;

    if (dx == 0)
    {
      x_inc = 0;
      difference = Mathf.Infinity;
    }
    else if (x1 > x0)
    {
      x_inc = 1;
      n += Mathf.FloorToInt(x1) - x;
      difference = (Mathf.FloorToInt(x0) + 1 - x0) * dy;
    }
    else
    {
      x_inc = -1;
      n += x - Mathf.FloorToInt(x1);
      difference = (x0 - Mathf.FloorToInt(x0)) * dy;
    }

    if (dy == 0)
    {
      y_inc = 0;
      difference -= Mathf.Infinity;
    }
    else if (y1 > y0)
    {
      y_inc = 1;
      n += Mathf.FloorToInt(y1) - y;
      difference -= (Mathf.FloorToInt(y0) + 1 - y0) * dx;
    }
    else
    {
      y_inc = -1;
      n += y - Mathf.FloorToInt(y1);
      difference -= (y0 - Mathf.FloorToInt(y0)) * dx;
    }

    for (; n > 0; --n)
    {
      res.Add(GridManager.Instance.GetTileAtTilemapLocation(x, y, floor));

      if (difference > 0)
      {
        y += y_inc;
        difference -= dx;
      }
      else
      {
        x += x_inc;
        difference += dy;
      }
    }
    // int w = Mathf.FloorToInt(target.x) - Mathf.FloorToInt(origin.x);
    // int h = Mathf.FloorToInt(target.y) - Mathf.FloorToInt(origin.y);
    // int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
    // if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
    // if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
    // if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
    // int longest = Mathf.Abs(w);
    // int shortest = Mathf.Abs(h);
    // if (!(longest > shortest))
    // {
    //   longest = Mathf.Abs(h);
    //   shortest = Mathf.Abs(w);
    //   if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
    //   dx2 = 0;
    // }
    // int numerator = longest >> 1;
    // HashSet<EnvironmentTileInfo> res = new HashSet<EnvironmentTileInfo>();
    // // UnityEngine.Debug.DrawLine(origin, new Vector3(target.x, target.y, 0), Color.green, .5f);
    // res.Add(GridManager.Instance.GetTileAtLocation(origin, floor));
    // int currentX = Mathf.FloorToInt(origin.x);
    // int currentY = Mathf.FloorToInt(origin.y);
    // for (int i = 0; i <= longest; i++)
    // {
    //   Vector3Int pos = new Vector3Int(currentX, currentY, 0);
    //   res.Add(GridManager.Instance.GetTileAtLocation(currentX, currentY, floor));
    //   numerator += shortest;
    //   if (!(numerator < longest))
    //   {
    //     numerator -= longest;
    //     currentX += dx1;
    //     currentY += dy1;
    //   }
    //   else
    //   {
    //     currentX += dx2;
    //     currentY += dy2;
    //   }
    // }
    return res;
  }

  bool ConnectionBetweenNodesOnDifferentFloorsExists(Node currentNode, FloorLayer newFloor)
  {
    LayerFloor layer = gridManager.layerFloors[currentNode.loc.floorLayer];
    if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null)
    {
      // this should not happen
      UnityEngine.Debug.LogError("missing layer information for " + currentNode.loc.floorLayer);
      return false;
    }
    EnvironmentTileInfo tileInfo = GridManager.Instance.GetTileAtLocation(currentNode.loc);
    if (tileInfo.IsEmpty() && currentNode.loc.floorLayer - 1 == newFloor)
    {
      return true;
    }
    if (tileInfo == null) { return false; }
    FloorLayer? targetLayer = null;
    if (tileInfo.ChangesFloorLayer())
    {
      targetLayer = tileInfo.GetTargetFloorLayer(currentNode.loc.floorLayer);
    }
    return (targetLayer != null && targetLayer == newFloor);
  }

  void MaybeAddNode(List<Node> nodeList, int x, int y, FloorLayer floor, Node originNode, TileLocation targetLocation, AiStateController ai, AiAction initiatingAction)
  {
    if (!gridManager.layerFloors.ContainsKey(floor)) { return; }
    LayerFloor layer = gridManager.layerFloors[floor];
    if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null)
    {
      // this floor doesn't exist, so don't worry about it
      return;
    }
    if (floor != originNode.loc.floorLayer && !ConnectionBetweenNodesOnDifferentFloorsExists(originNode, floor))
    {
      return;
    }
    Vector2Int tilePos = new Vector2Int(x, y);
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(new TileLocation(tilePos, floor));
    if (eti == null || eti.IsEmpty())
    {
      MaybeAddNode(nodeList, x, y, floor - 1, InitNewNode(x, y, 0, floor, originNode, targetLocation), targetLocation, ai, initiatingAction);
      return;
    }
    if (eti.groundTileType == null) { return; }
    int costToTravelOverNode = GetNodeTravelCost(eti, ai, initiatingAction);
    if (costToTravelOverNode < 0)
    {
      return;
    }
    // GridManager.Instance.DEBUGHighlightTile(eti.tileLocation);
    nodeList.Add(InitNewNode(x, y, costToTravelOverNode, floor, originNode, targetLocation));
  }

  Node InitNewNode(int x, int y, int g, FloorLayer floor, Node parent, TileLocation targetLocation)
  {
    Node newNode = new Node();
    newNode.loc = new TileLocation(new Vector2Int(x, y), floor);
    newNode.g = 0;
    newNode.h = 0;
    newNode.parent = null;
    if (parent != null)
    {
      newNode.parent = parent;
      newNode.g = parent.g + g;
      newNode.h = Mathf.Abs(targetLocation.tilemapPosition.x - x)
        + Mathf.Abs(targetLocation.tilemapPosition.y - y)
        + Mathf.Abs(targetLocation.floorLayer - floor);
      newNode.f = newNode.g + newNode.h;
    }
    return newNode;
  }
}