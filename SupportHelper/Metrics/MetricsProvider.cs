using System;
using System.Collections.Generic;
using System.Data;

using CMS.Helpers;
using CMS.UIControls;

namespace SupportHelper
{
    public class MetricsProvider : UniTreeProvider
    {
        private Metrics mTree;

        new public string IDColumn { get { return "MetricId"; } }

        new public string DisplayNameColumn { get { return "MetricDisplayName"; } }

        public string ParentColumn { get { return "MetricParent"; } }

        public string HasChildrenColumn { get { return "MetricHasChildren"; } }

        public string SelectedColumn { get { return "MetricSelected"; } }

        new public string PathColumn { get { return "MetricPath"; } }

        new public UniTreeNode RootNode
        {
            get
            {
                UniTreeNode node = new UniTreeNode(this, Convert.ToString(Tree.Rows[0][PathColumn]))
                {
                    ItemData = Tree.Rows[0]
                };

                return node;
            }
        }

        public Metrics Tree
        {
            get
            {
                if (mTree == null)
                {
                    throw new NoNullAllowedException("Missing Metrics tree for some reason.");
                }

                return mTree;
            }

            set
            {
                mTree = value;
            }
        }

        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        /// <param name="nodeValue">Parent node value in the format "NodeID_ObjectType"</param>
        /// <param name="nodeDepth">Specified level</param>
        new public List<UniTreeNode> GetChildNodes(string nodeValue)
        {
            int nodeId = ValidationHelper.GetInteger(nodeValue, 0);

            List<UniTreeNode> childNodes = new List<UniTreeNode>();

            foreach (Metric metric in Tree.Rows)
            {
                if (metric.MetricParent != null && metric.MetricParent.MetricId == nodeId)
                {
                    UniTreeNode node = new UniTreeNode(this, metric.MetricPath)
                    {
                        ItemData = metric
                    };

                    childNodes.Add(node);
                }
            }

            return childNodes;
        }
    }
}