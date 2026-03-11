GO
CREATE TRIGGER Trigg_Role_SetToDefault
ON Roles
INSTEAD OF DELETE
AS
BEGIN
	UPDATE Users
	SET role_id = 1
	WHERE role_id IN (SELECT id FROM deleted);

	DELETE Roles
	WHERE id IN (SELECT id FROM deleted);
END