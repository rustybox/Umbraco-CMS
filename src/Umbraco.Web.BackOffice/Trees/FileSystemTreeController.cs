﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected FileSystemTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory
        )
            : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected abstract IFileSystem FileSystem { get; }
        protected IMenuItemCollectionFactory MenuItemCollectionFactory { get; }
        protected abstract string[] Extensions { get; }
        protected abstract string FileIcon { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFileNode(ref TreeNode treeNode) { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode) {
            // TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
        {
            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
                ? WebUtility.UrlDecode(id).TrimStart("/")
                : "";

            var directories = FileSystem.GetDirectories(path);

            var nodes = new TreeNodeCollection();
            foreach (var directory in directories)
            {
                var hasChildren = FileSystem.GetFiles(directory).Any() || FileSystem.GetDirectories(directory).Any();

                var name = Path.GetFileName(directory);
                var node = CreateTreeNode(WebUtility.UrlEncode(directory), path, queryStrings, name, "icon-folder", hasChildren);
                OnRenderFolderNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            var files = FileSystem.GetFiles(path).Where(x =>
            {
                var extension = Path.GetExtension(x);

                if (Extensions.Contains("*"))
                    return true;

                return extension != null && Extensions.Contains(extension.Trim('.'), StringComparer.InvariantCultureIgnoreCase);
            });

            foreach (var file in files)
            {
                var withoutExt = Path.GetFileNameWithoutExtension(file);
                if (withoutExt.IsNullOrWhiteSpace()) continue;

                var name = Path.GetFileName(file);
                var node = CreateTreeNode(WebUtility.UrlEncode(file), path, queryStrings, name, FileIcon, false);
                OnRenderFileNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            return nodes;
        }

        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any children
            root.HasChildren = GetTreeNodes(Constants.System.RootString, queryStrings).Any();
            return root;
        }

        protected virtual MenuItemCollection GetMenuForRootNode(FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFolder(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

            var hasChildren = FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

            //We can only delete folders if it doesn't have any children (folders or files)
            if (hasChildren == false)
            {
                //delete action
                menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true);
            }

            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFile(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //if it's not a directory then we only allow to delete the item
            menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);

            return menu;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
        {
            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.RootString)
            {
                return GetMenuForRootNode(queryStrings);
            }

            var menu = MenuItemCollectionFactory.Create();

            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
                ? WebUtility.UrlDecode(id).TrimStart("/")
                : "";

            var isFile = FileSystem.FileExists(path);
            var isDirectory = FileSystem.DirectoryExists(path);

            if (isDirectory)
            {
                return GetMenuForFolder(path, queryStrings);
            }

            return isFile ? GetMenuForFile(path, queryStrings) : menu;
        }
    }
}
