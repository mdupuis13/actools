﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace AcTools.Kn5File {
    public partial class Kn5 {
        public string OriginalFilename { get; }

        private Kn5() {
            OriginalFilename = string.Empty;
            Header = new Kn5Header { Version = CommonAcConsts.Kn5ActualVersion };
            Textures = new Dictionary<string, Kn5Texture>();
            TexturesData = new Dictionary<string, byte[]>();
            Materials = new Dictionary<string, Kn5Material>();
        }

        private Kn5(string filename) {
            OriginalFilename = filename;
        }

        public Kn5Header Header;
        public Dictionary<string, Kn5Texture> Textures;
        public Dictionary<string, byte[]> TexturesData;
        public Dictionary<string, Kn5Material> Materials;

        [CanBeNull]
        public Kn5Material GetMaterial(uint id) {
            return Materials.Values.ElementAtOrDefault((int)id);
        }

        [CanBeNull]
        public Kn5Node GetNode([NotNull] string path) {
            var pieces = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var node = RootNode;

            for (var i = 0; i < pieces.Length; i++) {
                node = node?.GetByName(pieces[i]);
                if (node == null) return null;
            }

            return node;
        }

        [CanBeNull]
        private string GetObjectPath([NotNull] Kn5Node parent, [NotNull] Kn5Node node) {
            if (parent.Children.IndexOf(node) != -1) return node.Name;

            for (var i = 0; i < parent.Children.Count; i++) {
                var child = parent.Children[i];
                var v = GetObjectPath(child, node);
                if (v != null) {
                    return child.Name + '\\' + v;
                }
            }

            return null;
        }

        [CanBeNull]
        public string GetObjectPath([NotNull] Kn5Node node) {
            return GetObjectPath(RootNode, node);
        }

        public Kn5Node RootNode;

        public IEnumerable<Kn5Node> Nodes {
            get {
                var queue = new Queue<Kn5Node>();
                if (RootNode != null) {
                    queue.Enqueue(RootNode);
                }

                while (queue.Count > 0) {
                    var next = queue.Dequeue();

                    if (next.NodeClass == Kn5NodeClass.Base) {
                        foreach (var child in next.Children) {
                            queue.Enqueue(child);
                        }
                    }

                    yield return next;
                }
            }
        }

        [CanBeNull]
        public Kn5Node FirstByName(string name) {
            return Nodes.FirstOrDefault(node => node.Name == name);
        }

        public int RemoveAllByName(Kn5Node node, string name) {
            var result = 0;
            for (var i = 0; i < node.Children.Count; i++) {
                var child = node.Children[i];
                if (child.Name == name) {
                    node.Children.Remove(child);
                    result++;
                } else if (child.NodeClass == Kn5NodeClass.Base) {
                    result += RemoveAllByName(child, name);
                }
            }

            return result;
        }

        public int RemoveAllByName(string name) {
            return RemoveAllByName(RootNode, name);
        }

        [CanBeNull]
        public Kn5Node FirstFiltered(Func<Kn5Node, bool> filter) {
            return Nodes.FirstOrDefault(filter);
        }
    }
}
