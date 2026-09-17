-- LoginDemo canonical schema — corrected copy of the original script.sql
-- Source of truth: C:\Users\monst\OneDrive\Desktop\WORK\script.sql (original, unfixed)
-- Corrections applied per Phase 0 schema review (approved 2026-09-07) — see
-- docs/LoginDemo_Users_Module_Setup_Guide.md Appendix C for the defect list.
-- Use this file for a fresh database. If you already ran the original script,
-- see the ALTER-only migration provided alongside this checkpoint instead.

USE [master]
GO
CREATE DATABASE [loginDemo]
GO
ALTER DATABASE [loginDemo] SET COMPATIBILITY_LEVEL = 130
GO
ALTER DATABASE [loginDemo] SET RECOVERY SIMPLE
GO
USE [loginDemo]
GO

/****** Table [dbo].[USERS_ACCOUNTS] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[USERS_ACCOUNTS](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[USERNAME] [nvarchar](50) NOT NULL,
	[PASSWORD] [nvarchar](255) NOT NULL,
	[FIRST_NAME] [nvarchar](50) NULL,
	[LAST_NAME] [nvarchar](50) NULL,
	[IS_ACTIVE] [bit] NOT NULL,
	[ROLE] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[ID] ASC
) ON [PRIMARY],
UNIQUE NONCLUSTERED
(
	[USERNAME] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** View [dbo].[vw_Users] ******/
-- Deliberately unfiltered — includes both active and inactive users.
-- The admin UI (Users list) filters/toggles active vs inactive client-side
-- so deactivated accounts can still be found and reactivated. Login validation
-- never uses this view (queries USERS_ACCOUNTS directly — see sp/service layer).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_Users] AS
SELECT
	ID,
	USERNAME,
	[ROLE] as UserRole,
	FIRST_NAME,
	LAST_NAME,
	IS_ACTIVE
FROM USERS_ACCOUNTS;
GO

/****** Index [IX_USERS_ACCOUNTS_ID_ISACTIVE] ******/
CREATE NONCLUSTERED INDEX [IX_USERS_ACCOUNTS_ID_ISACTIVE] ON [dbo].[USERS_ACCOUNTS]
(
	[ID] ASC,
	[IS_ACTIVE] ASC
) ON [PRIMARY]
GO

-- NOTE: IX_USERS_ACCOUNTS_USERNAME intentionally omitted — the UNIQUE constraint
-- on USERNAME above already creates its own supporting index; a second explicit
-- index on the same single column was pure write-overhead with no read benefit.

/****** Index [IX_USERS_ACCOUNTS_NAME] ******/
CREATE NONCLUSTERED INDEX [IX_USERS_ACCOUNTS_NAME] ON [dbo].[USERS_ACCOUNTS]
(
	[FIRST_NAME] ASC,
	[LAST_NAME] ASC
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[USERS_ACCOUNTS] ADD DEFAULT ((1)) FOR [IS_ACTIVE]
GO
ALTER TABLE [dbo].[USERS_ACCOUNTS] ADD DEFAULT ('user') FOR [ROLE]
GO
ALTER TABLE [dbo].[USERS_ACCOUNTS] WITH CHECK ADD CONSTRAINT [CHK_UserRole]
	CHECK (([role]='user' OR [role]='manager' OR [role]='admin'))
GO
ALTER TABLE [dbo].[USERS_ACCOUNTS] CHECK CONSTRAINT [CHK_UserRole]
GO

/****** StoredProcedure [dbo].[sp_DeleteUser] ******/
-- ADDED: @ActingUserId + a guard so an admin cannot deactivate their own account.
-- Enforced here as the last line of defence — also checked in the UI and in
-- UserService.DeleteUser.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_DeleteUser]
	@UserID INT,
	@ActingUserId INT
AS
BEGIN
	IF @UserID = @ActingUserId
	BEGIN
		RAISERROR('You cannot deactivate your own account.', 16, 1);
		RETURN;
	END

	UPDATE [dbo].[USERS_ACCOUNTS]
	SET [IS_ACTIVE] = 0
	WHERE [ID] = @UserID;
END;
GO

/****** StoredProcedure [dbo].[sp_InsertUserAccount] ******/
-- FIXED: now accepts @ROLE (was silently defaulted to 'user' with no way to
-- create a manager/admin account). Removed Check 2, which was byte-identical
-- to Check 1 and therefore unreachable dead code.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_InsertUserAccount]
	@USERNAME NVARCHAR(50),
	@PASSWORD NVARCHAR(255),
	@FIRST_NAME NVARCHAR(50),
	@LAST_NAME NVARCHAR(50),
	@IS_ACTIVE BIT = 1,
	@ROLE VARCHAR(50) = 'user'
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Check 1: Prevent duplicate usernames
		IF EXISTS (SELECT 1 FROM [loginDemo].[dbo].[USERS_ACCOUNTS] WHERE [USERNAME] = @USERNAME)
		BEGIN
			RAISERROR('Username already exists.', 16, 1);
			RETURN;
		END

		-- Check 2: Prevent a person (First Name + Last Name) from having multiple accounts
		IF EXISTS (SELECT 1 FROM [loginDemo].[dbo].[USERS_ACCOUNTS]
					WHERE [FIRST_NAME] = @FIRST_NAME
					  AND [LAST_NAME] = @LAST_NAME)
		BEGIN
			RAISERROR('An account for this First Name and Last Name already exists.', 16, 1);
			RETURN;
		END

		INSERT INTO [loginDemo].[dbo].[USERS_ACCOUNTS] ([USERNAME], [PASSWORD], [FIRST_NAME], [LAST_NAME], [IS_ACTIVE], [ROLE])
		VALUES (@USERNAME, @PASSWORD, @FIRST_NAME, @LAST_NAME, @IS_ACTIVE, @ROLE);

	END TRY
	BEGIN CATCH
		THROW;
	END CATCH
END
GO

/****** StoredProcedure [dbo].[sp_UpdateUser] ******/
-- FIXED: @Password widened from VARCHAR(50) to NVARCHAR(255) (was silently
-- truncating any password hash over 50 ASCII chars). @Role widened from
-- VARCHAR(10) to VARCHAR(50) to match the column. @UserName/@FirstName/
-- @LastName widened from VARCHAR(50) to NVARCHAR(50) to match their columns
-- (was silently corrupting non-ASCII names). Added a duplicate-username guard
-- on rename, matching sp_InsertUserAccount's pattern, so a rename collision
-- raises a friendly error instead of a raw constraint-violation exception.
-- ADDED: a duplicate First Name + Last Name guard (excluding the edited row),
-- mirroring sp_InsertUserAccount's Check 2, so an edit cannot make two accounts
-- share a person's name.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_UpdateUser]
	@Userid INT,
	@UserName NVARCHAR(50),
	@FirstName NVARCHAR(50),
	@LastName NVARCHAR(50),
	@Password NVARCHAR(255),
	@Role VARCHAR(50),
	@isActive BIT
AS
BEGIN
	IF NOT EXISTS (SELECT 1 FROM USERS_ACCOUNTS WHERE ID = @Userid)
	BEGIN
		RAISERROR('Cannot Update User, ID# does not exist!', 16, 1);
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USERS_ACCOUNTS WHERE USERNAME = @UserName AND ID <> @Userid)
	BEGIN
		RAISERROR('Username already exists.', 16, 1);
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USERS_ACCOUNTS
				WHERE FIRST_NAME = @FirstName
				  AND LAST_NAME = @LastName
				  AND ID <> @Userid)
	BEGIN
		RAISERROR('An account for this First Name and Last Name already exists.', 16, 1);
		RETURN;
	END

	UPDATE USERS_ACCOUNTS
	SET USERNAME = @UserName,
		FIRST_NAME = @FirstName,
		LAST_NAME = @LastName,
		PASSWORD = @Password,
		ROLE = @Role,
		IS_ACTIVE = @isActive
	WHERE ID = @Userid;
END
GO

/****** StoredProcedure [dbo].[sp_UpdateUserPassword] ******/
-- FIXED: @CurrentPassword/@NewPassword widened from VARCHAR(100) to
-- NVARCHAR(255) to match the PASSWORD column exactly (was truncating below
-- the column's own 255-char capacity).
-- ADDED: @ConfirmPassword parameter + two guards (confirmation must match the
-- new password; the new password must differ from the current one). Enforced
-- here as the last line of defence — also checked client-side and in
-- UserChangePasswordViewModel.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_UpdateUserPassword]
	@UserId INT,
	@CurrentPassword NVARCHAR(255),
	@NewPassword NVARCHAR(255),
	@ConfirmPassword NVARCHAR(255)
AS
BEGIN
	IF @NewPassword <> @ConfirmPassword
	BEGIN
		RAISERROR('New password and confirmation do not match.', 16, 1);
		RETURN;
	END

	IF @NewPassword = @CurrentPassword
	BEGIN
		RAISERROR('New password must be different from the current password.', 16, 1);
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USERS_ACCOUNTS WHERE ID = @UserId AND PASSWORD = @CurrentPassword)
	BEGIN
		UPDATE USERS_ACCOUNTS
		SET PASSWORD = @NewPassword
		WHERE ID = @UserId;
	END
	ELSE
	BEGIN
		RAISERROR('Current password is incorrect.', 16, 1);
	END
END
GO

USE [master]
GO
