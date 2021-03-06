-- INSTRUCTIONS
-- find and replace [user] with your notices user's username in [domain\username] format

USE [master]
GO
CREATE DATABASE [CLC_Notices]
GO
ALTER DATABASE [CLC_Notices] SET DISABLE_BROKER 
GO
ALTER DATABASE [CLC_Notices] SET RECOVERY FULL 
GO
USE [CLC_Notices]
GO
CREATE SCHEMA [HangFire]
GO
/****** Object:  Table [dbo].[Dialin_Ignore_List]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dialin_Ignore_List](
	[PhoneNumber] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Dialin_Ignore_List] PRIMARY KEY CLUSTERED 
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Dialin_Patrons]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dialin_Patrons](
	[Barcode] [varchar](50) NOT NULL,
	[PhoneNumber] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Dialin_Patrons] PRIMARY KEY CLUSTERED 
(
	[Barcode] ASC,
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Dialout_String_Types]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dialout_String_Types](
	[StringTypeID] [int] NOT NULL,
	[Description] [varchar](max) NOT NULL,
 CONSTRAINT [PK_CLC_Custom_Dialout_String_Types] PRIMARY KEY CLUSTERED 
(
	[StringTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Dialout_Strings]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dialout_Strings](
	[StringID] [int] IDENTITY(1,1) NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[StringTypeID] [int] NOT NULL,
	[Value] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Dialout_Strings] PRIMARY KEY CLUSTERED 
(
	[StringID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EmailDomains]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EmailDomains](
	[Domain] [varchar](255) NOT NULL,
	[OrganizationID] [int] NOT NULL,
 CONSTRAINT [PK_EmailDomains] PRIMARY KEY CLUSTERED 
(
	[Domain] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RecordSets]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordSets](
	[RecordSetID] [int] NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[RecordSetTypeID] [int] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RecordSetTypes]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RecordSetTypes](
	[RecordSetTypeID] [int] NOT NULL,
	[Description] [varchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SMS_Group]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SMS_Group](
	[PhoneNumber] [varchar](50) NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_SMS_Group] PRIMARY KEY CLUSTERED 
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SMS_Queue]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SMS_Queue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PhoneNumber] [varchar](50) NOT NULL,
	[Message] [varchar](255) NOT NULL,
	[InsertDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SMS_Queue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [HangFire].[AggregatedCounter]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[AggregatedCounter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [bigint] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Counter]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Counter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [smallint] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Counter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Hash]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Hash](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Field] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime2](7) NULL,
 CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Job]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Job](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StateId] [int] NULL,
	[StateName] [nvarchar](20) NULL,
	[InvocationData] [nvarchar](max) NOT NULL,
	[Arguments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[JobParameter]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobParameter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobId] [int] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[JobQueue]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobId] [int] NOT NULL,
	[Queue] [nvarchar](50) NOT NULL,
	[FetchedAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[List]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[List](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Schema]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Schema](
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Server]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Server](
	[Id] [nvarchar](100) NOT NULL,
	[Data] [nvarchar](max) NULL,
	[LastHeartbeat] [datetime] NOT NULL,
 CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[Set]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Set](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Score] [float] NOT NULL,
	[Value] [nvarchar](256) NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [HangFire].[State]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[State](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobId] [int] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Reason] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (1, N'Goodbye')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (2, N'Greeting')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (3, N'Hold date format string')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (4, N'First hold message format string')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (5, N'Additonal hold message format string')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (6, N'placeholder')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (7, N'Intro')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (8, N'Branch name')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (9, N'Overdue')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (10, N'Renewal')
INSERT [dbo].[Dialout_String_Types] ([StringTypeID], [Description]) VALUES (11, N'Repeat')
INSERT [dbo].[RecordSetTypes] ([RecordSetTypeID], [Description]) VALUES (1, N'Bounce')
INSERT [dbo].[RecordSetTypes] ([RecordSetTypeID], [Description]) VALUES (2, N'Spam')
SET IDENTITY_INSERT [dbo].[Dialout_Strings] ON 
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (1, 1, 1, N'If you have questions regarding your library account, please contact the library. Thank you for using the library. Goodbye.')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (2, 1, 2, N'Hello, this is a message from the library for')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (3, 1, 3, N'dddd, MMMM dd')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (4, 1, 4, N'you have {0} held {1} at {2} available for pickup by {3}')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (5, 1, 5, N'{0} held {1} at {2} available for pickup by {3}')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (6, 1, 7, N'According to our library records, ')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (7, 1, 9, N'You have items overdue.')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (8, 1, 10, N'If you''d like to be connected to our automated renewals system, press 1 now.')
INSERT [dbo].[Dialout_Strings] ([StringID], [OrganizationID], [StringTypeID], [Value]) VALUES (9, 1, 11, N'This message will now be repeated.')
SET IDENTITY_INSERT [dbo].[Dialout_Strings] OFF

GO
/****** Object:  View [dbo].[PolarisNotifications]    Script Date: 11/17/2015 7:38:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[PolarisNotifications]
AS
SELECT        nq.NotificationTypeID, p.PatronID, p.Barcode AS PatronBarcode, pr.NameFirst, pr.NameLast, ISNULL(nq.ItemRecordID, 0) AS ItemRecordID, p.OrganizationID AS PatronBranch, 
                         o_patron.Abbreviation AS PatronBranchAbbr, o_patron.ParentOrganizationID AS PatronLibrary, o_patron2.Abbreviation AS PatronLibraryAbbr, nq.DeliveryOptionID, 
                         CASE nq.DeliveryOptionID WHEN 3 THEN pr.PhoneVoice1 WHEN 4 THEN pr.PhoneVoice2 WHEN 5 THEN pr.PhoneVoice3 END AS DeliveryString, ISNULL(nq.ReportingOrgID, 0) AS ReportingBranchID, 
                         o_notice.Abbreviation AS ReportingBranchAbbr, ISNULL(o_notice.ParentOrganizationID, 0) AS ReportingLibraryID, shr.HoldTillDate
FROM            Results.Polaris.NotificationQueue AS nq WITH (nolock) INNER JOIN
                         Polaris.Polaris.Patrons AS p WITH (nolock) ON nq.PatronID = p.PatronID INNER JOIN
                         Polaris.Polaris.PatronRegistration AS pr WITH (nolock) ON p.PatronID = pr.PatronID INNER JOIN
                         Polaris.Polaris.Organizations AS o_patron WITH (nolock) ON p.OrganizationID = o_patron.OrganizationID INNER JOIN
                         Polaris.Polaris.Organizations AS o_patron2 WITH (nolock) ON o_patron.ParentOrganizationID = o_patron2.OrganizationID INNER JOIN
                         Polaris.Polaris.Organizations AS o_notice WITH (nolock) ON nq.ReportingOrgID = o_notice.OrganizationID LEFT OUTER JOIN
                         Polaris.Polaris.SysHoldRequests AS shr WITH (nolock) ON shr.PatronID = p.PatronID AND nq.ItemRecordID = shr.TrappingItemRecordID
WHERE        (nq.DeliveryOptionID IN (3, 4, 5)) AND (nq.NotificationTypeID IN (1, 2, 12, 13))


GO
/****** Object:  View [dbo].[PolarisOrganizations]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[PolarisOrganizations]
AS
SELECT        OrganizationID, ParentOrganizationID, OrganizationCodeID, Name, Abbreviation, SA_ContactPersonID, CreatorID, ModifierID, CreationDate, ModificationDate, DisplayName
FROM            Polaris.Polaris.Organizations


GO
/****** Object:  View [dbo].[TodaysHoldCalls]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TodaysHoldCalls]
AS
SELECT DISTINCT p.PatronID, nh.ReportingOrgId, ISNULL(o.ParentOrganizationID, 0) AS ReportingLibraryID
FROM            Results.Polaris.NotificationHistory AS nh WITH (nolock) INNER JOIN
                         Polaris.Polaris.Patrons AS p WITH (nolock) ON nh.PatronId = p.PatronID INNER JOIN
                         Polaris.Polaris.Organizations AS o ON o.OrganizationID = nh.ReportingOrgId
WHERE        (CAST(FLOOR(CAST(GETDATE() AS float)) AS datetime) = CAST(FLOOR(CAST(nh.NoticeDate AS float)) AS datetime)) AND (nh.DeliveryOptionId IN (3, 4, 5)) AND (nh.NotificationTypeId = 2)

GO
ALTER TABLE [dbo].[SMS_Queue] ADD  CONSTRAINT [DF_SMS_Queue_InsertDate]  DEFAULT (getdate()) FOR [InsertDate]
GO
ALTER TABLE [dbo].[Dialout_Strings]  WITH CHECK ADD  CONSTRAINT [FK_Dialout_Strings_Dialout_String_Types] FOREIGN KEY([StringTypeID])
REFERENCES [dbo].[Dialout_String_Types] ([StringTypeID])
GO
ALTER TABLE [dbo].[Dialout_Strings] CHECK CONSTRAINT [FK_Dialout_Strings_Dialout_String_Types]
GO
ALTER TABLE [HangFire].[JobParameter]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[JobParameter] CHECK CONSTRAINT [FK_HangFire_JobParameter_Job]
GO
ALTER TABLE [HangFire].[State]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[State] CHECK CONSTRAINT [FK_HangFire_State_Job]
GO
/****** Object:  StoredProcedure [dbo].[CLC_Custom_Refresh_DatesClosed_For_Dialout]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CLC_Custom_Refresh_DatesClosed_For_Dialout]
	@orgid int, @threshold float
AS
BEGIN
	SET NOCOUNT ON;
	
	declare @libraries decimal = ( select count(*) from polaris.polaris.Organizations where OrganizationCodeID = 2 and OrganizationID != 4 )
	
    delete from polaris.polaris.DatesClosed where OrganizationID = @orgid

	insert into polaris.polaris.DatesClosed
	select	@orgid,
			DateClosed
	from 
	(
		select	DateClosed,
				COUNT(distinct o.ParentOrganizationID) as NumClosed
		from polaris.polaris.DatesClosed dc
		join polaris.polaris.Organizations o
			on dc.OrganizationID = o.OrganizationID
		where dc.DateClosed >= DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()), 0) -- first day of current year
		group by DateClosed
	) as sub
	where (NumClosed / @libraries) >= @threshold
	order by DateClosed

END


GO
/****** Object:  StoredProcedure [dbo].[GetCheckedOutItems]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCheckedOutItems]
	@patronId int,
	@itemType int = 0
AS
BEGIN
	SET NOCOUNT ON;

	declare @dueDate datetime = case when @itemType = 1 then current_timestamp + 10000 else current_timestamp end;

    select	ic.ItemRecordID,
			cir.Barcode,
			br.BrowseTitle,
			ic.OrganizationID,
			ic.CheckOutDate,
			ic.DueDate
	from Polaris.Polaris.ItemCheckouts ic
	join Polaris.Polaris.CircItemRecords cir on
		ic.ItemRecordID = cir.ItemRecordID
	join Polaris.Polaris.BibliographicRecords br on
		cir.AssociatedBibRecordID = br.BibliographicRecordID
	where ic.PatronID = @patronId and ic.CheckOutDate <= @dueDate
END

GO
/****** Object:  StoredProcedure [dbo].[GetClosedDates]    Script Date: 11/17/2015 3:18:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetClosedDates]
	@orgid int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    select * from Polaris.Polaris.DatesClosed dc where dc.OrganizationID = @orgid
END

-- CREATE USER AND PERMISSIONS

GO
USE [master]
GO
CREATE LOGIN [user] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]

USE [CLC_Notices]
GO
CREATE USER [user] FOR LOGIN [user]
GRANT EXECUTE TO [user]
ALTER ROLE [db_datareader] ADD MEMBER [user]
ALTER ROLE [db_datawriter] ADD MEMBER [user]
GO

USE [Polaris]
GO
CREATE USER [user] FOR LOGIN [user]
GRANT SELECT ON [Polaris].[DatesClosed] TO [user]
GRANT SELECT ON [Polaris].[Organizations] TO [user]
GRANT SELECT ON [Polaris].[Patrons] TO [user]
GRANT SELECT ON [Polaris].[BibliographicRecords] TO [user]
GRANT SELECT ON [Polaris].[CircItemRecords] TO [user]
GRANT SELECT ON [Polaris].[ItemCheckouts] TO [user]
GRANT SELECT ON [Polaris].[SysHoldRequests] TO [user]
GRANT SELECT ON [Polaris].[PatronNotes] TO [user]
GRANT DELETE ON [Polaris].[PatronNotes] TO [user]
GRANT UPDATE ON [Polaris].[PatronNotes] TO [user]
GRANT INSERT ON [Polaris].[PatronNotes] TO [user]
GRANT SELECT ON [Polaris].[PatronRegistration] TO [user]
GRANT DELETE ON [Polaris].[PatronRegistration] TO [user]
GRANT UPDATE ON [Polaris].[PatronRegistration] TO [user]
GRANT INSERT ON [Polaris].[PatronRegistration] TO [user]
GRANT SELECT ON [Polaris].[RecordSets] TO [user]
GRANT DELETE ON [Polaris].[RecordSets] TO [user]
GRANT UPDATE ON [Polaris].[RecordSets] TO [user]
GRANT INSERT ON [Polaris].[RecordSets] TO [user]
GRANT SELECT ON [Polaris].[PatronRecordSets] TO [user]
GRANT DELETE ON [Polaris].[PatronRecordSets] TO [user]
GRANT UPDATE ON [Polaris].[PatronRecordSets] TO [user]
GRANT INSERT ON [Polaris].[PatronRecordSets] TO [user]


USE [Results]
GO
CREATE USER [user] FOR LOGIN [user]
GRANT SELECT ON [Polaris].[NotificationQueue] TO [user]
GRANT SELECT ON [Polaris].[NotificationHistory] TO [user]
GO