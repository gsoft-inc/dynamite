using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
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
        /// Validates that when the hierarchy is modified, re-ensuring isn't overzealous and doesn't attempt to
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

        /// <summary>
        /// Validate that attempting to provision a publishing page outside the dedicated Pages library throws an exception
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenNotInPagesLibrary_AndPageInfosAreDefined_ShouldThrowException()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                var rootFolderInfo = new FolderInfo("somepath")
                {
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

                var listInfo = new ListInfo("somelistparth", "ListNameKey", "ListDescrKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create a custom library (not a Pages lib)
                    var customLibrary = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Act
                    try
                    {
                        folderHelper.EnsureFolderHierarchy(customLibrary, rootFolderInfo);
                        Assert.Fail("Should've thrown argument exception");
                    }
                    catch (ArgumentException e)
                    {
                        // Assert
                        Assert.IsTrue(e.Message.Contains("Publishing pages cannot be provisionned outside of the Pages library."));
                    }                    
                }
            }
        }

        /// <summary>
        /// Validate that attempting to provision a publishing page outside of a Publishing site throws an exception
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenNotInPulishingSite_AndPageInfosAreDefined_ShouldThrowException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                var rootFolderInfo = new FolderInfo("somepath")
                {
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

                var listInfo = new ListInfo("Pages", "PagesLibNameKey", "PagesLibDescrKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.Pages
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create a custom library (not a Pages lib)
                    var customLibrary = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Act
                    try
                    {
                        folderHelper.EnsureFolderHierarchy(customLibrary, rootFolderInfo);
                        Assert.Fail("Should've thrown argument exception");
                    }
                    catch (ArgumentException e)
                    {
                        // Assert
                        Assert.IsTrue(e.Message.Contains("Publishing pages cannot be provisionned outside of a Publishing web (choose the Publishing Site or Enterprise Wiki site definition)."));
                    }
                }
            }
        }

        #endregion

        #region Pages should be updated during re-ensure

        /// <summary>
        /// Validates that a folder's pages' property values are updated when re-ensured
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenPagesAlreadyEnsured_ShouldUpdatePublishingPageFieldValues()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                var articleLeftPageLayout = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");
                var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                var level1PageInfo = new PageInfo("Hello-root-page-path", welcomePageLayout)
                    {
                        FieldValues = new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>My HTML rocks!!!</p></div>")
                        }
                    };

                var level2PageInfo = new PageInfo("Hello-lvl-2-page-path", articleLeftPageLayout)
                    {
                        FieldValues = new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 2!!! My HTML rocks!!!</p></div>")
                        }
                    };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Pages = new List<PageInfo>()
                            {
                                level2PageInfo
                            }
                        }
                    },
                    Pages = new List<PageInfo>()
                    {
                        level1PageInfo
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();

                    // Ensure the hierarchy a first time with the initial page values
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Edit the PageInfos slightly
                    level1PageInfo.FieldValues[0].Value = "Level 1 updated HTML value";
                    level2PageInfo.FieldValues[0].Value = "Level 2 updated HTML value";

                    // Act: re-ensure the same hierarchy
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Assert: page instances should've been updated
                    var publishingSite = new PublishingSite(pagesLibrary.ParentWeb.Site);
                    var publishingWeb = PublishingWeb.GetPublishingWeb(pagesLibrary.ParentWeb);
                    var recursivePagesQuery = new SPQuery() { ViewAttributes = "Scope=\"Recursive\"" };
                    var publishingPages = publishingWeb.GetPublishingPages(recursivePagesQuery);

                    Assert.AreEqual(2, publishingPages.Cast<PublishingPage>().Where(p => p.Name.StartsWith("Hello")).Count());

                    var ensuredWelcomePage = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-root-page-path"));
                    Assert.AreEqual("Level 1 updated HTML value", ensuredWelcomePage.ListItem[PublishingFields.PublishingPageContent.Id]);
                    Assert.AreEqual(2, ensuredWelcomePage.ListItem.Versions.Count);

                    var ensuredLevel2Page = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-lvl-2-page-path"));
                    Assert.AreEqual("Level 2 updated HTML value", ensuredLevel2Page.ListItem[PublishingFields.PublishingPageContent.Id]);
                    Assert.AreEqual(2, ensuredLevel2Page.ListItem.Versions.Count);
                }
            }
        }

        /// <summary>
        /// Validates that a renamed PageInfo title causes a new page to be created (and the old page to be left alone
        /// and not overzealously deleted)
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenPagesAlreadyEnsured_AndPageInfoIsRenamed_ShouldNotDeleteExistingPage_AndCreateABrandNewPage()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                var articleLeftPageLayout = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");
                var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                var level1PageInfo = new PageInfo("Hello-root-page-path", welcomePageLayout)
                {
                    FieldValues = new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>My HTML rocks!!!</p></div>")
                        }
                };

                var level2PageInfo = new PageInfo("Hello-lvl-2-page-path", articleLeftPageLayout)
                {
                    FieldValues = new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 2!!! My HTML rocks!!!</p></div>")
                        }
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Pages = new List<PageInfo>()
                            {
                                level2PageInfo
                            }
                        }
                    },
                    Pages = new List<PageInfo>()
                    {
                        level1PageInfo
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();

                    // Ensure the hierarchy a first time with the initial page values
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Edit the PageInfos slightly
                    level1PageInfo.FileName = "Hello-welcome-page-renamed";
                    level2PageInfo.FileName = "Hello-level-2-page-renamed";

                    // Act: re-ensure the same hierarchy
                    folderHelper.EnsureFolderHierarchy(pagesLibrary, rootFolderInfo);

                    // Assert: new pages should've been created and the old ones should still be there
                    var publishingSite = new PublishingSite(pagesLibrary.ParentWeb.Site);
                    var publishingWeb = PublishingWeb.GetPublishingWeb(pagesLibrary.ParentWeb);
                    var recursivePagesQuery = new SPQuery() { ViewAttributes = "Scope=\"Recursive\"" };
                    var publishingPages = publishingWeb.GetPublishingPages(recursivePagesQuery);

                    Assert.AreEqual(4, publishingPages.Cast<PublishingPage>().Where(p => p.Name.StartsWith("Hello")).Count());

                    var ensuredWelcomePage = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-root-page-path"));
                    var ensuredLevel2Page = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-lvl-2-page-path"));

                    var extraEnsuredWelcomePage = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-welcome-page-renamed"));
                    var extraEnsuredLevel2Page = publishingPages.Cast<PublishingPage>().Single(p => p.Name.StartsWith("Hello-level-2-page-renamed"));
                }
            }
        }

        #endregion

        #region Document Library's folder MetadataDefaults (field default values) should be created and/or updated during Ensure

        /// <summary>
        /// Validates that ensuring MetadataDefaults on a non-doclib SPList throws an error
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureFolderHierarchy_WhenFolderIsNotInDocumentLibrary_ShouldThrowException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl2 = new FolderInfo("somelevel2path");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2      // just add one of the three subfolders for now
                    }
                };

                DateTimeFieldInfo dateTimeFieldInfoEmpty = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{CD09FF50-0CE3-40AE-90ED-AC68961FA980}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey");

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    // By default, a GenericList/CustomList will be created
                    FieldDefinitions = new List<IFieldInfo>() { dateTimeFieldInfoEmpty }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Define some folder defaults on the subfolder
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1990, 5, 20))
                    };

                    // Act
                    folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Assert: should throw!!!
                }
            }
        }

        /// <summary>
        /// Validates that document library sub-folder MetadataDefaults get initialized if DefaultValues are specified
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenDocLibFolderDefaultValuesAreSpecified_AndFirstEnsure_AndInSubFolder_ShouldInitializeFolderMetadataDefaults()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl2 = new FolderInfo("somelevel2path");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2      // just add one of the three subfolders for now
                    }
                };

                DateTimeFieldInfo dateTimeFieldInfoEmpty = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{CD09FF50-0CE3-40AE-90ED-AC68961FA980}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                    {
                        ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                        FieldDefinitions = new List<IFieldInfo>() { dateTimeFieldInfoEmpty, dateTimeFieldInfoFormula }
                    };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = (SPDocumentLibrary)listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Define some folder defaults on the subfolder
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1990, 5, 20)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2050, 9, 18))
                    };

                    // Act
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    var secondLevelEnsuredFolder = ensuredRootFolder.SubFolders["somelevel2path"];

                    var itemInSecondLevelFolder = secondLevelEnsuredFolder.Files.Add("SomeLevel2File.txt", new byte[0]);
                    itemInSecondLevelFolder.Item.Update();

                    // Assert

                    // In root folder, the field definitions should apply
                    Assert.IsNull(itemInRootFolder.Item[dateTimeFieldInfoEmpty.Id]);
                    Assert.AreEqual(DateTime.Today, (DateTime)itemInRootFolder.Item[dateTimeFieldInfoFormula.Id]);

                    // In second-level folder, the folder's MetadataDefaults should apply
                    Assert.AreEqual(new DateTime(1990, 5, 20), (DateTime)itemInSecondLevelFolder.Item[dateTimeFieldInfoEmpty.Id]);  // this also validates that LocalTime->UTCTime conversion is correct
                    Assert.AreEqual(new DateTime(2050, 9, 18), (DateTime)itemInSecondLevelFolder.Item[dateTimeFieldInfoFormula.Id]);
                }
            }
        }

        /// <summary>
        /// Validates that document library root-folder MetadataDefaults get initialized if DefaultValues are specified
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenDocLibFolderDefaultValuesAreSpecified_AndFirstEnsure_AndInRootFolder_ShouldInitializeFolderMetadataDefaults()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl2 = new FolderInfo("somelevel2path");

                DateTimeFieldInfo dateTimeFieldInfoEmpty = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{CD09FF50-0CE3-40AE-90ED-AC68961FA980}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1990, 5, 20)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2050, 9, 18))
                    }
                };

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() { dateTimeFieldInfoEmpty, dateTimeFieldInfoFormula }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = (SPDocumentLibrary)listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();
                    
                    // Act
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    // Assert

                    // In root folder, both fields' metadata defaults should apply
                    Assert.AreEqual(new DateTime(1990, 5, 20), (DateTime)itemInRootFolder.Item[dateTimeFieldInfoEmpty.Id]);
                    Assert.AreEqual(new DateTime(2050, 9, 18), (DateTime)itemInRootFolder.Item[dateTimeFieldInfoFormula.Id]);
                }
            }
        }

        /// <summary>
        /// Validates that folder MetadataDefaults get updated if DefaultValues are specified
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenDocLibFolderDefaultValuesAreSpecified_AndRepeatEnsure_ShouldUpdateFolderMetadataDefaults()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl2 = new FolderInfo("somelevel2path");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2      // just add one of the three subfolders for now
                    }
                };

                DateTimeFieldInfo dateTimeFieldInfoEmpty = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{CD09FF50-0CE3-40AE-90ED-AC68961FA980}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() { dateTimeFieldInfoEmpty, dateTimeFieldInfoFormula }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = (SPDocumentLibrary)listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Define some folder defaults on the subfolder
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1990, 5, 20)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2050, 9, 18))
                    };

                    // Ensure a first time with the initial defauls (initially, only the 2nd level folder has defaults)
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Add some defaults to root folder definition
                    rootFolderInfo.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1977, 3, 3)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(1978, 4, 4))
                    };

                    // Change the defaults on the level-2 folder definition
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1999, 1, 1)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2000, 12, 31))
                    };

                    // Act: re-ensure the folder hierarchy with updated Defaults settings
                    ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    var secondLevelEnsuredFolder = ensuredRootFolder.SubFolders["somelevel2path"];

                    var itemInSecondLevelFolder = secondLevelEnsuredFolder.Files.Add("SomeLevel2File.txt", new byte[0]);
                    itemInSecondLevelFolder.Item.Update();

                    // Assert

                    // In root folder, the field definitions should apply
                    Assert.AreEqual(new DateTime(1977, 3, 3), (DateTime)itemInRootFolder.Item[dateTimeFieldInfoEmpty.Id]);
                    Assert.AreEqual(new DateTime(1978, 4, 4), (DateTime)itemInRootFolder.Item[dateTimeFieldInfoFormula.Id]);

                    // In second-level folder, the folder's MetadataDefaults should apply
                    Assert.AreEqual(new DateTime(1999, 1, 1), (DateTime)itemInSecondLevelFolder.Item[dateTimeFieldInfoEmpty.Id]);  // this also validates that LocalTime->UTCTime conversion is correct
                    Assert.AreEqual(new DateTime(2000, 12, 31), (DateTime)itemInSecondLevelFolder.Item[dateTimeFieldInfoFormula.Id]);
                }
            }
        }

        /// <summary>
        /// Validates that folder MetadataDefaults are removed if DefaultValues are cleared between two EnsureFolderHierarchy runs
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenDocLibFolderDefaultValuesAreRemoved_AndRepeatEnsure_ShouldUpdateFolderAndDropMetadataDefaults()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var folderInfoLvl2 = new FolderInfo("somelevel2path");

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2      // just add one of the three subfolders for now
                    }
                };

                DateTimeFieldInfo dateTimeFieldInfoEmpty = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{CD09FF50-0CE3-40AE-90ED-AC68961FA980}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                var listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() { dateTimeFieldInfoEmpty, dateTimeFieldInfoFormula }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = (SPDocumentLibrary)listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Define some folder defaults on the root and subfolder
                    rootFolderInfo.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1977, 3, 3)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(1978, 4, 4))
                    };

                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(dateTimeFieldInfoEmpty, new DateTime(1990, 5, 20)),
                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2050, 9, 18))
                    };

                    // Ensure a first time with the initial defauls (initially, both root and the 2nd level folder have defaults)
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Remove defaults on root folder definition
                    rootFolderInfo.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                    };

                    // Remove defaults on the level-2 folder definition
                    folderInfoLvl2.FieldDefaultValues = null;

                    // Act: re-ensure the folder hierarchy with cleared Defaults settings
                    ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    var secondLevelEnsuredFolder = ensuredRootFolder.SubFolders["somelevel2path"];

                    var itemInSecondLevelFolder = secondLevelEnsuredFolder.Files.Add("SomeLevel2File.txt", new byte[0]);
                    itemInSecondLevelFolder.Item.Update();

                    // Assert

                    // In root folder, the field definitions should apply
                    Assert.IsNull(itemInRootFolder.Item[dateTimeFieldInfoEmpty.Id]);
                    Assert.AreEqual(DateTime.Today, (DateTime)itemInRootFolder.Item[dateTimeFieldInfoFormula.Id]);

                    // In second-level folder, the list's field definitions should apply as well
                    Assert.IsNull(itemInSecondLevelFolder.Item[dateTimeFieldInfoEmpty.Id]);
                    Assert.AreEqual(DateTime.Today, (DateTime)itemInSecondLevelFolder.Item[dateTimeFieldInfoFormula.Id]);
                }
            }
        }

        /// <summary>
        /// Validates exception is thrown when attempting to set doclib  folder column default on a boolean field which already has
        /// a TRUE value as its SPField's DefaultValue.
        /// This is a weird edge-case where SharePoint's folder column defaults logic breaks down.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void EnsureFolderHierarchy_WhenAttemptingFALSEDefaultFolderValueInDocLib_AndSPFieldAlreadHasTRUEDefaultValue_ShouldExplodeAndWarnYouThatThisWeirdEdgeCaseIsNotSupported()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                   "TestInternalNameBoolTrue",
                   new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                   "NameKeyBoolTrue",
                   "DescriptionKeyBoolTrue",
                   "GroupKey")
                {
                    DefaultValue = true
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() { boolFieldInfoDefaultTrue }
                };

                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                    FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(boolFieldInfoDefaultTrue, false) }
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    
                    // Create the test doc lib
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act: this should throw NotSupportedException
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                }
            }
        }

        /// <summary>
        /// Validated that attempting to ensure doclib folder column default values on Lookup fields fails and warns the developer
        /// that this behavior is not supported.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenAttemptingToSetLookupFolderDefaultValueInDocLib_ShouldFailWithNotSupportedException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                   "TestInternalNameLookup",
                   new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                   "NameKeyLookup",
                   "DescriptionKey",
                   "GroupKey")
                {
                    // ShowField should be Title by default
                };

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKeyLookupAlt",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyLookupMulti",
                    "DescriptionKeyMulti",
                    "GroupKey")
                {
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() 
                    { 
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo
                    }
                };

                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the lookup list
                    ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the test doc lib
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act 1: First lookup field - this should throw NotSupportedException
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(lookupFieldInfo, new LookupValue(1, "Test Item 1")) };
                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    { 
                    }

                    // Act 2: Alternate lookup field - this should throw NotSupportedException
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(lookupFieldInfo, new LookupValue(2, "2")) };
                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    { 
                    }

                    // Act 3: First lookup field - this should throw NotSupportedException
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(lookupMultiFieldInfo, new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") }) 
                    };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    { 
                    }
                }
            }
        }

        /// <summary>
        /// Validated that attempting to ensure doclib folder column default values on User fields fails and warns the developer
        /// that this behavior is not supported.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenAttemptingToSetUserFolderDefaultValueInDocLib_ShouldFailWithNotSupportedException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\" + Environment.UserName);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = new List<IFieldInfo>() 
                    { 
                        userFieldInfo,
                        userMultiFieldInfo
                    }
                };

                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the test doc lib
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act 1: User field - this should throw NotSupportedException
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(userFieldInfo, new UserValue(ensuredUser1)) };
                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    { 
                    }

                    // Act 2: User Multi field - this should throw NotSupportedException
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(userMultiFieldInfo, new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) }) };
                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    { 
                    }
                }
            }
        }

        /// <summary>
        /// Validates that default subfolder values in document library are applied when you upload a document
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenFolderDefaultValuesAreSpecifiedInDocumentLibrarySubFolder_AndYouUploadADocument_ThenDocumentShouldHaveDefaultValueForAllSupportedFieldTypes()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    LocaleId = 3084 // fr-CA
                };

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");
                
                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    // see related test above: EnsureFolderHierarchy_WhenAttemptingFALSEDefaultFolderValue_AndSPFieldAlreadHasTRUEDefaultValue_ShouldExplodeAndWarnYouThatThisWeirdEdgeCaseIsNotSupported
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey")
                {
                    DefaultValue = new DateTime(2005, 10, 21)
                };

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKeyText",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = "Text default value"
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyNote",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "Note default value"
                };

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyHtml",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "<p class=\"some-css-class\">HTML default value</p>"
                };

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = "Image"
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKeyTaxo",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyTaxoMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                // Create a list that contains all the fields we've prepared
                var fieldsToEnsure = new List<IFieldInfo>()
                    {
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ContentTypeInfo contentTypeWithAllFields = new ContentTypeInfo("0x0101007403019827FD4C68AF50C5F41781D262", "CTNameKey", "CTDescrKey", "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");
                ListInfo listInfo1 = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    ContentTypes = new List<ContentTypeInfo>() { contentTypeWithAllFields }
                };

                // Prepare some MetadataDefaults that we'll apply on the second-level folder
                var fieldDefaultValues = new List<FieldValueInfo>()
                {
                    new FieldValueInfo(numberFieldInfo, 5.0),
                    new FieldValueInfo(currencyFieldInfo, 535.95),
                    new FieldValueInfo(boolFieldInfoBasic, true),
                    new FieldValueInfo(boolFieldInfoDefaultFalse, true),
                    new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(1977, 1, 1)),
                    new FieldValueInfo(dateTimeFieldInfoDefault, new DateTime(1978, 1, 1)),
                    new FieldValueInfo(textFieldInfo, "TextAltDefaultValue"),
                    new FieldValueInfo(noteFieldInfo, "NoteAltDefaultValue"),
                    new FieldValueInfo(htmlFieldInfo, "HtmlAltDefaultValue"),
                    new FieldValueInfo(
                        imageFieldInfo, 
                        new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                        }),
                    new FieldValueInfo(
                        urlFieldInfo, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        urlFieldInfoImage, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        mediaFieldInfo, 
                        new MediaValue()
                        {
                            Title = "Some media file title",
                            Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                            IsAutoPlay = true,
                            IsLoop = true,
                            PreviewImageUrl = "/_layouts/15/Images/logo.png"
                        }),
                    new FieldValueInfo(taxoFieldInfo, new TaxonomyFullValue(levelOneTermB)),
                    new FieldValueInfo(
                        taxoMultiFieldInfo, 
                        new TaxonomyFullValueCollection(
                            new List<TaxonomyFullValue>() 
                                { 
                                    new TaxonomyFullValue(levelTwoTermAA), 
                                    new TaxonomyFullValue(levelTwoTermAB)
                                }))
                };

                // Default values are configured on the level 2 folder (not on the root folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                    FieldDefaultValues = fieldDefaultValues
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the test doc lib
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo1);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act: ensure the folder hierarchy with 2nd level subfolder which has MetadataDefaults for all possible types
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    var secondLevelEnsuredFolder = ensuredRootFolder.SubFolders["somelevel2path"];

                    var itemInSecondLevelFolder = secondLevelEnsuredFolder.Files.Add("SomeLevel2File.txt", new byte[0]);
                    itemInSecondLevelFolder.Update();

                    // Assert

                    // In root folder, the field definitions should apply
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameNumber"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameCurrency"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameBool"]);
                    Assert.IsFalse((bool)itemInRootFolder.Item["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(DateTime.Today, itemInRootFolder.Item["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), itemInRootFolder.Item["TestInternalNameDateDefault"]);
                    Assert.AreEqual("Text default value", itemInRootFolder.Item["TestInternalNameText"]);
                    Assert.AreEqual("Note default value", itemInRootFolder.Item["TestInternalNameNote"]);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", itemInRootFolder.Item["TestInternalNameHtml"]);

                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameImage"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameUrl"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameUrlImg"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameMedia"]);
                    Assert.IsNull(itemInRootFolder.Item["TestInternalNameTaxo"]);
                    Assert.AreEqual(0, ((TaxonomyFieldValueCollection)itemInRootFolder.Item["TestInternalNameTaxoMulti"]).Count);

                    // In second-level folder, our MetadataDefaults should've been applied
                    Assert.AreEqual(5.0, itemInSecondLevelFolder.Item["TestInternalNameNumber"]);
                    Assert.AreEqual(535.95, itemInSecondLevelFolder.Item["TestInternalNameCurrency"]);
                    Assert.IsTrue((bool)itemInSecondLevelFolder.Item["TestInternalNameBool"]);
                    Assert.IsTrue((bool)itemInSecondLevelFolder.Item["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(new DateTime(1977, 1, 1), itemInSecondLevelFolder.Item["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(1978, 1, 1), itemInSecondLevelFolder.Item["TestInternalNameDateDefault"]);
                    Assert.AreEqual("TextAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameText"]);
                    Assert.AreEqual("NoteAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameNote"]);
                    Assert.AreEqual("HtmlAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameHtml"]);

                    var imageFieldVal = (ImageFieldValue)itemInSecondLevelFolder.Item["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    var urlFieldVal = new SPFieldUrlValue(itemInSecondLevelFolder.Item["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);

                    var urlImageFieldVal = new SPFieldUrlValue(itemInSecondLevelFolder.Item["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);

                    var mediaFieldVal = MediaFieldValue.FromString(itemInSecondLevelFolder.Item["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    var taxoFieldValue = (TaxonomyFieldValue)itemInSecondLevelFolder.Item["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    var taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemInSecondLevelFolder.Item["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                }
            }
        }

        /// <summary>
        /// Validates that default root folder values in document library are applied when you upload a document
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenFolderDefaultValuesAreSpecifiedInDocumentLibraryRootFolder_AndYouUploadADocument_ThenDocumentShouldHaveDefaultValueForAllSupportedFieldTypes()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    LocaleId = 3084 // fr-CA
                };

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    // see related test above: EnsureFolderHierarchy_WhenAttemptingFALSEDefaultFolderValue_AndSPFieldAlreadHasTRUEDefaultValue_ShouldExplodeAndWarnYouThatThisWeirdEdgeCaseIsNotSupported
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey")
                {
                    DefaultValue = new DateTime(2005, 10, 21)
                };

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKeyText",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = "Text default value"
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyNote",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "Note default value"
                };

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyHtml",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "<p class=\"some-css-class\">HTML default value</p>"
                };

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = "Image"
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKeyTaxo",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyTaxoMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                // Create a list that contains all the fields we've prepared
                var fieldsToEnsure = new List<IFieldInfo>()
                    {
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ContentTypeInfo contentTypeWithAllFields = new ContentTypeInfo("0x0101007403019827FD4C68AF50C5F41781D262", "CTNameKey", "CTDescrKey", "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");
                ListInfo listInfo1 = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    ContentTypes = new List<ContentTypeInfo>() { contentTypeWithAllFields }
                };

                // Prepare some MetadataDefaults that we'll apply on the second-level folder
                var fieldDefaultValues = new List<FieldValueInfo>()
                {
                    new FieldValueInfo(numberFieldInfo, 5.0),
                    new FieldValueInfo(currencyFieldInfo, 535.95),
                    new FieldValueInfo(boolFieldInfoBasic, true),
                    new FieldValueInfo(boolFieldInfoDefaultFalse, true),
                    new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(1977, 1, 1)),
                    new FieldValueInfo(dateTimeFieldInfoDefault, new DateTime(1978, 1, 1)),
                    new FieldValueInfo(textFieldInfo, "TextAltDefaultValue"),
                    new FieldValueInfo(noteFieldInfo, "NoteAltDefaultValue"),
                    new FieldValueInfo(htmlFieldInfo, "HtmlAltDefaultValue"),
                    new FieldValueInfo(
                        imageFieldInfo, 
                        new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                        }),
                    new FieldValueInfo(
                        urlFieldInfo, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        urlFieldInfoImage, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        mediaFieldInfo, 
                        new MediaValue()
                        {
                            Title = "Some media file title",
                            Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                            IsAutoPlay = true,
                            IsLoop = true,
                            PreviewImageUrl = "/_layouts/15/Images/logo.png"
                        }),
                    new FieldValueInfo(taxoFieldInfo, new TaxonomyFullValue(levelOneTermB)),
                    new FieldValueInfo(
                        taxoMultiFieldInfo, 
                        new TaxonomyFullValueCollection(
                            new List<TaxonomyFullValue>() 
                                { 
                                    new TaxonomyFullValue(levelTwoTermAA), 
                                    new TaxonomyFullValue(levelTwoTermAB)
                                }))
                };

                // Default values are configured on the root folder (not on the 2nd level folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path");
                var rootFolderInfo = new FolderInfo("somepath")
                {
                    FieldDefaultValues = fieldDefaultValues,
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the test doc lib
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo1);

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act: ensure the folder hierarchy with 2nd level subfolder which has MetadataDefaults for all possible types
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    var itemInRootFolder = ensuredRootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    itemInRootFolder.Update();

                    var secondLevelEnsuredFolder = ensuredRootFolder.SubFolders["somelevel2path"];

                    var itemInSecondLevelFolder = secondLevelEnsuredFolder.Files.Add("SomeLevel2File.txt", new byte[0]);
                    itemInSecondLevelFolder.Update();

                    // Assert

                    // In root folder, our MetadataDefaults should've been applied
                    Assert.AreEqual(5.0, itemInRootFolder.Item["TestInternalNameNumber"]);
                    Assert.AreEqual(535.95, itemInRootFolder.Item["TestInternalNameCurrency"]);
                    Assert.IsTrue((bool)itemInRootFolder.Item["TestInternalNameBool"]);
                    Assert.IsTrue((bool)itemInRootFolder.Item["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(new DateTime(1977, 1, 1), itemInRootFolder.Item["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(1978, 1, 1), itemInRootFolder.Item["TestInternalNameDateDefault"]);
                    Assert.AreEqual("TextAltDefaultValue", itemInRootFolder.Item["TestInternalNameText"]);
                    Assert.AreEqual("NoteAltDefaultValue", itemInRootFolder.Item["TestInternalNameNote"]);
                    Assert.AreEqual("HtmlAltDefaultValue", itemInRootFolder.Item["TestInternalNameHtml"]);

                    var imageFieldVal = (ImageFieldValue)itemInRootFolder.Item["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    var urlFieldVal = new SPFieldUrlValue(itemInRootFolder.Item["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);

                    var urlImageFieldVal = new SPFieldUrlValue(itemInRootFolder.Item["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);

                    var mediaFieldVal = MediaFieldValue.FromString(itemInRootFolder.Item["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    var taxoFieldValue = (TaxonomyFieldValue)itemInRootFolder.Item["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    var taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemInRootFolder.Item["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                    
                    // In second-level folder, our MetadataDefaults should've been applied as well (inherited from root folder)
                    Assert.AreEqual(5.0, itemInSecondLevelFolder.Item["TestInternalNameNumber"]);
                    Assert.AreEqual(535.95, itemInSecondLevelFolder.Item["TestInternalNameCurrency"]);
                    Assert.IsTrue((bool)itemInSecondLevelFolder.Item["TestInternalNameBool"]);
                    Assert.IsTrue((bool)itemInSecondLevelFolder.Item["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(new DateTime(1977, 1, 1), itemInSecondLevelFolder.Item["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(1978, 1, 1), itemInSecondLevelFolder.Item["TestInternalNameDateDefault"]);
                    Assert.AreEqual("TextAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameText"]);
                    Assert.AreEqual("NoteAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameNote"]);
                    Assert.AreEqual("HtmlAltDefaultValue", itemInSecondLevelFolder.Item["TestInternalNameHtml"]);

                    imageFieldVal = (ImageFieldValue)itemInSecondLevelFolder.Item["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    urlFieldVal = new SPFieldUrlValue(itemInSecondLevelFolder.Item["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);

                    urlImageFieldVal = new SPFieldUrlValue(itemInSecondLevelFolder.Item["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);

                    mediaFieldVal = MediaFieldValue.FromString(itemInSecondLevelFolder.Item["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    taxoFieldValue = (TaxonomyFieldValue)itemInSecondLevelFolder.Item["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemInSecondLevelFolder.Item["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                }
            }
        }

        #endregion

        #region Pages Library's folder MetadataDefaults (field default values) should be created and/or updated during Ensure

        /// <summary>
        /// Validates that a NotSupportedException is thrown when attempting to set Boolean fields' default value to TRUE
        /// within a Pages library folder (SharePoint's OOTB support for Page-CT boolean default values is broken)
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenBooleanTRUEDefaultValuesAreSpecifiedInPagesLibraryFolder_ShouldThrownNotSupportedException()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                   "TestInternalNameBoolTrue",
                   new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                   "NameKeyBoolTrue",
                   "DescriptionKeyBoolTrue",
                   "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                var fieldsToEnsure = new List<IFieldInfo>()
                {
                    boolFieldInfoBasic,
                    boolFieldInfoDefaultTrue,
                    boolFieldInfoDefaultFalse
                };

                // We gotta update the ArticlePage Content type with our fields
                var articlePageCT = new ContentTypeInfo(
                    "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D",
                    "UpdatedArticlePageCT",
                    "UpdatedArticlePageCTDescription",
                    "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                // Default values will be  configured further down on the level 2 folder (not on the root folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Init the test Pages library (we're in a Pub Site, the Pages lib already exists and we want to add fields to it)
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    contentTypeHelper.EnsureContentType(list.ContentTypes, articlePageCT);      // this should add the field to the Pages lib

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act #1: try to set a TRUE folder default value on a default-less Boolean field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(boolFieldInfoBasic, true) };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    // Act #2: try to set a TRUE folder default value on a TRUE-default Boolean field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(boolFieldInfoDefaultTrue, true) };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    // Act #3: try to set a TRUE folder default value on a FALSE-default Boolean field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(boolFieldInfoDefaultFalse, true) };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a NotSupportedException is thrown when attempting to set DateTime fields' folder default value 
        /// when a SPField.DefaulValue or DefaultFormula already exists. The SPField's default DateTime value will always take precedence
        /// when in a Pages library.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenDateTimeFieldDefaultAlreadyDefined_AndAttemptingToSetFolderDefaultDate_ShouldThrownNotSupportedException()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                   "TestInternalNameDateFormula",
                   new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                   "NameKeyDateTimeFormula",
                   "DescriptionKeyDateTimeFormula",
                   "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey")
                {
                    DefaultValue = new DateTime(2005, 10, 21)
                };

                var fieldsToEnsure = new List<IFieldInfo>()
                {
                    dateTimeFieldInfoFormula,
                    dateTimeFieldInfoDefault
                };

                // We gotta update the ArticlePage Content type with our fields
                var articlePageCT = new ContentTypeInfo(
                    "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D",
                    "UpdatedArticlePageCT",
                    "UpdatedArticlePageCTDescription",
                    "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                // Default values will be configured further down on the level 2 folder (not on the root folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Init the test Pages library (we're in a Pub Site, the Pages lib already exists and we want to add fields to it)
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    contentTypeHelper.EnsureContentType(list.ContentTypes, articlePageCT);      // this should add the field to the Pages lib

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act #1: try to set a DateTime folder default on a field which already has a formula
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(2003, 12, 31)) };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    // Act #2: try to set a DateTime folder default value on a field which already has a default value
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(dateTimeFieldInfoDefault, new DateTime(2003, 12, 31)) };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a NotSupportedException is thrown when attempting to set string-based fields' folder default value 
        /// when in a Pages library. While folder-specific default values work for string-based fields in document libraries,
        /// this all breaks down in the Pages library, which somehow prevents per-folder string defaults.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInPagesLibrary_AndAttemptingToSetDefaultStringValue_ShouldThrownNotSupportedException()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKeyText",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyNote",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyHtml",
                    "DescriptionKeyAlt",
                    "GroupKey");

                var fieldsToEnsure = new List<IFieldInfo>()
                {
                    textFieldInfo,
                    noteFieldInfo,
                    htmlFieldInfo
                };

                // We gotta update the ArticlePage Content type with our fields
                var articlePageCT = new ContentTypeInfo(
                    "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D",
                    "UpdatedArticlePageCT",
                    "UpdatedArticlePageCTDescription",
                    "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                // Default values will be configured further down on the level 2 folder (not on the root folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Init the test Pages library (we're in a Pub Site, the Pages lib already exists and we want to add fields to it)
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    contentTypeHelper.EnsureContentType(list.ContentTypes, articlePageCT);      // this should add the field to the Pages lib

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act #1: try to set a folder default on a Text field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(textFieldInfo, "MyTextFolderDefault") };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    // Act #2: try to set a folder default on a Note field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(noteFieldInfo, "MyNoteFolderDefault") };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    // Act #3: try to set a folder default on a Html field
                    folderInfoLvl2.FieldDefaultValues = new List<FieldValueInfo>() { new FieldValueInfo(noteFieldInfo, "MyHtmlFolderDefault") };

                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a NotSupportedException is thrown when attempting to set Publishing Image folder-specific column default value.
        /// SharePoint simply doesn't support folder defaults for that field type.
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenInPagesLibrary_AndAttemptingToSetDefaultPublishingImageValue_ShouldThrownNotSupportedException()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey")
                {
                };

                var fieldsToEnsure = new List<IFieldInfo>()
                {
                    imageFieldInfo
                };

                // We gotta update the ArticlePage Content type with our fields
                var articlePageCT = new ContentTypeInfo(
                    "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D",
                    "UpdatedArticlePageCT",
                    "UpdatedArticlePageCTDescription",
                    "GroupKey")
                {
                    Fields = fieldsToEnsure
                };

                // Default values configured on the level 2 folder (not on the root folder)
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                    FieldDefaultValues = new List<FieldValueInfo>() 
                    { 
                        new FieldValueInfo(
                            imageFieldInfo, 
                            new ImageValue()
                            {
                                Hyperlink = "http://github.com/GSoft-SharePoint/",
                                ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                            })
                    }
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Init the test Pages library (we're in a Pub Site, the Pages lib already exists and we want to add fields to it)
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    contentTypeHelper.EnsureContentType(list.ContentTypes, articlePageCT);      // this should add the field to the Pages lib

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act: try to set a folder default on a Publishing Image field
                    try
                    {
                        var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);
                        Assert.Fail("Should've thrown NotSupportedException");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Validates that default folder values in Pages library are applied when you create a publishing page
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenFolderDefaultValuesAreSpecifiedInPagesLibraryFolder_AndYouCreateAPage_ThenPageShouldHaveDefaultValue()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    LocaleId = 3084 // fr-CA
                };

                DateTimeFieldInfo dateOnlyFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDate",
                    "DescriptionKeyDate",
                    "GroupKey")
                {
                    // Important that there be no DefaultFormula and no DefaultValue, otherwise the
                    // folder default column value would be ignored.
                    // See related test above: EnsureFolderHierarchy_WhenDateTimeFieldDefaultAlreadyDefined_AndAttemptingToSetFolderDefaultDate_ShouldThrownNotSupportedException
                };

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                  "TestInternalNameDateTime",
                  new Guid("{526F9055-7472-4CFA-A31D-E2B7BFB1FD7D}"),
                  "NameKeyDateTime",
                  "DescriptionKeyDateTime",
                  "GroupKey")
                {
                    // Important that there be no DefaultFormula and no DefaultValue, otherwise the
                    // folder default column value would be ignored.
                    // See related test above: EnsureFolderHierarchy_WhenDateTimeFieldDefaultAlreadyDefined_AndAttemptingToSetFolderDefaultDate_ShouldThrownNotSupportedException
                    Format = "DateTime"
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = "Image"
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKeyTaxo",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyTaxoMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                // Create a list that contains all the fields we've prepared
                var fieldsToEnsure = new List<IFieldInfo>()
                    {
                        numberFieldInfo,
                        currencyFieldInfo,
                        dateOnlyFieldInfo,
                        dateTimeFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };
                
                // Prepare some MetadataDefaults that we'll apply on the second-level folder
                var fieldDefaultValues = new List<FieldValueInfo>()
                {
                    new FieldValueInfo(numberFieldInfo, 5.0),
                    new FieldValueInfo(currencyFieldInfo, 535.95),
                    new FieldValueInfo(dateOnlyFieldInfo, new DateTime(1976, 1, 1)),
                    new FieldValueInfo(dateTimeFieldInfo, new DateTime(1977, 1, 1)),
                    new FieldValueInfo(
                        urlFieldInfo, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        urlFieldInfoImage, 
                        new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        }),
                    new FieldValueInfo(
                        mediaFieldInfo, 
                        new MediaValue()
                        {
                            Title = "Some media file title",
                            Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                            IsAutoPlay = true,
                            IsLoop = true,
                            PreviewImageUrl = "/_layouts/15/Images/logo.png"
                        }),
                    new FieldValueInfo(taxoFieldInfo, new TaxonomyFullValue(levelOneTermB)),
                    new FieldValueInfo(
                        taxoMultiFieldInfo, 
                        new TaxonomyFullValueCollection(
                            new List<TaxonomyFullValue>() 
                                { 
                                    new TaxonomyFullValue(levelTwoTermAA), 
                                    new TaxonomyFullValue(levelTwoTermAB)
                                }))
                };

                // We gotta update the ArticlePage Content type with our fields
                var articlePageCT = new ContentTypeInfo(
                    "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D",
                    "UpdatedArticlePageCT",
                    "UpdatedArticlePageCTDescription",
                    "GroupKey")
                    {
                        Fields = fieldsToEnsure
                    };

                // Default values are configured on the level 2 folder (not on the root folder)
                var articleLeftPageLayoutInfo = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");
                var folderInfoLvl2 = new FolderInfo("somelevel2path")
                {
                    FieldDefaultValues = fieldDefaultValues,
                    Pages = new List<PageInfo>() 
                    { 
                        new PageInfo("DynamiteTestPage", articleLeftPageLayoutInfo),
                        new PageInfo("DynamiteTestPageWithValues", articleLeftPageLayoutInfo)
                        {
                            FieldValues = new List<FieldValueInfo>()
                            {
                                new FieldValueInfo(dateOnlyFieldInfo, new DateTime(1998, 1, 1)),
                                new FieldValueInfo(dateTimeFieldInfo, new DateTime(1999, 1, 1)),
                                new FieldValueInfo(taxoFieldInfo, new TaxonomyFullValue(levelOneTermA))
                            }
                        }
                    }
                };

                var rootFolderInfo = new FolderInfo("somepath")
                {
                    Subfolders = new List<FolderInfo>()
                    {
                        folderInfoLvl2
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Init the test Pages library (we're in a Pub Site, the Pages lib already exists and we want to add fields to it)
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    contentTypeHelper.EnsureContentType(list.ContentTypes, articlePageCT);      // this should add the field to the Pages lib

                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Act: ensure the folder hierarchy with a page inside 2nd level subfolder which has MetadataDefaults for all possible types
                    var ensuredRootFolder = folderHelper.EnsureFolderHierarchy(list, rootFolderInfo);

                    // Assert
                    var pubWeb = PublishingWeb.GetPublishingWeb(testScope.SiteCollection.RootWeb);
                    var recursivePagesQuery = new SPQuery() { ViewAttributes = "Scope=\"Recursive\"" };

                    // Fetch all pages. WARNING: all dates will be returned in UTC time, because our SPQuery is modified
                    // by GetPublishingPages to force DatesInUtc=true.
                    var allPages = pubWeb.GetPublishingPages(recursivePagesQuery);
                    var ourPageWithDefaults = allPages["/Pages/somelevel2path/DynamiteTestPage.aspx"];
                    var ourPageWithDefaultsAndValues = allPages["/Pages/somelevel2path/DynamiteTestPageWithValues.aspx"];

                    // In 1st publishing page's list item, all metadata defaults should've been applied
                    Assert.AreEqual(5.0, ourPageWithDefaults.ListItem["TestInternalNameNumber"]);
                    Assert.AreEqual(535.95, ourPageWithDefaults.ListItem["TestInternalNameCurrency"]);
                    Assert.AreEqual(new DateTime(1976, 1, 1), ((DateTime)ourPageWithDefaults.ListItem["TestInternalNameDate"]).ToLocalTime());    // SPListItem should normally return DateTime as local time (not UTC), but since we used GetPublishingPage, dates are in UTC
                    Assert.AreEqual(new DateTime(1977, 1, 1), ((DateTime)ourPageWithDefaults.ListItem["TestInternalNameDateTime"]).ToLocalTime());
                    
                    var urlFieldVal = new SPFieldUrlValue(ourPageWithDefaults.ListItem["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);

                    var urlImageFieldVal = new SPFieldUrlValue(ourPageWithDefaults.ListItem["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);

                    var mediaFieldVal = MediaFieldValue.FromString(ourPageWithDefaults.ListItem["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    var taxoFieldValue = (TaxonomyFieldValue)ourPageWithDefaults.ListItem["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    var taxoFieldValueMulti = (TaxonomyFieldValueCollection)ourPageWithDefaults.ListItem["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);

                    // In 2nd publishing page's list item, metadata defaults should've been applied everywhere except where we specified item values
                    Assert.AreEqual(5.0, ourPageWithDefaultsAndValues.ListItem["TestInternalNameNumber"]);
                    Assert.AreEqual(535.95, ourPageWithDefaultsAndValues.ListItem["TestInternalNameCurrency"]);
                    Assert.AreEqual(new DateTime(1998, 1, 1), ((DateTime)ourPageWithDefaultsAndValues.ListItem["TestInternalNameDate"]).ToLocalTime());     // PageInfo Value should be applied, not folder MetadataDefaul
                    Assert.AreEqual(new DateTime(1999, 1, 1), ((DateTime)ourPageWithDefaultsAndValues.ListItem["TestInternalNameDateTime"]).ToLocalTime());     // PageInfo Value should be applied, not folder MetadataDefault
                    
                    urlFieldVal = new SPFieldUrlValue(ourPageWithDefaultsAndValues.ListItem["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description will never be set for Format=Hyperlink

                    urlImageFieldVal = new SPFieldUrlValue(ourPageWithDefaultsAndValues.ListItem["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description will never be set for Format=Image either

                    mediaFieldVal = MediaFieldValue.FromString(ourPageWithDefaultsAndValues.ListItem["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    taxoFieldValue = (TaxonomyFieldValue)ourPageWithDefaultsAndValues.ListItem["TestInternalNameTaxo"];  // PageInfo Value should be applied, not folder MetadataDefault
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermA.Label, taxoFieldValue.Label);

                    taxoFieldValueMulti = (TaxonomyFieldValueCollection)ourPageWithDefaultsAndValues.ListItem["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                }
            }
        }

        /// <summary>
        /// Validates that default folder values in Pages library are applied when you specify a collection of PageInfos in your FolderInfo definition
        /// </summary>
        [TestMethod]
        public void EnsureFolderHierarchy_WhenFolderDefaultValuesAreSpecifiedInPagesLibraryFolder_AndYouSpecifyPageInfosForFolderInfo_ThenFolderEnsureShouldCreatePagesWithDefaultValue()
        {
        }

        #endregion

        #region Root folder and sub-folders' UniqueContentTypeOrder should be ensured through the hierarchy

        //// TODO: write 'em tests

        #endregion
    }
}
