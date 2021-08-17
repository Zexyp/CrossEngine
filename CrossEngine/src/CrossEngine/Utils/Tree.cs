using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrossEngine.Utils
{
    public class TreeNode<T>
    {
        public TreeNode<T> Parent { get; private set; } = null;

        private List<TreeNode<T>> _children = new List<TreeNode<T>>();
        public ReadOnlyCollection<TreeNode<T>> Children { get => _children.AsReadOnly(); }
        public T Value;

        public TreeNode()
        {

        }

        public TreeNode(T value)
        {
            this.Value = value;
        }

        public void SetParent(TreeNode<T> parent)
        {
            if (Parent != null) Parent.RemoveChild(this);
            Parent = parent;
            if (Parent != null) Parent.AddChild(this);
        }

        private void AddChild(TreeNode<T> child)
        {
            _children.Add(child);
            child.Parent = this;

            // TODO: check if it's not cyclic
        }

        private void RemoveChild(TreeNode<T> child)
        {
            _children.Remove(child);
            child.Parent = null;
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
