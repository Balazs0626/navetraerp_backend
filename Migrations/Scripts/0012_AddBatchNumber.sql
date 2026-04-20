ALTER TABLE ProductionConsumptions
ADD batch_number NVARCHAR(50) NOT NULL;

ALTER TABLE SalesOrderItems
ADD batch_number NVARCHAR(50) NOT NULL;

ALTER TABLE DeliveryNoteItems
ADD batch_number NVARCHAR(50) NOT NULL;