using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine;

namespace PH.DataTree
{

   #region Delegates, EventArgs and Enumerations

   public enum ENodeEvent
   {
      ValueAccessed,
      ValueChanged,
      NodeChanged,
      ChildOrderChanged,
      ChildAdded,
      ChildRemoved,
      ChildsCleared,
   }

   /// <summary>EventArgs for changes in a DTreeNode</summary>
   public class DTreeEventArgs<AudioNodeData> : System.EventArgs
   {
      public DTreeEventArgs(DTreeNode node, ENodeEvent change, int index)
      {
         Node = node;
         Change = change;
         Index = index;
      }

      /// <summary>The Node for which the event occured.</summary>
      public DTreeNode  Node;

      /// <summary>
      /// <list>
      ///   <item>ValueAccessed: the get - accessor for node.Value was called. index is unused</item>
      ///   <item>ValueChanged: A new value was assigned to node.Value. index is unused</item>
      ///   <item>NodeChanged: The node itself has changed (e.g. another node was assigned) All child nodes may have changed, too</item>
      ///   <item>ChildOrderChanged: the order of the elements of node.Childs has changed</item>
      ///   <item>ChildAdded: a child node was added at position <c>index</c></item>
      ///   <item>ChildRemoved: a child node was removed at position <c>index</c>.
      ///         This notification is <b>not</b> sent when all items are removed using Clear
      ///   </item>
      ///   <item>ChildsCleared: all childs were removed.</item>
      /// </list>
      /// </summary>
      public ENodeEvent Change;

      /// <summary>Index of the child node affected. See the Change member for more information.</summary>
      public int Index;
   }

   public delegate string NodeToString<AudioNodeData>(DTreeNode node); 

   #endregion

   #region Tree Root

   /// <summary>
   /// A DTreeRoot object acts as source of tree node events. A single instance is associated
   /// with each tree (the Root property of all nodes of a tree return the same instance, nodes
   /// from different trees return different instances)
   /// </summary>
   /// <typeparam name="T">type of the data value at each tree node</typeparam>
   public class DTreeRoot
   {
      private DTreeNode mRoot;

      internal DTreeRoot(DTreeNode root)
      {
         mRoot = root;
      }

      public DTreeNode RootNode { get { return mRoot; } }

      #region Events


      /// <summary>
      /// signals that a new value was assigned to a given node. <br/>
      /// Note: if T is a reference type and modified indirectly, this event doesn't fire
      /// </summary>
      public event System.EventHandler OnValueChanged;

      /// <summary>
      /// signals that Node.Value was accessed.
      /// This can be used by a tree view controller to implement a defered 
      /// update even if T is a reference type and changed implicitely (i.e. 
      /// for cases where OnValueChanged does not fire)
      /// </summary>
      public event System.EventHandler OnValueAccessed;

      /// <summary>signals that the Node structure has changed</summary>
      public event System.EventHandler OnNodeChanged;


      #region Internal Helpers
      internal void SendValueChanged(DTreeNode node)
      {
         if (OnValueChanged != null)
            OnValueChanged(this, new DTreeEventArgs<AudioNodeData>(node, ENodeEvent.ValueChanged, -1));
      }

      internal void SendValueAccessed(DTreeNode node)
      {
         if (OnValueAccessed != null)
            OnValueAccessed(this, new DTreeEventArgs<AudioNodeData>(node, ENodeEvent.ValueAccessed, -1));
      }

      internal void SendNodeChanged(DTreeNode node, ENodeEvent change, int index)
      {
         if (OnNodeChanged!=null)
            OnNodeChanged(this, new DTreeEventArgs<AudioNodeData>(node, change, index));
      }
      #endregion // Internal Helpers



      #endregion // Events
   }


   #endregion // Tree Events

   #region DTreeNode
   /// <summary>
   /// Represents a single Tree Node
   /// </summary>
   /// <typeparam name="T">Type of the Data Value at the node</typeparam>
	[System.Serializable]
   public class DTreeNode 
   {
      #region Node Data
		[SerializeField]
      private AudioNodeData mValue;
		[SerializeField]
      private DTreeNode mParent = null;
		[SerializeField]
      private DTreeNodeCollection<AudioNodeData> mNodes = null;
		[SerializeField]
      private DTreeRoot mRoot = null;
      #endregion // Node Data
		public int pipi;

      #region CTORs

		[SerializeField]
      public DTreeNode()
      {
		//mValue = default(AudioNodeData);
         mRoot = new DTreeRoot(this);
      }

      /// <summary>
      /// creates a new root node, and sets Value to value.
      /// </summary>
      /// <param name="value"></param>
	[SerializeField]
      public DTreeNode(AudioNodeData value)
      {
         mValue = value;
         mRoot = new DTreeRoot(this);
      }

      /// <summary>
      /// Creates a new node as child of parent, and sets Value to value
      /// </summary>
      /// <param name="value"></param>
      /// <param name="parent"></param>
	[SerializeField]
      internal DTreeNode(AudioNodeData value, AudioNodeData parent)
      {
         mValue = value;
         //InternalSetParent(parent);
      }
      #endregion // CTORs

      #region Data Access
      /// <summary>
      /// Node data
      /// Setting the value fires Tree<AudioNodeData>.OnNodeChanged
      /// </summary>
      [System.Xml.Serialization.XmlElement(ElementName = "val")]
		[SerializeField]
      public AudioNodeData Value
      {
         get 
         {
            mRoot.SendValueAccessed(this);
            return mValue;
         }
         set
         {
            mValue = value;
            mRoot.SendValueChanged(this);
         }
      }
      #endregion // Data Access

      #region Navigation

      /// <summary>returns the parent node, or null if this is a root node</summary>
      [System.Xml.Serialization.XmlIgnore]
      public DTreeNode Parent { get { return mParent; } }

      /// <summary>
      /// returns all siblings as a NodeList<AudioNodeData>. If this is a root node, the function returns null.
      /// </summary>
      [System.Xml.Serialization.XmlIgnore]
      public DTreeNodeCollection<AudioNodeData> Siblings
      {
         get { return mParent != null ? mParent.Nodes : null; }
      }

      /// <summary>
      /// returns all child nodes as a NodeList<AudioNodeData>. 
      /// <para><b>Implementation note:</b> Childs always returns a non-null collection. 
      /// This collection is created on demand at the first access. To avoid unnecessary 
      /// creation of the collection, use HasChilds to check if the node has any child nodes</para>
      /// </summary>
      [System.Xml.Serialization.XmlArrayItem("node")]
      public DTreeNodeCollection<AudioNodeData> Nodes
      {
         get
         {
            if (mNodes == null)
               mNodes = new DTreeNodeCollection<AudioNodeData>(this);
            return mNodes;
         }
      }

      /// <summary>
      /// The Root object this Node belongs to. never null
      /// </summary>
      [System.Xml.Serialization.XmlIgnore]
      public DTreeRoot Root { get { return mRoot; } }

      internal void SetRootLink(DTreeRoot root)
      {
         if (mRoot != root) // assume sub trees are consistent
         {
            mRoot = root;
            if (HasChildren)
               foreach (DTreeNode n in Nodes)
                  n.SetRootLink(root);
         }
      }

      /// <summary>
      /// returns true if the node has child nodes.
      /// See also Implementation Note under this.Childs
      /// </summary>
      [System.Xml.Serialization.XmlIgnore]
      public bool HasChildren { get { return mNodes != null && mNodes.Count != 0; } }

      /// <summary>
      /// returns true if this node is a root node. (Equivalent to this.Parent==null)
      /// </summary>
      [System.Xml.Serialization.XmlIgnore]
      public bool IsRoot { get { return mParent == null; } }

      public bool IsAncestorOf(DTreeNode node)
      {
         if (node.Root != Root)
            return false; // different trees
         DTreeNode parent = node.Parent;
         while (parent != null && parent != this)
            parent = parent.Parent;
         return parent != null;
      }

      public bool IsChildOf(DTreeNode node) 
      { 
         return !IsAncestorOf(node); 
      }

      public bool IsInLineWith(DTreeNode node)
      {
         return node == this ||
                node.IsAncestorOf(this) ||
                node.IsChildOf(node);
      }

      [System.Xml.Serialization.XmlIgnore]
      public int Depth
      {
         get
         {
            int depth = 0;
            DTreeNode node = mParent;
            while (node != null)
            {
               ++depth;
               node = node.mParent;
            }
            return depth;
         }
      }

      #endregion // Navigation

      #region Node Path

      public DTreeNode GetNodeAt(int index)
      {
         return Nodes[index];
      }

      public DTreeNode GetNodeAt(IEnumerable<int> index)
      {
         DTreeNode node = this;
         foreach(int elementIndex in index)
         {
            node = node.Nodes[elementIndex];
         }
         return node;
      }

      public DTreeNode GetNodeAt(params int[] index)
      {
         return GetNodeAt(index as IEnumerable<int>);
      }

      public int[] GetIndexPathTo(DTreeNode node)
      {
         if (Root != node.Root)
            throw new ArgumentException("parameter node must belong to the same tree");
         List<int> index = new List<int>();

         while (node != this && node.mParent != null)
         {
            index.Add(node.mParent.Nodes.IndexOf(node));
            node = node.mParent;
         }

         if (node != this)
            throw new ArgumentException("node is not a child of this");

         index.Reverse();
         return index.ToArray();
      }

      public int[] GetIndexPath()
      {
         return Root.RootNode.GetIndexPathTo(this);
      }

      public System.Collections.IList GetNodePath()
      {
         List<DTreeNode> list = new List<DTreeNode>();
         DTreeNode node = mParent;

         while (node != null)
         {
            list.Add(node);
            node = node.Parent;
         }
         list.Reverse();
         list.Add(this);

         return list;
      }

      public IList<AudioNodeData> GetElementPath()
      {
         List<AudioNodeData> list = new List<AudioNodeData>();
         DTreeNode node = mParent;

         while (node != null)
         {
            list.Add(node.Value);
            node = node.Parent;
         }
         list.Reverse();
         list.Add(this.mValue);

         return list;
      }

      public string GetNodePathAsString(char separator, NodeToString<AudioNodeData> toString)
      {
         string s = "";
         DTreeNode node = this;

         while (node != null)
         {
            if (s.Length != 0)
               s = toString(node) + separator + s;
            else
               s = toString(node);
            node = node.Parent;
         }

         return s;
      }

      public string GetNodePathAsString(char separator)
      {
         return GetNodePathAsString(separator,
                  delegate(DTreeNode node) { return node.Value.ToString(); });
      }

      #endregion // Node Path

      #region Modify
      /// <summary>
      /// Removes the current node and all child nodes recursively from it's parent.
      /// Throws an InvalidOperationException if this is a root node.
      /// </summary>
      public void Remove()
      {
         if (mParent == null)
            throw new InvalidOperationException("cannot remove root node");
         Detach();
      }

      /// <summary>
      /// Detaches this node from it's parent. 
      /// Postcondition: this is a root node.
      /// </summary>
      /// <returns></returns>
      public DTreeNode Detach()
      {
         if (mParent != null)
            Siblings.Remove(this);

         return this;
      }
      #endregion // Modify

      #region Enumerators
      public IEnumerable<AudioNodeData> DepthFirstEnumerator
      {
         get
         {
            foreach (DTreeNode node in DepthFirstNodeEnumerator)
               yield return node.Value;
         }
      }

      public IEnumerable<DTreeNode> DepthFirstNodeEnumerator
      {
         get
         {
            yield return this;
            if (mNodes != null)
            {
               foreach (DTreeNode child in mNodes)
               {
                  IEnumerator<DTreeNode> childEnum = child.DepthFirstNodeEnumerator.GetEnumerator();
                  while (childEnum.MoveNext())
                     yield return childEnum.Current;
               }
            }
         }
      }

      public IEnumerable<DTreeNode> BreadthFirstNodeEnumerator
      {
         get
         {
            Queue<DTreeNode> todo = new Queue<DTreeNode>();
            todo.Enqueue(this);
            while (0 < todo.Count)
            {
               DTreeNode node = todo.Dequeue();
               if (node.mNodes != null)
               {
                  foreach (DTreeNode child in node.mNodes)
                     todo.Enqueue(child);
               }
               yield return node;
            }
         }
      }

      public IEnumerable<AudioNodeData> BreadthFirstEnumerator
      {
         get
         {
            foreach (DTreeNode node in BreadthFirstNodeEnumerator)
               yield return node.Value;
         }
      }
      #endregion

      #region Internal Helper

      internal void InternalDetach()
      {
         mParent = null;
         SetRootLink(new DTreeRoot(this));
      }
		  
      internal void InternalSetParent(DTreeNode parent)
      {
         mParent = parent;
         if (mParent != null)
            SetRootLink(parent.Root);
      }

      #endregion // Internal Helper
   }

   #endregion // DTreeNode


   /// <summary>
   /// Implements a collection of Tree Nodes (Node<AudioNodeData>)
   /// <para><b>Implementation Note:</b> The root of a data tree is always a Node<AudioNodeData>. You cannot
   /// create a standalone NodeList<AudioNodeData>.
   /// </para>
   /// </summary>
   /// <typeparam name="T">typeof the data value of each node</typeparam>
   public class DTreeNodeCollection<AudioNodeData> 
         : System.Collections.CollectionBase, IEnumerable<DTreeNode>
   {
      #region CTORs
      internal DTreeNodeCollection(DTreeNode owner)
      {
         if (owner == null)
            throw new ArgumentNullException("owner");
         mOwner = owner;
      }

      #endregion

      #region Additional public interface

      /// <summary>
      /// The Node to which this collection belongs (this==Owner.Childs). 
      /// Never null.
      /// </summary>
      public DTreeNode Owner { get { return mOwner; } }

      #endregion // public interface

      #region Collection implementation (indexer, add, remove)


      // Provide the strongly typed member for ICollection.
      public void CopyTo(DTreeNode[] array, int index)
      {
         ((ICollection<DTreeNode>)this).CopyTo(array, index);
      }

      public new IEnumerator<DTreeNode> GetEnumerator()
      {
         foreach (DTreeNode node in InnerList)
            yield return node;
      }

      public void Insert(int index, DTreeNode node)
      {
         List.Insert(index, node);
      }

      public bool Contains(DTreeNode node)
      {
         return List.Contains(node);
      }


      /// <summary>
      /// Indexer accessing the index'th Node.
      /// If the owning node belongs to a tree, Setting the node fires a NodeChanged event
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      public DTreeNode this[int index]
      {
         get { return ((DTreeNode)List[index]); }
         set { List[index] = value; }
      }

      /// <summary>
      /// Appends a new node with the specified value
      /// </summary>
      /// <param name="value">value for the new node</param>
      /// <returns>the node that was created</returns>
  /*    public DTreeNode Add(AudioNodeData value) 
      {
         DTreeNode n = new DTreeNode();
			n.Value = value;
         List.Add(n);

         SendOwnerNodeChanged(ENodeEvent.ChildAdded, List.Count-1);

         return n;
      }*/

      // required for XML Serializer, not to bad to have...
      public void Add(DTreeNode node)
      {
         List.Add(node);
         SendOwnerNodeChanged(ENodeEvent.ChildAdded, List.Count - 1);
      }

      /// <summary>
      /// Adds a range of nodes created from a range of values
      /// </summary>
      /// <param name="range">range of values </param>
     public void AddRange(IEnumerable<DTreeNode> range)
      {
         foreach (DTreeNode value in range)
            Add(value);
      }

      /// <summary>
      /// Adds a range of nodes created from a range of values passed as parameters
      /// </summary>
      /// <param name="range">range of values </param>
      public void AddRange(params DTreeNode[] args)
      {
         AddRange(args as IEnumerable<DTreeNode>);
      }


      /// <summary>
      /// Adds a new node with the given value at the specified index.
      /// </summary>
      /// <param name="index">Position where to insert the item.
      /// All values are accepted, if index is out of range, the new item is inserted as first or 
      /// last item</param>
      /// <param name="value">value for the new node</param>
      /// <returns></returns>
/*      public DTreeNode InsertAt(int index, AudioNodeData value)
      {
         DTreeNode n = new DTreeNode(value, mOwner);

         // "tolerant insert"
         if (index < 0)
            index = 0;

         if (index >= Count)
         {
            index = Count;
            List.Add(n);
         }
         else
            List.Insert(index, n);

         SendOwnerNodeChanged(ENodeEvent.ChildAdded, index);

         return n;
      }*/

      /// <summary>
      /// Inserts a range of nodes created from a range of values
      /// </summary>
      /// <param name="index">index where to start inserting. As with InsertAt, all values areaccepted</param>
      /// <param name="values">a range of values set for the nodes</param>
     /* public void InsertRangeAt(int index, IEnumerable<AudioNodeData> values)
      {
         foreach (AudioNodeData value in values)
         {
            InsertAt(index, value);
            ++index;
         }
      }
*/
      /// <summary>
      /// Inserts a new node before the specified node.
      /// </summary>
      /// <param name="insertPos">Existing node in front of which the new node is inserted</param>
      /// <param name="value">value for the new node</param>
      /// <returns>The newly created node</returns>
     /* public DTreeNode InsertBefore(DTreeNode insertPos, AudioNodeData value)
      {
         int index = IndexOf(insertPos);
         //return InsertAt(index, value);
      }*/

      /// <summary>
      /// Inserts a new node after the specified node
      /// </summary>
      /// <param name="insertPos">Existing node after which the new node is inserted</param>
      /// <param name="value">value for the new node</param>
      /// <returns>The newly created node</returns>
     /* public DTreeNode InsertAfter(DTreeNode insertPos, AudioNodeData value)
      {
         int index = IndexOf(insertPos) + 1;
         if (index == 0)
            index = Count;
         return InsertAt(index, value);
      }*/

      public int IndexOf(DTreeNode node) 
      {
         return (List.IndexOf(node)); 
      }

      public void Remove(DTreeNode node)
      {
         int index = IndexOf(node);
         if (index < 0) 
            throw new ArgumentException("the node to remove is not a in this collection");
         RemoveAt(index);
      }

      #endregion

      #region CollectionBase overrides (action handler)

      protected override void OnValidate(object value)
      {
         // Verify: value.Parent must be null or this.mOwner)
         base.OnValidate(value);
         DTreeNode parent = ((DTreeNode)value).Parent;
         if (parent != null && parent != mOwner)
            throw new ArgumentException("Cannot add a node referenced in another node collection");
      }

      protected override void OnInsert(int index, object value)
      {
         // set parent note to this.mOwner
         ((DTreeNode)value).InternalSetParent(mOwner);
      }

      protected override void OnRemoveComplete(int index, object value)
      {
         ((DTreeNode)value).InternalDetach();

         SendOwnerNodeChanged(ENodeEvent.ChildRemoved, index);

         base.OnRemoveComplete(index, value);
      }

      protected override void OnSet(int index, object oldValue, object newValue)
      {
         if (oldValue != newValue)
         {
            ((DTreeNode)oldValue).InternalDetach();
            ((DTreeNode)newValue).InternalSetParent(mOwner);

         }
         base.OnSet(index, oldValue, newValue);
      }

      protected override void OnSetComplete(int index, object oldValue, object newValue)
      {
         SendOwnerNodeChanged(ENodeEvent.NodeChanged, index);
         base.OnSetComplete(index, oldValue, newValue);
      }

      protected override void OnClear()
      {
         // set parent to null for all elements
         foreach (DTreeNode node in InnerList)
            node.InternalDetach();

         base.OnClear();
      }

      protected override void OnClearComplete()
      {
         SendOwnerNodeChanged(ENodeEvent.ChildsCleared, 0);
         base.OnClearComplete();
      }

      #endregion // CollectionBase overrides

      #region protected helpers
      private DTreeNode mOwner = null;

      protected void SendOwnerNodeChanged(ENodeEvent changeHint, int index)
      {
         mOwner.Root.SendNodeChanged(Owner, changeHint, index);
      }
      #endregion // Internal Helpers

      /* TODO:
       * Exists(rpedicate), Find* 
       * Swap (internal, expose at node?)
       * Reverse, Sort
       * TrueForAll
       * 
       */
   }


} // namespace ph.tree

   