INSERT INTO "Users" ("Id", "Email", "Password", "IsEmailVerified", "CreatedAt", "UpdatedAt")
SELECT 
    gen_random_uuid(),
    'admin@example.com',
    'admin123',  -- В реальности здесь должен быть хеш пароля
    TRUE,
    NOW(),
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'admin@example.com');

INSERT INTO "Users" ("Id", "Email", "Password", "IsEmailVerified", "CreatedAt", "UpdatedAt")
SELECT 
    gen_random_uuid(),
    'user1@example.com',
    'user123',
    TRUE,
    NOW(),
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'user1@example.com');

INSERT INTO "Users" ("Id", "Email", "Password", "IsEmailVerified", "CreatedAt", "UpdatedAt")
SELECT 
    gen_random_uuid(),
    'user2@example.com',
    'password123',
    FALSE,
    NOW(),
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'user2@example.com');