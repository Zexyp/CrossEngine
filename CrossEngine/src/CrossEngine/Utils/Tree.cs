using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

using CrossEngine.Serialization;

namespace CrossEngine.Utils
{
    public class TreeNode<T>/* : ISerializableUntyped*/
    {
        public TreeNode<T> _parent = null;
        private List<TreeNode<T>> _children = new List<TreeNode<T>>();
        public T Value;

        public TreeNode<T> Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != null)
                {
                    _parent.RemoveChild(this);
                }

                _parent = value;

                if (_parent != null)
                {
                    _parent.AddChild(this);
                }
            }
        }

        public readonly ReadOnlyCollection<TreeNode<T>> Children;

        public TreeNode()
        {
            Children = _children.AsReadOnly();
        }

        public TreeNode(T value) : this()
        {
            this.Value = value;
        }

        private void AddChild(TreeNode<T> child)
        {
            _children.Add(child);
            child._parent = this;

            // TODO: check if it's not cyclic
        }

        private void RemoveChild(TreeNode<T> child)
        {
            _children.Remove(child);
            child._parent = null;
        }

        public bool IsParentedBy(TreeNode<T> potpar)
        {
            if (this.Parent == null) return false;
            if (this.Parent == potpar) return true;
            return this.Parent.IsParentedBy(potpar);
        }

        public int GetChildIndex(TreeNode<T> child)
        {
            //if (!_children.Contains(child)) throw new InvalidOperationException("Node does not have given child node!");
            return _children.IndexOf(child);
        }

        public void ShiftChild(TreeNode<T> child, int destinationIndex)
        {
            if (!_children.Contains(child)) throw new InvalidOperationException("Node does not have given child node!");
            if (destinationIndex < 0 || destinationIndex > _children.Count - 1) throw new InvalidOperationException("Invalid index!");

            _children.Remove(child);
            _children.Insert(destinationIndex, child);
        }

        /*
        #region ISerializable
        public void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Value", Value);
            info.AddValue("Children", Children);
        }

        public void OnDeserialize(SerializationInfo info)
        {
            Value = (T) info.GetValue("Value", typeof(T));
            foreach (var child in (IEnumerable)info.GetValue("Children", typeof(List<TreeNode<T>>)))
            {
                AddChild((TreeNode<T>)child);
            }
        }
        #endregion
        */

        //public void SwapChildren(TreeNode<T> child1, TreeNode<T> child2)
        //{
        //    if (!_children.Contains(child1) || !_children.Contains(child2)) throw new InvalidOperationException("Node does not contain given child!");
        //
        //    int index1 = _children.IndexOf(child1);
        //    int index2 = _children.IndexOf(child2);
        //    _children[index1] = child2;
        //    _children[index2] = child1;
        //}

        //public bool HasRoot(TreeNode<T> node)
        //{
        //    TreeNode<T> current = this;
        //    while (current != null)
        //    {
        //        if (node.Parent == current)
        //        {
        //            return true;
        //            break;
        //        }
        //        else
        //        {
        //            current = current.Parent;
        //        }
        //    }
        //    return false;
        //}
        //
        //public bool HasChild(TreeNode<T> node)
        //{
        //    TreeNode<T> current = this;
        //    while (current != null)
        //    {
        //        if (node.Parent == current)
        //        {
        //            return true;
        //            break;
        //        }
        //        else
        //        {
        //            current = current.Parent;
        //        }
        //    }
        //    return false;
        //}
    }
}
