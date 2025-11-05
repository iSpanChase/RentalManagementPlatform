IF NOT EXISTS (SELECT 1 FROM PERMISSIONS WHERE perm_code='Admin.ApproveOperator')
BEGIN
  INSERT INTO PERMISSIONS(perm_code, perm_name, module, action)
  VALUES('Admin.ApproveOperator', 'Admin.ApproveOperator', 'Admin', 'ApproveOperator');
END;

-- ±Âµ¹ ADMIN ¨¤¦â
INSERT INTO ROLE_PERMISSIONS(role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM ROLES r CROSS JOIN PERMISSIONS p
WHERE r.role_code='ADMIN' AND p.perm_code='Admin.ApproveOperator'
  AND NOT EXISTS(SELECT 1 FROM ROLE_PERMISSIONS rp WHERE rp.role_id=r.role_id AND rp.permission_id=p.permission_id);
