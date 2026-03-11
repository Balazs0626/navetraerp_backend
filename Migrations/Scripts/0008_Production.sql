CREATE TABLE ProductionOrders(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	product_id INT NOT NULL REFERENCES Products(id),
	planned_quantity DECIMAL(18, 2) NOT NULL,
	[start_date] DATE NOT NULL,
	[end_date] DATE NOT NULL,
	[status] VARCHAR(20) NOT NULL,
	responsible_employee_id INT NOT NULL REFERENCES Employees(id)
);

CREATE TABLE ProductionConsumptions(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	production_order_id INT NOT NULL REFERENCES ProductionOrders(id),
	component_product_id INT NOT NULL REFERENCES Products(id),
	quantity_used DECIMAL(18, 2) NOT NULL,
	warehouse_id INT NOT NULL REFERENCES Warehouses(id),
	date_used DATE NOT NULL
);

CREATE TABLE Machines(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[name] VARCHAR(100) NOT NULL,
	code VARCHAR(50) NOT NULL,
	[description] VARCHAR(MAX),
	active BIT NOT NULL
);

CREATE TABLE ProductionOrderMachines(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	production_order_id INT NOT NULL REFERENCES ProductionOrders(id),
	machine_id INT NOT NULL REFERENCES Machines(id),
	[start_date] DATE NOT NULL,
	[end_date] DATE NOT NULL
);

CREATE TABLE ProductionOutputs(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	production_order_id INT NOT NULL REFERENCES ProductionOrders(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity_produced DECIMAL(18, 2) NOT NULL,
	warehouse_id INT NOT NULL REFERENCES Warehouses(id),
	date_produced DATE NOT NULL
);

CREATE TABLE BillOfMaterials(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	product_id INT NOT NULL REFERENCES Products(id),
	component_product_id INT NOT NULL REFERENCES Products(id),
	quantity_per_unit DECIMAL(18, 2) NOT NULL
);