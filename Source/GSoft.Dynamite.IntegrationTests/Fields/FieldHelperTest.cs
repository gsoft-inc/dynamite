using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.ValueTypes.Writers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Fields
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="FieldHelper"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class FieldHelperTest
    {
        #region "Ensure" should mean "Create if new or return existing"

        /// <summary>
        /// Validates that EnsureField adds a field to the site collection if it did not exist previously
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenNotAlreadyExists_ShouldAddAndReturnField()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName", 
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    int noOfFieldsBefore = fieldsCollection.Count;

                    // Act
                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    // Assert
                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(field);
                    Assert.AreEqual(textFieldInfo.Id, field.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, field.InternalName);
                    Assert.AreEqual(textFieldInfo.DisplayNameResourceKey, field.TitleResource.Value);
                    Assert.AreEqual(textFieldInfo.DescriptionResourceKey, field.DescriptionResource.Value);
                    Assert.AreEqual(textFieldInfo.GroupResourceKey, field.Group);

                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];
                    Assert.AreEqual(textFieldInfo.Id, fieldRefetched.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, fieldRefetched.InternalName);
                    Assert.AreEqual(textFieldInfo.DisplayNameResourceKey, fieldRefetched.TitleResource.Value);
                    Assert.AreEqual(textFieldInfo.DescriptionResourceKey, fieldRefetched.DescriptionResource.Value);
                    Assert.AreEqual(textFieldInfo.GroupResourceKey, fieldRefetched.Group);
                }                
            }
        }

        /// <summary>
        /// Validates that EnsureField returns the existing field if it was added previously
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenAlreadyExists_ShouldReturnExistingField()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // STEP 1: Create the field
                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(field);

                    // STEP 2: Try to re-ensure the field
                    SPField doubleEnsuredField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(doubleEnsuredField);
                    Assert.AreEqual(textFieldInfo.Id, doubleEnsuredField.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, doubleEnsuredField.InternalName);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureField deals with same-internal-name-but-different-Guid conflicts 
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenOtherFieldWithSameInternalNameAlreadyExists_ShouldNotAttemptFieldCreationAndReturnExistingMatch()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    Required = RequiredType.NotRequired,
                    MaxLength = 50
                };

                TextFieldInfo alternateTextFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{9EBF5EC3-5FC4-4ACF-B404-AC0A2D74A10F}"),     // new GUID, but same internal name
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Required = RequiredType.Required,
                    MaxLength = 500
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // STEP 1: Create the first field
                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField originalField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(originalField);
                    Assert.AreEqual(textFieldInfo.Id, originalField.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, originalField.InternalName);

                    // STEP 2: Try to create the internal-name-clashing alternate field
                    SPField alternateEnsuredField = fieldHelper.EnsureField(fieldsCollection, alternateTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(alternateEnsuredField);
                    Assert.AreEqual(textFieldInfo.Id, alternateEnsuredField.Id);               // metadata should be sane as original field, not alternate field
                    Assert.AreEqual(textFieldInfo.InternalName, alternateEnsuredField.InternalName);

                    // The returned field shouldn't have gotten its properties updated
                    // (as in this shouldn't happen: "Ensure and Update existing other
                    // unrelated field which has clashing Guid/Internal name")
                    Assert.IsFalse(alternateEnsuredField.Required);     // the original field was actually returned
                    Assert.AreEqual(50, ((SPFieldText)alternateEnsuredField).MaxLength);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureField deals with same-Guid-but-different-internal-names conflicts 
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenOtherFieldWithSameGuidAlreadyExists_ShouldNotAttemptFieldCreationAndReturnExistingMatch()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    Required = RequiredType.NotRequired,
                    MaxLength = 50
                };

                TextFieldInfo alternateTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameAlt",                                             // new internal name, but same Guid
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),   
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Required = RequiredType.Required,
                    MaxLength = 500
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // STEP 1: Create the first field
                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField originalField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(originalField);
                    Assert.AreEqual(textFieldInfo.Id, originalField.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, originalField.InternalName);

                    // STEP 2: Try to create the internal-name-clashing alternate field
                    SPField alternateEnsuredField = fieldHelper.EnsureField(fieldsCollection, alternateTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(alternateEnsuredField);
                    Assert.AreEqual(textFieldInfo.Id, alternateEnsuredField.Id);               // metadata should be same as original field, not alternate field
                    Assert.AreEqual(textFieldInfo.InternalName, alternateEnsuredField.InternalName);

                    // The returned field shouldn't have gotten its properties updated
                    // (as in this shouldn't happen: "Ensure and Update existing other
                    // unrelated field which has clashing Guid/Internal name")
                    Assert.IsFalse(alternateEnsuredField.Required);     // the original field was actually returned
                    Assert.AreEqual(50, ((SPFieldText)alternateEnsuredField).MaxLength);
                }
            }
        }

        #endregion
        
        #region Basic FieldInfo-to-SPField values should be mapped upon creation

        /// <summary>
        /// Validates that EnsureField intializes field definitions will all the FieldInfo's basic metadata
        /// </summary>
        [TestMethod]
        public void EnsureField_ShouldProperlyInitializeAllFieldBasicProperties()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                    {
                        EnforceUniqueValues = true,
                        IsHidden = true,
                        IsHiddenInDisplayForm = true,
                        IsHiddenInNewForm = false,
                        IsHiddenInEditForm = false,
                        IsHiddenInListSettings = false,
                        MaxLength = 50,
                        Required = RequiredType.Required
                    };

                TextFieldInfo alternateTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                    {
                        EnforceUniqueValues = false,
                        IsHidden = false,
                        IsHiddenInDisplayForm = false,
                        IsHiddenInNewForm = true,
                        IsHiddenInEditForm = true,
                        IsHiddenInListSettings = true,
                        MaxLength = 500,
                        Required = RequiredType.NotRequired
                    };

                TextFieldInfo defaultsTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameDefaults",
                    new Guid("{7BEB995F-C696-453B-BA86-09A32381C783}"),
                    "NameKeyDefaults",
                    "DescriptionKeyDefaults",
                    "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) First field definition
                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField originalField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(originalField);
                    this.ValidateFieldBasicValues(textFieldInfo, originalField);

                    SPField originalFieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];
                    this.ValidateFieldBasicValues(textFieldInfo, originalFieldRefetched);

                    // 2) Alternate field definition
                    SPField alternateEnsuredField = fieldHelper.EnsureField(fieldsCollection, alternateTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 2, fieldsCollection.Count);
                    Assert.IsNotNull(alternateEnsuredField);
                    this.ValidateFieldBasicValues(alternateTextFieldInfo, alternateEnsuredField);

                    SPField alternateFieldRefetched = testScope.SiteCollection.RootWeb.Fields[alternateTextFieldInfo.Id];
                    this.ValidateFieldBasicValues(alternateTextFieldInfo, alternateFieldRefetched);

                    // 3) Defaults-based field definition
                    SPField defaultBasedEnsuredField = fieldHelper.EnsureField(fieldsCollection, defaultsTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 3, fieldsCollection.Count);
                    Assert.IsNotNull(defaultBasedEnsuredField);
                    this.ValidateFieldBasicValues(defaultsTextFieldInfo, defaultBasedEnsuredField);

                    SPField defaultsFieldRefetched = testScope.SiteCollection.RootWeb.Fields[defaultsTextFieldInfo.Id];
                    this.ValidateFieldBasicValues(defaultsTextFieldInfo, defaultsFieldRefetched);
                }
            }
        }

        #endregion

        #region "Ensure" should also mean "Update existing field definition when FieldInfo is different than already deployed column" (Potentially bad idea?)

        /// <summary>
        /// Validates that EnsureField takes care of updating property changes in the field definition.
        /// I.E. "Ensure" means "1) create if not exist or 2) update and return updated existing"
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenFieldAlreadyExistsAndInfoObjectChanged_ShouldUpdateExistingBasicFieldProperties()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true,
                    IsHidden = true,
                    IsHiddenInDisplayForm = true,
                    IsHiddenInNewForm = false,
                    IsHiddenInEditForm = false,
                    IsHiddenInListSettings = false,
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyNote",
                    "DescriptionKeyNote",
                    "GroupKey")
                {
                    EnforceUniqueValues = false,
                    IsHidden = false,
                    IsHiddenInDisplayForm = false,
                    IsHiddenInNewForm = true,
                    IsHiddenInEditForm = true,
                    IsHiddenInListSettings = true,
                    Required = RequiredType.NotRequired,
                    HasRichText = true
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Ensure the basic fields and the first version of their properties
                    SPField textField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                    SPField noteField = fieldHelper.EnsureField(fieldsCollection, noteFieldInfo);
                    
                    this.ValidateFieldBasicValues(textFieldInfo, textField);
                    Assert.AreEqual(50, ((SPFieldText)textField).MaxLength);    // see MaxLength=50 above
                    this.ValidateFieldBasicValues(noteFieldInfo, noteField);
                    Assert.IsTrue(((SPFieldMultiLineText)noteField).RichText);  // see HasRichText=true above

                    SPField textFieldRefetched = testScope.SiteCollection.RootWeb.Fields[textField.Id];     // gotta make sure the re-fetched field has same definition as one returned by EnsureField
                    SPField noteFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noteField.Id];

                    this.ValidateFieldBasicValues(textFieldInfo, textFieldRefetched);
                    Assert.AreEqual(50, ((SPFieldText)textFieldRefetched).MaxLength);    // see MaxLength=50 above
                    this.ValidateFieldBasicValues(noteFieldInfo, noteFieldRefetched);
                    Assert.IsTrue(((SPFieldMultiLineText)noteFieldRefetched).RichText);  // see HasRichText=true above

                    // 2) Modify the FieldInfo values
                    textFieldInfo.DisplayNameResourceKey = "NameKeyUpdated";
                    textFieldInfo.DescriptionResourceKey = "DescriptionKeyUpdated";
                    textFieldInfo.GroupResourceKey = "GroupKeyUpdated";
                    textFieldInfo.EnforceUniqueValues = false;
                    textFieldInfo.IsHidden = false;
                    textFieldInfo.IsHiddenInDisplayForm = false;
                    textFieldInfo.IsHiddenInNewForm = true;
                    textFieldInfo.IsHiddenInEditForm = true;
                    textFieldInfo.IsHiddenInListSettings = true;
                    textFieldInfo.MaxLength = 500;
                    textFieldInfo.Required = RequiredType.NotRequired;

                    noteFieldInfo.DisplayNameResourceKey = "NameKeyNoteUpdated";
                    noteFieldInfo.DescriptionResourceKey = "DescriptionKeyNoteUpdated";
                    noteFieldInfo.GroupResourceKey = "GroupKeyNoteUpdated";
                    noteFieldInfo.IsHidden = true;
                    noteFieldInfo.IsHiddenInDisplayForm = true;
                    noteFieldInfo.IsHiddenInNewForm = false;
                    noteFieldInfo.IsHiddenInEditForm = false;
                    noteFieldInfo.IsHiddenInListSettings = false;
                    noteFieldInfo.Required = RequiredType.Required;
                    noteFieldInfo.HasRichText = false;

                    // Act
                    // 3) Update the site columns by re-ensuring with the updated FieldInfo values
                    fieldsCollection = testScope.SiteCollection.RootWeb.Fields;
                    textField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                    noteField = fieldHelper.EnsureField(fieldsCollection, noteFieldInfo);

                    // 4) Assert that the field contain the 2nd version's updates
                    this.ValidateFieldBasicValues(textFieldInfo, textField);
                    Assert.AreEqual(500, ((SPFieldText)textField).MaxLength);    // see MaxLength=500 above
                    this.ValidateFieldBasicValues(noteFieldInfo, noteField);
                    Assert.IsFalse(((SPFieldMultiLineText)noteField).RichText);  // see HasRichText=false above

                    // gotta make sure the re-fetched field has same definition as one returned by EnsureField
                    textFieldRefetched = testScope.SiteCollection.RootWeb.Fields[textField.Id];     
                    noteFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noteField.Id];

                    this.ValidateFieldBasicValues(textFieldInfo, textFieldRefetched);
                    Assert.AreEqual(500, ((SPFieldText)textFieldRefetched).MaxLength);    // see MaxLength=500 above
                    this.ValidateFieldBasicValues(noteFieldInfo, noteFieldRefetched);
                    Assert.IsFalse(((SPFieldMultiLineText)noteFieldRefetched).RichText);  // see HasRichText=false above
                }
            }
        }

        /// <summary>
        /// Validates that EnsureField takes care of updating property changes in the Taxonomy field definitions.
        /// I.E. "Ensure" means "1) create if not exist or 2) update and return updated existing".
        /// Gotta make sure the taxonomy default value and term store mapping are updated.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomyFieldAlreadyExists_ShouldUpdateExistingTaxonomyFieldProperties()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{00E3BCD8-3AD6-4259-BB7A-22808A92BD82}"),
                    "NameKeyTaxo",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true,
                    IsHidden = true,
                    IsHiddenInDisplayForm = true,
                    IsHiddenInNewForm = false,
                    IsHiddenInEditForm = false,
                    IsHiddenInListSettings = false,
                    Required = RequiredType.Required,
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyTaxoMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = false,
                    IsHidden = false,
                    IsHiddenInDisplayForm = false,
                    IsHiddenInNewForm = true,
                    IsHiddenInEditForm = true,
                    IsHiddenInListSettings = true,
                    Required = RequiredType.NotRequired,
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Ensure the basic fields and the first version of their properties
                    TaxonomyField taxoField = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    TaxonomyField taxoMultiField = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    this.ValidateFieldBasicValues(taxoFieldInfo, taxoField);
                    Assert.AreEqual(testTermSet.Id, taxoField.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, taxoField.SspId);
                    Assert.AreEqual(Guid.Empty, taxoField.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(taxoField.IsTermSetValid);
                    Assert.IsTrue(taxoField.IsAnchorValid);       // should always be valid

                    this.ValidateFieldBasicValues(taxoMultiFieldInfo, taxoMultiField);
                    Assert.AreEqual(testTermSet.Id, taxoMultiField.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, taxoMultiField.SspId);
                    Assert.AreEqual(levelOneTermA.Id, taxoMultiField.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(taxoMultiField.IsTermSetValid);
                    Assert.IsTrue(taxoMultiField.IsAnchorValid);       // should always be valid

                    TaxonomyField fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    TaxonomyField fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];

                    this.ValidateFieldBasicValues(taxoFieldInfo, fieldSingleFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingleFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldSingleFetchedAgain.IsAnchorValid);       // should always be valid

                    this.ValidateFieldBasicValues(taxoMultiFieldInfo, fieldMultiFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldMultiFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(levelOneTermA.Id, fieldMultiFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldMultiFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldMultiFetchedAgain.IsAnchorValid);       // should always be valid

                    // 2) Modify the FieldInfo values
                    taxoFieldInfo.DisplayNameResourceKey = "NameKeyUpdated";
                    taxoFieldInfo.DescriptionResourceKey = "DescriptionKeyUpdated";
                    taxoFieldInfo.GroupResourceKey = "GroupKeyUpdated";
                    taxoFieldInfo.EnforceUniqueValues = false;
                    taxoFieldInfo.IsHidden = false;
                    taxoFieldInfo.IsHiddenInDisplayForm = false;
                    taxoFieldInfo.IsHiddenInNewForm = true;
                    taxoFieldInfo.IsHiddenInEditForm = true;
                    taxoFieldInfo.IsHiddenInListSettings = true;
                    taxoFieldInfo.Required = RequiredType.NotRequired;
                    taxoFieldInfo.TermStoreMapping = new TaxonomyContext(levelOneTermA);   // choices limited to children of a specific term, instead of having full term set choices

                    taxoMultiFieldInfo.DisplayNameResourceKey = "NameKeyMultiUpdated";
                    taxoMultiFieldInfo.DescriptionResourceKey = "DescriptionKeyMultiUpdated";
                    taxoMultiFieldInfo.GroupResourceKey = "GroupKeyMultiUpdated";
                    taxoMultiFieldInfo.IsHidden = true;
                    taxoMultiFieldInfo.IsHiddenInDisplayForm = true;
                    taxoMultiFieldInfo.IsHiddenInNewForm = false;
                    taxoMultiFieldInfo.IsHiddenInEditForm = false;
                    taxoMultiFieldInfo.IsHiddenInListSettings = false;
                    taxoMultiFieldInfo.Required = RequiredType.Required;
                    taxoMultiFieldInfo.TermStoreMapping = null;             // remove term store mapping

                    // Act
                    // 3) Update the site columns by re-ensuring with the updated FieldInfo values
                    fieldsCollection = testScope.SiteCollection.RootWeb.Fields;
                    taxoField = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    taxoMultiField = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    // 4) Assert that the field contain the 2nd version's updates
                    this.ValidateFieldBasicValues(taxoFieldInfo, taxoField);
                    Assert.AreEqual(testTermSet.Id, taxoField.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, taxoField.SspId);
                    Assert.AreEqual(levelOneTermA.Id, taxoField.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(taxoField.IsTermSetValid);
                    Assert.IsTrue(taxoField.IsAnchorValid);       // should always be valid

                    this.ValidateFieldBasicValues(taxoMultiFieldInfo, taxoMultiField);
                    Assert.AreEqual(Guid.Empty, taxoMultiField.TermSetId);          // term store mapping should've been removed
                    Assert.AreEqual(Guid.Empty, taxoMultiField.SspId);
                    Assert.AreEqual(Guid.Empty, taxoMultiField.AnchorId);
                    Assert.IsFalse(taxoMultiField.IsTermSetValid);
                    Assert.IsTrue(taxoMultiField.IsAnchorValid);       // should always be valid

                    // gotta make sure the re-fetched field has same definition as one returned by EnsureField
                    fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoField.Id];
                    fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiField.Id];

                    this.ValidateFieldBasicValues(taxoFieldInfo, fieldSingleFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(levelOneTermA.Id, fieldSingleFetchedAgain.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldSingleFetchedAgain.IsAnchorValid);       // should always be valid

                    this.ValidateFieldBasicValues(taxoMultiFieldInfo, fieldMultiFetchedAgain);
                    Assert.AreEqual(Guid.Empty, fieldMultiFetchedAgain.TermSetId);          // term store mapping should've been removed
                    Assert.AreEqual(Guid.Empty, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldMultiFetchedAgain.AnchorId);
                    Assert.IsFalse(fieldMultiFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldMultiFetchedAgain.IsAnchorValid);       // should always be valid
                }
            }
        }

        /// <summary>
        /// Validates that EnsureField doesn't allow you to change the type of a field
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenAttemptingToChangeFieldType_ShouldFailToUpdateAndReturnExistingField()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    Required = RequiredType.NotRequired,
                    MaxLength = 50
                };

                NoteFieldInfo noteFieldInfoWithSameNameAndId = new NoteFieldInfo(   // different type
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),  // same GUID and same internal name
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Required = RequiredType.Required
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // STEP 1: Create the first field
                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField originalField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(textFieldInfo.Id, originalField.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, originalField.InternalName);

                    // STEP 2: Try to create the type-switching evil alternate field
                    SPField alternateEnsuredField = fieldHelper.EnsureField(fieldsCollection, noteFieldInfoWithSameNameAndId);

                    Assert.AreEqual("Text", alternateEnsuredField.TypeAsString);   // not a Note/SPFieldMultilineText

                    // The returned field shouldn't have gotten its properties updated
                    // (as in this shouldn't happen: "Ensure and Update existing other
                    // unrelated field which has clashing Guid/Internal name")
                    Assert.IsFalse(alternateEnsuredField.Required);     // false like original Text field (fail update Note was Required=True)
                }
            }
        }

        #endregion

        #region Ensuring a field directly on a content type should should fail (because only Web or List field collections are supported)

        /// <summary>
        /// Validates that EnsureField goes through site column creation when attempting to
        /// add a field directly on a content type. There should always be a site column defined 
        /// at site-collection level first.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureField_WhenContentTypeFieldCollection_ShouldThrowArgumentException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeInfo = new ContentTypeInfo(SPBuiltInContentTypeId.BasicPage.ToString() + "01", "CTNameKey", "CTDescrKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    SPContentType ensuredContentType = contentTypeHelper.EnsureContentType(testScope.SiteCollection.RootWeb.ContentTypes, contentTypeInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = ensuredContentType.Fields;

                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                }
            }
        }

        #endregion

        #region Ensuring a field directly on a list should ensure site column is present and update list field definition if needed

        /// <summary>
        /// Validates that EnsureField goes through site column creation when attempting to
        /// add a field directly on a list. I.E. to avoid "orphaned" list-only field definitions,
        /// there should always be a site column defined at site-collection level first.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenListFieldCollection_AndSiteColumnDoesntExist_ShouldAddFieldToBothListAndParentRootWeb()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = list.Fields;

                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    SPList testList = testScope.SiteCollection.RootWeb.Lists[list.ID];
                    Assert.IsNotNull(testList.Fields[fieldId]);
                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);    // would be null if we hadn't bothered ensuring the field on the root web
                }
            }
        }

        /// <summary>
        /// Validates that adding a field to a list works but that, if the corresponding site column already exists,
        /// that parent definition shouldn't be updated.
        /// This allows you to ensure a field on a list with a slightly different definition (e.g. different Hidden values,
        /// diffrent term set bindings, different default value) that what was defined on the root web's field definitions.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenListFieldCollection_AndSiteColumnAlreadyExist_ShouldAddFieldToListAndShouldAvoidModifyingSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                                                                                    // set will not be cleaned up and upon next test run we will
                                                                                    // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                defaultSiteCollectionTermStore.CommitAll();

                var textFieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    textFieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50
                };

                var taxoFieldId = new Guid("{9708BECA-D3EF-41C3-ABD3-5F1BAC3CE5AE}");
                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    taxoFieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                var taxoMultiFieldId = new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}");
                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    taxoMultiFieldId,
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    // no term store mapping
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    // 1) Ensure the fields on the site collection with first version of their definition
                    var siteCollectionFields = testScope.SiteCollection.RootWeb.Fields;
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    SPField textSiteColumn = fieldHelper.EnsureField(siteCollectionFields, textFieldInfo);
                    SPField taxoSiteColumn = fieldHelper.EnsureField(siteCollectionFields, taxoFieldInfo);
                    SPField taxoMultiSiteColumn = fieldHelper.EnsureField(siteCollectionFields, taxoMultiFieldInfo);

                    // 2) Change the field definitions slightly
                    textFieldInfo.Required = RequiredType.Required;
                    textFieldInfo.DefaultValue = "SomeDefaultValue";

                    taxoFieldInfo.TermStoreMapping = new TaxonomyContext(levelOneTermA);    // constrain the term to a child term of the term set

                    taxoMultiFieldInfo.TermStoreMapping = new TaxonomyContext(testTermSet); // list column has a mapping, whereas the site column doesn't

                    // 3) Ensure the modified field definitions directly on the list
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    var listFields = list.Fields;
                    SPField textListColumn = fieldHelper.EnsureField(listFields, textFieldInfo);
                    SPField taxoListColumn = fieldHelper.EnsureField(listFields, taxoFieldInfo);
                    SPField taxoMultiListColumn = fieldHelper.EnsureField(listFields, taxoMultiFieldInfo);

                    // 4) Assert that the site column definitions were not touched
                    list = testScope.SiteCollection.RootWeb.Lists[list.ID];

                    // Text field
                    var siteText = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];
                    var listText = list.Fields[textFieldInfo.Id];
                    Assert.IsFalse(siteText.Required);
                    Assert.IsTrue(string.IsNullOrEmpty(siteText.DefaultValue));
                    
                    Assert.IsTrue(listText.Required);
                    Assert.AreEqual("SomeDefaultValue", listText.DefaultValue);

                    // Taxo single field
                    var siteTaxo = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    var listTaxo = (TaxonomyField)list.Fields[taxoFieldInfo.Id];
                    Assert.AreEqual(testTermSet.Id, siteTaxo.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, siteTaxo.SspId);
                    Assert.AreEqual(Guid.Empty, siteTaxo.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(siteTaxo.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, listTaxo.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, listTaxo.SspId);
                    Assert.AreEqual(levelOneTermA.Id, listTaxo.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(listTaxo.IsTermSetValid);
                    Assert.IsTrue(listTaxo.IsAnchorValid);

                    // Taxo multi field
                    var siteTaxoMulti = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];
                    var listTaxoMulti = (TaxonomyField)list.Fields[taxoMultiFieldInfo.Id];

                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.TermSetId);    // empty binding on site column
                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.SspId);
                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.AnchorId);
                    Assert.IsFalse(siteTaxoMulti.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, listTaxoMulti.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, listTaxoMulti.SspId);
                    Assert.AreEqual(Guid.Empty, listTaxoMulti.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(listTaxoMulti.IsTermSetValid);
                }
            }
        }

        /// <summary>
        /// Validates that updating a list field definition works.
        /// This allows you to ensure a field on a list with a slightly different definition (e.g. different Hidden values,
        /// diffrent term set bindings, different default value) that what was defined on the root web's field definitions.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenListFieldCollection_AndListFieldAlreadyExist_ShouldUpdateListColumnDefinition()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                defaultSiteCollectionTermStore.CommitAll();

                var textFieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    textFieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50
                };

                var taxoFieldId = new Guid("{9708BECA-D3EF-41C3-ABD3-5F1BAC3CE5AE}");
                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    taxoFieldId,
                    "NameKeyTaxo",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                var taxoMultiFieldId = new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}");
                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    taxoMultiFieldId,
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    // no term store mapping
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    // 1) Ensure the fields on the list for the first time
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    var listFields = list.Fields;

                    SPField textListColumn = fieldHelper.EnsureField(listFields, textFieldInfo);
                    SPField taxoListColumn = fieldHelper.EnsureField(listFields, taxoFieldInfo);
                    SPField taxoMultiListColumn = fieldHelper.EnsureField(listFields, taxoMultiFieldInfo);

                    // 2) Change the field definitions slightly
                    textFieldInfo.Required = RequiredType.Required;
                    textFieldInfo.DefaultValue = "SomeDefaultValue";

                    taxoFieldInfo.TermStoreMapping = new TaxonomyContext(levelOneTermA);    // constrain the term to a child term of the term set

                    taxoMultiFieldInfo.TermStoreMapping = new TaxonomyContext(testTermSet); // list column has a mapping, whereas the site column doesn't

                    // Act
                    // 3) Ensure the modified field definitions on the list (second Ensure)
                    IListLocator listLocator = injectionScope.Resolve<IListLocator>();
                    list = listLocator.GetByUrl(testScope.SiteCollection.RootWeb, "sometestlistpath");
                    listFields = list.Fields;   // refetch the fields (to detect any missing Update() calls)
                    textListColumn = fieldHelper.EnsureField(listFields, textFieldInfo);
                    taxoListColumn = fieldHelper.EnsureField(listFields, taxoFieldInfo);
                    taxoMultiListColumn = fieldHelper.EnsureField(listFields, taxoMultiFieldInfo);

                    // 4) Assert that the site column definitions were not touched
                    list = testScope.SiteCollection.RootWeb.Lists[list.ID];

                    // Text field
                    var siteText = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];   // site-wide version of field should have first definition
                    var listText = list.Fields[textFieldInfo.Id];   // list-specific version of field should contain the update definition
                    Assert.IsFalse(siteText.Required);
                    Assert.IsTrue(string.IsNullOrEmpty(siteText.DefaultValue));

                    Assert.IsTrue(listText.Required);
                    Assert.AreEqual("SomeDefaultValue", listText.DefaultValue);

                    // Taxo single field
                    var siteTaxo = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    var listTaxo = (TaxonomyField)list.Fields[taxoFieldInfo.Id];
                    Assert.AreEqual(testTermSet.Id, siteTaxo.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, siteTaxo.SspId);
                    Assert.AreEqual(Guid.Empty, siteTaxo.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(siteTaxo.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, listTaxo.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, listTaxo.SspId);
                    Assert.AreEqual(levelOneTermA.Id, listTaxo.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(listTaxo.IsTermSetValid);
                    Assert.IsTrue(listTaxo.IsAnchorValid);

                    // Taxo multi field
                    var siteTaxoMulti = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];
                    var listTaxoMulti = (TaxonomyField)list.Fields[taxoMultiFieldInfo.Id];

                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.TermSetId);    // empty binding on site column
                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.SspId);
                    Assert.AreEqual(Guid.Empty, siteTaxoMulti.AnchorId);
                    Assert.IsFalse(siteTaxoMulti.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, listTaxoMulti.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, listTaxoMulti.SspId);
                    Assert.AreEqual(Guid.Empty, listTaxoMulti.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(listTaxoMulti.IsTermSetValid);
                }
            }
        }

        #endregion

        #region Ensuring a field on a sub-web should ensure site column exists on root web instead and prevent you from defining subweb-specific fields

        /// <summary>
        /// Validates that EnsureField goes through site column creation when attempting to
        /// add a field directly on a sub-web (sneaky, sneaky). I.E. to avoid "orphaned" sub-web-only field definitions,
        /// there should always be a site column defined at site-collection level first.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenSubWebFieldCollection_AndSiteColumnDoesntExist_ShouldAddFieldParentRootWebInAReallySneakyWay()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    SPWeb subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");
                    
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = subWeb.Fields;

                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    SPWeb testSubWeb = testScope.SiteCollection.RootWeb.Webs["subweb"];

                    try
                    {
                        var shouldBeMissingAndThrowException = testSubWeb.Fields[fieldId];
                        Assert.Fail();
                    }
                    catch (ArgumentException) 
                    { 
                        // we got sneaky and created the site column on the root web instead 
                        // (customizing a field definition in a sub-web is impossible once the rootweb
                        // column exists)
                    }

                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);    // would be null if we hadn't bothered ensuring the field on the root web
                }
            }
        }

        /// <summary>
        /// Validates that EnsureField doesn't allow you to re-define a site column in a sub-web
        /// when the RootWeb field already exists.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnsureField_WhenSubWebFieldCollection_AndSiteColumnAlreadyExist_ShouldThrowExceptionToShowHowImpossibleThisIs()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    SPWeb subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var rootWebFields = testScope.SiteCollection.RootWeb.Fields;
                    var subWebFields = subWeb.Fields;

                    SPField field = fieldHelper.EnsureField(rootWebFields, textFieldInfo);
                    textFieldInfo.Required = RequiredType.NotRequired;

                    // Act + Assert
                    // Should be impossible to re-define a field that already exists on root web
                    SPField sameSubWebFieldShouldThrowException = fieldHelper.EnsureField(subWebFields, textFieldInfo);
                }
            }
        }

        #endregion
        
        #region Text+Note+Html field type-specific values should be mapped (DefaultValue, EnforceUniqueValue, etc.)

        /// <summary>
        /// Validate that default value is set on Text+Note+HtmlFieldInfo type fields
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTextOrNoteOrHtmlFieldInfo_ShouldApplyStringDefaultValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = "Text default value",
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
                    DefaultValue = "HTML default value"
                };

                TextFieldInfo noValueTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameDefaultText",
                    new Guid("{7BEB995F-C696-453B-BA86-09A32381C783}"),
                    "NameKeyDefaults",
                    "DescriptionKeyDefaults",
                    "GroupKey");

                NoteFieldInfo noValueNoteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameDefaultNote",
                    new Guid("{0BB1677D-9B14-4EE8-ADB9-53834D5FD516}"),
                    "NameKeyDefaults",
                    "DescriptionKeyDefaults",
                    "GroupKey");

                HtmlFieldInfo noValueHtmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameDefaultHtml",
                    new Guid("{4B44FCBE-A8C3-43FB-9633-C2F89F28032D}"),
                    "NameKeyDefaults",
                    "DescriptionKeyDefaults",
                    "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Text field definition (with/without default value)
                    SPField textField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                    Assert.AreEqual("Text default value", textField.DefaultValue);
                    SPField textFieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    Assert.AreEqual("Text default value", textFieldRefetched.DefaultValue);

                    SPField noDefaultValueTextField = fieldHelper.EnsureField(fieldsCollection, noValueTextFieldInfo);
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueTextField.DefaultValue));
                    SPField noDefaultValueTextFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noValueTextFieldInfo.Id];
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueTextFieldRefetched.DefaultValue));

                    // 1) Note field definition (with/without default value)
                    SPField noteField = fieldHelper.EnsureField(fieldsCollection, noteFieldInfo);
                    Assert.AreEqual("Note default value", noteField.DefaultValue);
                    SPField noteFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noteFieldInfo.Id];
                    Assert.AreEqual("Note default value", noteFieldRefetched.DefaultValue);

                    SPField noDefaultValueNoteField = fieldHelper.EnsureField(fieldsCollection, noValueNoteFieldInfo);
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueNoteField.DefaultValue));
                    SPField noDefaultValueNoteFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noValueNoteFieldInfo.Id];
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueNoteFieldRefetched.DefaultValue));

                    // 3) HTML field definition (with/without default value)
                    SPField htmlField = fieldHelper.EnsureField(fieldsCollection, htmlFieldInfo);
                    Assert.AreEqual("HTML default value", htmlField.DefaultValue);
                    SPField htmlFieldRefetched = testScope.SiteCollection.RootWeb.Fields[htmlFieldInfo.Id];
                    Assert.AreEqual("HTML default value", htmlFieldRefetched.DefaultValue);

                    SPField noDefaultValueHtmlField = fieldHelper.EnsureField(fieldsCollection, noValueHtmlFieldInfo);
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueHtmlField.DefaultValue));
                    SPField noDefaultValueHtmlFieldRefetched = testScope.SiteCollection.RootWeb.Fields[noValueHtmlFieldInfo.Id];
                    Assert.IsTrue(string.IsNullOrEmpty(noDefaultValueHtmlFieldRefetched.DefaultValue));
                }
            }
        }

        /// <summary>
        /// Validate that default value is set on TextFieldInfo type fields
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTextFieldInfo_ShouldApplyEnforceUniqueValuesProperty()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                TextFieldInfo noValueTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameDefaultText",
                    new Guid("{7BEB995F-C696-453B-BA86-09A32381C783}"),
                    "NameKeyDefaults",
                    "DescriptionKeyDefaults",
                    "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Text field on/off
                    SPField textField = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                    Assert.AreEqual(textFieldInfo.EnforceUniqueValues, textField.EnforceUniqueValues);  // both should be true
   
                    SPField defaultValueTextField = fieldHelper.EnsureField(fieldsCollection, noValueTextFieldInfo);
                    Assert.AreEqual(false, defaultValueTextField.EnforceUniqueValues);  // default should be false
                }
            }
        }
        #endregion

        #region Taxonomy field type-specific values should be mapped (DefaultValue, TermStoreMapping, etc.)

        /// <summary>
        /// Validated that the term store mapping is properly applied to taxonomy column
        /// when we're dealing with Site Collection-specific term group (i.e. the kind of
        /// term store group that is created with Publishing Site automatically and which 
        /// is only visible from within that site's settings)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndWebField_AndSiteCollectionSpecificTermSet_ShouldApplyTermSetMappingToSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
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
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Act
                    TaxonomyField fieldSingle = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    TaxonomyField fieldMulti = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    // Assert
                    Assert.IsNotNull(fieldSingle);
                    Assert.AreEqual(testTermSet.Id, fieldSingle.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingle.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingle.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingle.IsTermSetValid);

                    Assert.IsNotNull(fieldMulti);
                    Assert.AreEqual(testTermSet.Id, fieldMulti.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMulti.SspId);
                    Assert.AreEqual(levelOneTermA.Id, fieldMulti.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(fieldMulti.IsTermSetValid);
                    Assert.IsTrue(fieldMulti.IsAnchorValid);   

                    // Gotta also make sure (by fetching the fields again) that the field properties were all persisted
                    TaxonomyField fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    TaxonomyField fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];

                    Assert.IsNotNull(fieldSingleFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingleFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);

                    Assert.IsNotNull(fieldMultiFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldMultiFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(levelOneTermA.Id, fieldMultiFetchedAgain.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(fieldMultiFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldMultiFetchedAgain.IsAnchorValid);                    
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validated that the term store mapping is properly applied to taxonomy column
        /// when we're dealing with Farm-wide term groups (i.e. the kind of
        /// term store group that is created by a farm administrator and which is
        /// visible from all site collections)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndWebField_AndGlobalFarmWideTermSet_ShouldApplyTermSetMappingToSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                Guid testGroupId = new Guid("{B7B56932-E191-46C7-956F-4C6E5E4F6020}");
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set") // keep Ids random because, if this test fails midway, the term
                    {
                        // must specify group, otherwise we would be describing a term set belonging to a site-specific group
                        Group = new TermGroupInfo(testGroupId, "Dynamite Test Group")
                    };

                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;

                // Cleanup group (maybe the test failed last time and the old group ended up polluting the term store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);
              
                Group testGroup = defaultSiteCollectionTermStore.CreateGroup("Dynamite Test Group", testGroupId);
                TermSet newTermSet = testGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    // Assert
                    TaxonomyField fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    TaxonomyField fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];

                    Assert.IsNotNull(fieldSingleFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingleFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);

                    Assert.IsNotNull(fieldMultiFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldMultiFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(levelOneTermA.Id, fieldMultiFetchedAgain.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldSingleFetchedAgain.IsAnchorValid);
                }

                // Cleanup term group so that we don't pollute the metadata store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);
            }
        }

        /// <summary>
        /// Validated that the term store mapping is properly applied to list-specifc taxonomy column
        /// when we're dealing with Site Collection-specific term group (i.e. the kind of
        /// term store group that is created with Publishing Site automatically and which 
        /// is only visible from within that site's settings)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndListField_AndSiteCollectionSpecificTermSet_ShouldApplyTermSetMappingToListColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
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
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = list.Fields;

                    // Ensure one of the two on the root web (tweak the definition a little bit on the list def)
                    fieldHelper.EnsureField(testScope.SiteCollection.RootWeb.Fields, taxoMultiFieldInfo);
                    taxoMultiFieldInfo.Required = RequiredType.Required;
                    taxoMultiFieldInfo.TermStoreMapping = new TaxonomyContext(levelTwoTermAB);

                    // Act
                    TaxonomyField fieldSingle = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    TaxonomyField fieldMulti = (TaxonomyField)fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    // Assert
                    Assert.AreEqual(testTermSet.Id, fieldSingle.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingle.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingle.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingle.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, fieldMulti.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMulti.SspId);
                    Assert.AreEqual(levelTwoTermAB.Id, fieldMulti.AnchorId);    // choices should be constrained to a 2nd level child term
                    Assert.IsTrue(fieldMulti.IsTermSetValid);
                    Assert.IsTrue(fieldMulti.IsAnchorValid);
                    Assert.IsTrue(fieldMulti.Required);

                    // Gotta also make sure (by fetching the fields again) that the field properties were all persisted
                    TaxonomyField fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoFieldInfo.Id];
                    TaxonomyField fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoMultiFieldInfo.Id];

                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingleFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);

                    Assert.AreEqual(testTermSet.Id, fieldMultiFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(levelTwoTermAB.Id, fieldMultiFetchedAgain.AnchorId);    // choices should be constrained to a 2nd level child term
                    Assert.IsTrue(fieldMultiFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldMultiFetchedAgain.IsAnchorValid);
                    Assert.IsTrue(fieldMultiFetchedAgain.Required);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validated that the term store mapping is properly applied to list-specific taxonomy column
        /// when we're dealing with Farm-wide term groups (i.e. the kind of
        /// term store group that is created by a farm administrator and which is
        /// visible from all site collections)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndListField_AndGlobalFarmWideTermSet_ShouldApplyTermSetMappingToListColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                Guid testGroupId = new Guid("{B7B56932-E191-46C7-956F-4C6E5E4F6020}");
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set") // keep Ids random because, if this test fails midway, the term
                {
                    // must specify group, otherwise we would be describing a term set belonging to a site-specific group
                    Group = new TermGroupInfo(testGroupId, "Dynamite Test Group")
                };

                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;

                // Cleanup group (maybe the test failed last time and the old group ended up polluting the term store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);

                Group testGroup = defaultSiteCollectionTermStore.CreateGroup("Dynamite Test Group", testGroupId);
                TermSet newTermSet = testGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = list.Fields;

                    // Ensure one of the two on the root web (tweak the definition a little bit on the list def)
                    fieldHelper.EnsureField(testScope.SiteCollection.RootWeb.Fields, taxoMultiFieldInfo);
                    taxoMultiFieldInfo.Required = RequiredType.Required;
                    taxoMultiFieldInfo.TermStoreMapping = new TaxonomyContext(levelTwoTermAB);

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    // Assert
                    TaxonomyField fieldSingleFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoFieldInfo.Id];
                    TaxonomyField fieldMultiFetchedAgain = (TaxonomyField)testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoMultiFieldInfo.Id];

                    Assert.IsNotNull(fieldSingleFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldSingleFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldSingleFetchedAgain.SspId);
                    Assert.AreEqual(Guid.Empty, fieldSingleFetchedAgain.AnchorId);    // choices should not be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);

                    Assert.IsNotNull(fieldMultiFetchedAgain);
                    Assert.AreEqual(testTermSet.Id, fieldMultiFetchedAgain.TermSetId);
                    Assert.AreEqual(defaultSiteCollectionTermStore.Id, fieldMultiFetchedAgain.SspId);
                    Assert.AreEqual(levelTwoTermAB.Id, fieldMultiFetchedAgain.AnchorId);    // choices should be constrained to a child term
                    Assert.IsTrue(fieldSingleFetchedAgain.IsTermSetValid);
                    Assert.IsTrue(fieldSingleFetchedAgain.IsAnchorValid);
                }

                // Cleanup term group so that we don't pollute the metadata store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);
            }
        }

        /// <summary>
        /// Validates that Taxonomy (single and multi) default value is set properly (with fully initialized lookup IDs to TaxonomyHiddenList) 
        /// on Web fields when linking to a term set belonging to a farm-wide term group.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndWebField_AndGlobalTermSet_ShouldApplyDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                Guid testGroupId = new Guid("{B7B56932-E191-46C7-956F-4C6E5E4F6020}");
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set") // keep Ids random because, if this test fails midway, the term
                {
                    // must specify group, otherwise we would be describing a term set belonging to a site-specific group
                    Group = new TermGroupInfo(testGroupId, "Dynamite Test Group")
                };

                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;

                // Cleanup group (maybe the test failed last time and the old group ended up polluting the term store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);

                Group testGroup = defaultSiteCollectionTermStore.CreateGroup("Dynamite Test Group", testGroupId);
                TermSet newTermSet = testGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValue(levelOneTermA),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>() 
                            { 
                                new TaxonomyValue(levelTwoTermAA), 
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    var fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    var fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    // Assert
                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    // Same asserts, but on re-fetched field (to make sure DefaultValue was persisted properly)
                    SPField fieldSingleRefetched = testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    SPField fieldMultiRefetched = testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];

                    fieldValue = new TaxonomyFieldValue(fieldSingleRefetched.DefaultValue);
                    fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMultiRefetched.DefaultValue);

                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));
                }

                // Cleanup term group so that we don't pollute the metadata store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);
            }
        }

        /// <summary>
        /// Validates that Taxonomy (single and multi) default value is set properly (with fully initialized lookup IDs to TaxonomyHiddenList) 
        /// on List fields when linking to a term set belonging to a farm-wide term group.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndListField_AndGlobalTermSet_ShouldApplyDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                Guid testGroupId = new Guid("{B7B56932-E191-46C7-956F-4C6E5E4F6020}");
                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set") // keep Ids random because, if this test fails midway, the term
                {
                    // must specify group, otherwise we would be describing a term set belonging to a site-specific group
                    Group = new TermGroupInfo(testGroupId, "Dynamite Test Group")
                };

                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;

                // Cleanup group (maybe the test failed last time and the old group ended up polluting the term store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);

                Group testGroup = defaultSiteCollectionTermStore.CreateGroup("Dynamite Test Group", testGroupId);
                TermSet newTermSet = testGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValue(levelOneTermA),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>() 
                            { 
                                new TaxonomyValue(levelTwoTermAA), 
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = list.Fields;

                    // Ensure one of the two on the root web (tweak the definition a little bit on the list def)
                    fieldHelper.EnsureField(testScope.SiteCollection.RootWeb.Fields, taxoMultiFieldInfo);
                    taxoMultiFieldInfo.Required = RequiredType.Required;

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    var fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    var fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    // Assert
                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    Assert.IsTrue(fieldMulti.Required);

                    // Same asserts, but on re-fetched field (to make sure DefaultValue was persisted properly)
                    SPField fieldSingleRefetched = testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoFieldInfo.Id];
                    SPField fieldMultiRefetched = testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoMultiFieldInfo.Id];

                    fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    Assert.IsTrue(fieldMultiRefetched.Required);
                }

                // Cleanup term group so that we don't pollute the metadata store
                this.DeleteGroupIfExists(defaultSiteCollectionTermStore, testGroupId);
            }
        }

        /// <summary>
        /// Validates that Taxonomy (single and multi) default value is set properly (with fully initialized lookup IDs to TaxonomyHiddenList) 
        /// on Web fields when linking to a term set belonging to a local site-collection-specific term group.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndWebField_AndSiteCollectionSpecificTermSet_ShouldApplyDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
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
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValue(levelOneTermA),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>() 
                            { 
                                new TaxonomyValue(levelTwoTermAA), 
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    var fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    var fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    // Assert
                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    // Same asserts, but on re-fetched field (to make sure DefaultValue was persisted properly)
                    SPField fieldSingleRefetched = testScope.SiteCollection.RootWeb.Fields[taxoFieldInfo.Id];
                    SPField fieldMultiRefetched = testScope.SiteCollection.RootWeb.Fields[taxoMultiFieldInfo.Id];

                    fieldValue = new TaxonomyFieldValue(fieldSingleRefetched.DefaultValue);
                    fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMultiRefetched.DefaultValue);

                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that Taxonomy (single and multi) default value is set properly (with fully initialized lookup IDs to TaxonomyHiddenList) 
        /// on List fields when linking to a term set belonging to a local site-collection-specific term group.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenTaxonomySingleOrMultiAndListField_AndSiteCollectionSpecificTermSet_ShouldApplyDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
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
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValue(levelOneTermA),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{B2517ECF-819E-4F75-88AF-18E926AD30BD}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>() 
                            { 
                                new TaxonomyValue(levelTwoTermAA), 
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = list.Fields;

                    // Ensure one of the two on the root web (tweak the definition a little bit on the list def)
                    fieldHelper.EnsureField(testScope.SiteCollection.RootWeb.Fields, taxoMultiFieldInfo);
                    taxoMultiFieldInfo.Required = RequiredType.Required;

                    // Act
                    SPField fieldSingle = fieldHelper.EnsureField(fieldsCollection, taxoFieldInfo);
                    SPField fieldMulti = fieldHelper.EnsureField(fieldsCollection, taxoMultiFieldInfo);

                    var fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    var fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    // Assert
                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    Assert.IsTrue(fieldMulti.Required);

                    // Same asserts, but on re-fetched field (to make sure DefaultValue was persisted properly)
                    SPField fieldSingleRefetched = testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoFieldInfo.Id];
                    SPField fieldMultiRefetched = testScope.SiteCollection.RootWeb.Lists[list.ID].Fields[taxoMultiFieldInfo.Id];

                    fieldValue = new TaxonomyFieldValue(fieldSingle.DefaultValue);
                    fieldMultiValueCollection = new TaxonomyFieldValueCollection(fieldMulti.DefaultValue);

                    Assert.AreNotEqual(-1, fieldValue.WssId);   // a lookup ID to the TaxonomyHiddenList should be properly initialized at all times (lookup ID == -1 means you're depending on too much magic)
                    Assert.AreEqual("Term A", fieldValue.Label);
                    Assert.AreEqual(levelOneTermA.Id, new Guid(fieldValue.TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[0].WssId);     // lookup ID to TaxoHiddenList should also be initialized on multi-values
                    Assert.AreEqual("Term A-A", fieldMultiValueCollection[0].Label);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(fieldMultiValueCollection[0].TermGuid));

                    Assert.AreNotEqual(-1, fieldMultiValueCollection[1].WssId);
                    Assert.AreEqual("Term A-B", fieldMultiValueCollection[1].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(fieldMultiValueCollection[1].TermGuid));

                    Assert.IsTrue(fieldMultiRefetched.Required);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        #endregion

        #region Other field types (Number+Guid+Url+Image+etc.) should get their field type-specific properties and DefaultValue mapped

        /// <summary>
        /// Validates that Number field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenNumberField_ShouldApplyNumberFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInt",
                    new Guid("{6816676E-4952-4CED-8EB2-F15FDD7A595B}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey")
                {
                    DefaultValue = 7777
                };

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                NumberFieldInfo numberFieldInfoAlt = new NumberFieldInfo(
                    "TestInternalNameNumberAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Decimals = 3,
                    IsPercentage = true,
                    Min = 5,
                    Max = 500.555,
                    DefaultValue = 77.77
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic integer field definition
                    SPFieldNumber integerField = (SPFieldNumber)fieldHelper.EnsureField(fieldsCollection, integerFieldInfo);
                    this.ValidateFieldBasicValues(integerFieldInfo, integerField);
                    Assert.AreEqual(SPNumberFormatTypes.Automatic, integerField.DisplayFormat);
                    Assert.IsFalse(integerField.ShowAsPercentage);
                    Assert.AreEqual("7777", integerField.DefaultValue);

                    SPFieldNumber integerFieldRefetched = (SPFieldNumber)testScope.SiteCollection.RootWeb.Fields[integerFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(integerFieldInfo, integerFieldRefetched);
                    Assert.AreEqual(SPNumberFormatTypes.Automatic, integerFieldRefetched.DisplayFormat);
                    Assert.IsFalse(integerFieldRefetched.ShowAsPercentage);
                    Assert.AreEqual("7777", integerFieldRefetched.DefaultValue);

                    // 2) Basic nunber field definition (all default property values)
                    SPFieldNumber numberField = (SPFieldNumber)fieldHelper.EnsureField(fieldsCollection, numberFieldInfo);
                    this.ValidateFieldBasicValues(numberFieldInfo, numberField);
                    Assert.AreEqual(SPNumberFormatTypes.NoDecimal, numberField.DisplayFormat);
                    Assert.IsFalse(numberField.ShowAsPercentage);
                    Assert.IsTrue(string.IsNullOrEmpty(numberField.DefaultValue));

                    SPFieldNumber numberFieldRefetched = (SPFieldNumber)testScope.SiteCollection.RootWeb.Fields[numberFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(numberFieldInfo, numberFieldRefetched);
                    Assert.AreEqual(SPNumberFormatTypes.NoDecimal, numberFieldRefetched.DisplayFormat);
                    Assert.IsFalse(numberFieldRefetched.ShowAsPercentage);
                    Assert.IsTrue(string.IsNullOrEmpty(numberFieldRefetched.DefaultValue));

                    // 3) Alternate number field definition (with all property values customized and a default value assigned)
                    SPFieldNumber numberFieldAlt = (SPFieldNumber)fieldHelper.EnsureField(fieldsCollection, numberFieldInfoAlt);
                    this.ValidateFieldBasicValues(numberFieldInfoAlt, numberFieldAlt);
                    Assert.AreEqual(SPNumberFormatTypes.ThreeDecimals, numberFieldAlt.DisplayFormat);
                    Assert.IsTrue(numberFieldAlt.ShowAsPercentage);
                    Assert.AreEqual(5, numberFieldAlt.MinimumValue);
                    Assert.AreEqual(500.555, numberFieldAlt.MaximumValue);
                    Assert.AreEqual("77.77", numberFieldAlt.DefaultValue);

                    SPFieldNumber numberFieldAltRefetched = (SPFieldNumber)testScope.SiteCollection.RootWeb.Fields[numberFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(numberFieldInfoAlt, numberFieldAltRefetched);
                    Assert.AreEqual(SPNumberFormatTypes.ThreeDecimals, numberFieldAltRefetched.DisplayFormat);
                    Assert.IsTrue(numberFieldAltRefetched.ShowAsPercentage);
                    Assert.AreEqual(5, numberFieldAltRefetched.MinimumValue);
                    Assert.AreEqual(500.555, numberFieldAltRefetched.MaximumValue);
                    Assert.AreEqual("77.77", numberFieldAltRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that DateTime field type properties are mapped along with its formula or default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenDateTimeField_ShouldApplyDateTimeFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDateTime",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                DateTimeFieldInfo dateTimeFieldInfoWithFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Format = DateTimeFieldFormat.DateTime,
                    DefaultFormula = "=[Today]",
                    HasFriendlyRelativeDisplay = true
                };

                DateTimeFieldInfo dateTimeFieldInfoWithDefaultValue = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{2F901D99-F2B4-41E6-BD06-4B2D69BA363F}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Format = DateTimeFieldFormat.DateTime,
                    DefaultValue = new DateTime(1999, 1, 28),
                    HasFriendlyRelativeDisplay = true
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic datetime field definition (all default property values)
                    SPFieldDateTime dateTimeField = (SPFieldDateTime)fieldHelper.EnsureField(fieldsCollection, dateTimeFieldInfo);
                    this.ValidateFieldBasicValues(dateTimeFieldInfo, dateTimeField);
                    Assert.AreEqual(SPDateTimeFieldFormatType.DateOnly, dateTimeField.DisplayFormat);
                    Assert.AreEqual(SPDateTimeFieldFriendlyFormatType.Disabled, dateTimeField.FriendlyDisplayFormat);
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeField.DefaultFormula));
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeField.DefaultValue));

                    SPFieldDateTime dateTimeFieldRefetched = (SPFieldDateTime)testScope.SiteCollection.RootWeb.Fields[dateTimeFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(dateTimeFieldInfo, dateTimeFieldRefetched);
                    Assert.AreEqual(SPDateTimeFieldFormatType.DateOnly, dateTimeFieldRefetched.DisplayFormat);
                    Assert.AreEqual(SPDateTimeFieldFriendlyFormatType.Disabled, dateTimeFieldRefetched.FriendlyDisplayFormat);
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeFieldRefetched.DefaultFormula));
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeFieldRefetched.DefaultValue));

                    // 2) Alternate datetime field definition (with all property values customized and a Calculated Formula assigned)
                    SPFieldDateTime dateTimeFieldWithFormula = (SPFieldDateTime)fieldHelper.EnsureField(fieldsCollection, dateTimeFieldInfoWithFormula);
                    this.ValidateFieldBasicValues(dateTimeFieldInfoWithFormula, dateTimeFieldWithFormula);
                    Assert.AreEqual(SPDateTimeFieldFormatType.DateTime, dateTimeFieldWithFormula.DisplayFormat);
                    Assert.AreEqual(SPDateTimeFieldFriendlyFormatType.Relative, dateTimeFieldWithFormula.FriendlyDisplayFormat);
                    Assert.AreEqual("=[Today]", dateTimeFieldWithFormula.DefaultFormula);
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeFieldWithFormula.DefaultValue));

                    // 3) Alternate datetime field definition #2 (with all property values customized and a Default Value assigned)
                    SPFieldDateTime dateTimeFieldWithDefaultValue = (SPFieldDateTime)fieldHelper.EnsureField(fieldsCollection, dateTimeFieldInfoWithDefaultValue);
                    this.ValidateFieldBasicValues(dateTimeFieldInfoWithDefaultValue, dateTimeFieldWithDefaultValue);
                    Assert.AreEqual(SPDateTimeFieldFormatType.DateTime, dateTimeFieldWithDefaultValue.DisplayFormat);
                    Assert.AreEqual(SPDateTimeFieldFriendlyFormatType.Relative, dateTimeFieldWithDefaultValue.FriendlyDisplayFormat);
                    Assert.IsTrue(string.IsNullOrEmpty(dateTimeFieldWithDefaultValue.DefaultFormula));
                    Assert.AreEqual("1999-01-28T00:00:00Z", dateTimeFieldWithDefaultValue.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that DateTime field type properties are mapped along with its formula or default value
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnsureField_WhenDateTimeField_AndBothFormulaAndDefaultValueSpecified_ShouldThrowExceptionToWarnYouThatYouShouldOnlySpecifyOneOfTheTwo()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                DateTimeFieldInfo dateTimeFieldInfoWithFormulaAndDefaultValue = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Format = DateTimeFieldFormat.DateTime,
                    DefaultFormula = "=[Today]",
                    DefaultValue = new DateTime(1999, 1, 28),   // both formula and defaul val are specified
                    HasFriendlyRelativeDisplay = true
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Creating field should fail (only formula OR default value should be specified)
                    SPFieldDateTime dateTimeField = (SPFieldDateTime)fieldHelper.EnsureField(fieldsCollection, dateTimeFieldInfoWithFormulaAndDefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that Number field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenGuidField_ShouldApplyGuidFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                GuidFieldInfo guidFieldInfo = new GuidFieldInfo(
                    "TestInternalNameGuid",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                GuidFieldInfo guidFieldInfoAlt = new GuidFieldInfo(
                    "TestInternalNameGuidAlt",
                    new Guid("{04EDC708-CD42-434D-860D-85D8CF09AE3D}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = new Guid("{365193B4-77F9-4C69-A131-6963B3DE3C38}")
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic guid field definition (all default property values)
                    SPFieldGuid guidField = (SPFieldGuid)fieldHelper.EnsureField(fieldsCollection, guidFieldInfo);
                    this.ValidateFieldBasicValues(guidFieldInfo, guidField);
                    Assert.IsTrue(string.IsNullOrEmpty(guidField.DefaultValue));

                    SPFieldGuid guidFieldRefetched = (SPFieldGuid)testScope.SiteCollection.RootWeb.Fields[guidFieldInfo.Id];
                    this.ValidateFieldBasicValues(guidFieldInfo, guidFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(guidFieldRefetched.DefaultValue));

                    // 2) Guid field with a default value
                    SPFieldGuid guidFieldAlt = (SPFieldGuid)fieldHelper.EnsureField(fieldsCollection, guidFieldInfoAlt);
                    this.ValidateFieldBasicValues(guidFieldInfoAlt, guidFieldAlt);
                    Assert.AreEqual(new Guid("{365193B4-77F9-4C69-A131-6963B3DE3C38}"), new Guid(guidFieldAlt.DefaultValue));

                    SPFieldGuid guidFieldAltRefetched = (SPFieldGuid)testScope.SiteCollection.RootWeb.Fields[guidFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(guidFieldInfoAlt, guidFieldAltRefetched);
                    Assert.AreEqual(new Guid("{365193B4-77F9-4C69-A131-6963B3DE3C38}"), new Guid(guidFieldAltRefetched.DefaultValue));
                }
            }
        }

        /// <summary>
        /// Validates that Boolean field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenBooleanField_ShouldApplyBooleanFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                BooleanFieldInfo booleanFieldInfo = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                BooleanFieldInfo booleanFieldInfoTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0645A21C-4D08-4EDF-8618-55DC46CA0842}"),
                    "NameKeyTrue",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo booleanFieldInfoFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{34006DFA-3EE0-4471-9076-B2B940F350F6}"),
                    "NameKeyFalse",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic boolean field definition (all default property values)
                    SPFieldBoolean booleanField = (SPFieldBoolean)fieldHelper.EnsureField(fieldsCollection, booleanFieldInfo);
                    this.ValidateFieldBasicValues(booleanFieldInfo, booleanField);

                    SPFieldBoolean booleanFieldRefetched = (SPFieldBoolean)testScope.SiteCollection.RootWeb.Fields[booleanFieldInfo.Id];
                    this.ValidateFieldBasicValues(booleanFieldInfo, booleanFieldRefetched);

                    // 2) Boolean field with a default value = TRUE
                    SPFieldBoolean booleanFieldTrue = (SPFieldBoolean)fieldHelper.EnsureField(fieldsCollection, booleanFieldInfoTrue);
                    this.ValidateFieldBasicValues(booleanFieldInfoTrue, booleanFieldTrue);
                    Assert.AreEqual("True", booleanFieldTrue.DefaultValue);

                    SPFieldBoolean booleanFieldTrueRefetched = (SPFieldBoolean)testScope.SiteCollection.RootWeb.Fields[booleanFieldInfoTrue.Id];
                    this.ValidateFieldBasicValues(booleanFieldInfoTrue, booleanFieldTrueRefetched);
                    Assert.AreEqual("True", booleanFieldTrueRefetched.DefaultValue);

                    // 3) Boolean field with a default value = FALSE
                    SPFieldBoolean booleanFieldFalse = (SPFieldBoolean)fieldHelper.EnsureField(fieldsCollection, booleanFieldInfoFalse);
                    this.ValidateFieldBasicValues(booleanFieldInfoFalse, booleanFieldFalse);
                    Assert.AreEqual("False", booleanFieldFalse.DefaultValue);

                    SPFieldBoolean booleanFieldFalseRefetched = (SPFieldBoolean)testScope.SiteCollection.RootWeb.Fields[booleanFieldInfoFalse.Id];
                    this.ValidateFieldBasicValues(booleanFieldInfoFalse, booleanFieldFalseRefetched);
                    Assert.AreEqual("False", booleanFieldFalseRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that Currency field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenCurrencyField_ShouldApplyCurrencyFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                CurrencyFieldInfo currencyFieldInfoAlt = new CurrencyFieldInfo(
                    "TestInternalNameCurrencyAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    LocaleId = new CultureInfo("fr-CA").LCID,
                    Min = 5,
                    Max = 500.99,
                    DefaultValue = 77.77
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic nunber field definition (all default property values)
                    SPFieldCurrency currencyField = (SPFieldCurrency)fieldHelper.EnsureField(fieldsCollection, currencyFieldInfo);
                    this.ValidateFieldBasicValues(currencyFieldInfo, currencyField);
                    Assert.AreEqual(1033, currencyField.CurrencyLocaleId);
                    Assert.IsFalse(currencyField.ShowAsPercentage);
                    Assert.IsTrue(string.IsNullOrEmpty(currencyField.DefaultValue));

                    SPFieldCurrency currencyFieldRefetched = (SPFieldCurrency)testScope.SiteCollection.RootWeb.Fields[currencyFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(currencyFieldInfo, currencyFieldRefetched);
                    Assert.AreEqual(1033, currencyFieldRefetched.CurrencyLocaleId);
                    Assert.IsFalse(currencyFieldRefetched.ShowAsPercentage);
                    Assert.IsTrue(string.IsNullOrEmpty(currencyFieldRefetched.DefaultValue));

                    // 2) Alternate currency field definition (with all property values customized and a default value assigned)
                    SPFieldCurrency currencyFieldAlt = (SPFieldCurrency)fieldHelper.EnsureField(fieldsCollection, currencyFieldInfoAlt);
                    this.ValidateFieldBasicValues(currencyFieldInfoAlt, currencyFieldAlt);
                    Assert.AreEqual(3084, currencyFieldAlt.CurrencyLocaleId);
                    Assert.IsFalse(currencyFieldAlt.ShowAsPercentage);
                    Assert.AreEqual(5, currencyFieldAlt.MinimumValue);
                    Assert.AreEqual(500.99, currencyFieldAlt.MaximumValue);
                    Assert.AreEqual("77.77", currencyFieldAlt.DefaultValue);

                    SPFieldCurrency currencyFieldAltRefetched = (SPFieldCurrency)testScope.SiteCollection.RootWeb.Fields[currencyFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(currencyFieldInfoAlt, currencyFieldAltRefetched);
                    Assert.AreEqual(3084, currencyFieldAltRefetched.CurrencyLocaleId);
                    Assert.IsFalse(currencyFieldAltRefetched.ShowAsPercentage);
                    Assert.AreEqual(5, currencyFieldAltRefetched.MinimumValue);
                    Assert.AreEqual(500.99, currencyFieldAltRefetched.MaximumValue);
                    Assert.AreEqual("77.77", currencyFieldAltRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that Image field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenImageField_ShouldApplyImageFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                ImageFieldInfo imageFieldInfoAlt = new ImageFieldInfo(
                    "TestInternalNameImageAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = new ImageValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic image field definition (all default property values)
                    SPField imageField = fieldHelper.EnsureField(fieldsCollection, imageFieldInfo);
                    this.ValidateFieldBasicValues(imageFieldInfo, imageField);
                    Assert.IsTrue(string.IsNullOrEmpty(imageField.DefaultValue));

                    SPField imageFieldRefetched = testScope.SiteCollection.RootWeb.Fields[imageFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(imageFieldInfo, imageFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(imageFieldRefetched.DefaultValue));

                    // 2) Alternate image field definition (with all property values customized and a default value assigned)
                    SPField imageFieldAlt = fieldHelper.EnsureField(fieldsCollection, imageFieldInfoAlt);
                    this.ValidateFieldBasicValues(imageFieldInfoAlt, imageFieldAlt);
                    Assert.AreEqual(
                        "<a href=\"http://github.com/GSoft-SharePoint/\"><img alt=\"\" src=\"/_layouts/15/MyFolder/MyImage.png\" style=\"BORDER: 0px solid; \"></a>", 
                        imageFieldAlt.DefaultValue);

                    SPField imageFieldAltRefetched = testScope.SiteCollection.RootWeb.Fields[imageFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(imageFieldInfoAlt, imageFieldAltRefetched);
                    Assert.AreEqual(
                        "<a href=\"http://github.com/GSoft-SharePoint/\"><img alt=\"\" src=\"/_layouts/15/MyFolder/MyImage.png\" style=\"BORDER: 0px solid; \"></a>",
                        imageFieldAltRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that Image field type forces you to give either a) a full absolute ImageUrl with domain
        /// or b) a relative URL that begins with a forward slash. If the slash is ommitted, the field value will
        /// not work.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenImageField_ShouldThrowExceptionIfYouUseARelativeImageUrlWithoutSlashInFront()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                try
                {
                    ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                        "TestInternalNameImage",
                        new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                        "NameKey",
                        "DescriptionKey",
                        "GroupKey")
                    {
                        DefaultValue = new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "_layouts/15/MyFolder/MyImage.png"
                        }
                    };

                    Assert.Fail("Should've trown exception because forgetting the leading slash on a relative ImageUrl would break.");
                }
                catch (ArgumentException)
                {
                }

                try
                {
                    ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                        "TestInternalNameImage",
                        new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                        "NameKey",
                        "DescriptionKey",
                        "GroupKey")
                    {
                        DefaultValue = new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "\\\\\\bad    \\Url/yeah#?  123"
                        }
                    };

                    Assert.Fail("Should've trown exception because that string should never be able to initalize a proper Uri.");
                }
                catch (ArgumentException)
                {
                }

                ImageFieldInfo imageFieldInfoAltRelative = new ImageFieldInfo(
                    "TestInternalNameImageAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = new ImageValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    }
                };

                ImageFieldInfo imageFieldInfoAltAbsolute = new ImageFieldInfo(
                    "TestInternalNameImageAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = new ImageValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "http://github.com/_layouts/15/MyFolder/MyImage.png"
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // Make sure the two valid image field are ensurable without drama
                    fieldHelper.EnsureField(fieldsCollection, imageFieldInfoAltRelative);
                    fieldHelper.EnsureField(fieldsCollection, imageFieldInfoAltAbsolute);
                }
            }
        }

        /// <summary>
        /// Validates that URL field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenUrlField_ShouldApplyUrlFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                };

                UrlFieldInfo urlFieldInfoAlt = new UrlFieldInfo(
                    "TestInternalNameUrlAlt",
                    new Guid("{E5157693-43E2-4651-8A60-C0B96AF25A4F}"),
                    "NameKeyAlt",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/somethingsomething",
                        Description = "Lalalalala description"
                    }
                };

                UrlFieldInfo urlFieldInfoAlt2 = new UrlFieldInfo(
                    "TestInternalNameUrlAlt2",
                    new Guid("{2A3DAD08-F9F7-4BF7-82D5-9E490DAEC242}"),
                    "NameKeyAlt2",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image,
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/somethingsomething",
                        Description = "Lalalalala description"
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic URL field definition (all default property values)
                    SPFieldUrl urlField = (SPFieldUrl)fieldHelper.EnsureField(fieldsCollection, urlFieldInfo);
                    this.ValidateFieldBasicValues(urlFieldInfo, urlField);
                    Assert.IsTrue(string.IsNullOrEmpty(urlField.DefaultValue));

                    SPFieldUrl urlFieldRefetched = (SPFieldUrl)testScope.SiteCollection.RootWeb.Fields[urlFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(urlFieldInfo, urlFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(urlFieldRefetched.DefaultValue));

                    // 2) Alternate URL field definition
                    SPFieldUrl urlFieldAlt = (SPFieldUrl)fieldHelper.EnsureField(fieldsCollection, urlFieldInfoAlt);
                    this.ValidateFieldBasicValues(urlFieldInfoAlt, urlFieldAlt);
                    Assert.AreEqual(SPUrlFieldFormatType.Hyperlink, urlFieldAlt.DisplayFormat);
                    Assert.AreEqual(
                        "http://github.com/GSoft-SharePoint/somethingsomething",
                        urlFieldAlt.DefaultValue);      // Description should be missing (i.e. ignored by field helper), since OOTB url field don't support it properly

                    SPFieldUrl urlFieldAltRefetched = (SPFieldUrl)testScope.SiteCollection.RootWeb.Fields[urlFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(urlFieldInfoAlt, urlFieldAltRefetched);
                    Assert.AreEqual(SPUrlFieldFormatType.Hyperlink, urlFieldAltRefetched.DisplayFormat);
                    Assert.AreEqual(
                        "http://github.com/GSoft-SharePoint/somethingsomething",
                        urlFieldAltRefetched.DefaultValue);

                    // 3) Alternate URL field definition (as Image)
                    urlFieldAlt = (SPFieldUrl)fieldHelper.EnsureField(fieldsCollection, urlFieldInfoAlt2);
                    this.ValidateFieldBasicValues(urlFieldInfoAlt2, urlFieldAlt);
                    Assert.AreEqual(SPUrlFieldFormatType.Image, urlFieldAlt.DisplayFormat);
                    Assert.AreEqual(
                        "http://github.com/GSoft-SharePoint/somethingsomething",
                        urlFieldAlt.DefaultValue);

                    urlFieldAltRefetched = (SPFieldUrl)testScope.SiteCollection.RootWeb.Fields[urlFieldInfoAlt2.Id];
                    this.ValidateFieldBasicValues(urlFieldInfoAlt2, urlFieldAltRefetched);
                    Assert.AreEqual(SPUrlFieldFormatType.Image, urlFieldAltRefetched.DisplayFormat);
                    Assert.AreEqual(
                        "http://github.com/GSoft-SharePoint/somethingsomething",
                        urlFieldAltRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that Lookup field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenLookupSingleOrMultiField_ShouldApplyLookupFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Gotta create the list before we even think about provisioning a lookup
                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    // ShowField should be Title by default
                    // ListId will be known only once the SPList is created
                };

                LookupFieldInfo lookupFieldInfoWithDefault = new LookupFieldInfo(
                    "TestInternalNameLookupD",
                    new Guid("{0F413213-9B75-49AD-850E-38EF551B1D1F}"),
                    "NameKeyDef",
                    "DescriptionKeyDef",
                    "GroupKey")
                {
                    // Default value will be assigned below once the list and the lookup item are created
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2A3DAD08-F9F7-4BF7-82D5-9E490DAEC242}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfoWithDefault = new LookupMultiFieldInfo(
                    "TestInternalNameLookupMD",
                    new Guid("{9ACF13BF-F42C-4488-AE54-5E971B7619AB}"),
                    "NameKeyMultiDef",
                    "DescriptionKeyMultiDef",
                    "GroupKey")
                {
                    // ShowField should be Title by default
                    // Default value will be assigned below once the list and the lookup items are created
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    // Create the lookup list
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Add an item to lookup list to act as default lookup field value
                    SPListItem item1 = list.Items.Add();
                    item1["Title"] = "Test Item 1";
                    item1.Update();
                    SPListItem item2 = list.Items.Add();
                    item2["Title"] = "Test Item 2";
                    item2.Update();

                    // Add the list ID to the Lookup field definitions
                    lookupFieldInfo.ListId = list.ID;

                    lookupFieldInfoWithDefault.ListId = list.ID;
                    lookupFieldInfoWithDefault.DefaultValue = new LookupValue(item1.ID, item1.ID.ToString());

                    lookupMultiFieldInfo.ListId = list.ID;

                    lookupMultiFieldInfoWithDefault.ListId = list.ID;
                    lookupMultiFieldInfoWithDefault.DefaultValue = new LookupValueCollection() { new LookupValue(item1.ID, "Test Item 1"), new LookupValue(item2.ID, "Test Item 2") };

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic lookup field (no default value)
                    SPFieldLookup lookupField = (SPFieldLookup)fieldHelper.EnsureField(fieldsCollection, lookupFieldInfo);
                    this.ValidateFieldBasicValues(lookupFieldInfo, lookupField);
                    Assert.IsTrue(string.IsNullOrEmpty(lookupField.DefaultValue));
                    Assert.AreEqual("Title", lookupField.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupField.LookupList));
                    Assert.IsFalse(lookupField.AllowMultipleValues);

                    SPFieldLookup lookupFieldRefetched = (SPFieldLookup)testScope.SiteCollection.RootWeb.Fields[lookupFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(lookupFieldInfo, lookupFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(lookupFieldRefetched.DefaultValue));
                    Assert.AreEqual("Title", lookupFieldRefetched.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupFieldRefetched.LookupList));
                    Assert.IsFalse(lookupFieldRefetched.AllowMultipleValues);

                    // 2) Basic lookup field (with default value)
                    lookupField = (SPFieldLookup)fieldHelper.EnsureField(fieldsCollection, lookupFieldInfoWithDefault);
                    this.ValidateFieldBasicValues(lookupFieldInfoWithDefault, lookupField);
                    Assert.AreEqual("ID", lookupField.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupField.LookupList));
                    Assert.IsFalse(lookupField.AllowMultipleValues);
                    Assert.AreEqual("1;#1", lookupField.DefaultValue);

                    lookupFieldRefetched = (SPFieldLookup)testScope.SiteCollection.RootWeb.Fields[lookupFieldInfoWithDefault.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(lookupFieldInfoWithDefault, lookupFieldRefetched);
                    Assert.AreEqual("ID", lookupFieldRefetched.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupFieldRefetched.LookupList));
                    Assert.IsFalse(lookupFieldRefetched.AllowMultipleValues);
                    Assert.AreEqual("1;#1", lookupFieldRefetched.DefaultValue);

                    // 3) Basic lookup multi field (no default value)
                    lookupField = (SPFieldLookup)fieldHelper.EnsureField(fieldsCollection, lookupMultiFieldInfo);
                    this.ValidateFieldBasicValues(lookupMultiFieldInfo, lookupField);
                    Assert.IsTrue(string.IsNullOrEmpty(lookupField.DefaultValue));
                    Assert.AreEqual("ID", lookupField.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupField.LookupList));
                    Assert.IsTrue(lookupField.AllowMultipleValues);

                    lookupFieldRefetched = (SPFieldLookup)testScope.SiteCollection.RootWeb.Fields[lookupMultiFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(lookupMultiFieldInfo, lookupFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(lookupFieldRefetched.DefaultValue));
                    Assert.AreEqual("ID", lookupFieldRefetched.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupFieldRefetched.LookupList));
                    Assert.IsTrue(lookupFieldRefetched.AllowMultipleValues);

                    // 4) Basic lookup multi field (with default value)
                    lookupField = (SPFieldLookup)fieldHelper.EnsureField(fieldsCollection, lookupMultiFieldInfoWithDefault);
                    this.ValidateFieldBasicValues(lookupMultiFieldInfoWithDefault, lookupField);
                    Assert.AreEqual("Title", lookupField.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupField.LookupList));
                    Assert.IsTrue(lookupField.AllowMultipleValues);
                    Assert.AreEqual("1;#Test Item 1;#2;#Test Item 2", lookupField.DefaultValue);

                    lookupFieldRefetched = (SPFieldLookup)testScope.SiteCollection.RootWeb.Fields[lookupMultiFieldInfoWithDefault.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(lookupMultiFieldInfoWithDefault, lookupFieldRefetched);
                    Assert.AreEqual("Title", lookupFieldRefetched.LookupField);
                    Assert.AreEqual(list.ID, new Guid(lookupFieldRefetched.LookupList));
                    Assert.IsTrue(lookupFieldRefetched.AllowMultipleValues);
                    Assert.AreEqual("1;#Test Item 1;#2;#Test Item 2", lookupFieldRefetched.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Validates that User field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenUserSingleOrMultiField_ShouldApplyUserFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserName);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey")
                {
                    // ShowField should be Title by default
                    UserSelectionMode = UserFieldSelectionMode.PeopleAndGroups
                };

                UserFieldInfo userFieldInfoAlt = new UserFieldInfo(
                    "TestInternalNameUserAlt",
                    new Guid("{0F413213-9B75-49AD-850E-38EF551B1D1F}"),
                    "NameKeyUserAlt",
                    "DescriptionKeyUserAlt",
                    "GroupKey")
                {
                    ShowField = "ID",
                    UserSelectionScope = 1,
                    DefaultValue = new UserValue(ensuredUser1),
                    EnforceUniqueValues = true
                };

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{9ACF13BF-F42C-4488-AE54-5E971B7619AB}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey")
                {
                    DefaultValue = new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic user field (no default value)
                    SPFieldUser userField = (SPFieldUser)fieldHelper.EnsureField(fieldsCollection, userFieldInfo);
                    this.ValidateFieldBasicValues(userFieldInfo, userField);
                    Assert.IsTrue(string.IsNullOrEmpty(userField.DefaultValue));
                    Assert.IsFalse(userField.AllowMultipleValues);
                    Assert.AreEqual("ImnName", userField.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleAndGroups, userField.SelectionMode);
                    Assert.AreEqual(0, userField.SelectionGroup);

                    SPFieldUser userFieldRefetched = (SPFieldUser)testScope.SiteCollection.RootWeb.Fields[userFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(userFieldInfo, userFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(userFieldRefetched.DefaultValue));
                    Assert.IsFalse(userFieldRefetched.AllowMultipleValues);
                    Assert.AreEqual("ImnName", userFieldRefetched.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleAndGroups, userFieldRefetched.SelectionMode);
                    Assert.AreEqual(0, userFieldRefetched.SelectionGroup);

                    // 2) Basic user field (with default value)
                    userField = (SPFieldUser)fieldHelper.EnsureField(fieldsCollection, userFieldInfoAlt);
                    this.ValidateFieldBasicValues(userFieldInfoAlt, userField);
                    Assert.AreEqual("1;#" + ensuredUser1.Name, userField.DefaultValue);
                    Assert.IsFalse(userField.AllowMultipleValues);
                    Assert.AreEqual("ID", userField.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleOnly, userField.SelectionMode);
                    Assert.AreEqual(1, userField.SelectionGroup);

                    userFieldRefetched = (SPFieldUser)testScope.SiteCollection.RootWeb.Fields[userFieldInfoAlt.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(userFieldInfoAlt, userFieldRefetched);
                    Assert.AreEqual("1;#" + ensuredUser1.Name, userFieldRefetched.DefaultValue);
                    Assert.IsFalse(userField.AllowMultipleValues);
                    Assert.AreEqual("ID", userFieldRefetched.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleOnly, userFieldRefetched.SelectionMode);
                    Assert.AreEqual(1, userFieldRefetched.SelectionGroup);

                    // 3) Basic user multi field (with default value)
                    userField = (SPFieldUser)fieldHelper.EnsureField(fieldsCollection, userMultiFieldInfo);
                    this.ValidateFieldBasicValues(userMultiFieldInfo, userField);
                    Assert.AreEqual("1;#" + ensuredUser1.Name + ";#5;#Maxime Boissonneault", userField.DefaultValue);
                    Assert.IsTrue(userField.AllowMultipleValues);
                    Assert.AreEqual("ImnName", userField.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleOnly, userField.SelectionMode);
                    Assert.AreEqual(0, userField.SelectionGroup);

                    userFieldRefetched = (SPFieldUser)testScope.SiteCollection.RootWeb.Fields[userMultiFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(userMultiFieldInfo, userFieldRefetched);
                    Assert.AreEqual("1;#" + ensuredUser1.Name + ";#5;#Maxime Boissonneault", userFieldRefetched.DefaultValue);
                    Assert.IsTrue(userFieldRefetched.AllowMultipleValues);
                    Assert.AreEqual("ImnName", userFieldRefetched.LookupField);
                    Assert.AreEqual(SPFieldUserSelectionMode.PeopleOnly, userFieldRefetched.SelectionMode);
                    Assert.AreEqual(0, userFieldRefetched.SelectionGroup);
                }
            }
        }

        /// <summary>
        /// Validates that Media field type properties are mapped along with its default value
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenRichPublishingMediaField_ShouldApplyMediaFieldDefinitionAndDefaultValue()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKeyMedia",
                    "DescriptionKeyMedia",
                    "GroupKey")
                {
                };

                MediaFieldInfo mediaFieldInfoAlt = new MediaFieldInfo(
                    "TestInternalNameMediaAlt",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyMediaAlt",
                    "DescriptionKeyMediaAlt",
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

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    // 1) Basic image field definition (all default property values)
                    SPField mediaField = fieldHelper.EnsureField(fieldsCollection, mediaFieldInfo);
                    this.ValidateFieldBasicValues(mediaFieldInfo, mediaField);
                    Assert.IsTrue(string.IsNullOrEmpty(mediaField.DefaultValue));

                    SPField mediaFieldRefetched = testScope.SiteCollection.RootWeb.Fields[mediaFieldInfo.Id]; // refetch to make sure .Update() was properly called on SPField
                    this.ValidateFieldBasicValues(mediaFieldInfo, mediaFieldRefetched);
                    Assert.IsTrue(string.IsNullOrEmpty(mediaFieldRefetched.DefaultValue));

                    // 2) Alternate image field definition (with all property values customized and a default value assigned)
                    SPField mediaFieldAlt = fieldHelper.EnsureField(fieldsCollection, mediaFieldInfoAlt);
                    this.ValidateFieldBasicValues(mediaFieldInfoAlt, mediaFieldAlt);
                    Assert.AreEqual(
                       @"<dl><dt>Title</dt><dd>Some media file title</dd><dt>MediaSource</dt><dd><a href=""/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"">link</a></dd><dt>PreviewImageSource</dt><dd><a href=""/_layouts/15/Images/logo.png"">link</a></dd><dt>DisplayMode</dt><dd>Inline</dd><dt>AutoPlay</dt><dd>True</dd><dt>Loop</dt><dd>True</dd><dt>ShowEmbedControl</dt><dd>False</dd><dt>ConfigureFromContext</dt><dd>False</dd></dl>".Replace(" ", string.Empty).Replace("%20", string.Empty),
                        mediaFieldAlt.DefaultValue.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));

                    SPField mediaFieldAltRefetched = testScope.SiteCollection.RootWeb.Fields[mediaFieldInfoAlt.Id];
                    this.ValidateFieldBasicValues(mediaFieldInfoAlt, mediaFieldAltRefetched);
                    Assert.AreEqual(
                       @"<dl><dt>Title</dt><dd>Some media file title</dd><dt>MediaSource</dt><dd><a href=""/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"">link</a></dd><dt>PreviewImageSource</dt><dd><a href=""/_layouts/15/Images/logo.png"">link</a></dd><dt>DisplayMode</dt><dd>Inline</dd><dt>AutoPlay</dt><dd>True</dd><dt>Loop</dt><dd>True</dd><dt>ShowEmbedControl</dt><dd>False</dd><dt>ConfigureFromContext</dt><dd>False</dd></dl>".Replace(" ", string.Empty).Replace("%20", string.Empty),
                        mediaFieldAltRefetched.DefaultValue.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));
                }
            }
        }

        #endregion

        #region Ensuring fields directly on lists should make those fields work on the list's items

        /// <summary>
        /// Validates that list field default values are applied on new items created directly on that list.
        /// Doing so for Number, Text, Note, Html, Taxonomy and TaxonomyMulti field types.
        /// TODO: validated the behavior for other field types like Choice, User and UserMulti, etc.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureField_WhenFieldAddedToListWithDefaultValue_NewItemsCreatedOnListShouldHaveDefaultValues()
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
                    Format = UrlFieldFormat.Image,
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
                    DefaultValue = new TaxonomyValue(levelOneTermB),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>() 
                            { 
                                new TaxonomyValue(levelTwoTermAA), 
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
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

                ListInfo listInfo1 = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");
                ListInfo listInfo2 = new ListInfo("sometestlistpathalt", "DynamiteTestListNameKeyAlt", "DynamiteTestListDescriptionKeyAlt")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
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
                    SPList list1 = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo1);

                    var fieldHelper = injectionScope.Resolve<IFieldHelper>();

                    // we need to ensure all fields on first list directly
                    IList<SPField> ensuredFieldsOnList1 = fieldHelper.EnsureField(list1.Fields, fieldsToEnsure).ToList();

                    // second ListInfo object holds its own field definitions (which should be ensured at same time as list through listHelper)
                    SPList list2 = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo2);

                    // Act
                    var itemOnList1 = list1.AddItem();
                    itemOnList1.Update();
                    var itemOnList2 = list2.AddItem();
                    itemOnList2.Update();

                    // Assert
                    // List item #1 (fields on list ensured via FieldHelper.EnsureField)
                    Assert.AreEqual(555, itemOnList1["TestInternalNameInteger"]);
                    Assert.AreEqual(5.5, itemOnList1["TestInternalNameNumber"]);
                    Assert.AreEqual(500.95, itemOnList1["TestInternalNameCurrency"]);
                    Assert.IsNull(itemOnList1["TestInternalNameBool"]);
                    Assert.IsTrue((bool)itemOnList1["TestInternalNameBoolTrue"]);
                    Assert.IsFalse((bool)itemOnList1["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(DateTime.Today, itemOnList1["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), itemOnList1["TestInternalNameDateDefault"]);
                    Assert.AreEqual("Text default value", itemOnList1["TestInternalNameText"]);
                    Assert.AreEqual("Note default value", itemOnList1["TestInternalNameNote"]);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", itemOnList1["TestInternalNameHtml"]);

                    var imageFieldVal = (ImageFieldValue)itemOnList1["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    var urlFieldVal = new SPFieldUrlValue(itemOnList1["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description will never be set for Format=Hyperlink
            
                    var urlImageFieldVal = new SPFieldUrlValue(itemOnList1["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description will never be set for Format=Image either

                    var lookupFieldVal = new SPFieldLookupValue(itemOnList1["TestInternalNameLookup"].ToString());
                    Assert.AreEqual(1, lookupFieldVal.LookupId);
                    Assert.AreEqual("Test Item 1", lookupFieldVal.LookupValue);

                    var lookupAltFieldVal = new SPFieldLookupValue(itemOnList1["TestInternalNameLookupAlt"].ToString());
                    Assert.AreEqual(2, lookupAltFieldVal.LookupId);
                    Assert.AreEqual("2", lookupAltFieldVal.LookupValue); // ShowField/LookupField is ID

                    var lookupMultiFieldVal = new SPFieldLookupValueCollection(itemOnList1["TestInternalNameLookupM"].ToString());
                    Assert.AreEqual(1, lookupMultiFieldVal[0].LookupId);
                    Assert.AreEqual("Test Item 1", lookupMultiFieldVal[0].LookupValue);
                    Assert.AreEqual(2, lookupMultiFieldVal[1].LookupId);
                    Assert.AreEqual("Test Item 2", lookupMultiFieldVal[1].LookupValue);

                    var userFieldVal = new SPFieldUserValue(testScope.SiteCollection.RootWeb, itemOnList1["TestInternalNameUser"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userFieldVal.User.Name);

                    var userMultiFieldVal = new SPFieldUserValueCollection(testScope.SiteCollection.RootWeb, itemOnList1["TestInternalNameUserMulti"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userMultiFieldVal[0].User.Name);
                    Assert.AreEqual("Maxime Boissonneault", userMultiFieldVal[1].User.Name);

                    var mediaFieldVal = MediaFieldValue.FromString(itemOnList1["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    var taxoFieldValue = (TaxonomyFieldValue)itemOnList1["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    var taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemOnList1["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);

                    // List item #2 (fields on list ensured via ListHelper.EnsureList)
                    Assert.AreEqual(555, itemOnList2["TestInternalNameInteger"]);
                    Assert.AreEqual(5.5, itemOnList2["TestInternalNameNumber"]);
                    Assert.AreEqual(500.95, itemOnList2["TestInternalNameCurrency"]);
                    Assert.IsNull(itemOnList2["TestInternalNameBool"]);
                    Assert.IsTrue((bool)itemOnList2["TestInternalNameBoolTrue"]);
                    Assert.IsFalse((bool)itemOnList2["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(DateTime.Today, itemOnList2["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), itemOnList2["TestInternalNameDateDefault"]);
                    Assert.AreEqual("Text default value", itemOnList2["TestInternalNameText"]);
                    Assert.AreEqual("Note default value", itemOnList2["TestInternalNameNote"]);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", itemOnList2["TestInternalNameHtml"]);

                    imageFieldVal = (ImageFieldValue)itemOnList2["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    urlFieldVal = new SPFieldUrlValue(itemOnList2["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description will never be set for Format=Hyperlink

                    urlImageFieldVal = new SPFieldUrlValue(itemOnList2["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description will never be set for Format=Image either

                    lookupFieldVal = new SPFieldLookupValue(itemOnList2["TestInternalNameLookup"].ToString());
                    Assert.AreEqual(1, lookupFieldVal.LookupId);
                    Assert.AreEqual("Test Item 1", lookupFieldVal.LookupValue);

                    lookupAltFieldVal = new SPFieldLookupValue(itemOnList2["TestInternalNameLookupAlt"].ToString());
                    Assert.AreEqual(2, lookupAltFieldVal.LookupId);
                    Assert.AreEqual("2", lookupAltFieldVal.LookupValue); // ShowField/LookupField is ID

                    lookupMultiFieldVal = new SPFieldLookupValueCollection(itemOnList2["TestInternalNameLookupM"].ToString());
                    Assert.AreEqual(1, lookupMultiFieldVal[0].LookupId);
                    Assert.AreEqual("Test Item 1", lookupMultiFieldVal[0].LookupValue);
                    Assert.AreEqual(2, lookupMultiFieldVal[1].LookupId);
                    Assert.AreEqual("Test Item 2", lookupMultiFieldVal[1].LookupValue);

                    userFieldVal = new SPFieldUserValue(testScope.SiteCollection.RootWeb, itemOnList2["TestInternalNameUser"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userFieldVal.User.Name);

                    userMultiFieldVal = new SPFieldUserValueCollection(testScope.SiteCollection.RootWeb, itemOnList2["TestInternalNameUserMulti"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userMultiFieldVal[0].User.Name);
                    Assert.AreEqual("Maxime Boissonneault", userMultiFieldVal[1].User.Name);

                    mediaFieldVal = MediaFieldValue.FromString(itemOnList2["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    taxoFieldValue = (TaxonomyFieldValue)itemOnList2["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemOnList2["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        #endregion

        #region Field Title, Description and Group properties should be localized (if you configure ResourceLocator to access your RESX file)

        /// <summary>
        /// Validates that English field name is initialized on English-language web
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenEnglishOnlySiteCollection_ShouldCreateFieldWithEnglishDisplayName()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.English.Culture.LCID))
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();

                    var rootWebFieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    SPField field = fieldHelper.EnsureField(rootWebFieldsCollection, textFieldInfo);
                    SPField fieldFromOldCollection = rootWebFieldsCollection[textFieldInfo.Id];
                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];

                    Assert.AreEqual("EN Field Title", field.Title);
                    Assert.AreEqual("EN Field Description", field.Description);
                    Assert.AreEqual("EN Content Group", field.Group);

                    Assert.AreEqual("EN Field Title", fieldFromOldCollection.Title);
                    Assert.AreEqual("EN Field Description", fieldFromOldCollection.Description);
                    Assert.AreEqual("EN Content Group", fieldFromOldCollection.Group);

                    Assert.AreEqual("EN Field Title", fieldRefetched.Title);
                    Assert.AreEqual("EN Field Description", fieldRefetched.Description);
                    Assert.AreEqual("EN Content Group", fieldRefetched.Group);
                }
            }
        }

        /// <summary>
        /// Validates that French field name is initialized on French-language web
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenFrenchOnlySiteCollection_ShouldCreateFieldWithFrenchDisplayName()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.French.Culture.LCID))
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var rootWebFieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    SPField field = fieldHelper.EnsureField(rootWebFieldsCollection, textFieldInfo);
                    SPField fieldFromOldCollection = rootWebFieldsCollection[textFieldInfo.Id];
                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];

                    // Set MUI to french
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = Language.French.Culture;

                    Assert.AreEqual("FR Nom de champ", field.Title);
                    Assert.AreEqual("FR Description de champ", field.Description);
                    Assert.AreEqual("FR Groupe de contenu", field.Group);

                    Assert.AreEqual("FR Nom de champ", fieldFromOldCollection.Title);
                    Assert.AreEqual("FR Description de champ", fieldFromOldCollection.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldFromOldCollection.Group);

                    Assert.AreEqual("FR Nom de champ", fieldRefetched.Title);
                    Assert.AreEqual("FR Description de champ", fieldRefetched.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        /// <summary>
        /// Validates that display names for field in both languages is initialized on Enlish-and-French-languages web
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureField_WhenEnglishAndFrenchSiteCollection_ShouldCreateFieldWithDisplayNameAvailableInBothLanguages()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.English.Culture.LCID))
            {
                // Add French so that both languages are supported
                var rootWeb = testScope.SiteCollection.RootWeb;
                rootWeb.AddSupportedUICulture(Language.French.Culture);
                rootWeb.Update();

                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldCollection = testScope.SiteCollection.RootWeb.Fields;

                    SPField field = fieldHelper.EnsureField(fieldCollection, textFieldInfo);
                    SPField fieldFromOldCollection = fieldCollection[textFieldInfo.Id];
                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Fields[textFieldInfo.Id];

                    // Set MUI to english
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = Language.English.Culture;

                    Assert.AreEqual("EN Field Title", field.Title);
                    Assert.AreEqual("EN Field Description", field.Description);
                    Assert.AreEqual("EN Content Group", field.Group);

                    Assert.AreEqual("EN Field Title", fieldFromOldCollection.Title);
                    Assert.AreEqual("EN Field Description", fieldFromOldCollection.Description);
                    Assert.AreEqual("EN Content Group", fieldFromOldCollection.Group);

                    Assert.AreEqual("EN Field Title", fieldRefetched.Title);
                    Assert.AreEqual("EN Field Description", fieldRefetched.Description);
                    Assert.AreEqual("EN Content Group", fieldRefetched.Group);

                    // Set MUI to french
                    Thread.CurrentThread.CurrentUICulture = Language.French.Culture;

                    Assert.AreEqual("FR Nom de champ", field.Title);
                    Assert.AreEqual("FR Description de champ", field.Description);
                    Assert.AreEqual("FR Groupe de contenu", field.Group);

                    Assert.AreEqual("FR Nom de champ", fieldFromOldCollection.Title);
                    Assert.AreEqual("FR Description de champ", fieldFromOldCollection.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldFromOldCollection.Group);

                    Assert.AreEqual("FR Nom de champ", fieldRefetched.Title);
                    Assert.AreEqual("FR Description de champ", fieldRefetched.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        /// <summary>
        /// Validates that field localization works on French sub-web lists
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenOnListInFrenchSubWeb_ShouldCreateListFieldWithFrenchDisplayName()
        {
            // English root web
            using (var testScope = SiteTestScope.BlankSite(Language.English.Culture.LCID))
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    SPWeb frenchSubWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb", "French sub-web", string.Empty, (uint)Language.French.Culture.LCID, "STS#1", false, false);

                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList subWebList = listHelper.EnsureList(frenchSubWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    SPFieldCollection listFields = testScope.SiteCollection.RootWeb.Webs["subweb"].Lists[subWebList.ID].Fields;

                    SPField field = fieldHelper.EnsureField(listFields, textFieldInfo);
                    SPField fieldOnOldCollection = listFields[textFieldInfo.Id];
                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Webs["subweb"].Lists[subWebList.ID].Fields[textFieldInfo.Id];

                    // Set MUI to french
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = Language.French.Culture;

                    Assert.AreEqual("FR Nom de champ", field.Title);
                    Assert.AreEqual("FR Description de champ", field.Description);
                    Assert.AreEqual("FR Groupe de contenu", field.Group);

                    Assert.AreEqual("FR Nom de champ", fieldOnOldCollection.Title);
                    Assert.AreEqual("FR Description de champ", fieldOnOldCollection.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldOnOldCollection.Group);

                    Assert.AreEqual("FR Nom de champ", fieldRefetched.Title);
                    Assert.AreEqual("FR Description de champ", fieldRefetched.Description);
                    Assert.AreEqual("FR Groupe de contenu", fieldRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        /// <summary>
        /// Validates that field localization works on Enlish sub-web lists
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenOnListInEnglishSubWeb_ShouldCreateListFieldWithEnglishDisplayName()
        {
            // French root web
            using (var testScope = SiteTestScope.BlankSite(Language.French.Culture.LCID))
            {
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    SPWeb frenchSubWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb", "English sub-web", string.Empty, (uint)Language.English.Culture.LCID, "STS#1", false, false);

                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList subWebList = listHelper.EnsureList(frenchSubWeb, listInfo);

                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    SPFieldCollection listFields = testScope.SiteCollection.RootWeb.Webs["subweb"].Lists[subWebList.ID].Fields;

                    SPField field = fieldHelper.EnsureField(listFields, textFieldInfo);
                    SPField fieldOnOldCollection = listFields[textFieldInfo.Id];
                    SPField fieldRefetched = testScope.SiteCollection.RootWeb.Webs["subweb"].Lists[subWebList.ID].Fields[textFieldInfo.Id];

                    // Set MUI to english
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture; 
                    Thread.CurrentThread.CurrentUICulture = Language.English.Culture;

                    Assert.AreEqual("EN Field Title", field.Title);
                    Assert.AreEqual("EN Field Description", field.Description);
                    Assert.AreEqual("EN Content Group", field.Group);

                    Assert.AreEqual("EN Field Title", fieldOnOldCollection.Title);
                    Assert.AreEqual("EN Field Description", fieldOnOldCollection.Description);
                    Assert.AreEqual("EN Content Group", fieldOnOldCollection.Group);

                    Assert.AreEqual("EN Field Title", fieldRefetched.Title);
                    Assert.AreEqual("EN Field Description", fieldRefetched.Description);
                    Assert.AreEqual("EN Content Group", fieldRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        #endregion

        #region OOTB Field Types - Ensuring MinimalFieldInfo should work to add column to CT or list (but not to add as site column)

        /// <summary>
        /// Validates that MinimalFieldInfos cannot be ensured as Site Columns (because they lack a full field definition)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenEnsuringAMinimalFieldInfoOOTBColumnAsSiteColumn_ShouldFailBecauseSuchOOTBSiteColumnShouldBeAddedByOOTBFeatures()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                BaseFieldInfo textFieldInfo = new MinimalFieldInfo<string>(
                    "Hobbies",
                    new Guid("{203fa378-6eb8-4ed9-a4f9-221a4c1fbf46}"));

                BaseFieldInfo dateFieldInfo = new MinimalFieldInfo<DateTime?>(
                    "Birthday",
                    new Guid("{C4C7D925-BC1B-4f37-826D-AC49B4FB1BC1}"));

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var fieldsCollection = testScope.SiteCollection.RootWeb.Fields;

                    int noOfFieldsBefore = fieldsCollection.Count;

                    // Act + Assert
                    try
                    {
                        SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because a MinimalFieldInfo lacks information for site collection ensure operation.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        SPField field = fieldHelper.EnsureField(fieldsCollection, dateFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because a MinimalFieldInfo lacks information for site collection ensure operation.");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Validates that MinimalFieldInfos can be used to define additions to list fields (provided the OOTB site column exists in the site collection)
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenEnsuringAMinimalFieldInfoOOTBColumnAsFieldOnList_ShouldMakeFieldAvailableOnList()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                ListInfo listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    FieldDefinitions = new List<BaseFieldInfo>()
                        {
                            BuiltInFields.AssignedTo,   // OOTB User field
                            BuiltInFields.Cellphone,    // OOTB Text field
                        }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();

                    // Act (ensure fields on list through both listHelper and fieldHelper)
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    SPField field = fieldHelper.EnsureField(list.Fields, BuiltInFields.EnterpriseKeywords);     // OOTB Taxonomy Multi field

                    // Assert
                    Assert.IsNotNull(list.Fields[BuiltInFields.AssignedTo.Id]);
                    Assert.IsNotNull(list.Fields[BuiltInFields.Cellphone.Id]);
                    Assert.IsNotNull(list.Fields[BuiltInFields.EnterpriseKeywords.Id]);

                    // Use the a list and create an item just for kicks
                    SPListItem item = list.AddItem();
                    item.Update();

                    var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserName);

                    IFieldValueWriter writer = injectionScope.Resolve<IFieldValueWriter>();
                    writer.WriteValuesToListItem(
                        item,
                        new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(BuiltInFields.AssignedTo, new UserValue(ensuredUser1)),
                            new FieldValueInfo(BuiltInFields.Cellphone, "Test Cellphone Value"),
                            new FieldValueInfo(BuiltInFields.EnterpriseKeywords, new TaxonomyValueCollection())
                        });

                    item.Update();

                    Assert.IsNotNull(item[BuiltInFields.AssignedTo.Id]);
                    Assert.IsNotNull(item[BuiltInFields.Cellphone.Id]);
                    Assert.IsNotNull(item[BuiltInFields.EnterpriseKeywords.Id]);
                }
            }
        }

        /// <summary>
        /// Validates that MinimalFieldInfos cannot be used to define additions to content types when the relevant site column doesn't exist
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenEnsuringAMinimalFieldInfoOOTBColumnAsFieldOnList_AndOOTBSiteColumnIsNOTAvailable_ShouldFailBecauseSuchOOTBSiteColumnShouldBeAddedByOOTBFeatures()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                ListInfo listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                {
                    FieldDefinitions = new List<BaseFieldInfo>()
                        {
                            PublishingFields.PublishingPageContent  // Should be missing from site columns (only available in Publishing sites)
                        }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    IFieldHelper fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    SPList list = null;

                    // Act
                    try
                    {
                        // Field ensured through ListHelper->FieldHelper
                        list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                        Assert.Fail("Should've thrown NotSupportedException because pre-requisite site column doesn't exist on RootWeb.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        listInfo.FieldDefinitions.Clear();
                        list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                        // Field ensured through FieldHelper only
                        fieldHelper.EnsureField(list.Fields, PublishingFields.PublishingPageContent);
                        Assert.Fail("Should've thrown NotSupportedException because pre-requisite site column doesn't exist on RootWeb.");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        #endregion

        #region EnforceUniqueValues should be applied only on supported field types and should force list field to be indexed

        /// <summary>
        /// Validates that site columns can be defined such that they'll enfore uniqueness of values
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenAddingSiteColumnWithEnforceUniqueValue_AndFieldInfoIsSupportedFieldType_ShouldInitializeFieldToEnforceUniqueness_AndForceIndexation()
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
                    EnforceUniqueValues = true
                };

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    EnforceUniqueValues = true,
                    LocaleId = 3084 // fr-CA
                };

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTime",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };
                
                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserDomainName + "\\" + Environment.UserName);

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
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
                    EnforceUniqueValues = true,
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        dateTimeFieldInfo,
                        textFieldInfo,
                        userFieldInfo,
                        lookupFieldInfo,
                        taxoFieldInfo,
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo1 = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo1);

                    // Setup the fields as site columns first
                    var fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    fieldHelper.EnsureField(testScope.SiteCollection.RootWeb.Fields, fieldsToEnsure);

                    // we need to ensure all fields on first list directly after the site columns were created
                    IList<SPField> ensuredFieldsOnList = fieldHelper.EnsureField(list.Fields, fieldsToEnsure).ToList();

                    // Act
                    var originalItemWithUniqueValues = list.AddItem();
                    originalItemWithUniqueValues["TestInternalNameInteger"] = 555;
                    originalItemWithUniqueValues["TestInternalNameNumber"] = 5.5;
                    originalItemWithUniqueValues["TestInternalNameCurrency"] = 500.95;
                    originalItemWithUniqueValues["TestInternalNameDate"] = new DateTime(2005, 10, 21);
                    originalItemWithUniqueValues["TestInternalNameText"] = "Text value";
                    originalItemWithUniqueValues["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    originalItemWithUniqueValues["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    var taxonomyField = (TaxonomyField)originalItemWithUniqueValues.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(originalItemWithUniqueValues, createdTermB);
                    originalItemWithUniqueValues.Update();

                    // Assert
                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameInteger").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameInteger"] = 555;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Integer field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameNumber").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameNumber"] = 5.5;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Number field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameCurrency").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameCurrency"] = 500.95;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Currency field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameDate").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameDate"] = new DateTime(2005, 10, 21);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this DateTime field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameText").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameText"] = "Text value";
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Text field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameLookup").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Lookup field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameUser").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this User field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameTaxo").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        taxonomyField = (TaxonomyField)uniquenessBreakingItem.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                        taxonomyField.SetFieldValue(uniquenessBreakingItem, createdTermB);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Taxonomy field");
                    }
                    catch (SPException)
                    {
                    }
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that ensuring columns directly on a list also supports EnforceUniqueValues correctly for all supported field types.
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenEnsuringListColumnWithEnforceUniqueValue_AndFieldInfoIsSupportedFieldType_ShouldInitializeFieldToEnforceUniqueness_AndForceIndexation()
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
                    EnforceUniqueValues = true
                };

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    EnforceUniqueValues = true,
                    LocaleId = 3084 // fr-CA
                };

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTime",
                    "DescriptionKeyDateTime",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserDomainName + "\\" + Environment.UserName);

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
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
                    EnforceUniqueValues = true,
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        dateTimeFieldInfo,
                        textFieldInfo,
                        userFieldInfo,
                        lookupFieldInfo,
                        taxoFieldInfo,
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo1 = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the test list (which should provision both site columns and list columns)
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo1);

                    // Act
                    var originalItemWithUniqueValues = list.AddItem();
                    originalItemWithUniqueValues["TestInternalNameInteger"] = 555;
                    originalItemWithUniqueValues["TestInternalNameNumber"] = 5.5;
                    originalItemWithUniqueValues["TestInternalNameCurrency"] = 500.95;
                    originalItemWithUniqueValues["TestInternalNameDate"] = new DateTime(2005, 10, 21);
                    originalItemWithUniqueValues["TestInternalNameText"] = "Text value";
                    originalItemWithUniqueValues["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    originalItemWithUniqueValues["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    var taxonomyField = (TaxonomyField)originalItemWithUniqueValues.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(originalItemWithUniqueValues, createdTermB);
                    originalItemWithUniqueValues.Update();

                    // Assert
                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameInteger").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameInteger"] = 555;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Integer field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameNumber").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameNumber"] = 5.5;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Number field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameCurrency").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameCurrency"] = 500.95;
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Currency field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameDate").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameDate"] = new DateTime(2005, 10, 21);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this DateTime field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameText").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameText"] = "Text value";
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Text field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameLookup").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Lookup field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameUser").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        uniquenessBreakingItem["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this User field");
                    }
                    catch (SPException)
                    {
                    }

                    Assert.IsTrue(list.Fields.GetFieldByInternalName("TestInternalNameTaxo").Indexed);
                    try
                    {
                        var uniquenessBreakingItem = list.AddItem();
                        taxonomyField = (TaxonomyField)uniquenessBreakingItem.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                        taxonomyField.SetFieldValue(uniquenessBreakingItem, createdTermB);
                        uniquenessBreakingItem.Update();
                        Assert.Fail("Should've thrown SPException because values should be unique on this Taxonomy field");
                    }
                    catch (SPException)
                    {
                    }
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that ensuring EnforceUniqueValues on unsupported field type throws NotSupportedException
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenAttemptingToSetEnforceUniqueValue_ButFieldTypeDoesntSupportUniqueness_ShouldThrowNotSupportedException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                GuidFieldInfo guidFieldInfo = new GuidFieldInfo(
                    "TestInternalNameGuid",
                    new Guid("{7F486426-D3BC-48D8-8E28-85CFC9A457A0}"),
                    "NameKeyGuid",
                    "DescriptionKeyGuid",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image,
                    EnforceUniqueValues = true
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserDomainName + "\\" + Environment.UserName);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey")
                {
                    EnforceUniqueValues = true
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

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    EnforceUniqueValues = true,
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpathalt", "DynamiteTestListNameKeyAlt", "DynamiteTestListDescriptionKeyAlt");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the test list (which should provision both site columns and list columns)
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Act + Assert
                    try
                    {
                        fieldHelper.EnsureField(list.Fields, boolFieldInfoBasic);
                        Assert.Fail("Should've thrown NotSupportedException because Boolean fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, guidFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because Guid fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, noteFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because Note fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, htmlFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because Html fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, imageFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because Image fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, urlFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because URL fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, urlFieldInfoImage);
                        Assert.Fail("Should've thrown NotSupportedException because URL-Image fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, lookupMultiFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because LookupMulti fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, userMultiFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because UserMulti fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, mediaFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because Media fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }

                    try
                    {
                        fieldHelper.EnsureField(list.Fields, taxoMultiFieldInfo);
                        Assert.Fail("Should've thrown NotSupportedException because TaxoMulti fields don't support EnforceUniqueValues.");
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        #endregion

        private void ValidateFieldBasicValues(BaseFieldInfo fieldInfo, SPField field)
        {
            Assert.AreEqual(fieldInfo.Id, field.Id);
            Assert.AreEqual(fieldInfo.InternalName, field.InternalName);
            Assert.AreEqual(fieldInfo.DisplayNameResourceKey, field.TitleResource.Value);
            Assert.AreEqual(fieldInfo.DescriptionResourceKey, field.DescriptionResource.Value);
            Assert.AreEqual(fieldInfo.GroupResourceKey, field.Group);
            Assert.AreEqual(fieldInfo.EnforceUniqueValues, field.EnforceUniqueValues);
            Assert.AreEqual(fieldInfo.IsHidden, field.Hidden);
            Assert.AreEqual(!fieldInfo.IsHiddenInDisplayForm, field.ShowInDisplayForm);
            Assert.AreEqual(!fieldInfo.IsHiddenInNewForm, field.ShowInNewForm);
            Assert.AreEqual(!fieldInfo.IsHiddenInEditForm, field.ShowInEditForm);
            Assert.AreEqual(!fieldInfo.IsHiddenInListSettings, field.ShowInListSettings);
            Assert.AreEqual(fieldInfo.Required == RequiredType.Required, field.Required);
        }

        private void DeleteGroupIfExists(TermStore defaultSiteCollectionTermStore, Guid testGroupId)
        {
            Group existingTestGroup = defaultSiteCollectionTermStore.GetGroup(testGroupId);
            if (existingTestGroup != null)
            {
                foreach (var termSet in existingTestGroup.TermSets)
                {
                    termSet.Delete();
                }

                existingTestGroup.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }
    }
}
