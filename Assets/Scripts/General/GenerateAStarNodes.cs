// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// // A lot of this comes from https://www.youtube.com/watch?v=htZFdfSLiYo, adapted to my own purposes
// public class GenerateAStarNodes : MonoBehaviour {
//     public Grid grid;
//     public Tilemap groundLayer;
//     public List<Tilemap> obstacleLayers;
//     public GameObject pathfindingNodePrefab;
    
//     // TODO: this needs to be level-specific? prrrrobably?
// 	public int scanStartX=-250,scanStartY=-250,scanFinishX=250,scanFinishY=250;

//     public List<GameObject> unsortedNodes // all nodes
//     public GameObject[,] nodes; // sorted 2d array of nodes
// 	public int gridBoundX = 0, gridBoundY = 0;

//     void Awake() {
// 		unsortedNodes = new List<GameObject> ();
// 		//Debug.Log ("Floor is size "+floor.size);
// 		//foreach (Tilemap t in obstacleLayers) {
// 		//	Debug.Log ("Obstacle " + t.name + " Is size " + t.size);
// 		//}
//     }

//     public void GenerateNodes() {
// 		CreateNodes();
// 		//just call this and plug the resulting 2d array of nodes into your own A* algorithm
// 	}

//     void CreateNodes() {
//         int gridX = 0; 
// 		int gridY = 0;
// 		bool foundTileOnLastPass = false;

// 		for(int x = scanStartX;x<scanFinishX;x++) {
// 			for (int y = scanStartY; y < scanFinishY; y++) {
// 				TileBase tb = floor.GetTile (new Vector3Int (x, y, 0)); //check if we have a floor tile at that world coords
//                 if (tb != null) {
// 					// if there is a floor: look for obstacle at this location
// 					bool foundObstacle = false;
//                     foreach (Tilemap t in obstacleLayers) {
// 						TileBase tb2 = t.GetTile (new Vector3Int (x, y, 0));

// 						if (tb2 != null) {
// 							foundObstacle = true;
//                             break;
// 						}
//                     }
//                     if (foundObstacle) {
//                         //if we have found an obstacle then we make the node unwalkable and assign its grid coords
//                         GameObject node = (GameObject)Instantiate (nodePrefab, new Vector3 (x + 0.5f+ gridBase.transform.position.x, y + 0.5f+ gridBase.transform.position.y, 0), Quaternion.Euler (0, 0, 0));
//                         //we add the gridBase position to ensure that the nodes are ontop of the tile they relate too
//                         node.GetComponent<SpriteRenderer> ().color = Color.red;
//                         WorldTile wt = node.GetComponent<WorldTile> ();
//                         wt.gridX = gridX;
//                         wt.gridY = gridY;
//                         wt.walkable = false;
//                         foundTileOnLastPass = true; //say that we have found a tile so we know to increment the index counters
//                         unsortedNodes.Add (node);
//                         node.name = "UNWALKABLE NODE " + gridX.ToString () + " : " + gridY.ToString ();
//                     } else {

//                         //if we havent found an obstacle then we create a walkable node and assign its grid coords
//                         GameObject node = (GameObject)Instantiate (nodePrefab, new Vector3 (x + 0.5f + gridBase.transform.position.x, y + 0.5f+ gridBase.transform.position.y, 0), Quaternion.Euler (0, 0, 0));
//                         WorldTile wt = node.GetComponent<WorldTile> ();
//                         wt.gridX = gridX;
//                         wt.gridY = gridY;
//                         foundTileOnLastPass = true; 
//                         unsortedNodes.Add (node);
 
//                         node.name = "NODE " + gridX.ToString () + " : " + gridY.ToString ();
//                     }
//                     gridY++; //increment the y counter
 
 
//                     if (gridX > gridBoundX) { //if the current gridX/gridY is higher than the existing then replace it with the new value
//                         gridBoundX = gridX;
//                     }
 
//                     if (gridY > gridBoundY) {
//                         gridBoundY = gridY;
//                     }

//                 }
                
//             }
//             if (foundTileOnLastPass == true) {//since the grid is going from bottom to top on the Y axis on each iteration of the inside loop, if we have found tiles on this iteration we increment the gridX value and
//                 //reset the y value
//                 gridX++;
//                 gridY = 0;
//                 foundTileOnLastPass = false;
//             }
//         }
//         nodes = new GameObject[gridBoundX+1,gridBoundY+1];//initialise the 2d array that will store our nodes in their position
//         foreach (GameObject g in unsortedNodes) { //go through the unsorted list of nodes and put them into the 2d array in the correct position
//             WorldTile wt = g.GetComponent<WorldTile> ();
//             //Debug.Log (wt.gridX + " " + wt.gridY);
//             nodes [wt.gridX, wt.gridY] = g;
//         }

//     }
// }