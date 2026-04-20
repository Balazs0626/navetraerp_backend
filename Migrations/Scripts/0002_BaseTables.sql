
CREATE TABLE Addresses (
    id INT IDENTITY(1, 1) PRIMARY KEY,
    country NVARCHAR(50) NOT NULL,
    region NVARCHAR(50),
    post_code NVARCHAR(10) NOT NULL,
    city NVARCHAR(50) NOT NULL,
    address_1 NVARCHAR(100) NOT NULL,
    address_2 NVARCHAR(100)
);

CREATE TABLE Products (
    id INT IDENTITY(1, 1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    sku NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(MAX),
    unit NVARCHAR(50) NOT NULL,
    price_per_unit DECIMAL(18, 2) NOT NULL,
    active BIT NOT NULL,
    created_at DATETIME NOT NULL
);