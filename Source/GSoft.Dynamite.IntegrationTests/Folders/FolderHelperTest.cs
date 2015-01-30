using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Pages;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Folders
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="FolderHelper"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class FolderHelperTest
    {
        #region Ensure should create folder hierarchy within the specified library

        /// <summary>
        /// Validates that hierarchy of subfolders is created in Pages library
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInPagesLibrary_AndNotEnsuredYet_ShouldCreateFolderHierarchy()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                            }
                        },
                        new FolderInfo("somelevel2path alt")
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();

                    // Act
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Assert
                    Assert.IsTrue(pagesLibrary.EnableFolderCreation);
                    Assert.AreEqual(2, pagesLibrary.RootFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms").Count());
                    Assert.AreEqual(3, pagesLibrary.Folders.Count);   // all created folders, exclusing the special Forms folder at library root

                    var lvl2Folder = pagesLibrary.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    pagesLibrary.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }

        /// <summary>
        /// Validates that hierarchy of subfolders is creates in Documents library
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInDocumentLibrary_AndNotEnsuredYet_ShouldCreateFolderHierarchy()
        {
            using (var testScope = SiteTestScope.TeamSite())
            {
                // Arrange
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                            }
                        },
                        new FolderInfo("somelevel2path alt")
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    var documentLibrary = testScope.SiteCollection.RootWeb.Lists["Documents"];

                    // Act
                    folderHelper.EnsureFolderHierarchy(documentLibrary, rootFolderInfo);

                    // Assert
                    Assert.IsTrue(documentLibrary.EnableFolderCreation);
                    Assert.AreEqual(2, documentLibrary.RootFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms").Count());
                    Assert.AreEqual(3, documentLibrary.Folders.Count);   // all created folders, exclusing the special Forms folder at library root

                    var lvl2Folder = documentLibrary.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    documentLibrary.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }

        /// <summary>
        /// Validates that hierarchy of subfolders is created in a normal generic list
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInList_AndNotEnsuredYet_ShouldCreateFolderHierarchy_AndEnableFoldersOnList()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                            }
                        },
                        new FolderInfo("somelevel2path alt")
                    }
                };

                var listInfo = new ListInfo("somelistparth", "ListNameKey", "ListDescrKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Assert
                    Assert.IsTrue(list.EnableFolderCreation);
                    Assert.AreEqual(2, list.RootFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms" && folder.Name != "Item" && folder.Name != "Attachments").Count());
                    Assert.AreEqual(0, list.Folders.Count);   // Since this isn't a doclib, Folders array will always be empty (gotta use RootFolder.SubFolders)

                    var lvl2Folder = list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }

        #endregion
        
        #region Ensure should update the folder hierarchy if run more than once

        /// <summary>
        /// Validates that when the hierarchy is modified, re-ensuring adds the missing folders
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenUpdating_ShouldUpdateFolderHierarchyWithAddedSubfolders()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl3 = new FolderInfo("level3");
                var folderInfoLvl2 = new FolderInfo("somelevel2path");
                var folderInfoLvl2Alt = new FolderInfo("somelevel2path alt");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2      // just add one of the three subfolders for now
                    }
                };

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Ensure the initial (partial) folder hierarchy
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Modify the folder hierarchy to add the 2 missing subfolders
                    folderInfoLvl2.Subfolders.Add(folderInfoLvl3);
                    rootFolderInfo.Subfolders.Add(folderInfoLvl2Alt);

                    // Act: re-ensure the hierarchy with update definition
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Assert
                    Assert.IsTrue(list.EnableFolderCreation);
                    Assert.AreEqual(2, list.RootFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms" && folder.Name != "Item" && folder.Name != "Attachments").Count());
                    Assert.AreEqual(0, list.Folders.Count);   // Since this isn't a doclib, Folders array will always be empty (gotta use RootFolder.SubFolders)

                    var lvl2Folder = list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }
        
        /// <summary>
        /// Validates that when the hierarchy is modified, re-ensuring isn't overzealous and doesn't attemps to
        /// delete any already existing folders.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenUpdating_AndDefinitionChangedToRemoveSubFolder_ShouldNeverDeleteExistingSubfolder()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                            }
                        },
                        new FolderInfo("somelevel2path alt")
                    }
                };

                var listInfo = new ListInfo("somelistparth", "ListNameKey", "ListDescrKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Start by ensuring a full folder hierarchy
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Edit the definition to remove some subfolder
                    rootFolderInfo.Subfolders = rootFolderInfo.Subfolders.Where(f => f.Name != "somelevel2path").ToList();
                    rootFolderInfo.Subfolders.First().Subfolders.Clear();

                    // Act: re-ensure to determine if the existing (and now superfluous) folders stay in place
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                    
                    // Assert (nothing in the initial tree should've been removed)
                    Assert.IsTrue(list.EnableFolderCreation);
                    Assert.AreEqual(2, list.RootFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms" && folder.Name != "Item" && folder.Name != "Attachments").Count());
                    Assert.AreEqual(0, list.Folders.Count);   // Since this isn't a doclib, Folders array will always be empty (gotta use RootFolder.SubFolders)

                    var lvl2Folder = list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    list.RootFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }

        #endregion

        #region Ensure should returned the created (or updated) root SPFolder instance

        /// <summary>
        /// Validates that a properly updated instance of the list root folder is returned from EnsureFolderHierarchy
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_ShouldReturnListRootFolderInstance()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                            }
                        },
                        new FolderInfo("somelevel2path alt")
                    }
                };

                var listInfo = new ListInfo("somelistparth", "ListNameKey", "ListDescrKey")
                    {
                        ListTemplateInfo = BuiltInListTemplates.DocumentLibrary
                    };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act
                    SPFolder sharePointFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Assert
                    Assert.AreEqual(sharePointFolder.UniqueId, list.RootFolder.UniqueId);

                    Assert.AreEqual(2, sharePointFolder.SubFolders.Cast<SPFolder>().Where(folder => folder.Name != "Forms").Count());

                    var lvl2Folder = sharePointFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path");
                    sharePointFolder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "somelevel2path alt");

                    Assert.AreEqual(1, lvl2Folder.SubFolders.Count);
                    lvl2Folder.SubFolders.Cast<SPFolder>().Single(f => f.Name == "level3");
                }
            }
        }

        #endregion

        #region Specified pages should be created if list is a page library

        /// <summary>
        /// Validates that folders' pages are provisioned
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInPagesLibrary_AndNotEnsuredYet_ShouldCreatePublishingPages()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                var articleLeftPageLayout = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");
                var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Subfolders = new List<FolderInfo>()
                            {
                                new FolderInfo("level3")
                                {
                                    Pages = new List<PageInfo>()
                                    {
                                        new PageInfo("Hello-lvl-3-page-path", articleLeftPageLayout)
                                        {
                                            FieldValues = new List<FieldValueInfo>()
                                            {
                                                new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 3!!! My HTML rocks!!!</p></div>")
                                            }
                                        },
                                        new PageInfo("Hello-lvl-3-page-path-bis", articleLeftPageLayout)
                                        {
                                            FieldValues = new List<FieldValueInfo>()
                                            {
                                                new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 3 AGAIN!!! My HTML rocks!!!</p></div>")
                                            }
                                        },
                                    }
                                }
                            },
                            Pages = new List<PageInfo>()
                            {
                                new PageInfo("Hello-lvl-2-page-path", articleLeftPageLayout)
                                {
                                    FieldValues = new List<FieldValueInfo>()
                                    {
                                        new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 2!!! My HTML rocks!!!</p></div>")
                                    }
                                }
                            }
                        }
                    },
                    Pages = new List<PageInfo>()
                    {
                        new PageInfo("Hello-root-page-path", welcomePageLayout)
                        {
                            FieldValues = new List<FieldValueInfo>()
                            {
                                new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>My HTML rocks!!!</p></div>")
                            }
                        }
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();

                    // Act
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Assert
                    var publishingSite = new PublishingSite(pagesLibrary.ParentWeb.Site);
                    var publishingWeb = PublishingWeb.GetPublishingWeb(pagesLibrary.ParentWeb);
                    var recursivePagesQuery = new SPQuery() { ViewAttributes = "Scope=\"Recursive\"" };
                    var publishingPages = publishingWeb.GetPublishingPages(recursivePagesQuery);

                    Assert.AreEqual(4, publishingPages.Cast<PublishingPage>().Where(p => p.Name.StartsWith("Hello")).Count());

                    var ensuredWelcomePage = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-root-page-path"));
                    Assert.IsTrue(ensuredWelcomePage.ContentType.Id.IsChildOf(new SPContentTypeId(welcomePageLayout.AssociatedContentTypeId)));

                    var ensuredLevel2Page = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-lvl-2-page-path"));
                    Assert.IsTrue(ensuredLevel2Page.ContentType.Id.IsChildOf(new SPContentTypeId(articleLeftPageLayout.AssociatedContentTypeId)));
                    Assert.AreEqual("<div><p>Hi LVL 2!!! My HTML rocks!!!</p></div>", ensuredLevel2Page.ListItem[PublishingFields.PublishingPageContent.Id].ToString());
                }
            }
        }

        ////[TestMethod]
        ////public void EnsureFolderHierarchy_WhenNotInPagesLibrary_AndPageInfosAreDefined_ShouldThrowException()
        ////{
        ////}

        ////[TestMethod]
        ////public void EnsureFolderHierarchy_WhenNotInPulishingSite_AndPageInfosAreDefined_ShouldThrowException()
        ////{
        ////}

        #endregion

        #region Pages should be updated during re-ensure

        //// TODO: write 'em tests

        #endregion

        #region Folder MetadataDefaults (field default values) should be created and/or updated during Ensure

        //// TODO: write 'em tests

        #endregion

        #region Root folder and sub-folders' UniqueContentTypeOrder should be ensured through the hierarchy

        //// TODO: write 'em tests

        #endregion
    }
}
