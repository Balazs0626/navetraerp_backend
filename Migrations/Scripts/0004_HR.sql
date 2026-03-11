CREATE TABLE Departments (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	department_name VARCHAR(50) NOT NULL,
	[description] VARCHAR(MAX)
);

CREATE TABLE Positions (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	position_name VARCHAR(50) NOT NULL,
	[description] VARCHAR(MAX)
);

CREATE TABLE Employees (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	first_name VARCHAR(100) NOT NULL,
	last_name VARCHAR(100) NOT NULL,
	birth_date DATE NOT NULL,
	id_number VARCHAR(50) NOT NULL,
	residence_number VARCHAR(50) NOT NULL,
	health_insurance_number VARCHAR(50) NOT NULL,
	tax_id_number VARCHAR(50) NOT NULL,
	bank_account_number VARCHAR(50) NOT NULL,
	address_id INT NOT NULL REFERENCES Addresses(id),
	temp_address_id INT REFERENCES Addresses(id),
	hire_date DATE NOT NULL,
	department_id INT NOT NULL REFERENCES Departments(id),
	position_id INT NOT NULL REFERENCES Positions(id),
	[user_id] INT REFERENCES Users(id),
	email VARCHAR(50),
	phone_number VARCHAR(50),
	salary DECIMAL(18, 2) NOT NULL,
	[status] VARCHAR(20) NOT NULL
);

CREATE TABLE LeaveRequests (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	[start_date] DATE NOT NULL,
	end_date DATE NOT NULL,
	leave_type VARCHAR(50) NOT NULL,
	[status] VARCHAR(20) NOT NULL
);

CREATE TABLE PerformanceReviews (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	review_date DATE NOT NULL,
	score INT NOT NULL,
	comment VARCHAR(MAX)
);

CREATE TABLE Shifts (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	shift_name VARCHAR(50) NOT NULL,
	start_time TIME NOT NULL,
	end_time TIME NOT NULL
);

CREATE TABLE EmployeeShifts (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	employee_id INT NOT NULL REFERENCES Employees(id),
	shift_id INT NOT NULL REFERENCES Shifts(id),
	[date] DATE NOT NULL
);