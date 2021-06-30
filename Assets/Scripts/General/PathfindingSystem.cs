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

  public IEnumerator CalculatePathToTarget(Vector3 startPosition, TileLocation targetLocation, AiStateController ai, MoveAiAction initiatingAction, WorldObject objectOfInterest = null)
  {
    ai.SetIsCalculatingPath(true);
    bool foundPath = false;
    Stopwatch timeSpentThisFrame = new Stopwatch();
    SimplePriorityQueue<TileLocation> closedNodes = new SimplePriorityQueue<TileLocation>();
    SimplePriorityQueue<TileLocation> openNodes = new SimplePriorityQueue<TileLocation>();
    Dictionary<TileLocation, Node> nodeLocationsToNodes = new Dictionary<TileLocation, Node>();
    List<Node> adjacentNodes;
    List<Node> finalPath = new List<Node>();
    if (!GridManager.Instance.GetTileAtLocation(targetLocation).HasSolidObject()) // if false there won't be a path. I think?
    {
      Node startNode = InitNewNode(new TileLocation(startPosition, ai.currentFloor), 0, null, targetLocation);
      openNodes.Enqueue(startNode.loc, startNode.f);
      nodeLocationsToNodes[startNode.loc] = startNode;
    }
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
    if (foundPath)
    {
      DebugDrawPath(finalPath, .01f);
      // foreach (Node node in finalPath)
      // {
      //   GridManager.Instance.DEBUGHighlightTile(node.loc);
      // }
      if (objectOfInterest != null) { ai.objectOfInterest = objectOfInterest; }
      ai.SetPathToTarget(finalPath);
    }
    else
    {
      ai.SetPathToTarget(null);
    }
    // yield return new WaitForSeconds(3);
    ai.SetIsCalculatingPath(false);
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
      UnityEngine.Debug.DrawLine(path[i].loc.worldPosition, path[i + 1].loc.worldPosition, Color.blue, t);
    }
  }

  List<Node> GetAdjacentNodes(Node originNode, TileLocation targetLocation, AiStateController ai, MoveAiAction initiatingAction)
  {
    List<Node> nodes = new List<Node>();
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.UpperRight), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.Right), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.LowerRight), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.LowerLeft), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.Left), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.UpperLeft), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.Above), originNode, targetLocation, ai, initiatingAction);
    MaybeAddNode(nodes, GridManager.Instance.GetAdjacentTileLocation(originNode.loc, TilemapDirection.Below), originNode, targetLocation, ai, initiatingAction);
    return nodes;
  }


  public static HashSet<TileLocation> GetTilesAlongLine(Vector3 a, Vector3 b, FloorLayer floor, bool drawTiles = false)
  {
    return GetTilesAlongLine(
      new TileLocation(a, floor),
      new TileLocation(b, floor),
      drawTiles
    );
  }

  public static HashSet<TileLocation> GetTilesAlongLine(TileLocation a, TileLocation b, bool drawTiles = false)
  {
    HashSet<TileLocation> results = new HashSet<TileLocation>();
    const float LINE_EPSILON = .01f;
    if (a == b)
      return results;
    Vector3 perpendicular = Vector3.Normalize(Vector3.Cross(a.worldPosition - b.worldPosition, Vector3.forward));

    float distance = a.IntDistanceTo(b);
    float distanceDelta = 1.0f / distance;

    for (int lineIndex = 0; lineIndex < 2; ++lineIndex)
    {
      if (lineIndex == 1)
        perpendicular = -perpendicular;

      TileLocation aLoc = new TileLocation(a.worldPosition + perpendicular * LINE_EPSILON, a.floorLayer);
      TileLocation bLoc = new TileLocation(b.worldPosition + perpendicular * LINE_EPSILON, b.floorLayer);
      Vector3 aCoord = new TileLocation(a.worldPosition + perpendicular * LINE_EPSILON, a.floorLayer).cubeCoords;
      Vector3 bCoord = new TileLocation(b.worldPosition + perpendicular * LINE_EPSILON, b.floorLayer).cubeCoords;

      float xDiff = bCoord.x - aCoord.x;
      float yDiff = bCoord.y - aCoord.y;
      float zDiff = bCoord.z - aCoord.z;
      // UnityEngine.Debug.Log("xDiff: " + xDiff + ",yDiff: " + yDiff + "zDiff: " + zDiff);
      for (int i = 1; i <= distance; ++i)
      {
        float fi = (float)i;
        TileLocation lerpedWorldPosition = TileLocation.FromCubicCoords(
            new Vector3(aCoord.x + xDiff * distanceDelta * fi,
            aCoord.y + yDiff * distanceDelta * fi,
            aCoord.z + zDiff * distanceDelta * fi),
            a.floorLayer);
        if (drawTiles)
        {
          GridManager.Instance.DEBUGHighlightTile(lerpedWorldPosition);
        }
        results.Add(lerpedWorldPosition);
      }
    }

    return results;
  }


  // public HashSet<Vector2Int> GetTilesCoordsAlongLine(Vector2Int a, Vector2Int b, bool drawTiles = false)
  // {
  //   HashSet<Vector2Int> results = new HashSet<Vector2Int>();
  //   const float LINE_EPSILON = .01f;
  //   if (a == b)
  //     return results;
  //   float slope = (b.y - a.y) / (b.x - a.x);
  //   float perpendicular = -1 / slope; // right?

  //   float distance = Vector2.Distance(a, b);
  //   float distanceDelta = 1.0f / distance;

  //   for (int lineIndex = 0; lineIndex < 2; ++lineIndex)
  //   {
  //     if (lineIndex == 1)
  //       perpendicular = -perpendicular;

  //     TileLocation aLoc = new TileLocation(a.worldPosition + perpendicular * LINE_EPSILON, a.floorLayer);
  //     TileLocation bLoc = new TileLocation(b.worldPosition + perpendicular * LINE_EPSILON, b.floorLayer);
  //     Vector3 aCoord = new TileLocation(a.worldPosition + perpendicular * LINE_EPSILON, a.floorLayer).cubeCoords;
  //     Vector3 bCoord = new TileLocation(b.worldPosition + perpendicular * LINE_EPSILON, b.floorLayer).cubeCoords;

  //     float xDiff = bCoord.x - aCoord.x;
  //     float yDiff = bCoord.y - aCoord.y;
  //     float zDiff = bCoord.z - aCoord.z;
  //     // UnityEngine.Debug.Log("xDiff: " + xDiff + ",yDiff: " + yDiff + "zDiff: " + zDiff);
  //     for (int i = 1; i <= distance; ++i)
  //     {
  //       float fi = (float)i;
  //       TileLocation lerpedWorldPosition = TileLocation.FromCubicCoords(
  //           new Vector3(aCoord.x + xDiff * distanceDelta * fi,
  //           aCoord.y + yDiff * distanceDelta * fi,
  //           aCoord.z + zDiff * distanceDelta * fi),
  //           a.floorLayer);
  //       if (drawTiles)
  //       {
  //         GridManager.Instance.DEBUGHighlightTile(lerpedWorldPosition);
  //       }
  //       results.Add(GridManager.Instance.GetTileAtLocation(lerpedWorldPosition));
  //     }
  //   }

  //   return results;
  // }
  public bool IsPathClearOfHazards_SquareGrid(Vector3 targetPosition, FloorLayer targetFloor, AiStateController ai)
  {
    return false; // eat shiiiiit
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
  public bool IsPathClearOfHazards(Vector3 targetPosition, FloorLayer targetFloor, Character character)
  {
    if (targetFloor != character.currentFloor)
    {
      return false;
    }
    // Vector3[] colliderCorners = new Vector3[]{
    //   new Vector3 (character.circleCollider.radius, 0, 0),
    //   new Vector3 (-character.circleCollider.radius, 0, 0),
    //   new Vector3 (0, character.circleCollider.radius, 0),
    //   new Vector3 (0, -character.circleCollider.radius, 0),
    // 	// new Vector3 (col.bounds.extents.x, col.bounds.extents.y, 0),
    // 	// new Vector3 (-col.bounds.extents.x, col.bounds.extents.y, 0),
    // 	// new Vector3 (col.bounds.extents.x, -col.bounds.extents.y, 0),
    // 	// new Vector3 (-col.bounds.extents.x, -col.bounds.extents.y, 0),
    // };
    HashSet<TileLocation> tilesAlongPath = new HashSet<TileLocation>();
    foreach (Vector3 pt in character.physicsCollider.points)
    {
      tilesAlongPath.UnionWith(GetTilesAlongLine(character.transform.TransformPoint(pt), targetPosition + pt, targetFloor));
    }
    foreach (TileLocation tileLocation in tilesAlongPath)
    {
      EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(tileLocation);
      if (eti == null || eti.dealsDamage)
      {
        UnityEngine.Debug.DrawLine(targetPosition, character.transform.position, Color.yellow);
        return false;
      }
      if (!CanPassOverTile(eti, character))
      {
        UnityEngine.Debug.DrawLine(targetPosition, character.transform.position, Color.red);
        return false;
      }
    }
    UnityEngine.Debug.DrawLine(targetPosition, character.transform.position, Color.green);
    return true;
  }
  //idk if this is good for anything
  // public void IsPathClearOfHazards(Vector3 targetPosition, FloorLayer targetFloor, Character character)
  // {
  //   if (targetFloor != character.currentFloor)
  //   {
  //     return;
  //   }
  //   TileLocation originHex = character.GetTileLocation();
  //   TileLocation destinationHex = new TileLocation(targetPosition, targetFloor);
  //   TileLocation currentHex = originHex;
  //   float e = 0;
  //   Vector2 delta = new Vector2(
  //     (destinationHex.worldPosition.x - originHex.worldPosition.x),
  //     (destinationHex.worldPosition.y - originHex.worldPosition.y) / (-GridConstants.Y_SPACING)
  //   );
  //   UnityEngine.Debug.DrawLine(
  //     originHex.worldPosition,
  //     destinationHex.worldPosition,
  //     Color.red
  //   );
  //   int i = 0;
  //   while (currentHex.tilemapCoordinates != destinationHex.tilemapCoordinates && i < 30)
  //   {
  //     i++;
  //     if (e >= 4 * delta.x)
  //     {
  //       currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.UpperLeft);
  //       e = e - 3 * delta.y - 6 * delta.x;
  //     }
  //     else
  //     {
  //       e = e + 3 * delta.y;
  //       if (e > 2 * delta.x)
  //       {
  //         currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.UpperRight);
  //         e = e - 6 * delta.x;
  //       }
  //       else
  //       {
  //         if (e < -2 * delta.x)
  //         {
  //           currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.LowerRight);
  //           e = e + 6 * delta.x;
  //         }
  //         else
  //         {
  //           currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.Right);
  //           e = e + 3 * delta.y;
  //         }
  //       }
  //     }
  //     GridManager.Instance.DEBUGHighlightTile(currentHex);
  //   }
  // }

  // Some variables:
  // hex side length is .5f
  //
  // public bool IsPathClearOfHazards(Vector3 targetPosition, FloorLayer targetFloor, Character character)
  // {
  //   if (targetFloor != character.currentFloor)
  //   {
  //     return false;
  //   }
  //   Vector3[] colliderCorners = new Vector3[]{
  //     new Vector3 (character.circleCollider.radius, 0, 0),
  //     new Vector3 (-character.circleCollider.radius, 0, 0),
  //     new Vector3 (0, character.circleCollider.radius, 0),
  //     new Vector3 (0, -character.circleCollider.radius, 0),
  // 		// new Vector3 (col.bounds.extents.x, col.bounds.extents.y, 0),
  // 		// new Vector3 (-col.bounds.extents.x, col.bounds.extents.y, 0),
  // 		// new Vector3 (col.bounds.extents.x, -col.bounds.extents.y, 0),
  // 		// new Vector3 (-col.bounds.extents.x, -col.bounds.extents.y, 0),
  // 	};
  //   TileLocation originHex = character.GetTileLocation();
  //   TileLocation destinationHex = new TileLocation(targetPosition, targetFloor);
  //   TileLocation currentHex = originHex;
  //   GridManager.Instance.DEBUGHighlightTile(currentHex);
  //   // UnityEngine.Debug.DrawLine(originHex.worldPosition, destinationHex.worldPosition, Color.green, .2f);
  //   float e = 0;
  //   Vector2 delta = new Vector2(
  //     destinationHex.tilemapPosition.x - originHex.tilemapPosition.x,
  //     destinationHex.tilemapPosition.y - originHex.tilemapPosition.y
  //   );
  //   UnityEngine.Debug.Log("delta: " + delta);
  //   UnityEngine.Debug.DrawLine(
  //     originHex.worldPosition,
  //     destinationHex.worldPosition,
  //     Color.red
  //   );
  //   // float k = Mathf.Sqrt(3) * delta.y / (2 * delta.x);
  //   float k = delta.y * .87625f / (delta.x); // slope of the line
  //   float s = 0.87625f / 2; // horizontal distance between two hexes in the same row
  //   int i = 0; // TODO: delete this
  //   // int xSign = (int)Mathf.Sign(delta.x);
  //   // int ySign = (int)Mathf.Sign(delta.y);
  //   while (currentHex.tilemapPosition != destinationHex.tilemapPosition && i < 50)
  //   {
  //     //   i++;
  //     // UnityEngine.Debug.Log("currentHex tilemap pos: " + currentHex.tilemapPosition);
  //     if (e >= .5f)
  //     // if (e >= 1f)
  //     {
  //       // UnityEngine.Debug.DrawLine(
  //       //   currentHex.worldPosition,
  //       //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //       //   Color.red,
  //       //   .2f
  //       // );
  //       UnityEngine.Debug.Log("choosing hex: upper-left");
  //       currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.UpperLeft);
  //       e = e - (k * s) - .75f;
  //       // e = e - (k * s) - 1.5f;
  //       // UnityEngine.Debug.Log("e" + i++ + ": " + e);
  //     }
  //     else
  //     {
  //       // UnityEngine.Debug.DrawLine(
  //       //   currentHex.worldPosition,
  //       //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //       //   Color.red,
  //       //   .2f
  //       // );
  //       e = e + (k * s);
  //       // UnityEngine.Debug.Log("e" + i++ + ": " + e);
  //       if (e > .25f)
  //       // if (e > .5f)
  //       {
  //         // UnityEngine.Debug.DrawLine(
  //         //   currentHex.worldPosition,
  //         //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //         //   Color.red,
  //         //   .2f
  //         // );
  //         UnityEngine.Debug.Log("choosing hex: upper-right");
  //         currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.UpperRight);
  //         e = e - .75f;
  //         // e = e - 1.5f;
  //         //   UnityEngine.Debug.Log("e" + i++ + ": " + e);
  //         //   UnityEngine.Debug.DrawLine(
  //         //   currentHex.worldPosition,
  //         //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //         //   Color.red,
  //         //   .2f
  //         // );
  //       }
  //       else
  //       {
  //         if (e < -.25)
  //         // if (e < -.5)
  //         {
  //           // UnityEngine.Debug.Log("getting hex adjacent to " + currentHex.tilemapPosition + "lower right");
  //           // UnityEngine.Debug.DrawLine(
  //           //   currentHex.worldPosition,
  //           //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //           //   Color.red,
  //           //   .2f
  //           // );
  //           UnityEngine.Debug.Log("choosing hex: lower-right");
  //           currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.LowerRight);
  //           e = e + .75f;
  //           // e = e + 1.5f;
  //         }
  //         else
  //         {
  //           // UnityEngine.Debug.Log("getting hex adjacent to " + currentHex.tilemapPosition + "right");
  //           // UnityEngine.Debug.DrawLine(
  //           //   currentHex.worldPosition,
  //           //   new Vector3(currentHex.worldPosition.x, currentHex.worldPosition.y - e, currentHex.worldPosition.z),
  //           //   Color.red,
  //           //   .2f
  //           // );
  //           UnityEngine.Debug.Log("choosing hex: right");
  //           currentHex = GridManager.Instance.GetAdjacentTileLocation(currentHex, TilemapDirection.Right);
  //           e = e + (k * s);
  //           // UnityEngine.Debug.Log("e" + i++ + ": " + e);
  //         }
  //       }
  //     }
  //     // UnityEngine.Debug.Log("Hex advanced to " + currentHex.tilemapPosition);
  //     // UnityEngine.Debug.Break();
  //     if (currentHex.x > 20) { break; }
  //     GridManager.Instance.DEBUGHighlightTile(currentHex);
  //     // UnityEngine.Debug.Log("current tile:" + currentHex.tilemapPosition);
  //   }
  //   // UnityEngine.Debug.Break();
  //   return false;
  // }

  // you fuckin name it then
  // you're so god damn smart
  public TilemapDirection GetNextHexStepDirectionForClearPathAlgorithm(int i, int j)
  {
    switch (i)
    {
      case -1:
        return j > 0 ? TilemapDirection.UpperLeft : TilemapDirection.LowerLeft;
      case 0:
        return j > 0 ? TilemapDirection.Right : TilemapDirection.Left;
      case 1:
        return j > 0 ? TilemapDirection.UpperRight : TilemapDirection.LowerRight;
    }
    return TilemapDirection.None;
  }


  // USUALLY returns either 1 (can cross) or -1 (can't cross).
  // MAY return a higher value if the tile deals damage; edit this function to adjust how hard that's weighed
  private int GetNodeTravelCost(EnvironmentTileInfo tileInfo, AiStateController ai, MoveAiAction initiatingAction)
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
  private bool CanPassOverTile(EnvironmentTileInfo tile, Character ai)
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

  void MaybeAddNode(List<Node> nodeList, TileLocation possibleNodeLocation, Node originNode, TileLocation targetLocation, AiStateController ai, MoveAiAction initiatingAction)
  {
    if (!gridManager.layerFloors.ContainsKey(possibleNodeLocation.floorLayer)) { return; }
    LayerFloor layer = gridManager.layerFloors[possibleNodeLocation.floorLayer];
    if (layer == null || layer.groundTilemap == null || layer.objectTilemap == null)
    {
      // this floor doesn't exist, so don't worry about it
      return;
    }
    if (possibleNodeLocation.floorLayer != originNode.loc.floorLayer && !ConnectionBetweenNodesOnDifferentFloorsExists(originNode, possibleNodeLocation.floorLayer))
    {
      return;
    }
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(possibleNodeLocation);
    if ((eti == null || eti.IsEmpty()) && GridManager.Instance.AdjacentTileIsValid(possibleNodeLocation, TilemapDirection.Below))
    {
      MaybeAddNode(nodeList, GridManager.Instance.GetAdjacentTileLocation(possibleNodeLocation, TilemapDirection.Below), InitNewNode(possibleNodeLocation, 0, originNode, targetLocation), targetLocation, ai, initiatingAction);
      return;
    }
    if (eti.groundTileType == null) { return; }
    int costToTravelOverNode = GetNodeTravelCost(eti, ai, initiatingAction);
    if (costToTravelOverNode < 0)
    {
      return;
    }
    // GridManager.Instance.DEBUGHighlightTile(eti.tileLocation);
    nodeList.Add(InitNewNode(possibleNodeLocation, costToTravelOverNode, originNode, targetLocation));
  }

  Node InitNewNode(TileLocation nodeLocation, int g, Node parent, TileLocation targetLocation)
  {
    Node newNode = new Node();
    newNode.loc = nodeLocation;
    newNode.g = 0;
    newNode.h = 0;
    newNode.parent = null;
    if (parent != null)
    {
      newNode.parent = parent;
      newNode.g = parent.g + g;
      newNode.h = Mathf.RoundToInt(Mathf.Abs(targetLocation.tilemapCoordinates.x - nodeLocation.worldPosition.x)
        + Mathf.Abs(targetLocation.tilemapCoordinates.y - nodeLocation.worldPosition.y)
        + Mathf.Abs(targetLocation.floorLayer - nodeLocation.floorLayer));
      newNode.f = newNode.g + newNode.h;
    }
    return newNode;
  }
}