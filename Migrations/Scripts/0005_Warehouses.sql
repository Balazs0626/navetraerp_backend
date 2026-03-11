CREATE TABLE Warehouses (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[name] VARCHAR(100) NOT NULL,
	address_id INT NOT NULL REFERENCES Addresses(id),
	manager_employee_id INT NOT NULL REFERENCES Employees(id)
);