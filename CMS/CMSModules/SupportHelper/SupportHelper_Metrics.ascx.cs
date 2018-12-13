using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

using SupportHelper;

public partial class CMSModules_SupportHelper_SupportHelper_Metrics : UniTree, IPostBackEventHandler
{
    #region Variables

    private TreeNode mRootNode;
    private string mSelectedItem;
    private string selectedPath = String.Empty;
    private string mCollapseTooltip;
    private string mExpandTooltip;
    private string mLineImagesFolder = String.Empty;

    private readonly ArrayList defaultItems = new ArrayList();
    private MetricsProvider mProviderObject;

    #endregion Variables

    #region Public properties

    /// <summary>
    /// Indicates whether tree displays all roots elements (parentID IS NULL)
    /// </summary>
    //public bool MultipleRoots
    //{
    //    get;
    //    set;
    //}

    /// <summary>
    /// Indicates if the root element action should be None or Select.
    /// </summary>
    public bool EnableRootAction { get; set; } = true;

    /// <summary>
    /// Indicates if ##NODENAME## should be localized.
    /// </summary>
    public bool Localize
    {
        get;
        set;
    }

    /// <summary>
    /// If true, spans IDs are general (f.e. category not reportcategory)
    /// </summary>
    public bool GeneralIDs
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates if all nodes should be expanded.
    /// </summary>
    public bool ExpandAll
    {
        get;
        set;
    }

    /// <summary>
    /// Gets root node from provider object.
    /// </summary>
    public TreeNode RootNode
    {
        get
        {
            if (mRootNode == null)
            {
                return CustomRootNode;
            }

            return mRootNode;
        }
    }

    /// <summary>
    /// Tree view control
    /// </summary>
    protected override UITreeView TreeView
    {
        get
        {
            return treeElem;
        }
    }

    /// <summary>
    /// Gets or sets the value which indicates whether populating indicator should be displayed or not.
    /// </summary>
    public bool DisplayPopulatingIndicator { get; set; } = true;

    /// <summary>
    /// Gets or sets the ToolTip for the image that is displayed for the expandable node indicator.
    /// </summary>
    public string ExpandTooltip
    {
        get
        {
            return mExpandTooltip;
        }
        set
        {
            mExpandTooltip = value;
            TreeView.ExpandImageToolTip = value;
        }
    }

    /// <summary>
    /// Gets or sets the ToolTip for the image that is displayed for the collapsible node indicator.
    /// </summary>
    public string CollapseTooltip
    {
        get
        {
            return mCollapseTooltip;
        }
        set
        {
            TreeView.CollapseImageToolTip = value;
            mCollapseTooltip = value;
        }
    }

    /// <summary>
    /// Gets or sets the path to a folder that contains the line images that are used to connect child nodes to parent nodes.
    /// </summary>
    public string LineImagesFolder
    {
        get
        {
            if (String.IsNullOrEmpty(mLineImagesFolder))
            {
                if ((IsLiveSite && CultureHelper.IsPreferredCultureRTL()) || (!IsLiveSite && CultureHelper.IsUICultureRTL()))
                {
                    mLineImagesFolder = "~" + RequestContext.CurrentRelativePath + "?cmsimg=/rt";
                }
                else
                {
                    mLineImagesFolder = "~" + RequestContext.CurrentRelativePath + "?cmsimg=/t";
                }
            }
            return mLineImagesFolder;
        }
        set
        {
            mLineImagesFolder = value;
            TreeView.LineImagesFolder = value;
        }
    }

    /// <summary>
    /// Gets or sets selected item.
    /// </summary>
    public override string SelectedItem
    {
        get
        {
            return mSelectedItem ?? (mSelectedItem = hdnSelectedItem.Value);
        }
        set
        {
            hdnSelectedItem.Value = value;
            mSelectedItem = value;
        }
    }

    /// <summary>
    /// Gets the client ID of hidden field with selected item value.
    /// </summary>
    public string SelectedItemFieldId
    {
        get
        {
            return hdnSelectedItem.ClientID;
        }
    }

    new public MetricsProvider ProviderObject
    {
        get
        {
            if (mProviderObject == null)
            {
                mProviderObject = new MetricsProvider();
            }

            return mProviderObject;
        }
    }

    public Metrics Metrics
    {
        set
        {
            ProviderObject.Tree = value;
        }
    }

    #endregion Public properties

    #region Events

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        TreeView.TreeNodePopulate += (s, args) => PopulateNode(args.Node);
    }

    /// <summary>
    /// Page load event.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        TreeView.ExpandImageToolTip = GetString("general.expand");
        TreeView.CollapseImageToolTip = GetString("general.collapse");

        TreeView.LineImagesFolder = LineImagesFolder;
    }

    /// <summary>
    /// Page PreRender.
    /// </summary>
    /// <param name="e">Arguments</param>
    protected override void OnPreRender(EventArgs e)
    {
        int index = 0;

        foreach (object item in defaultItems)
        {
            string[] defaultItem = (string[])item;

            if (defaultItem != null)
            {
                // Generate link HTML tag
                string selectedItem = ValidationHelper.GetString(SelectedItem, "").ToLowerCSafe();
                string template = (selectedItem == defaultItem[2].ToLowerCSafe()) ? SelectedDefaultItemTemplate : DefaultItemTemplate;

                string link = ReplaceMacros(template, 0, 0, defaultItem[0], 0);

                TreeNode tn = new TreeNode
                {
                    Text = link,
                    NavigateUrl = RequestContext.CurrentURL + "#"
                };

                TreeView.Nodes.AddAt(index, tn);
                index++;
            }
        }

        if (DisplayPopulatingIndicator && !RequestHelper.IsCallback())
        {
            // Register tree progress icon
            ScriptHelper.RegisterTreeProgress(Page);
        }

        base.OnPreRender(e);
    }

    #endregion Events

    #region Methods

    /// <summary>
    /// Handle node is populated.
    /// </summary>
    protected void PopulateNode(TreeNode node)
    {
        if (node != null)
        {
            int nodeID = ValidationHelper.GetInteger(node.Value, 0);

            // Get child nodes
            List<UniTreeNode> childNodes = ProviderObject.GetChildNodes(node.Value);

            // Add to treeview
            foreach (UniTreeNode childNode in childNodes)
            {
                // Get ID
                int childNodeId = ((Metric)childNode.ItemData).MetricId;

                // Don't insert one object more than once
                if ((childNodeId != nodeID))
                {
                    TreeNode createdNode = CreateNode(childNode);
                    RaiseOnNodeCreated(childNode, ref createdNode);
                    if (createdNode != null)
                    {
                        node.ChildNodes.Add(createdNode);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reloads data.
    /// </summary>
    public override void ReloadData()
    {
        if (!StopProcessing)
        {
            TreeView.Nodes.Clear();
            TreeView.EnableViewState = false;

            // Add root node from provider
            if (ProviderObject.RootNode != null)
            {
                mRootNode = CreateNode(ProviderObject.RootNode);

                RaiseOnNodeCreated(ProviderObject.RootNode, ref mRootNode);
                TreeView.Nodes.Add(mRootNode);
            }
        }
    }

    /// <summary>
    /// Creates node.
    /// </summary>
    /// <param name="uniNode">Node to create</param>
    protected TreeNode CreateNode(UniTreeNode uniNode)
    {
        var data = (Metric)uniNode.ItemData;
        if (data == null)
        {
            return null;
        }

        // Get data
        bool hasChildren = ValidationHelper.GetBoolean(data.MetricHasChildren, false);

        // Node ID
        int nodeId = ValidationHelper.GetInteger(data.MetricId, 0);

        // Node value
        string nodeValue = nodeId.ToString();

        // Display name
        string displayName = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(ValidationHelper.GetString(data.MetricDisplayName, "")));

        // Path
        string nodePath = HTMLHelper.HTMLEncode(ValidationHelper.GetString(data.MetricPath, "")).ToLowerCSafe();

        // Parent ID
        int parentId = 0;
        if (data.MetricParent != null)
        {
            parentId = ValidationHelper.GetInteger(data.MetricParent.MetricId, 0);
        }

        // Set text
        string text;
        string selectedItem = ValidationHelper.GetString(SelectedItem, "");

        if (nodeValue.EqualsCSafe(selectedItem, true))
        {
            text = ReplaceMacros(SelectedNodeTemplate, nodeId, hasChildren.ToInteger(0), displayName, parentId);
        }
        else
        {
            text = ReplaceMacros(NodeTemplate, nodeId, hasChildren.ToInteger(0), displayName, parentId);
        }

        TreeNode node = new TreeNode
        {
            Value = nodeValue,
            Text = text
        };

        // Set populate node automatically
        if (hasChildren)
        {
            node.PopulateOnDemand = true;
        }

        // Handle expand path
        if (!nodePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            nodePath += "/";
        }

        // Path expanded by user
        if (selectedPath.StartsWithCSafe(nodePath) && (selectedPath != nodePath))
        {
            node.Expanded = true;
        }

        return node;
    }

    /// <summary>
    /// Replaces all macros in template by values.
    /// </summary>
    /// <param name="template">Template with macros</param>
    /// <param name="itemID">Item ID</param>
    /// <param name="childCount">Child count</param>
    /// <param name="nodeName">Node name</param>
    /// <param name="parentNodeID">Parent item ID</param>
    /// <param name="icon">Icon</param>
    /// <param name="objectType">Object type</param>
    /// <param name="parameter">Additional parameter</param>
    public string ReplaceMacros(string template, int itemID, int childCount, string nodeName, int parentNodeID)
    {
        template = template.Replace("##NODEID##", itemID.ToString());
        template = template.Replace("##NODEJAVA##", ScriptHelper.GetString(nodeName));
        template = template.Replace("##NODECHILDNODESCOUNT##", childCount.ToString());
        if (Localize)
        {
            nodeName = ResHelper.LocalizeString(nodeName);
        }
        template = template.Replace("##NODENAME##", nodeName);
        template = template.Replace("##PARENTNODEID##", parentNodeID.ToString());

        return template;
    }

    #endregion Methods

    #region "IPostBackEventHandler Members"

    /// <summary>
    /// Raises event postback event.
    /// </summary>
    public void RaisePostBackEvent(string eventArgument)
    {
        return;
    }

    #endregion "IPostBackEventHandler Members"
}