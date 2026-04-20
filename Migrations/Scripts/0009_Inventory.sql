CREATE TABLE InventoryItems(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	warehouse_id INT NOT NULL REFERENCES Warehouses(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity_on_hand DECIMAL(18, 2) NOT NULL,
	batch_number NVARCHAR(50) NOT NULL,
	last_updated DATETIME NOT NULL,
);

CREATE TABLE StockMovements(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	product_id INT NOT NULL REFERENCES Products(id),
	from_warehouse_id INT REFERENCES Warehouses(id),
	to_warehouse_id INT REFERENCES Warehouses(id),
	movement_type NVARCHAR(20) NOT NULL,
	quantity DECIMAL(18, 2) NOT NULL,
	reference_document NVARCHAR(50),
	movement_date DATE NOT NULL,
	performed_by_id INT REFERENCES Employees(id)
);

CREATE TABLE InventoryCounts(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	warehouse_id INT NOT NULL REFERENCES Warehouses(id),
	count_date DATE NOT NULL,
	counted_by_id INT NOT NULL REFERENCES Employees(id)
);

CREATE TABLE InventoryCountItems(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	count_id INT NOT NULL REFERENCES InventoryCounts(id),
	product_id INT NOT NULL REFERENCES Products(id),
	counted_quantity DECIMAL(18, 2) NOT NULL,
	system_quantity DECIMAL(18, 2) NOT NULL
);

CREATE TABLE DeliveryNotes(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	customer_id INT NOT NULL REFERENCES Customers(id),
	license_plate NVARCHAR(20) NOT NULL,
	[status] NVARCHAR(20) NOT NULL,
	create_date DATE NOT NULL,
	shipping_date DATE NOT NULL
);

CREATE TABLE DeliveryNoteItems(
	id INT IDENTITY(1, 1) PRIMARY KEY,
	delivery_note_id INT NOT NULL REFERENCES DeliveryNotes(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity DECIMAL(18, 2) NOT NULL
);