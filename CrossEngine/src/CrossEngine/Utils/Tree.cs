using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrossEngine.Utils
{
    public class TreeNode<T>
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
