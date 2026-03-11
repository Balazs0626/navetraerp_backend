
CREATE TABLE Addresses (
    id INT IDENTITY(1, 1) PRIMARY KEY,
    country VARCHAR(50) NOT NULL,
    region VARCHAR(50),
    post_code VARCHAR(10) NOT NULL,
    city VARCHAR(50) NOT NULL,
    address_1 VARCHAR(100) NOT NULL,
    address_2 VARCHAR(100)
);

CREATE TABLE Products (
    id INT IDENTITY(1, 1) PRIMARY KEY,
    [name] VARCHAR(100) NOT NULL,
    sku VARCHAR(50) NOT NULL,
    [description] VARCHAR(MAX),
    unit VARCHAR(50) NOT NULL,
    price_per_unit DECIMAL(18, 2) NOT NULL,
    active BIT NOT NULL,
    created_at DATETIME NOT NULL
);