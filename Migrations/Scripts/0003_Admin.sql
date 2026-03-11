CREATE TABLE Roles (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	role_name VARCHAR(50) NOT NULL
);

CREATE TABLE Modules (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	module_name VARCHAR(50) NOT NULL
);

CREATE TABLE [Permissions] (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[permission_name] VARCHAR(50) NOT NULL,
	module_id INT NOT NULL REFERENCES Modules(id)
);

CREATE TABLE RolePermissions (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	role_id INT NOT NULL REFERENCES Roles(id),
	permission_id INT NOT NULL REFERENCES Permissions(id)
);

CREATE TABLE Users (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	username VARCHAR(50) NOT NULL,
	password_hash VARCHAR(100) NOT NULL,
	email VARCHAR(50) NOT NULL,
	active BIT NOT NULL,
    role_id INT NOT NULL REFERENCES Roles(id),
	reset_token VARCHAR(255),
	reset_token_expires DATETIME
);