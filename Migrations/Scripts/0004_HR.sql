CREATE TABLE Departments (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	department_name NVARCHAR(50) NOT NULL,
	[description] NVARCHAR(MAX)
);

CREATE TABLE Positions (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	position_name NVARCHAR(50) NOT NULL,
	[description] NVARCHAR(MAX)
);

CREATE TABLE Employees (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	first_name NVARCHAR(100) NOT NULL,
	last_name NVARCHAR(100) NOT NULL,
	birth_date DATE NOT NULL,
	id_number NVARCHAR(50) NOT NULL,
	residence_number NVARCHAR(50) NOT NULL,
	health_insurance_number NVARCHAR(50) NOT NULL,
	tax_id_number NVARCHAR(50) NOT NULL,
	bank_account_number NVARCHAR(50) NOT NULL,
	address_id INT NOT NULL REFERENCES Addresses(id),
	temp_address_id INT REFERENCES Addresses(id),
	hire_date DATE NOT NULL,
	department_id INT NOT NULL REFERENCES Departments(id),
	position_id INT NOT NULL REFERENCES Positions(id),
	[user_id] INT REFERENCES Users(id),
	email NVARCHAR(50),
	phone_number NVARCHAR(50),
	salary DECIMAL(18, 2) NOT NULL,
	[status] NVARCHAR(20) NOT NULL
);

CREATE TABLE LeaveRequests (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	[start_date] DATE NOT NULL,
	end_date DATE NOT NULL,
	leave_type NVARCHAR(50) NOT NULL,
	[status] NVARCHAR(20) NOT NULL
);

CREATE TABLE PerformanceReviews (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	review_date DATE NOT NULL,
	score INT NOT NULL,
	comment NVARCHAR(MAX)
);

CREATE TABLE Shifts (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	shift_name NVARCHAR(50) NOT NULL,
	start_time TIME NOT NULL,
	end_time TIME NOT NULL
);

CREATE TABLE EmployeeShifts (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	shift_id INT NOT NULL REFERENCES Shifts(id),
	[date] DATE NOT NULL
);