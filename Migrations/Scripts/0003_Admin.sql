CREATE TABLE Roles (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	role_name NVARCHAR(50) NOT NULL
);

CREATE TABLE Modules (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	module_name NVARCHAR(50) NOT NULL
);

CREATE TABLE [Permissions] (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[permission_name] NVARCHAR(50) NOT NULL,
	module_id INT NOT NULL REFERENCES Modules(id)
);

CREATE TABLE RolePermissions (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	role_id INT NOT NULL REFERENCES Roles(id),
	permission_id INT NOT NULL REFERENCES Permissions(id)
);

CREATE TABLE Users (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	username NVARCHAR(50) NOT NULL,
	password_hash NVARCHAR(100) NOT NULL,
	email NVARCHAR(50) NOT NULL,
	active BIT NOT NULL,
    role_id INT NOT NULL REFERENCES Roles(id),
	reset_token NVARCHAR(255),
	reset_token_expires DATETIME
);