CREATE TABLE Customers (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	[name] VARCHAR(100) NOT NULL,
	tax_number VARCHAR(50) NOT NULL,
	eu_tax_number VARCHAR(50) NOT NULL,
	bank_account_number VARCHAR(50) NOT NULL,
	email VARCHAR(50) NOT NULL,
	phone_number VARCHAR(50) NOT NULL,
	billing_address_id INT NOT NULL REFERENCES Addresses(id),
	shipping_address_id INT NOT NULL REFERENCES Addresses(id)
);

CREATE TABLE SalesOrders (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	customer_id INT NOT NULL REFERENCES Customers(id),
	order_date DATE NOT NULL,
	required_delivery_date DATE NOT NULL,
	[status] VARCHAR(20) NOT NULL,
	total_amount DECIMAL(18, 2) NOT NULL
);

CREATE TABLE SalesOrderItems (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	sales_order_id INT NOT NULL REFERENCES SalesOrders(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity_ordered DECIMAL(18, 2) NOT NULL,
	unit_price DECIMAL(18, 2) NOT NULL,
	discount DECIMAL(5, 2) NOT NULL,
	tax_rate DECIMAL(5, 2) NOT NULL,
	shipped_quantity DECIMAL(18, 2) NOT NULL,
);

CREATE TABLE Invoices (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	sales_order_id INT NOT NULL REFERENCES SalesOrders(id),
	invoice_date DATE NOT NULL,
	due_date DATE NOT NULL,
	total_amount DECIMAL(18, 2) NOT NULL,
	paid_amount DECIMAL(18, 2) NOT NULL,
	[status] VARCHAR(20) NOT NULL
);

CREATE TABLE InvoiceItems (
	id INT IDENTITY(1, 1) PRIMARY KEY,
	invoice_id INT NOT NULL REFERENCES Invoices(id),
	product_id INT NOT NULL REFERENCES Products(id),
	quantity DECIMAL(18, 2) NOT NULL,
	unit_price DECIMAL(18, 2) NOT NULL,
	tax_rate DECIMAL(5, 2) NOT NULL
);