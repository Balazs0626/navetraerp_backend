CREATE TABLE Suppliers (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[name] NVARCHAR(100) NOT NULL,
	tax_number NVARCHAR(50) NOT NULL,
	eu_tax_number NVARCHAR(50) NOT NULL,
	bank_account_number NVARCHAR(50) NOT NULL,
	contact_person NVARCHAR(50),
	email NVARCHAR(50) NOT NULL,
	phone_number NVARCHAR(50) NOT NULL,
	address_id INT NOT NULL REFERENCES Addresses(id)
);

CREATE TABLE PurchaseOrders (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	supplier_id INT NOT NULL REFERENCES Suppliers(id),
	order_date DATE NOT NULL,
	expected_delivery_date DATE NOT NULL,
	[status] NVARCHAR(20) NOT NULL,
	total_amount DECIMAL(18, 2) NOT NULL
);

CREATE TABLE PurchaseOrderItems (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	purchase_order_id INT NOT NULL REFERENCES PurchaseOrders(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity_ordered DECIMAL(18, 2) NOT NULL,
	price_per_unit DECIMAL(18, 2) NOT NULL,
	discount DECIMAL(5, 2) NOT NULL,
	tax_rate DECIMAL(5, 2) NOT NULL
);

CREATE TABLE GoodsReceipts (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	purchase_order_id INT NOT NULL REFERENCES PurchaseOrders(id),
	warehouse_id INT NOT NULL REFERENCES Warehouses(id),
	receipt_date DATE NOT NULL,
	received_by_employee_id INT NOT NULL REFERENCES Employees(id)
);

CREATE TABLE GoodsReceiptItems (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	goods_receipt_id INT NOT NULL REFERENCES GoodsReceipts(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity_received DECIMAL(18, 2) NOT NULL,
	batch_number NVARCHAR(50) NOT NULL
);