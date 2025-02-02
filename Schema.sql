USE [ToDoList]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 2025/2/2 下午 07:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](100) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 2025/2/2 下午 07:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefreshTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Token] [nchar](50) NOT NULL,
	[UserId] [int] NOT NULL,
	[JwtId] [nvarchar](50) NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[IsRevorked] [bit] NOT NULL,
	[AddedDate] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Todo]    Script Date: 2025/2/2 下午 07:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Todo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Task] [nvarchar](50) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[IsComplete] [bit] NOT NULL,
 CONSTRAINT [PK_TodoList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Account] ADD  CONSTRAINT [DF_Account_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Todo] ADD  CONSTRAINT [DF_TodoList_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Todo] ADD  CONSTRAINT [DF_TodoList_IsComplete]  DEFAULT ((0)) FOR [IsComplete]
GO
