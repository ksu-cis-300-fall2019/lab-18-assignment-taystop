/* Dictionary.cs
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KansasStateUniversity.TreeViewer2;
using Ksu.Cis300.ImmutableBinaryTrees;

namespace Ksu.Cis300.NameLookup
{
    /// <summary>
    /// A generic dictionary in which keys must implement IComparable.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class Dictionary<TKey, TValue> where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The keys and values in the dictionary.
        /// </summary>
        private BinaryTreeNode<KeyValuePair<TKey, TValue>> _elements = null;

        /// <summary>
        /// Gets a drawing of the underlying binary search tree.
        /// </summary>
        public TreeForm Drawing => new TreeForm(_elements, 100);

        /// <summary>
        /// Checks to see if the given key is null, and if so, throws an
        /// ArgumentNullException.
        /// </summary>
        /// <param name="key">The key to check.</param>
        private static void CheckKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// Finds the given key in the given binary search tree.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="t">The binary search tree.</param>
        /// <returns>The node containing key, or null if the key is not found.</returns>
        private static BinaryTreeNode<KeyValuePair<TKey, TValue>> Find(TKey key, BinaryTreeNode<KeyValuePair<TKey, TValue>> t)
        {
            if (t == null)
            {
                return null;
            }
            else
            {
                int comp = key.CompareTo(t.Data.Key);
                if (comp == 0)
                {
                    return t;
                }
                else if (comp < 0)
                {
                    return Find(key, t.LeftChild);
                }
                else
                {
                    return Find(key, t.RightChild);
                }
            }
        }

        /// <summary>
        /// Builds the binary search tree that results from adding the given key and value to the given tree.
        /// If the tree already contains the given key, throws an ArgumentException.
        /// </summary>
        /// <param name="t">The binary search tree.</param>
        /// <param name="k">The key.</param>
        /// <param name="v">The value.</param>
        /// <returns>The binary search tree that results from adding k and v to t.</returns>
        private static BinaryTreeNode<KeyValuePair<TKey, TValue>> Add(BinaryTreeNode<KeyValuePair<TKey, TValue>> t, TKey k, TValue v)
        {
            if (t == null)
            {
                return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(k, v), null, null);
            }
            else
            {
                int comp = k.CompareTo(t.Data.Key);
                if (comp == 0)
                {
                    throw new ArgumentException();
                }
                else if (comp < 0)
                {
                    return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(t.Data, Add(t.LeftChild, k, v), t.RightChild);
                }
                else
                {
                    return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(t.Data, t.LeftChild, Add(t.RightChild, k, v));
                }
            }
        }

        /// <summary>
        /// Tries to get the value associated with the given key.
        /// </summary>
        /// <param name="k">The key.</param>
        /// <param name="v">The value associated with k, or the default value if
        /// k is not in the dictionary.</param>
        /// <returns>Whether k was found as a key in the dictionary.</returns>
        public bool TryGetValue(TKey k, out TValue v)
        {
            CheckKey(k);
            BinaryTreeNode<KeyValuePair<TKey, TValue>> p = Find(k, _elements);
            if (p == null)
            {
                v = default(TValue);
                return false;
            }
            else
            {
                v = p.Data.Value;
                return true;
            }
        }

        /// <summary>
        /// Finds minimum available value of new tree right child
        /// </summary>
        /// <param name="t">tree to search</param>
        /// <param name="min">smallest available cell</param>
        /// <returns></returns>
        private static BinaryTreeNode<KeyValuePair<TKey, TValue>> RemoveMininumKey(BinaryTreeNode<KeyValuePair<TKey, TValue>> t, out KeyValuePair<TKey, TValue> min)
        {
            if(t.LeftChild != null)
            {
                BinaryTreeNode<KeyValuePair<TKey, TValue>> newLeft = RemoveMininumKey(t.LeftChild, out min);
                return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(t.Data, newLeft, t.RightChild);
            }
            else
            {
                min = t.Data;
                return t.RightChild;
            }
        }

        /// <summary>
        /// Removes selected key from tree
        /// </summary>
        /// <param name="key">key to remove</param>
        /// <param name="t">tree key is removed from</param>
        /// <param name="removed">if key is found and removed</param>
        /// <returns>resulting tree with removed data</returns>
        private static BinaryTreeNode<KeyValuePair<TKey, TValue>> Remove(TKey key, BinaryTreeNode<KeyValuePair<TKey, TValue>> t, out bool removed)
        {
            if(t == null)
            {
                removed = false;
                return null;
            }
            else if (t.Data.Key.CompareTo(key) == -1)
            {
                BinaryTreeNode<KeyValuePair<TKey, TValue>> rightUpdate = Remove(key, t.RightChild, out removed);
                return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(t.Data, t.LeftChild, rightUpdate);
            }
            else if(t.Data.Key.CompareTo(key) == 1)
            {
                BinaryTreeNode<KeyValuePair<TKey, TValue>> leftUpdate = Remove(key, t.LeftChild, out removed);
                return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(t.Data, leftUpdate, t.RightChild);
            }
            else
            {
                removed = true;
                if(t.RightChild == null && t.LeftChild == null)
                {
                    return null;
                }
                else if ((t.LeftChild == null && t.RightChild != null) || (t.LeftChild != null && t.RightChild == null))
                {
                    if (t.LeftChild != null)
                        return t.LeftChild;
                    else
                        return t.RightChild;
                }
                else
                {
                    KeyValuePair<TKey, TValue> smallest;
                    BinaryTreeNode<KeyValuePair<TKey, TValue>> rightUpdate = RemoveMininumKey(t.RightChild, out smallest);
                    return new BinaryTreeNode<KeyValuePair<TKey, TValue>>(smallest, t.LeftChild, rightUpdate);
                }
            }
        }

        /// <summary>
        /// Adds the given key with the given associated value.
        /// If the given key is already in the dictionary, throws an
        /// InvalidOperationException.
        /// </summary>
        /// <param name="k">The key.</param>
        /// <param name="v">The value.</param>
        public void Add(TKey k, TValue v)
        {
            CheckKey(k);
            _elements = Add(_elements, k, v);
        }

        /// <summary>
        /// Updated dictionary remove function
        /// </summary>
        /// <param name="k">dictionary key to return</param>
        /// <returns>if key was removed</returns>
        public bool Remove(TKey k)
        {
            CheckKey(k);
            _elements = Remove(k, _elements, out bool removed);
            return removed;
        }
    }
}
