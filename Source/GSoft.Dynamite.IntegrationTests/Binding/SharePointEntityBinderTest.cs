using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Binding
{
    /// <summary>
    /// Validates the behavior of the default-configured implementation 
    /// of <see cref="ISharePointEntityBinder"/>, the mapper interface.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class SharePointEntityBinderTest
    {
        [TestMethod]
        public void ToEntity_ShouldWork()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey")
                {
                    DefaultValue = 555
                };

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey")
                {
                    DefaultValue = 5.5
                };

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    DefaultValue = 500.95,
                    LocaleId = 3084 // fr-CA
                };

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
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = "Text default value"
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "Note default value"
                };

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
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
                    DefaultValue = new ImageValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    }
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    }
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = "Image",
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    }
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    // ShowField should be Title by default
                    DefaultValue = new LookupValue(1, "Test Item 1")
                };

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID",
                    DefaultValue = new LookupValue(2, "2")
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey")
                {
                    DefaultValue = new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") }
                };

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserDomainName + "\\" + Environment.UserName);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey")
                {
                    DefaultValue = new UserValue(ensuredUser1)
                };

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey")
                {
                    DefaultValue = new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) }
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey")
                {
                    DefaultValue = new MediaValue()
                    {
                        Title = "Some media file title",
                        Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        IsAutoPlay = true,
                        IsLoop = true,
                        PreviewImageUrl = "/_layouts/15/Images/logo.png"
                    }
                };

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
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyFullValue(levelOneTermB),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyFullValueCollection(
                        new List<TaxonomyFullValue>() 
                            { 
                                new TaxonomyFullValue(levelTwoTermAA), 
                                new TaxonomyFullValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<IFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

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

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    
                    // Create item on list
                    var itemOnList = list.AddItem();
                    itemOnList.Update();    // force DefaultValue to be applied

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entity = new TestItemEntity();

                    // Act
                    entityBinder.ToEntity<TestItemEntity>(entity, itemOnList);

                    // Assert
                    Assert.AreEqual(555, entity.IntegerProperty);
                    Assert.AreEqual(5.5, entity.DoubleProperty);
                    Assert.AreEqual(500.95, entity.CurrencyProperty);
                    Assert.IsFalse(entity.BoolProperty.HasValue);
                    Assert.IsTrue(entity.BoolDefaultTrueProperty);
                    Assert.IsFalse(entity.BoolDefaultFalseProperty);
                    Assert.AreEqual(DateTime.Today, entity.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(2005, 10, 21), entity.DateTimeProperty);
                    Assert.AreEqual("Text default value", entity.TextProperty);
                    Assert.AreEqual("Note default value", entity.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", entity.HtmlProperty);

                    Assert.IsNotNull(entity.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entity.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entity.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entity.UrlProperty.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description will never be set for Format=Hyperlink

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entity.UrlImageProperty.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description will never be set for Format=Image either

                    Assert.AreEqual(1, entity.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entity.LookupProperty.Value);

                    Assert.AreEqual(2, entity.LookupAltProperty.Id);
                    Assert.AreEqual("2", entity.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entity.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entity.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entity.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entity.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entity.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entity.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entity.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entity.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entity.MediaProperty.Url);
                    Assert.IsTrue(entity.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entity.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entity.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entity.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entity.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entity.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entity.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entity.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entity.TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        public class TestItemEntity : BaseEntity
        {
            [Property("TestInternalNameInteger")]
            public int IntegerProperty { get; set; }

            [Property("TestInternalNameNumber")]
            public double DoubleProperty { get; set; }

            [Property("TestInternalNameCurrency")]
            public double CurrencyProperty { get; set; }   // TODO: right now currency (which, ideally, would be mapped to type decimal) will use the DoubleValueWriter, which may map wrong to decimal

            [Property("TestInternalNameBool")]
            public bool? BoolProperty { get; set; }

            [Property("TestInternalNameBoolTrue")]
            public bool BoolDefaultTrueProperty { get; set; }

            [Property("TestInternalNameBoolFalse")]
            public bool BoolDefaultFalseProperty { get; set; }

            [Property("TestInternalNameDateFormula")]
            public DateTime DateTimeFormulaProperty { get; set; }

            [Property("TestInternalNameDateDefault")]
            public DateTime? DateTimeProperty { get; set; }

            [Property("TestInternalNameText")]
            public string TextProperty { get; set; }

            [Property("TestInternalNameNote")]
            public string NoteProperty { get; set; }

            [Property("TestInternalNameHtml")]
            public string HtmlProperty { get; set; }

            [Property("TestInternalNameImage")]
            public ImageValue ImageProperty { get; set; }

            [Property("TestInternalNameUrl")]
            public UrlValue UrlProperty { get; set; }

            [Property("TestInternalNameUrlImg")]
            public UrlValue UrlImageProperty { get; set; }

            [Property("TestInternalNameLookup")]
            public LookupValue LookupProperty { get; set; }

            [Property("TestInternalNameLookupAlt")]
            public LookupValue LookupAltProperty { get; set; }

            [Property("TestInternalNameLookupM")]
            public LookupValueCollection LookupMultiProperty { get; set; }

            [Property("TestInternalNameUser")]
            public UserValue UserProperty { get; set; }

            [Property("TestInternalNameUserMulti")]
            public UserValueCollection UserMultiProperty { get; set; }

            [Property("TestInternalNameMedia")]
            public MediaValue MediaProperty { get; set; }

            [Property("TestInternalNameTaxo")]
            public TaxonomyValue TaxonomyProperty { get; set; }    // TODO: consolidate TaxonomyValue and TaxonomyFullValue

            [Property("TestInternalNameTaxoMulti")]
            public TaxonomyValueCollection TaxonomyMultiProperty { get; set; } 
        }
    }
}
