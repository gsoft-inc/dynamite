using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Lists.Constants
{
    /// <summary>
    /// Out of the box SP2013 List Templates Types (ID's) and related Feature ID's
    /// </summary>
    public static class BuiltInListTemplates
    {        
        /// <summary>
        /// Built-in List Template 'XML Form' with ID '115'
        /// </summary>
        public static ListTemplateInfo XMLForm
        {
             get { return new ListTemplateInfo(115, new Guid("00BFEA71-1E1D-4562-B56A-F05371BB0115")); }
        }

        /// <summary>
        /// Built-in List Template 'Workflow Service Store' with ID '4501'
        /// </summary>
        public static ListTemplateInfo WorkflowServiceStore
        {
             get { return new ListTemplateInfo(4501, new Guid("2C63DF2B-CEAB-42c6-AEFF-B3968162D4B1")); }
        }

        /// <summary>
        /// Built-in List Template 'Workflow Process' with ID '118'
        /// </summary>
        public static ListTemplateInfo WorkflowProcess
        {
             get { return new ListTemplateInfo(118, new Guid("00BFEA71-2D77-4A75-9FCA-76516689E21A")); }
        }

        /// <summary>
        /// Built-in List Template 'Workflow History' with ID '140'
        /// </summary>
        public static ListTemplateInfo WorkflowHistory
        {
             get { return new ListTemplateInfo(140, new Guid("00BFEA71-4EA5-48D4-A4AD-305CF7030140")); }
        }

        /// <summary>
        /// Built-in List Template 'Where Abouts' with ID '403'
        /// </summary>
        public static ListTemplateInfo Whereabouts
        {
             get { return new ListTemplateInfo(403, new Guid("9c2ef9dc-f733-432e-be1c-2e79957ea27b")); }
        }

        /// <summary>
        /// Built-in List Template 'What's New' with ID '425'
        /// </summary>
        public static ListTemplateInfo WhatsNew
        {
             get { return new ListTemplateInfo(425, new Guid("d7670c9c-1c29-4f44-8691-584001968a74")); }
        }

        /// <summary>
        /// Built-in List Template 'Web Page Library' with ID '119'
        /// </summary>
        public static ListTemplateInfo WebpageLibrary
        {
             get { return new ListTemplateInfo(119, new Guid("00BFEA71-C796-4402-9F2F-0EB9A6E71B18")); }
        }

        /// <summary>
        /// Built-in List Template 'Visio Process Repository Us' with ID '506'
        /// </summary>
        public static ListTemplateInfo VisioProcessRepositoryUs
        {
             get { return new ListTemplateInfo(506, new Guid("7E0AABEE-B92B-4368-8742-21AB16453D02")); }
        }

        /// <summary>
        /// Built-in List Template 'Visio Process Repository' with ID '505'
        /// </summary>
        public static ListTemplateInfo VisioProcessRepository
        {
             get { return new ListTemplateInfo(505, new Guid("7E0AABEE-B92B-4368-8742-21AB16453D01")); }
        }

        /// <summary>
        /// Built-in List Template 'xlatelist' with ID '1301'
        /// </summary>
        public static ListTemplateInfo Xlatelist
        {
             get { return new ListTemplateInfo(1301, new Guid("29D85C25-170C-4df9-A641-12DB0B9D4130")); }
        }
        
        /// <summary>
        /// Built-in List Template 'TransMgmtLib' with ID '1300'
        /// </summary>
        public static ListTemplateInfo TransMgmtLib
        {
             get { return new ListTemplateInfo(1300, new Guid("29D85C25-170C-4df9-A641-12DB0B9D4130")); }
        }

        /// <summary>
        /// Built-in List Template 'timecard' with ID '420'
        /// </summary>
        public static ListTemplateInfo Timecard
        {
             get { return new ListTemplateInfo(420, new Guid("d5191a77-fa2d-4801-9baf-9f4205c9e9d2")); }
        }

        /// <summary>
        /// Built-in List Template 'tasks Legacy' with ID '107'
        /// </summary>
        public static ListTemplateInfo TasksLegacy
        {
             get { return new ListTemplateInfo(107, new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107")); }
        }

        /// <summary>
        /// Built-in List Template 'Survey' with ID '102'
        /// </summary>
        public static ListTemplateInfo Survey
        {
             get { return new ListTemplateInfo(102, new Guid("00BFEA71-EB8A-40B1-80C7-506BE7590102")); }
        }

        /// <summary>
        /// Built-in List Template 'Social Data Store' with ID '550'
        /// </summary>
        public static ListTemplateInfo SocialDataStore
        {
             get { return new ListTemplateInfo(550, new Guid("FA8379C9-791A-4FB0-812E-D0CFCAC809C8")); }
        }

        /// <summary>
        /// Built-in List Template 'Slide Library' with ID '2100'
        /// </summary>
        public static ListTemplateInfo SlideLibrary
        {
             get { return new ListTemplateInfo(2100, new Guid("0BE49FE9-9BC9-409d-ABF9-702753BD878D")); }
        }

        /// <summary>
        /// Built-in List Template 'Search Config' with ID '101'
        /// </summary>
        public static ListTemplateInfo SearchConfig
        {
             get { return new ListTemplateInfo(101, new Guid("E47705EC-268D-4C41-AA4E-0D8727985EBC")); }
        }

        /// <summary>
        /// Built-in List Template 'schEdule' with ID '400'
        /// </summary>
        public static ListTemplateInfo Schedule
        {
             get { return new ListTemplateInfo(400, new Guid("636287a7-7f62-4a6e-9fcc-081f4672cbf8")); }
        }

        /// <summary>
        /// Built-in List Template 'Report Document Library' with ID '701'
        /// </summary>
        public static ListTemplateInfo ReportDocumentLibrary
        {
             get { return new ListTemplateInfo(701, new Guid("B435069A-E096-46E0-AE30-899DACA4B304")); }
        }

        /// <summary>
        /// Built-in List Template 'Report List' with ID '433'
        /// </summary>
        public static ListTemplateInfo ReportList
        {
             get { return new ListTemplateInfo(433, new Guid("2510D73F-7109-4ccc-8A1C-314894DEEB3A")); }
        }

        /// <summary>
        /// Built-in List Template 'Pages' with ID '850'
        /// </summary>
        public static ListTemplateInfo Pages
        {
             get { return new ListTemplateInfo(850, new Guid("22A9EF51-737B-4ff2-9346-694633FE4416")); }
        }

        /// <summary>
        /// Built-in List Template 'Promoted Links' with ID '170'
        /// </summary>
        public static ListTemplateInfo PromotedLinks
        {
             get { return new ListTemplateInfo(170, new Guid("192EFA95-E50C-475e-87AB-361CEDE5DD7F")); }
        }

        /// <summary>
        /// Built-in List Template 'Product Catalog' with ID '751'
        /// </summary>
        public static ListTemplateInfo ProductCatalog
        {
             get { return new ListTemplateInfo(751, new Guid("DD926489-FC66-47A6-BA00-CE0E959C9B41")); }
        }

        /// <summary>
        /// Built-in List Template 'Preservation' with ID '1310'
        /// </summary>
        public static ListTemplateInfo Preservation
        {
             get { return new ListTemplateInfo(1310, new Guid("BFC789AA-87BA-4d79-AFC7-0C7E45DAE01A")); }
        }

        /// <summary>
        /// Built-in List Template 'Performancepoint Services Workspace' with ID '450'
        /// </summary>
        public static ListTemplateInfo PerformancepointServicesWorkspace
        {
             get { return new ListTemplateInfo(450, new Guid("481333E1-A246-4d89-AFAB-D18C6FE344CE")); }
        }

        /// <summary>
        /// Built-in List Template 'Performancepoint Services Datasource' with ID '460'
        /// </summary>
        public static ListTemplateInfo PerformancepointServicesDatasource
        {
             get { return new ListTemplateInfo(460, new Guid("5D220570-DF17-405e-B42D-994237D60EBF")); }
        }

        /// <summary>
        /// Built-in List Template 'Picture Library' with ID '109'
        /// </summary>
        public static ListTemplateInfo PictureLibrary
        {
             get { return new ListTemplateInfo(109, new Guid("00BFEA71-52D4-45B3-B544-B1C71B620109")); }
        }

        /// <summary>
        /// Built-in List Template 'PhonePNSubscribers' with ID '2000'
        /// </summary>
        public static ListTemplateInfo PhonePNSubscribers
        {
             get { return new ListTemplateInfo(2000, new Guid("41E1D4BF-B1A2-47F7-AB80-D5D6CBBA3092")); }
        }

        /// <summary>
        /// Built-in List Template 'OfficeExtensionCatalog' with ID '332'
        /// </summary>
        public static ListTemplateInfo OfficeExtensionCatalog
        {
             get { return new ListTemplateInfo(332, new Guid("61E874CD-3AC3-4531-8628-28C3ACB78279")); }
        }

        /// <summary>
        /// Built-in List Template 'No Code Workflow' with ID '117'
        /// </summary>
        public static ListTemplateInfo NoCodeWorkflow
        {
             get { return new ListTemplateInfo(117, new Guid("00BFEA71-F600-43F6-A895-40C0DE7B0117")); }
        }

        /// <summary>
        /// Built-in List Template 'No Code Public Workflow' with ID '122'
        /// </summary>
        public static ListTemplateInfo NoCodePublicWorkflow
        {
             get { return new ListTemplateInfo(122, new Guid("00BFEA71-F600-43F6-A895-40C0DE7B0117")); }
        }

        /// <summary>
        /// Built-in List Template 'MySite MicroBlogging List' with ID '544'
        /// </summary>
        public static ListTemplateInfo MySiteMicrobloggingList
        {
             get { return new ListTemplateInfo(544, new Guid("EA23650B-0340-4708-B465-441A41C37AF7")); }
        }

        /// <summary>
        /// Built-in List Template 'MySite Document Library' with ID '700'
        /// </summary>
        public static ListTemplateInfo MySiteDocumentLibrary
        {
             get { return new ListTemplateInfo(700, new Guid("E9C0FF81-D821-4771-8B4C-246AA7E5E9EB")); }
        }

        /// <summary>
        /// Built-in List Template 'Monitored Apps List' with ID '401'
        /// </summary>
        public static ListTemplateInfo MonitoredAppsList
        {
             get { return new ListTemplateInfo(401, new Guid("345FF4F9-F706-41e1-92BC-3F0EC2D9F6EA")); }
        }

        /// <summary>
        /// Built-in List Template 'Membership List' with ID '880'
        /// </summary>
        public static ListTemplateInfo MembershipList
        {
             get { return new ListTemplateInfo(880, new Guid("947AFD14-0EA1-46c6-BE97-DEA1BF6F5BAE")); }
        }

        /// <summary>
        /// Built-in List Template 'Maintenance Logs' with ID '175'
        /// </summary>
        public static ListTemplateInfo MaintenanceLogs
        {
             get { return new ListTemplateInfo(175, new Guid("8c6f9096-388d-4eed-96ff-698b3ec46fc4")); }
        }

        /// <summary>
        /// Built-in List Template 'links' with ID '103'
        /// </summary>
        public static ListTemplateInfo Links
        {
             get { return new ListTemplateInfo(103, new Guid("00BFEA71-2062-426C-90BF-714C59600103")); }
        }

        /// <summary>
        /// Built-in List Template 'Legacy Document Library' with ID '101'
        /// </summary>
        public static ListTemplateInfo LegacyDocumentLibrary2010
        {
             get { return new ListTemplateInfo(101, new Guid("6E53DD27-98F2-4AE5-85A0-E9A8EF4AA6DF")); }
        }

        /// <summary>
        /// Built-in List Template 'Legacy Document Library' with ID '200'
        /// </summary>
        public static ListTemplateInfo LegacyDocumentLibrary2013
        {
             get { return new ListTemplateInfo(200, new Guid("6E53DD27-98F2-4AE5-85A0-E9A8EF4AA6DF")); }
        }

        /// <summary>
        /// Built-in List Template 'Issues list' with ID '1100'
        /// </summary>
        public static ListTemplateInfo Issueslist
        {
             get { return new ListTemplateInfo(1100, new Guid("00BFEA71-5932-4F9C-AD71-1557E5751100")); }
        }

        /// <summary>
        /// Built-in List Template 'Support Feature Converted List' with ID '10102'
        /// </summary>
        public static ListTemplateInfo SupportFeatureConvertedList
        {
             get { return new ListTemplateInfo(10102, new Guid("A0E5A010-1329-49d4-9E09-F280CDBED37D")); }
        }

        /// <summary>
        /// Built-in List Template 'In Place Records library' with ID '1302'
        /// </summary>
        public static ListTemplateInfo InPlaceRecordslibrary
        {
             get { return new ListTemplateInfo(1302, new Guid("DA2E115B-07E4-49d9-BB2C-35E93BB9FCA9")); }
        }

        /// <summary>
        /// Built-in List Template 'Input Method Editor Dictionary' with ID '499'
        /// </summary>
        public static ListTemplateInfo InputMethodEditorDictionary
        {
             get { return new ListTemplateInfo(499, new Guid("1C6A572C-1B58-49ab-B5DB-75CAF50692E6")); }
        }

        /// <summary>
        /// Built-in List Template 'Holidays List' with ID '421'
        /// </summary>
        public static ListTemplateInfo HolidaysList
        {
             get { return new ListTemplateInfo(421, new Guid("9ad4c2d4-443b-4a94-8534-49a23f20ba3c")); }
        }

        /// <summary>
        /// Built-in List Template 'Hierarchy Tasks List' with ID '171'
        /// </summary>
        public static ListTemplateInfo HierarchyTasksList
        {
             get { return new ListTemplateInfo(171, new Guid("F9CE21F8-F437-4f7e-8BC6-946378C850F0")); }
        }

        /// <summary>
        /// Built-in List Template 'Help Library' with ID '151'
        /// </summary>
        public static ListTemplateInfo HelpLibrary
        {
             get { return new ListTemplateInfo(151, new Guid("071DE60D-4B02-4076-B001-B456E93146FE")); }
        }

        /// <summary>
        /// Built-in List Template 'Grid List' with ID '120'
        /// </summary>
        public static ListTemplateInfo GridList
        {
             get { return new ListTemplateInfo(120, new Guid("00BFEA71-3A1D-41D3-A0EE-651D11570120")); }
        }

        /// <summary>
        /// Built-in List Template 'Gantt Tasks List' with ID '150'
        /// </summary>
        public static ListTemplateInfo GanttTasksList
        {
             get { return new ListTemplateInfo(150, new Guid("00BFEA71-513D-4CA0-96C2-6A47775C0119")); }
        }

        /// <summary>
        /// Built-in List Template 'FC Groups List' with ID '401'
        /// </summary>
        public static ListTemplateInfo FCGroupsList
        {
             get { return new ListTemplateInfo(401, new Guid("08386d3d-7cc0-486b-a730-3b4cfe1b5509")); }
        }

        /// <summary>
        /// Built-in List Template 'Facility List' with ID '402'
        /// </summary>
        public static ListTemplateInfo FacilityList
        {
             get { return new ListTemplateInfo(402, new Guid("58160a6b-4396-4d6e-867c-65381fb5fbc9")); }
        }

        /// <summary>
        /// Built-in List Template 'External Subscriptions' with ID '2001'
        /// </summary>
        public static ListTemplateInfo ExternalSubscriptions
        {
             get { return new ListTemplateInfo(2001, new Guid("5B10D113-2D0D-43BD-A2FD-F8BC879F5ABD")); }
        }

        /// <summary>
        /// Built-in List Template 'External List' with ID '600'
        /// </summary>
        public static ListTemplateInfo ExternalList
        {
             get { return new ListTemplateInfo(600, new Guid("00BFEA71-9549-43f8-B978-E47E54A10600")); }
        }

        /// <summary>
        /// Built-in List Template 'Events List' with ID '106'
        /// </summary>
        public static ListTemplateInfo EventsList
        {
             get { return new ListTemplateInfo(106, new Guid("00BFEA71-EC85-4903-972D-EBE475780106")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduentity' with ID '10051'
        /// </summary>
        public static ListTemplateInfo Eduentity
        {
             get { return new ListTemplateInfo(10051, new Guid("7F52C29E-736D-11E0-80B8-9EDD4724019B")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduusersetting' with ID '10060'
        /// </summary>
        public static ListTemplateInfo Eduusersetting
        {
             get { return new ListTemplateInfo(10060, new Guid("7F52C29E-736D-11E0-80B8-9EDD4724019B")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduannouncement' with ID '10401'
        /// </summary>
        public static ListTemplateInfo Eduannouncement
        {
             get { return new ListTemplateInfo(10401, new Guid("A46935C3-545F-4C15-A2FD-3A19B62D8A02")); }
        }

        /// <summary>
        /// Built-in List Template 'Educalendar' with ID '10631'
        /// </summary>
        public static ListTemplateInfo Educalendar
        {
             get { return new ListTemplateInfo(10631, new Guid("A46935C3-545F-4C15-A2FD-3A19B62D8A02")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduentity' with ID '10001'
        /// </summary>
        public static ListTemplateInfo Eduentity1
        {
             get { return new ListTemplateInfo(10001, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduexternalsyncsetting' with ID '10061'
        /// </summary>
        public static ListTemplateInfo Eduexternalsyncsetting
        {
             get { return new ListTemplateInfo(10061, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'Edudocument' with ID '10101'
        /// </summary>
        public static ListTemplateInfo Edudocument
        {
             get { return new ListTemplateInfo(10101, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'Eduannouncement' with ID '10401'
        /// </summary>
        public static ListTemplateInfo Eduannouncement1
        {
             get { return new ListTemplateInfo(10401, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'EduWorkItem' with ID '10007'
        /// </summary>
        public static ListTemplateInfo EduWorkItem
        {
             get { return new ListTemplateInfo(10007, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'EduQuiz' with ID '10008'
        /// </summary>
        public static ListTemplateInfo EduQuiz
        {
             get { return new ListTemplateInfo(10008, new Guid("A16E895C-E61A-11DF-8F6E-103EDFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Sources' with ID '1305'
        /// </summary>
        public static ListTemplateInfo EDiscoverySources
        {
             get { return new ListTemplateInfo(1305, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Source Instances' with ID '1306'
        /// </summary>
        public static ListTemplateInfo EDiscoverySourceInstances
        {
             get { return new ListTemplateInfo(1306, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Source Groups' with ID '1307'
        /// </summary>
        public static ListTemplateInfo EDiscoverySourceGroups
        {
             get { return new ListTemplateInfo(1307, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Custodians' with ID '1308'
        /// </summary>
        public static ListTemplateInfo EDiscoveryCustodians
        {
             get { return new ListTemplateInfo(1308, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Saved Searches' with ID '1309'
        /// </summary>
        public static ListTemplateInfo EDiscoverySavedSearches
        {
             get { return new ListTemplateInfo(1309, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'EDiscovery Exports' with ID '1310'
        /// </summary>
        public static ListTemplateInfo EDiscoveryExports
        {
             get { return new ListTemplateInfo(1310, new Guid("E8C02A2A-9010-4F98-AF88-6668D59F91A7")); }
        }

        /// <summary>
        /// Built-in List Template 'Document Library' with ID '101'
        /// </summary>
        public static ListTemplateInfo DocumentLibrary
        {
             get { return new ListTemplateInfo(101, new Guid("00BFEA71-E717-4E80-AA17-D0C71B360101")); }
        }

        /// <summary>
        /// Built-in List Template 'Acquisition History List' with ID '10099'
        /// </summary>
        public static ListTemplateInfo AcquisitionHistoryList
        {
             get { return new ListTemplateInfo(10099, new Guid("184C82E7-7EB1-4384-8E8C-62720EF397A0")); }
        }

        /// <summary>
        /// Built-in List Template 'Discussions List' with ID '108'
        /// </summary>
        public static ListTemplateInfo DiscussionsList
        {
             get { return new ListTemplateInfo(108, new Guid("00BFEA71-6A49-43FA-B535-D15C05500108")); }
        }

        /// <summary>
        /// Built-in List Template 'Draft Apps List' with ID '1230'
        /// </summary>
        public static ListTemplateInfo DraftAppsList
        {
             get { return new ListTemplateInfo(1230, new Guid("E374875E-06B6-11E0-B0FA-57F5DFD72085")); }
        }

        /// <summary>
        /// Built-in List Template 'Data Source Library' with ID '110'
        /// </summary>
        public static ListTemplateInfo DataSourceLibrary
        {
             get { return new ListTemplateInfo(110, new Guid("00BFEA71-F381-423D-B9D1-DA7A54C50110")); }
        }

        /// <summary>
        /// Built-in List Template 'Data Connection Library' with ID '130'
        /// </summary>
        public static ListTemplateInfo DataConnectionLibrary
        {
             get { return new ListTemplateInfo(130, new Guid("00BFEA71-DBD7-4F72-B8CB-DA7AC0440130")); }
        }

        /// <summary>
        /// Built-in List Template 'Custom List' with ID '100'
        /// </summary>
        public static ListTemplateInfo CustomList
        {
             get { return new ListTemplateInfo(100, new Guid("00BFEA71-DE22-43B2-A848-C05709900100")); }
        }

        /// <summary>
        /// Built-in List Template 'Corporate Catalog' with ID '330'
        /// </summary>
        public static ListTemplateInfo CorporateCatalog
        {
             get { return new ListTemplateInfo(330, new Guid("0AC11793-9C2F-4CAC-8F22-33F93FAC18F2")); }
        }

        /// <summary>
        /// Built-in List Template 'Content Following List' with ID '530'
        /// </summary>
        public static ListTemplateInfo ContentFollowingList
        {
             get { return new ListTemplateInfo(530, new Guid("A34E5458-8D20-4C0D-B137-E1390F5824A1")); }
        }

        /// <summary>
        /// Built-in List Template 'Contacts list' with ID '105'
        /// </summary>
        public static ListTemplateInfo Contactslist
        {
             get { return new ListTemplateInfo(105, new Guid("00BFEA71-7E6D-4186-9BA8-C047AC750105")); }
        }

        /// <summary>
        /// Built-in List Template 'Circulation List' with ID '405'
        /// </summary>
        public static ListTemplateInfo CirculationList
        {
             get { return new ListTemplateInfo(405, new Guid("a568770a-50ba-4052-ab48-37d8029b3f47")); }
        }

        /// <summary>
        /// Built-in List Template 'Categories list' with ID '500'
        /// </summary>
        public static ListTemplateInfo Categorieslist
        {
             get { return new ListTemplateInfo(500, new Guid("D32700C7-9EC5-45e6-9C89-EA703EFCA1DF")); }
        }

        /// <summary>
        /// Built-in List Template 'Phone Call Memo List' with ID '404'
        /// </summary>
        public static ListTemplateInfo PhoneCallMemoList
        {
             get { return new ListTemplateInfo(404, new Guid("239650e3-ee0b-44a0-a22a-48292402b8d8")); }
        }

        /// <summary>
        /// Built-in List Template 'Blog Site Posts list' with ID '301'
        /// </summary>
        public static ListTemplateInfo BlogSitePostslist
        {
             get { return new ListTemplateInfo(301, new Guid("FAF00902-6BAB-4583-BD02-84DB191801D8")); }
        }

        /// <summary>
        /// Built-in List Template 'Blog Site Comments list' with ID '302'
        /// </summary>
        public static ListTemplateInfo BlogSiteCommentslist
        {
             get { return new ListTemplateInfo(302, new Guid("FAF00902-6BAB-4583-BD02-84DB191801D8")); }
        }

        /// <summary>
        /// Built-in List Template 'Blog Site Categories list' with ID '303'
        /// </summary>
        public static ListTemplateInfo BlogSiteCategorieslist
        {
             get { return new ListTemplateInfo(303, new Guid("FAF00902-6BAB-4583-BD02-84DB191801D8")); }
        }

        /// <summary>
        /// Built-in List Template 'SharePoint Portal Server Status Indicator List' with ID '432'
        /// </summary>
        public static ListTemplateInfo SharePointPortalServerStatusIndicatorList
        {
             get { return new ListTemplateInfo(432, new Guid("065C78BE-5231-477e-A972-14177CC5B3C7")); }
        }

        /// <summary>
        /// Built-in List Template 'BI Data Connections Library' with ID '470'
        /// </summary>
        public static ListTemplateInfo BIDataConnectionsLibrary
        {
             get { return new ListTemplateInfo(470, new Guid("26676156-91A0-49F7-87AA-37B1D5F0C4D0")); }
        }

        /// <summary>
        /// Built-in List Template 'BI Dashboards Library' with ID '480'
        /// </summary>
        public static ListTemplateInfo BIDashboardsLibrary
        {
             get { return new ListTemplateInfo(480, new Guid("F979E4DC-1852-4F26-AB92-D1B2A190AFC9")); }
        }

        /// <summary>
        /// Built-in List Template 'Asset Library' with ID '851'
        /// </summary>
        public static ListTemplateInfo AssetLibrary
        {
             get { return new ListTemplateInfo(851, new Guid("4BCCCD62-DCAF-46dc-A7D4-E38277EF33F4")); }
        }

        /// <summary>
        /// Built-in List Template 'App Request List' with ID '333'
        /// </summary>
        public static ListTemplateInfo AppRequestList
        {
             get { return new ListTemplateInfo(333, new Guid("334DFC83-8655-48A1-B79D-68B7F6C63222")); }
        }

        /// <summary>
        /// Built-in List Template 'Announcements List' with ID '104'
        /// </summary>
        public static ListTemplateInfo AnnouncementsList
        {
             get { return new ListTemplateInfo(104, new Guid("00BFEA71-D1CE-42de-9C63-A44004CE0104")); }
        }

        /// <summary>
        /// Built-in List Template 'Access Services User Application Log' with ID '398'
        /// </summary>
        public static ListTemplateInfo AccessServicesUserApplicationLog
        {
             get { return new ListTemplateInfo(398, new Guid("28101B19-B896-44f4-9264-DB028F307A62")); }
        }

        /// <summary>
        /// Built-in List Template 'Access Services Restricted List' with ID '397'
        /// </summary>
        public static ListTemplateInfo AccessServicesRestrictedList
        {
             get { return new ListTemplateInfo(397, new Guid("A4D4EE2C-A6CB-4191-AB0A-21BB5BDE92FB")); }
        }

        /// <summary>
        /// Built-in List Template 'Access Services System Objects' with ID '399'
        /// </summary>
        public static ListTemplateInfo AccessServicesSystemObjects
        {
             get { return new ListTemplateInfo(399, new Guid("29EA7495-FCA1-4dc6-8AC1-500C247A036E")); }
        }

        /// <summary>
        /// Built-in List Template 'Access Requests' with ID '160'
        /// </summary>
        public static ListTemplateInfo AccessRequests
        {
             get { return new ListTemplateInfo(160, new Guid("A0F12EE4-9B60-4ba4-81F6-75724F4CA973")); }
        }

        /// <summary>
        /// Built-in List Template 'Abuse Reports List' with ID '925'
        /// </summary>
        public static ListTemplateInfo AbuseReportsList
        {
             get { return new ListTemplateInfo(925, new Guid("C6A92DBF-6441-4b8b-882F-8D97CB12C83A")); }
        }
    }
}
