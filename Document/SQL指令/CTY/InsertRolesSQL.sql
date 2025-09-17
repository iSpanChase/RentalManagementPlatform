INSERT INTO dbo.Roles (role_code, role_name, description)
VALUES
('GUEST',  N'訪客',        N'未登入的訪客'),
('TENANT', N'房客會員',    N'一般消費端會員/訂房者'),
('HOST',   N'房東會員',    N'上架房源之會員'),
('SUPPLIER', N'廠商會員',  N'上架服務公告之會員'),
('OPERATOR', N'系統管理員', N'平台後台管理員'),
('ADMIN',  N'最高權限系統管理員',  N'平台後台管理員/超級管理者');
