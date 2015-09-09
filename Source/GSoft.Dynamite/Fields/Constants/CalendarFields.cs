using System;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields.Constants
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
        public const string StartDateName = "StartDate";

        /// <summary>
        /// EndDate field internal name (this one is hidden by default)
        /// </summary>
        public const string EndDateName = "EndDate";

        /// <summary>
        /// EndDate: Date and time field to display the end date (this one is visible by default).
        /// </summary>
        public const string EndDateVisibleName = "_EndDate";

        /// <summary>
        /// Location field internal name
        /// </summary>
        public const string LocationName = "Location";    

        /// <summary>
        /// Description field internal name
        /// </summary>
        public const string CommentsName = "Description";       

        /// <summary>
        /// Category field internal name
        /// </summary>
        public const string CategoryName = "Category";       

        /// <summary>
        /// fAllDayEvent field internal name
        /// </summary>
        public const string AllDayEventName = "fAllDayEvent";
        
        /// <summary>
        /// EventType field internal name
        /// </summary>
        public const string EventTypeName = "EventType";

        /// <summary>
        /// UID field internal name
        /// </summary>
        public const string UIDName = "UID";

        /// <summary>
        /// EventCanceled field internal name
        /// </summary>
        public const string EventCanceledName = "EventCanceled";

        /// <summary>
        /// Duration field internal name
        /// </summary>
        public const string DurationName = "Duration";

        /// <summary>
        /// fRecurrence field internal name
        /// </summary>
        public const string HasRecurrenceName = "fRecurrence";

        /// <summary>
        /// RecurrenceID field internal name
        /// </summary>
        public const string RecurrenceIDName = "RecurrenceID";

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
        /// WorkspaceLink field internal name
        /// </summary>
        public const string WorkspaceLinkName = "WorkspaceLink";

        /// <summary>
        /// Workspace field internal name
        /// </summary>
        public const string WorkspaceName = "Workspace";

        #endregion

        #region FieldInfo

        /// <summary>
        /// EventDate field info (OOTB type = Date, Format = DateOnly)
        /// </summary>
        public static BaseFieldInfo StartDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(StartDateName, new Guid("{64cd368d-2f95-4bfc-a1f9-8d4324ecb007}")); 
            } 
        }

        /// <summary>
        /// EndDate field info (this one is hidden by default) (OOTB type = DateTime, format = DateTime)
        /// </summary>
        public static BaseFieldInfo EndDate
        {
            get
            {
                return new MinimalFieldInfo<DateTime?>(EndDateName, new Guid("{2684f9f2-54be-429f-ba06-76754fc056bf}"));
            }
        }

        /// <summary>
        /// End date field info (this one is visible by default) (OOTB type = DateTime, format = DateTime)
        /// </summary>
        public static BaseFieldInfo EndDateVisible
        {
            get
            {
                return new MinimalFieldInfo<DateTime?>(EndDateVisibleName, new Guid("{8a121252-85a9-443d-8217-a1b57020fadf}"));
            }
        }

        /// <summary>
        /// Location field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Location 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(LocationName, new Guid("{288f5f32-8462-4175-8f09-dd7ba29359a9}")); 
            } 
        }

        /// <summary>
        /// Comments field info (OOTB type = Note, RichText = TRUE, Display Name = "Description")
        /// </summary>
        public static BaseFieldInfo Comments 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(CommentsName, new Guid("{9da97a8a-1da5-4a77-98d3-4bc10456e700}")); 
            } 
        }

        /// <summary>
        /// Category field info (OOTB type = Choice)
        /// </summary>
        public static BaseFieldInfo Category 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(CategoryName, new Guid("{6df9bd52-550e-4a30-bc31-a4366832a87d}")); 
            } 
        }

        /// <summary>
        /// fAllDayEvent field info (OOTB type = AllDayEvent)
        /// </summary>
        public static BaseFieldInfo AllDayEvent 
        { 
            get 
            { 
                return new MinimalFieldInfo<bool?>(AllDayEventName, new Guid("{7d95d1f4-f5fd-4a70-90cd-b35abc9b5bc8}")); 
            } 
        }

        /// <summary>
        /// EventType field info (OOTB type = Integer)
        /// </summary>
        public static BaseFieldInfo EventType 
        { 
            get 
            { 
                return new MinimalFieldInfo<double?>(EventTypeName, new Guid("{5d1d4e76-091a-4e03-ae83-6a59847731c0}")); 
            } 
        }

        /// <summary>
        /// UID field info (OOTB type = Guid)
        /// </summary>
        public static BaseFieldInfo UID 
        { 
            get 
            { 
                return new MinimalFieldInfo<Guid?>(UIDName, new Guid("{63055d04-01b5-48f3-9e1e-e564e7c6b23b}")); 
            } 
        }

        /// <summary>
        /// EventCanceled field info (OOTB type = Boolean)
        /// </summary>
        public static BaseFieldInfo EventCanceled 
        { 
            get 
            { 
                return new MinimalFieldInfo<bool?>(EventCanceledName, new Guid("{b8bbe503-bb22-4237-8d9e-0587756a2176}")); 
            } 
        }

        /// <summary>
        /// Duration field info (OOTB type = Integer)
        /// </summary>
        public static BaseFieldInfo Duration 
        { 
            get 
            { 
                return new MinimalFieldInfo<int?>(DurationName, new Guid("{4d54445d-1c84-4a6d-b8db-a51ded4e1acc}")); 
            } 
        }

        /// <summary>
        /// fRecurrence field info - indicates whether recurrence is configured on the item or not (OOTB type = Recurrence)
        /// </summary>
        public static BaseFieldInfo HasRecurrence
        {
            get
            {
                return new MinimalFieldInfo<bool?>(HasRecurrenceName, new Guid("{f2e63656-135e-4f1c-8fc2-ccbe74071901}"));
            }
        }

        /// <summary>
        /// RecurrenceID field info (OOTB type = DateTime, Format = ISO8601Gregorian)
        /// </summary>
        public static BaseFieldInfo RecurrenceID
        {
            get
            {
                return new MinimalFieldInfo<DateTime?>(RecurrenceIDName, new Guid("{dfcc8fff-7c4c-45d6-94ed-14ce0719efef}"));
            }
        }

        /// <summary>
        /// RecurrenceData field info (OOTB type = Note)
        /// </summary>
        public static BaseFieldInfo RecurrenceData 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(RecurrenceDataName, new Guid("{d12572d0-0a1e-4438-89b5-4d0430be7603}")); 
            } 
        }

        /// <summary>
        /// TimeZone field info (OOTB type = Integer)
        /// </summary>
        public static BaseFieldInfo TimeZone 
        { 
            get 
            { 
                return new MinimalFieldInfo<int?>(TimeZoneName, new Guid("{6cc1c612-748a-48d8-88f2-944f477f301b}")); 
            } 
        }

        /// <summary>
        /// XMLTZone field info (OOTB type = Note)
        /// </summary>
        public static BaseFieldInfo XMLTZone 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(XMLTZoneName, new Guid("{c4b72ed6-45aa-4422-bff1-2b6750d30819}")); 
            } 
        }

        /// <summary>
        /// MasterSeriesItemID field info (OOTB type = Integer)
        /// </summary>
        public static BaseFieldInfo MasterSeriesItemID 
        { 
            get 
            { 
                return new MinimalFieldInfo<int?>(MasterSeriesItemIDName, new Guid("{9b2bed84-7769-40e3-9b1d-7954a4053834}")); 
            } 
        }

        /// <summary>
        /// WorkspaceLink field info (OOTB type = CrossProjectLink)
        /// </summary>
        public static BaseFieldInfo WorkspaceLink
        {
            get
            {
                return new MinimalFieldInfo<string>(WorkspaceLinkName, new Guid("{08fc65f9-48eb-4e99-bd61-5946c439e691}"));
            }
        }

        /// <summary>
        /// Workspace field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo Workspace 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(WorkspaceName, new Guid("{881eac4a-55a5-48b6-a28e-8329d7486120}")); 
            } 
        }

        #endregion
    }
}
