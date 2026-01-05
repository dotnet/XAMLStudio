// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;
using XamlStudio.ViewModels;
using muxc = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Explorer : Page
    {
        public MainViewModel MainViewModel { get; set; }

        private static readonly string[] IgnoreFileTypes = new string[] { ".csproj", ".sln", ".user", ".cs", ".appxmanifest" };

        private static readonly string[] IgnoreFolders = new string[] { "bin", "obj" };

        public Explorer()
        {
            InitializeComponent();

            Loaded += Explorer_Loaded;
        }

        private void Explorer_Loaded(object sender, RoutedEventArgs e)
        {
            WorkspaceTreeView.RootNodes.Clear();
            if (MainViewModel?.WorkspaceFolders.Count > 0)
            {
                // TODO: May already be initialized?
                InitializeTreeView(MainViewModel.WorkspaceFolders.FirstOrDefault());
            }

            MainViewModel.WorkspaceFolders.CollectionChanged += WorkspaceFolders_CollectionChanged;
        }

        private void WorkspaceFolders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WorkspaceTreeView.RootNodes.Clear();
            if (MainViewModel.WorkspaceFolders.Count > 0)
            {
                InitializeTreeView(MainViewModel.WorkspaceFolders.FirstOrDefault());
            }
        }

        private void InitializeTreeView(FolderLocation folder)
        {
            // FYI A TreeView can have more than 1 root node. TODO: Support multiple workspaces? Or would we just separate outside the TreeView anyway?
            muxc.TreeViewNode mainNode = new muxc.TreeViewNode();
            mainNode.Content = folder.BackingFolder;
            mainNode.IsExpanded = true;
            mainNode.HasUnrealizedChildren = true;
            FillTreeNode(mainNode);

            WorkspaceTreeView.RootNodes.Add(mainNode);
        }

        // From: https://docs.microsoft.com/en-us/windows/apps/design/controls/tree-view
        private async void FillTreeNode(muxc.TreeViewNode node)
        {
            // Get the contents of the folder represented by the current tree node.
            // Add each item as a new child node of the node that's being expanded.

            // Only process the node if it's a folder and has unrealized children.
            StorageFolder folder = null;

            if (node.Content is StorageFolder && node.HasUnrealizedChildren == true)
            {
                folder = node.Content as StorageFolder;
            }
            else
            {
                // The node isn't a folder, or it's already been filled.
                return;
            }

            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();

            if (itemsList.Count == 0)
            {
                // The item is a folder, but it's empty. Leave HasUnrealizedChildren = true so
                // that the chevron appears, but don't try to process children that aren't there.
                return;
            }

            foreach (var item in itemsList)
            {
                var newNode = new muxc.TreeViewNode();
                newNode.Content = item;

                if (item is StorageFolder)
                {
                    // If the item is a folder, set HasUnrealizedChildren to true.
                    // This makes the collapsed chevron show up.
                    newNode.HasUnrealizedChildren = true;

                    // Skip unwanted directories
                    if (IgnoreFolders.Contains(item.Name.ToLower()))
                    {
                        continue;
                    }
                }
                else if (item is StorageFile file)
                {
                    // Item is StorageFile. No processing needed for this scenario.
                    // Skip unwanted file types
                    if (IgnoreFileTypes.Contains(file.FileType.ToLower()))
                    {
                        continue;
                    }
                }

                node.Children.Add(newNode);
            }

            // Children were just added to this node, so set HasUnrealizedChildren to false.
            node.HasUnrealizedChildren = false;
        }

        private void WorkspaceTreeView_Expanding(muxc.TreeView sender, muxc.TreeViewExpandingEventArgs args)
        {
            if (args.Node.HasUnrealizedChildren)
            {
                FillTreeNode(args.Node);
            }
        }

        private void WorkspaceTreeView_Collapsed(muxc.TreeView sender, muxc.TreeViewCollapsedEventArgs args)
        {
            args.Node.Children.Clear();
            args.Node.HasUnrealizedChildren = true;
        }

        private void WorkspaceTreeView_ItemInvoked(muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as muxc.TreeViewNode;

            // TODO: Can we get here accidently without a backing folder?
            var workspacePathLength = MainViewModel.WorkspaceFolders.First().BackingFolder.Path.Length;

            switch (node.Content)
            {
                case StorageFolder folder:
                    node.IsExpanded = !node.IsExpanded;
                    break;
                case StorageFile file when ExplorerItemTemplateSelector.DataFileTypes.Contains(file.FileType.ToLower()):
                    // Insert the d:DataContext="{d:DesignData /SampleData/XAMLing.json}" attribute
                    // TODO: Should we instead explicitly update/insert this into root tag?
                    WeakReferenceMessenger.Default.Send<InsertTextMessage>(new($"d:DataContext=\"{{d:DesignData {file.Path.Replace('\\', '/').Substring(workspacePathLength)}}}\""));
                    Analytics.TrackEvent("Explorer_Workspace_InsertDataContext");
                    break;
                case StorageFile file when ExplorerItemTemplateSelector.ImageFileTypes.Contains(file.FileType.ToLower()):
                    // Insert image tag into document
                    WeakReferenceMessenger.Default.Send<InsertTextMessage>(new($"<Image Source=\"{file.Path.Replace('\\', '/').Substring(workspacePathLength)}\" />"));
                    Analytics.TrackEvent("Explorer_Workspace_InsertImage");
                    break;
                case StorageFile file when ExplorerItemTemplateSelector.XamlFileTypes.Contains(file.FileType.ToLower()):
                    // Find top-level node.
                    var parent = node;
                    while (parent.Parent?.Content != null) // There seems to be an extra TreeViewNode, so look at content as indicator.
                    {
                        parent = parent.Parent;
                    }

                    MainViewModel.OpenFileFromWorkspace(file, parent.Content as StorageFolder);
                    break;
            }
        }
    }

    public class ExplorerItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultFileTemplate { get; set; }
        public DataTemplate DataFileTemplate { get; set; }
        public DataTemplate ImageFileTemplate { get; set; }
        public DataTemplate XamlFileTemplate { get; set; }
        public DataTemplate FolderTemplate { get; set; }

        public static readonly string[] DataFileTypes = new string[] { ".json" }; // TODO: , ".xml" };
        public static readonly string[] ImageFileTypes = new string[] { ".png", ".gif", ".svg", ".jpg", ".jpeg", ".bmp", ".tiff", ".ico" };
        public static readonly string[] XamlFileTypes = new string[] { ".xaml" };

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var node = (muxc.TreeViewNode)item;

            return node.Content switch
            {
                StorageFolder folder => FolderTemplate,
                StorageFile file when DataFileTypes.Contains(file.FileType.ToLower()) => DataFileTemplate,
                StorageFile file when ImageFileTypes.Contains(file.FileType.ToLower()) => ImageFileTemplate,
                StorageFile file when XamlFileTypes.Contains(file.FileType.ToLower()) => XamlFileTemplate,
                _ => DefaultFileTemplate
            };
        }
    }
}
