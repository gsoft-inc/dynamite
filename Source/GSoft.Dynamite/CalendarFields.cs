using System;
using GSoft.Dynamite.Sharepoint2013.Utils;

namespace GSoft.Dynamite.Sharepoint2013
{
    /// <summary>
    /// Site columns constants for (OOTB) event content types
    /// </summary>
    public static class CalendarFields
    {
        #region Name

        /// <summary>
        /// EventDate field internal name
        /// </summary>
        public const string EventDateName = "EventDate";

        /// <summary>
        /// Location field internal name
        /// </summary>
        public const string LocationName = "Location";    

        /// <summary>
        /// EndDate field internal name
        /// </summary>
        public const string EndDateName = "EndDate";

        /// <summary>
        /// Description field internal name
        /// </summary>
        public const string DescriptionName = "Description";       

        /// <summary>
        /// Category field internal name
        /// </summary>
        public const string CategoryName = "Category";       

        /// <summary>
        /// fAllDayEvent field internal name
        /// </summary>
        public const string AllDayEventName = "fAllDayEvent";

        /// <summary>
        /// fRecurrence field internal name
        /// </summary>
        public const string RecurrenceName = "fRecurrence";

        /// <summary>
        /// WorkspaceLink field internal name
        /// </summary>
        public const string WorkspaceLinkName = "WorkspaceLink";

        /// <summary>
        /// EventType field internal name
        /// </summary>
        public const string EventTypeName = "EventType";

        /// <summary>
        /// UID field internal name
        /// </summary>
        public const string UIDName = "UID";

        /// <summary>
        /// RecurrenceID field internal name
        /// </summary>
        public const string RecurrenceIDName = "RecurrenceID";

        /// <summary>
        /// EventCanceled field internal name
        /// </summary>
        public const string EventCanceledName = "EventCanceled";

        /// <summary>
        /// Duration field internal name
        /// </summary>
        public const string DurationName = "Duration";

        /// <summary>
        /// RecurrenceData field internal name
        /// </summary>
        public const string RecurrenceDataName = "RecurrenceData";

        /// <summary>
        /// TimeZone field internal name
        /// </summary>
        public const string TimeZoneName = "TimeZone";
        
        /// <summary>
        /// XMLTZone field internal name
        /// </summary>
        public const string XMLTZoneName = "XMLTZone";
        
        /// <summary>
        /// MasterSeriesItemID field internal name
        /// </summary>
        public const string MasterSeriesItemIDName = "MasterSeriesItemID";
        
        /// <summary>
        /// Workspace field internal name
        /// </summary>
        public const string WorkspaceName = "Workspace";

        #endregion

        #region FieldInfo

        /// <summary>
        /// EventDate field info
        /// </summary>
        public static readonly FieldInfo EventDate = new FieldInfo(EventDateName, new Guid("{64cd368d-2f95-4bfc-a1f9-8d4324ecb007}"));

        /// <summary>
        /// Location field info
        /// </summary>
        public static readonly FieldInfo Location = new FieldInfo(LocationName, new Guid("{288f5f32-8462-4175-8f09-dd7ba29359a9}"));

        /// <summary>
        /// EndDate field info
        /// </summary>
        public static readonly FieldInfo EndDate = new FieldInfo(EndDateName, new Guid("{2684f9f2-54be-429f-ba06-76754fc056bf}"));

        /// <summary>
        /// Description field info
        /// </summary>
        public static readonly FieldInfo Description = new FieldInfo(DescriptionName, new Guid("{9da97a8a-1da5-4a77-98d3-4bc10456e700}"));

        /// <summary>
        /// Category field info
        /// </summary>
        public static readonly FieldInfo Category = new FieldInfo(CategoryName, new Guid("{6df9bd52-550e-4a30-bc31-a4366832a87d}"));

        /// <summary>
        /// fAllDayEvent field info
        /// </summary>
        public static readonly FieldInfo AllDayEvent = new FieldInfo(AllDayEventName, new Guid("{7d95d1f4-f5fd-4a70-90cd-b35abc9b5bc8}"));

        /// <summary>
        /// fRecurrence field info
        /// </summary>
        public static readonly FieldInfo Recurrence = new FieldInfo(RecurrenceName, new Guid("{f2e63656-135e-4f1c-8fc2-ccbe74071901}"));

        /// <summary>
        /// WorkspaceLink field info
        /// </summary>
        public static readonly FieldInfo WorkspaceLink = new FieldInfo(WorkspaceLinkName, new Guid("{08fc65f9-48eb-4e99-bd61-5946c439e691}"));

        /// <summary>
        /// EventType field info
        /// </summary>
        public static readonly FieldInfo EventType = new FieldInfo(EventTypeName, new Guid("{5d1d4e76-091a-4e03-ae83-6a59847731c0}"));

        /// <summary>
        /// UID field info
        /// </summary>
        public static readonly FieldInfo UID = new FieldInfo(UIDName, new Guid("{63055d04-01b5-48f3-9e1e-e564e7c6b23b}"));

        /// <summary>
        /// RecurrenceID field info
        /// </summary>
        public static readonly FieldInfo RecurrenceID = new FieldInfo(RecurrenceIDName, new Guid("{dfcc8fff-7c4c-45d6-94ed-14ce0719efef}"));

        /// <summary>
        /// EventCanceled field info
        /// </summary>
        public static readonly FieldInfo EventCanceled = new FieldInfo(EventCanceledName, new Guid("{b8bbe503-bb22-4237-8d9e-0587756a2176}"));

        /// <summary>
        /// Duration field info
        /// </summary>
        public static readonly FieldInfo Duration = new FieldInfo(DurationName, new Guid("{4d54445d-1c84-4a6d-b8db-a51ded4e1acc}"));

        /// <summary>
        /// RecurrenceData field info
        /// </summary>
        public static readonly FieldInfo RecurrenceData = new FieldInfo(RecurrenceDataName, new Guid("{d12572d0-0a1e-4438-89b5-4d0430be7603}"));

        /// <summary>
        /// TimeZone field info
        /// </summary>
        public static readonly FieldInfo TimeZone = new FieldInfo(TimeZoneName, new Guid("{6cc1c612-748a-48d8-88f2-944f477f301b}"));

        /// <summary>
        /// XMLTZone field info
        /// </summary>
        public static readonly FieldInfo XMLTZone = new FieldInfo(XMLTZoneName, new Guid("{c4b72ed6-45aa-4422-bff1-2b6750d30819}"));

        /// <summary>
        /// MasterSeriesItemID field info
        /// </summary>
        public static readonly FieldInfo MasterSeriesItemID = new FieldInfo(MasterSeriesItemIDName, new Guid("{9b2bed84-7769-40e3-9b1d-7954a4053834}"));
        
        /// <summary>
        /// Workspace field info
        /// </summary>
        public static readonly FieldInfo Workspace = new FieldInfo(WorkspaceName, new Guid("{881eac4a-55a5-48b6-a28e-8329d7486120}"));

        #endregion
    }
}
