UPDATE u
SET u.is_operator_pending = 1
FROM [USER] u
WHERE u.is_operator_pending = 0
  AND EXISTS (
      SELECT 1
      FROM USER_ROLES ur
      JOIN ROLES r ON r.role_id = ur.role_id
      WHERE ur.user_id = u.user_id
        AND r.role_code = 'OPERATOR'
  );
