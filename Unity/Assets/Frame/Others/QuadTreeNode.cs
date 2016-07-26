using System;
using System.Collections.Generic;
using UnityEngine;
using VTree.BiomeDefenseN.ObjectsN;

public interface IHasRect {
	VRect GetBoundsRect();
}
namespace QuadTreeLib {
	public class QuadTreeNode<T> where T : class, IHasRect {
		public QuadTreeNode(VRect bounds) { this.bounds = bounds; }

		VRect bounds = VRect.Null;

		// The contents of this node. Note that the contents have no limit. (this is not the standard way to implement a QuadTree)
		//public List<T> contents = new List<T>();
		public HashSet<T> contents = new HashSet<T>();
		List<QuadTreeNode<T>> childNodes = new List<QuadTreeNode<T>>(4);
		//public bool IsEmpty { get { return bounds == VRect.Null || childNodes.Count == 0; } }
		//public bool IsEmpty { get { return bounds == VRect.Null || (childNodes.Count == 0 && contents.Count == 0); } }
		public bool hasItems;
		void UpdateHasItems() {
			bool newHasItems = contents.Count > 0;
			if (!newHasItems)
				foreach (QuadTreeNode<T> node2 in childNodes)
					if (node2.hasItems) {
						newHasItems = true;
						break;
					}
			hasItems = newHasItems;
		}
		public VRect Bounds { get { return bounds; } }

		/// <summary>Total number of nodes in the this node and all SubNodes</summary>
		public int Count {
			get {
				int count = contents.Count;
				foreach (QuadTreeNode<T> node in childNodes)
					count += node.Count;
				return count;
			}
		}

		/// <summary>Return the contents of this node and all subnodes in the true below this one.</summary>
		public List<T> SubTreeContents {
			get {
				var results = new List<T>();
				foreach (QuadTreeNode<T> node in childNodes)
					results.AddRange(node.SubTreeContents);
				results.AddRange(contents);
				return results;
			}
		}

		// maybe todo: add GetItemsWithCenterInside method
		public List<T> GetItemsIntersecting(VRect queryArea) {
			// create a list of the items that are found
			var results = new List<T>();

			// this quad contains items that are not entirely contained by
			// it's four sub-quads. Iterate through the items in this quad to see if they intersect.
			foreach (T item in contents) {
				//if (queryArea.Intersects(item.GetRect()))
				//if (queryArea.Intersects_Init(item.GetRect()))
				//	results.Add(item);
			
				//var itemRect = item.GetRect();
					VRect itemRect;
					if (item is MeshData.Triangle)
						itemRect = (item as MeshData.Triangle).rect;
					/*else if (item is VObject && (item as VObject).transform._boundsRect.HasValue)
						itemRect = (item as VObject).transform._boundsRect.Value;*/
					else
						itemRect = item.GetBoundsRect();
					if (queryArea.init_right >= itemRect.x && queryArea.x <= itemRect.init_right && queryArea.init_top >= itemRect.y && queryArea.y <= itemRect.init_top)
						results.Add(item);
				}

			foreach (QuadTreeNode<T> node in childNodes) {
				if (!node.hasItems)
					continue;

				// Case 1: search area completely contained by sub-quad
				// if a node completely contains the query area, go down that branch and skip the remaining nodes (break this loop)
				if (node.Bounds.Contains(queryArea)) {
					results.AddRange(node.GetItemsIntersecting(queryArea));
					break;
				}

				// Case 2: Sub-quad completely contained by search area 
				// if the query area completely contains a sub-quad,
				// just add all the contents of that quad and it's children 
				// to the result set. You need to continue the loop to test the other quads
				if (queryArea.Contains(node.Bounds)) {
					results.AddRange(node.SubTreeContents);
					continue;
				}

				// Case 3: search area intersects with sub-quad
				// traverse into this quad, continue the loop to search other quads
				//if (node.Bounds.Intersects(queryArea))
				if (node.Bounds.Intersects_Init(queryArea))
					results.AddRange(node.GetItemsIntersecting(queryArea));
			}

			return results;
		}

		public void Insert(T item, bool allowAddingNonContainedItemToRoot = true) {
			// if the item is not contained in this quad, there's a problem
			if (!allowAddingNonContainedItemToRoot && !bounds.Contains(item.GetBoundsRect()))
			/*{
				Debug.LogWarning("Attempted to insert an item that would be out of the bounds of this quadtree node.");
				return;
			}*/
				throw new Exception("Attempted to insert an item that would be out of the bounds of this quadtree node.");

			// if the subnodes are null create them. may not be sucessfull: see below
			// we may be at the smallest allowed size in which case the subnodes will not be created
			if (childNodes.Count == 0)
				CreateSubNodes();

			hasItems = true;

			// for each subnode:
			// if the node contains the item, add the item to that node and return
			// this recurses into the node that is just large enough to fit this item
			foreach (QuadTreeNode<T> node in childNodes)
				if (node.Bounds.Contains(item.GetBoundsRect())) {
					node.Insert(item);
					return;
				}

			// if we make it to here, either
			// 1) none of the subnodes completely contained the item. or
			// 2) we're at the smallest subnode size allowed 
			// add the item to this node's contents.
			contents.Add(item);
		}

		/*public void ForEach(Func<T, bool> action)
		{
			action(this);

			// draw the child quads
			foreach (QuadTreeNode<T> node in this.childNodes)
				node.ForEach(action);
		}*/

		/// <summary>Internal method to create the subnodes (partitions space)</summary>
		void CreateSubNodes() {
			// the smallest subnode has an area 
			//if (bounds.height * bounds.width <= 10)
			if (bounds.width == 1)
				return;

			double halfWidth = bounds.width / 2;
			double halfHeight = bounds.height / 2;

			childNodes.Add(new QuadTreeNode<T>(new VRect(bounds.x, bounds.y, halfWidth, halfHeight)));
			childNodes.Add(new QuadTreeNode<T>(new VRect(bounds.x, bounds.y + halfHeight, halfWidth, halfHeight)));
			childNodes.Add(new QuadTreeNode<T>(new VRect(bounds.x + halfWidth, bounds.y, halfWidth, halfHeight)));
			childNodes.Add(new QuadTreeNode<T>(new VRect(bounds.x + halfWidth, bounds.y + halfHeight, halfWidth, halfHeight)));
		}

		// extra - deletion and update
		// ==========

		// return the contents of this node and all subnodes in the true below this one
		//public void EchoSubTreeContents(Func<T, bool> matchFunc)
		/*public void EchoSubTreeContents(T item)
		{
			foreach (QuadTreeNode<T> node in childNodes)
				//node.EchoSubTreeContents(matchFunc);
				node.EchoSubTreeContents(item);
			for (int i = 0; i < Contents.Count; i++)
			{
				//if (matchFunc(Contents[i]))
				if (Contents[i] == item)
				{
					Contents.RemoveAt(i);
					break;
				}
			}
		}
		//public void Delete(Func<T, bool> matchFunc, VRect? queryArea = null)
		public void Delete(T item, VRect? queryArea = null)
		{
			queryArea = queryArea ?? bounds;

			if (Contents != null)
				foreach (T item2 in Contents)
					//if (queryArea.Value.Intersects(item.GetRect()) && matchFunc(item))
					if (queryArea.Value.Intersects(item.GetRect()) && item2 == item)
						Contents.Remove(item);
			foreach (QuadTreeNode<T> node in childNodes)
			{
				if (node.IsEmpty)
					continue;

				if (node.Bounds.Contains(queryArea.Value))
				{
					node.GetItemsIntersecting(queryArea.Value);
					break;
				}
				if (queryArea.Value.Contains(node.bounds))
				{
					//node.EchoSubTreeContents(matchFunc);
					node.EchoSubTreeContents(item);
					continue;
				}
				if (node.Bounds.Intersects(queryArea.Value))
					node.GetItemsIntersecting(queryArea.Value);
			}
		}*/

		/*public void Delete(T item, VRect? queryArea = null)
		{
			queryArea = queryArea ?? bounds;

			if (Contents != null && Contents.Remove(item))
				return;
			foreach (QuadTreeNode<T> node in childNodes)
				if (!node.IsEmpty && node.Bounds.Intersects(queryArea.Value))
					node.Delete(item, queryArea);
		}*/
		public bool Delete(T item, VRect? oldBoundsRect = null, bool allowRemovalFromChildren = true) {
			if (contents.Remove(item)) { // if has item directly, no need to check child-nodes
				if (contents.Count == 0)
					hasItems = false;
				return true;
			}

			//queryArea = queryArea ?? bounds;
			oldBoundsRect = oldBoundsRect ?? item.GetBoundsRect();
			foreach (QuadTreeNode<T> node in childNodes)
				//if (node.hasItems && node.Bounds.Intersects(queryArea.Value))
				if (node.hasItems && node.bounds.Contains(oldBoundsRect.Value))
					if (node.Delete(item, oldBoundsRect)) {
						UpdateHasItems();
						return true;
					}

			return false;
		}
		/*public void Update(T item, VRect oldBoundsRect) {
			//Delete(item);
			//Delete(item, oldBoundsRect).RuntimeAssertEquals(true); // todo: fix that this sometimes fails!
			Delete(item, oldBoundsRect); // must remove item from its old location in quad-tree, so query using old bounds-rect
			Insert(item);
		}*/
		public bool Update(T item, VRect oldBoundsRect) {
			//var S = M.GetCurrentMethod().Profile_LastDataFrame();
			var S = BlockRunInfo.fakeBlockRunInfo; // (disabled)
			try {

			S._____("for each child: if contained item, update child; else if only now contains item, add to child (i.e. move item down)");
			foreach (QuadTreeNode<T> node in childNodes)
				if (node.hasItems && node.bounds.Contains(oldBoundsRect)) { // if child contained item, update child
					if (node.Update(item, oldBoundsRect)) // if item confirmed to still exist in child, return true
						return true;
				}
				else if (node.Bounds.Contains(item.GetBoundsRect())) { // else if child only now contains item, add to child (i.e. move item down)
					contents.Remove(item);
					node.Insert(item);
					return true; // item is confirmed to now exist in child, so return true
				}

			S._____("if not contained in self-node/own-rect anymore, remove from self (i.e. move item up)");
			if (bounds.Contains(item.GetBoundsRect())) // if still contains item, return true
				return true;
			Delete(item, oldBoundsRect, false); // child nodes will have already removed item, so just remove from self
			return false; // no longer contains item, so return false

			} finally { S._____(null); }
		}
	}
}