using System;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Fields
{
    /// <summary>
    /// Validates the behavior of <see cref="FieldHelper"/>
    /// </summary>
    [TestClass]
    public class FieldHelperTest
    {
        /// <summary>
        /// Validates that EnsureField adds a field to the site collection if it did not exist previously
        /// </summary>
        [TestMethod]
        public void EnsureField_WhenNotAlreadyExists_ShouldAddAndReturnField()
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

                    int noOfFieldsBefore = fieldsCollection.Count;
                    SPField field = fieldHelper.EnsureField(fieldsCollection, textFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 1, fieldsCollection.Count);
                    Assert.IsNotNull(field);
                    Assert.AreEqual(textFieldInfo.Id, field.Id);
                    Assert.AreEqual(textFieldInfo.InternalName, field.InternalName);
                    Assert.AreEqual(textFieldInfo.DisplayNameResourceKey, field.TitleResource.Value);
                    Assert.AreEqual(textFieldInfo.DescriptionResourceKey, field.DescriptionResource.Value);
                    Assert.AreEqual(textFieldInfo.GroupResourceKey, field.Group);
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
                    "GroupKey");

                TextFieldInfo alternateTextFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{9EBF5EC3-5FC4-4ACF-B404-AC0A2D74A10F}"),     // new GUID, but same internal name
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

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
                    "GroupKey");

                TextFieldInfo alternateTextFieldInfo = new TextFieldInfo(
                    "TestInternalNameAlt",                                             // new internal name, but same Guid
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),   
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

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
                }
            }
        }

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
                        DefaultValue = "SomeDefaultValue",
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
                        DefaultValue = "SomeDefaultValueAlt",
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
                    ////Assert.AreEqual(textFieldInfo.DefaultValue, originalField.DefaultValue);    // TODO: test default value

                    // 2) Alternate field definition
                    SPField alternateEnsuredField = fieldHelper.EnsureField(fieldsCollection, alternateTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 2, fieldsCollection.Count);
                    Assert.IsNotNull(alternateEnsuredField);
                    this.ValidateFieldBasicValues(alternateTextFieldInfo, alternateEnsuredField);
                    ////Assert.AreEqual(alternateTextFieldInfo.DefaultValue, alternateEnsuredField.DefaultValue);       // TODO: test default value

                    // 3) Defautls-based field definition
                    SPField defaultBasedEnsuredField = fieldHelper.EnsureField(fieldsCollection, defaultsTextFieldInfo);

                    Assert.AreEqual(noOfFieldsBefore + 3, fieldsCollection.Count);
                    Assert.IsNotNull(defaultBasedEnsuredField);
                    this.ValidateFieldBasicValues(defaultsTextFieldInfo, defaultBasedEnsuredField);
                }
            }
        }

        private void ValidateFieldBasicValues(IFieldInfo fieldInfo, SPField field)
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
            Assert.AreEqual(fieldInfo.Required == RequiredType.Required ? true : false, field.Required);
        }
    }
}
