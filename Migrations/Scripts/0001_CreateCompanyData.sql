CREATE TABLE CompanyData (
    id INT PRIMARY KEY DEFAULT 1,
    [name] NVARCHAR(100) NOT NULL,
    tax_number NVARCHAR(50) NOT NULL,
    eu_tax_number NVARCHAR(50) NOT NULL,
    bank_account_number NVARCHAR(50) NOT NULL,
    registration_number NVARCHAR(50) NOT NULL,
    email NVARCHAR(50),
    phone_number NVARCHAR(50),
    billing_country NVARCHAR(50) NOT NULL,
    billing_region NVARCHAR(50) NOT NULL,
    billing_post_code NVARCHAR(50) NOT NULL,
    billing_city NVARCHAR(50) NOT NULL,
    billing_address_1 NVARCHAR(50) NOT NULL,
    billing_address_2 NVARCHAR(50),
    shipping_country NVARCHAR(50) NOT NULL,
    shipping_region NVARCHAR(50) NOT NULL,
    shipping_post_code NVARCHAR(50) NOT NULL,
    shipping_city NVARCHAR(50) NOT NULL,
    shipping_address_1 NVARCHAR(50) NOT NULL,
    shipping_address_2 NVARCHAR(50),

    CONSTRAINT CK_CompanyData_Limit CHECK (Id = 1)
);