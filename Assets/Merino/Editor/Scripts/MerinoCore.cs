﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yarn;

namespace Merino
{
    internal static class MerinoCore
    {
	    public static double LastSaveTime;

        public static void ReimportFiles(bool forceReimportAll = false)
        {
            if (forceReimportAll)
            {
                foreach (var file in MerinoData.CurrentFiles)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(file));
                }
            }
            else if (MerinoData.DirtyFiles.Count > 0 )
            {
                foreach (var file in MerinoData.DirtyFiles)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(file));
                }
            }

	        MerinoData.DirtyFiles.Clear();
        }

        // TODO: eventually, add more data on what's dirty about it (what nodes modified? when it was last modified? etc)
        public static void MarkFileDirty(TextAsset dirtyFile)
        {
            if (MerinoData.DirtyFiles.Contains(dirtyFile) == false)
            {
	            MerinoData.DirtyFiles.Add(dirtyFile);
            }
        }
	    
        public static void SaveDataToFiles()
        {
            if (MerinoData.CurrentFiles.Count > 0 )
            {
                foreach (var file in MerinoData.CurrentFiles)
                {
	                if (MerinoData.FileToNodeID.ContainsKey(file))
	                {
		                File.WriteAllText(AssetDatabase.GetAssetPath(file), SaveFileNodesAsString(MerinoData.FileToNodeID[file]));
		                EditorUtility.SetDirty(file);
		                LastSaveTime = EditorApplication.timeSinceStartup;
	                }
	                else
	                {
		                MerinoDebug.Log(LoggingLevel.Warning, file.name + " has not been mapped to a NodeID and cannot be saved, reload the file and try again.");
	                }
                }
            }
        }

		// used for file saving
		public static string SaveFileNodesAsString(int fileNodeID)
		{
			var nodeInfoList = new List<YarnSpinnerLoader.NodeInfo>();
			var toTraverse = new List<int>() {fileNodeID};
			var filterList = new List<int>();

			while (toTraverse.Count > 0)
			{
				if (filterList.Contains(toTraverse[0]) == false)
				{
					filterList.Add(toTraverse[0]);
					var node = MerinoData.GetNode(toTraverse[0]);
					if (node != null && node.hasChildren)
					{
						toTraverse.AddRange(node.children.Select(x => x.id));
					}
				}
				toTraverse.RemoveAt(0);
			}
			
			// export these nodes
			//todo: move back over to linq, we were getting null ref exception so using this for the time being
			var treeNodes = new List<MerinoTreeElement>();
			foreach (var merinoTreeElement in MerinoData.TreeElements)
			{
				if (filterList.Contains(merinoTreeElement.id))
				{
					treeNodes.Add(merinoTreeElement);
				}
			}

			// save data to string
			foreach (var treeNode in treeNodes)
			{
				// skip the root, and skip any non-node nodes
				if (treeNode.depth == -1 || treeNode.leafType != MerinoTreeElement.LeafType.Node) continue;

				nodeInfoList.Add(TreeNodeToYarnNode(treeNode));
			}
				
			return YarnSpinnerFileFormatConverter.ConvertNodesToYarnText(nodeInfoList);
		}

		// used internally for playtest preview
		public static string SaveAllNodesAsString()
		{	
			// gather data
			if (MerinoPrefs.validateNodeTitles) 
			{
				ValidateNodeTitles();
			}
			
			var nodeInfo = new List<YarnSpinnerLoader.NodeInfo>();

			// save data to string
			//todo: move back over to linq, we were getting null ref exception so using this for the time being
			var treeNodes = new List<MerinoTreeElement>();
			foreach (var merinoTreeElement in MerinoData.TreeElements)
			{
				if (merinoTreeElement.leafType == MerinoTreeElement.LeafType.Node)
				{
					treeNodes.Add(merinoTreeElement);
				}
			}
			
			foreach (var treeNode in treeNodes)
			{
				// skip the root
				if (treeNode.depth == -1)
				{
					continue;
				}

				nodeInfo.Add( TreeNodeToYarnNode(treeNode) );
			}

			return YarnSpinnerFileFormatConverter.ConvertNodesToYarnText(nodeInfo);
		}

		private static YarnSpinnerLoader.NodeInfo TreeNodeToYarnNode(MerinoTreeElement treeNode)
		{
			var info = new YarnSpinnerLoader.NodeInfo();

			info.title = treeNode.name;
			info.body = treeNode.nodeBody;
			info.tags = treeNode.nodeTags;
			
			var newPosition = new YarnSpinnerLoader.NodeInfo.Position
			{
				x = treeNode.nodePosition.x, 
				y = treeNode.nodePosition.y
			};
			info.position = newPosition;
			
			if (((MerinoTreeElement) treeNode.parent).leafType != MerinoTreeElement.LeafType.File)
			{
				info.parent = treeNode.parent.name;
			}

			return info;
		}

		// ensure unique node titles, very important for YarnSpinner
		private static void ValidateNodeTitles(List<MerinoTreeElement> nodes = null)
		{
			if (!MerinoPrefs.validateNodeTitles) return;

			if (nodes == null) // if null, then let's just use all currently loaded nodes
			{
				nodes = MerinoData.TreeElements;
			}
			
			// make sure we're not doing this to any folder or file nodes, ONLY YARN NODES
			nodes = nodes.Where(x => x.leafType == MerinoTreeElement.LeafType.Node).ToList();

			// validate data: ensure unique node names
			var nodeTitles = new Dictionary<int,string>(); // index, newTitle
			var duplicateCount = new Dictionary<string, int>(); // increment counter for each duplicate name, and use for rename suffix
			bool foundDuplicate = false;
			for (int i=0;i<nodes.Count;i++)
			{
				// if there's a node already with that name, then append unique suffix
				if (nodeTitles.Values.Contains(nodes[i].name))
				{
					// count duplicates
					if (!duplicateCount.ContainsKey(nodes[i].name))
					{
						duplicateCount.Add(nodes[i].name, 2);
					}
					
					// when we make a new name, we have to ensure it's unique...
					if (nodeTitles.ContainsKey(nodes[i].id) == false)
					{
						nodeTitles.Add(nodes[i].id, nodes[i].name);
					}
					
					nodeTitles[nodes[i].id] = nodes[i].name + "_" + duplicateCount[nodes[i].name]++;
					foundDuplicate = true;
				} // but if there's not already a node with that name, we should still make a note of it
				else if (nodeTitles.ContainsKey(nodes[i].id) == false)
				{
					nodeTitles.Add(nodes[i].id, nodes[i].name);
				}
	
			}
			
			if (foundDuplicate)
			{
				string renamedNodes = "Merino found nodes with duplicate names (which aren't allowed for Yarn) and renamed them. This might break node links, you can undo it. The following nodes were renamed: ";
				Undo.RecordObject(MerinoData.Instance, "Merino: AutoRename");
				
				foreach (var kvp in nodeTitles)
				{
					if (MerinoData.TreeElements[kvp.Key].name != kvp.Value)
					{
						renamedNodes += string.Format("\n* {0} > {1}", MerinoData.TreeElements[kvp.Key].name, kvp.Value);
						MerinoData.TreeElements[kvp.Key].name = kvp.Value;
					}
				}
				EditorUtility.SetDirty(MerinoData.Instance);
				MerinoDebug.Log(LoggingLevel.Warning, renamedNodes);
				//todo: repaint MerinoEditorWindow tree view so names get updated
				
				// this is bad, but we're gonna do some recursion here, just to make extra sure there's STILL no duplicates...
				ValidateNodeTitles(MerinoData.TreeElements);
			}
			else if (MerinoPrefs.useAutosave)
			{
				SaveDataToFiles();
			}
			
		}
	    
		// FYI: TextAsset source basically does nothing right now, will be removed
		public static IList<MerinoTreeElement> GetData()
		{
			// init variables, create global tree root
			var treeElements = new List<MerinoTreeElement>();
			var root = new MerinoTreeElement("Root", -1, 0);
			root.children = new List<TreeElement>();
			treeElements.Add(root);
			
			// ok, now let's load the data
	
			// then go through each file and get nodes for it, adding folder nodes as appropriate
			int nodeID = 1;

			foreach (var yarnFile in MerinoData.CurrentFiles)
			{
				// all folders are now created, let's add the yarn data now
				var yarnData = GetDataFromFile(yarnFile, nodeID);
				
				// set the file node's parent to root
				yarnData[0].parent = root;
				root.children.Add(yarnData[0]);
				
				// add all data to tree elements
				treeElements.AddRange( yarnData );
				nodeID += yarnData.Count;
			}
			
			// IMPORTANT: sort the treeElements by id!!!
			treeElements = treeElements.OrderBy(x => x.id).ToList();
			
			// if there's already parent data then I don't really know what the depth value is used for (a cache to speed up GUI drawing?)
			// but I think we're supposed to do this thing so let's do it
			TreeElementUtility.UpdateDepthValues( root );
			
			MerinoData.TreeElements = treeElements;
			return MerinoData.TreeElements;
		}
	    
		public static IList<MerinoTreeElement> GetDataFromFile(TextAsset source, int startID = 1)
		{
			var treeElements = new List<MerinoTreeElement>();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(source)); // TODO: only reload assets that need it? how to do that
			//var format = YarnSpinnerLoader.GetFormatFromFileName(AssetDatabase.GetAssetPath(currentFile)); // TODO: add JSON and ByteCode support?
			
			// ROOT: create a root node for the file itself
			var fileRoot = new MerinoTreeElement(source.name, 0, startID);
			fileRoot.leafType = MerinoTreeElement.LeafType.File;
			fileRoot.children = new List<TreeElement>();
			treeElements.Add(fileRoot);
			if (MerinoData.FileToNodeID.ContainsKey(source))
			{
				MerinoData.FileToNodeID[source] = startID;
			}
			else
			{
				MerinoData.FileToNodeID.Add(source, startID);
			}

			// load nodes
			var nodes = YarnSpinnerLoader.GetNodesFromText(source.text, NodeFormat.Text);
			var parents = new Dictionary<MerinoTreeElement, string>();
			foreach (var node in nodes)
			{
				// clean some of the stuff to help prevent file corruption
				string cleanName = MerinoUtils.CleanYarnField(node.title, true);
				string cleanBody = MerinoUtils.CleanYarnField(node.body);
				string cleanTags = MerinoUtils.CleanYarnField(node.tags, true);
				string cleanParent = string.IsNullOrEmpty(node.parent) ? "" : MerinoUtils.CleanYarnField(node.parent, true);
				
				// write data to the objects
				var newItem = new MerinoTreeElement( cleanName, 0, startID + treeElements.Count);
				newItem.nodeBody = cleanBody;
				newItem.nodePosition = new Vector2Int(node.position.x, node.position.y);
				newItem.nodeTags = cleanTags;
				if (string.IsNullOrEmpty(cleanParent) || cleanParent == "Root")
				{
					newItem.parent = fileRoot;
					fileRoot.children.Add(newItem);
				}
				else
				{
					parents.Add(newItem, cleanParent); // we have to assign parents in a second pass later on, not right now
				}
				treeElements.Add(newItem);
			}
			
			// second pass: now that all nodes have been created, we can finally assign parents
			foreach (var kvp in parents )
			{
				var parent = treeElements.Where(x => x.name == kvp.Value).ToArray();
				if (parent.Length == 0)
				{
					MerinoDebug.LogFormat(LoggingLevel.Error, "Merino couldn't assign parent for node {0}: can't find a parent called {1}", kvp.Key.name, kvp.Value);
				}
				else
				{
					// tell child about it's parent
					kvp.Key.parent = parent[0];
					// tell parent about it's child
					if (kvp.Key.parent.children == null) // init parent's list of children if not already initialized
					{
						kvp.Key.parent.children = new List<TreeElement>();
					}
					kvp.Key.parent.children.Add(kvp.Key);
				}
			}
			return treeElements;
		}

		#region TempData
				
		/// <summary>
		/// Delete all Merino temp data instances in the project.
		/// </summary>
		public static void CleanupTempData()
		{
			var tempData = Resources.FindObjectsOfTypeAll<MerinoData>();
			foreach (var data in tempData)
			{
				var path = AssetDatabase.GetAssetPath(data);
				if (!string.IsNullOrEmpty(path)) //don't attempt to delete ghost(?) scriptable objects
					AssetDatabase.DeleteAsset(path);
			}
		}

		/// <summary>
		/// Returns the path of the Merino folder, based on the location of MerinoEditorWindow.cs since that should always be in there.
		/// </summary>
		private static string LocateMerinoFolder()
		{
			string[] results = Directory.GetFiles(Application.dataPath, "MerinoEditorWindow.cs", SearchOption.AllDirectories);
			if (results.Length > 0)
			{
				var parent = Directory.GetParent(results[0]);
				while (parent.Name != "Merino")
					parent = parent.Parent;

				return parent.FullName;
			}

			return null;
		}

		/// <summary>
		/// Returns the path Merino temp data should live.
		/// </summary>
		public static string GetTempDataPath()
		{
			var path = LocateMerinoFolder(); //find folder in project...
			path += "\\Editor\\MerinoTempData.asset"; //append on the path for temp data;
			path = path.Substring(path.IndexOf("Assets")); //remove path before the assets folder
			return (path);
		}

		#endregion
    }
}
