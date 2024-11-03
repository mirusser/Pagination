-- Active: 1730640167544@@127.0.0.1@5432@paginationdemo@public
DROP TABLE IF EXISTS "Blogs";

CREATE TABLE "Blogs" (
    "Id" INT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
    "Name" TEXT,
    "LastUpdated" date DEFAULT now()
);

CREATE INDEX idx_lastupdated ON "Blogs" ("LastUpdated");

DO $$
    BEGIN
        FOR i IN 1..1000000 LOOP
                INSERT INTO "Blogs" ("Name", "LastUpdated")
                VALUES (i::TEXT, now() + (i * interval '1 day'));
            END LOOP;
    END $$;

SELECT * FROM "Blogs";

------------------------------------------------------

-- What if there are multiple rows with the same value?
SELECT b."Id", b."LastUpdated"
FROM "Blogs" as b
    WHERE b."LastUpdated" > '2024-12-03'
ORDER BY b."LastUpdated", b."Id"
LIMIT 10;

-- Query using keyset pagination to retrieve the next set of rows after a specified point
-- This is useful when there are multiple rows with the same "LastUpdated" value,
-- ensuring that pagination remains stable and predictable by using a composite key.
SELECT b."Id", b."LastUpdated"
FROM "Blogs" as b
-- WHERE b."LastUpdated" > '2024-12-03' OR (b."LastUpdated" = '2024-12-03' AND b."Id" > 31)
-- tuple comparison (or row-wise comparison) 
-- it's a shorter version of the where clause above 
-- supported by PostgreSQL and many others (not supported by MS SQL tho)
WHERE (b."LastUpdated", b."Id") > ('2024-12-03', 31)
ORDER BY b."LastUpdated", b."Id"
LIMIT 10;
